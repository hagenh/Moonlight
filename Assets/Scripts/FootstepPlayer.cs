using UnityEngine;
using UnityEngine.Tilemaps;

public class FootstepPlayer : MonoBehaviour
{
    [SerializeField] internal AudioClip[] dirtClips;
    [SerializeField] internal AudioClip[] sandClips;
    [SerializeField] internal AudioClip[] stoneClips;
    [SerializeField] internal AudioClip[] waterClips;
    [SerializeField] private Tilemap surfaceMap;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float walkCadence = 0.4f;
    [SerializeField] private float sprintCadence = 0.25f;
    [SerializeField] private FootstepSurface defaultSurface = FootstepSurface.Dirt;
    [SerializeField] private float moveThreshold = 0.1f;

    private FootstepSurface currentSurface;
    private float stepTimer;
    private Rigidbody2D rb;

    public FootstepSurface CurrentSurface => currentSurface;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stepTimer = walkCadence;
    }

    private void Update()
    {
        UpdateCurrentSurface();

        if (IsMoving())
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayStep();
                stepTimer = IsSprinting() ? sprintCadence : walkCadence;
            }
        }
        else
        {
            stepTimer = walkCadence;
        }
    }

    private void UpdateCurrentSurface()
    {
        if (surfaceMap == null) return;

        Vector3Int cellPos = surfaceMap.WorldToCell(transform.position);
        var tile = groundTilemap.GetTile<FootstepTile>(cellPos);

        if (tile != null)
            currentSurface = tile.Surface;
        else
            currentSurface = defaultSurface;
    }

    private bool IsMoving()
    {
        if (rb == null) return false;
        return rb.linearVelocity.magnitude > moveThreshold;
    }

    private bool IsSprinting()
    {
        if (PlayerController.Instance == null) return false;
        return PlayerController.Instance.IsSprintHeld;
    }

    public AudioClip[] GetClipsForSurface(FootstepSurface surface)
    {
        return surface switch
        {
            FootstepSurface.Sand => sandClips,
            FootstepSurface.Stone => stoneClips,
            FootstepSurface.Water => waterClips,
            _ => dirtClips
        };
    }

    private void PlayStep()
    {
        var clips = GetClipsForSurface(currentSurface);
        if (clips == null || clips.Length == 0) return;
        if (audioSource == null) return;

        var clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
}
