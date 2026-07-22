using UnityEngine;

public class Guard : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float pauseAtWaypoint = 2f;

    [Header("Vision")]
    [SerializeField] private float visionRange = 6f;
    [SerializeField] private float visionHalfAngle = 35f;
    [SerializeField] private float sweepAmplitude = 25f;
    [SerializeField] private float sweepSpeed = 0.4f;

    [Header("Bribe")]
    [SerializeField] private int bribeCost = 50;
    [SerializeField] private float lookAwayDuration = 8f;

    private int _currentWaypoint;
    private bool _pausing;
    private float _pauseTimer;
    private float _detection;
    private bool _caught;
    private bool _lookingAway;
    private float _lookAwayTimer;
    private float _baseFacing;
    private GameObject _coneObject;
    private MeshFilter _coneMeshFilter;
    private MeshRenderer _coneRenderer;
    private Material _coneMaterial;

    private const int CONE_SEGMENTS = 16;
    private static Shader _coneShader;
    private static Shader ConeShader => _coneShader ??= Shader.Find("Sprites/Default");
    private readonly RaycastHit2D[] _coverHits = new RaycastHit2D[16];
    private static readonly ContactFilter2D _coverFilter = new ContactFilter2D().NoFilter();

    public void SetWaypoints(Transform[] pts)
    {
        waypoints = pts;
        if (pts != null && pts.Length > 0 && pts[0] != null)
        {
            Vector2 dir = pts[0].position - transform.position;
            if (dir.magnitude > 0.001f)
                _baseFacing = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
    }

    private void Awake()
    {
        CreateCone();
    }

    private void OnDestroy()
    {
        if (_coneMaterial != null)
            Destroy(_coneMaterial);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (_caught)
        {
            UpdateConeColor();
            return;
        }

        UpdatePatrol(dt);

        if (_lookingAway)
        {
            _lookAwayTimer -= dt;
            if (_lookAwayTimer <= 0) _lookingAway = false;
            SetConeAlpha(0.05f);
            return;
        }

        float currentFacing = GetCurrentFacing();
        UpdateConeRotation(currentFacing);

        CheckDetection();

        if (_detection >= 100f && !_caught)
            TriggerCaught();

        UpdateConeColor();
    }

    private void UpdatePatrol(float dt)
    {
        if (waypoints == null || waypoints.Length == 0) return;

        if (_pausing)
        {
            _pauseTimer -= dt;
            if (_pauseTimer <= 0)
            {
                _currentWaypoint = (_currentWaypoint + 1) % waypoints.Length;
                _pausing = false;
            }
            return;
        }

        Transform target = waypoints[_currentWaypoint];
        if (target == null) return;

        Vector2 dir = (target.position - transform.position);
        float dist = dir.magnitude;

        if (dist < 0.1f)
        {
            _pausing = true;
            _pauseTimer = pauseAtWaypoint;
            return;
        }

        Vector2 move = dir.normalized * walkSpeed * dt;
        if (move.magnitude >= dist)
            transform.position = target.position;
        else
            transform.position += (Vector3)move;

        _baseFacing = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    private float GetCurrentFacing()
    {
        if (_pausing)
            return _baseFacing + Mathf.Sin(Time.time * sweepSpeed * Mathf.PI * 2f) * sweepAmplitude;
        return _baseFacing;
    }

    private void CheckDetection()
    {
        var player = PlayerController.Instance;
        if (player == null || !player.IsCarryingCrate)
        {
            _detection = 0;
            return;
        }

        Vector2 from = transform.position;
        Vector2 to = player.RB.position;
        float dist = Vector2.Distance(from, to);

        if (dist > visionRange)
        {
            _detection = 0;
            return;
        }

        Vector2 dirToPlayer = (to - from).normalized;
        float currentFacing = GetCurrentFacing();
        float playerAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        float delta = Mathf.DeltaAngle(currentFacing, playerAngle);

        if (Mathf.Abs(delta) > visionHalfAngle)
        {
            _detection = 0;
            return;
        }

        if (IsBlockedByCover(from, to))
        {
            _detection = 0;
            return;
        }

        _detection = 100;
    }

    private bool IsBlockedByCover(Vector2 from, Vector2 to)
    {
        Vector2 dir = (to - from).normalized;
        float dist = Vector2.Distance(from, to);
        int count = Physics2D.Raycast(from, dir, _coverFilter, _coverHits, dist);
        for (int i = 0; i < count; i++)
        {
            if (_coverHits[i].collider.isTrigger) continue;
            if (_coverHits[i].collider.GetComponentInParent<Building>() != null) return true;
        }
        return false;
    }

    private void TriggerCaught()
    {
        _caught = true;
        _detection = 0;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;
        GameEvents.OnCaughtBribe(bribeCost);
    }

    private void ResolveBribe(bool paid)
    {
        if (paid && GameManager.Instance != null && GameManager.Instance.TrySpend(bribeCost))
            GameEvents.OnToastRequested("The guard looks the other way.");
        else
            ConfiscateCrate();
        _lookingAway = true;
        _lookAwayTimer = lookAwayDuration;
        _caught = false;
    }

    internal void OnBribePaid() => ResolveBribe(paid: true);
    internal void OnBribeRefused() => ResolveBribe(paid: false);

    internal void ClearCaught()
    {
        _caught = false;
        _detection = 0;
    }

    public void ResetToStart()
    {
        _currentWaypoint = 0;
        _pausing = false;
        _pauseTimer = 0;
        _detection = 0;
        _caught = false;
        _lookingAway = false;
        if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
            int next = waypoints.Length > 1 ? 1 : 0;
            if (waypoints[next] != null)
            {
                Vector2 dir = waypoints[next].position - transform.position;
                if (dir.magnitude > 0.001f)
                    _baseFacing = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }
        }
    }

    private void ConfiscateCrate()
    {
        var player = PlayerController.Instance;
        if (player != null && player.IsCarryingCrate && player.CarriedCrate != null)
        {
            Destroy(player.CarriedCrate.gameObject);
            player.DropCrate();
            GameEvents.OnToastRequested("Moonshine confiscated!");
        }
    }

    private void CreateCone()
    {
        _coneObject = new GameObject("VisionCone");
        _coneObject.transform.SetParent(transform, false);

        _coneMeshFilter = _coneObject.AddComponent<MeshFilter>();
        _coneRenderer = _coneObject.AddComponent<MeshRenderer>();

        _coneMaterial = new Material(ConeShader);
        _coneMaterial.color = new Color(1f, 1f, 0.3f, 0.12f);
        _coneRenderer.material = _coneMaterial;
        _coneRenderer.sortingOrder = -1;

        GenerateConeMesh();
    }

    private void GenerateConeMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[CONE_SEGMENTS + 2];
        int[] triangles = new int[CONE_SEGMENTS * 3];

        vertices[0] = Vector3.zero;

        float halfRad = visionHalfAngle * Mathf.Deg2Rad;
        for (int i = 0; i <= CONE_SEGMENTS; i++)
        {
            float t = (float)i / CONE_SEGMENTS;
            float angle = -halfRad + t * (2f * halfRad);
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * visionRange,
                Mathf.Sin(angle) * visionRange,
                0
            );
        }

        for (int i = 0; i < CONE_SEGMENTS; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        _coneMeshFilter.mesh = mesh;
    }

    private void UpdateConeRotation(float facingDegrees)
    {
        _coneObject.transform.localRotation = Quaternion.Euler(0, 0, facingDegrees);
    }

    private void UpdateConeColor()
    {
        if (_coneMaterial == null) return;
        float t = _detection / 100f;
        Color safe = new Color(1f, 1f, 0.3f, 0.12f);
        Color danger = new Color(1f, 0.3f, 0.1f, 0.4f);
        _coneMaterial.color = Color.Lerp(safe, danger, t);
    }

    private void SetConeAlpha(float alpha)
    {
        if (_coneMaterial == null) return;
        var c = _coneMaterial.color;
        _coneMaterial.color = new Color(c.r, c.g, c.b, alpha);
    }

    public float DetectionLevel => _detection;
    public bool IsCaught => _caught;
}
