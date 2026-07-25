using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DirectionalSpriteAnimator))]
public class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float moveDeadzone = 0.1f;
    [SerializeField] private DirectionalSpriteAnimator spriteAnimator;
    [SerializeField] private SpriteRenderer carrySpriteRenderer;

    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private PlayerState currentState;
    public FacingDirection facingDirection;

    private Vector2 moveInput;
    private bool isSprintHeld;
    
    [SerializeField] private Vector2 interactBoxSize = new Vector2(0.25f, 0.25f);
    [SerializeField] private LayerMask interactableLayer;

    public IInteractable CurrentInteractable { get; internal set; }
    public bool IsMenuOpen { get; set; }

    public bool IsCarrying { get; private set; }
    public Debris CarriedDebris { get; private set; }

    public bool IsCarryingCrate { get; private set; }
    public Crate CarriedCrate { get; private set; }

    public bool IsCarryingAnything => IsCarrying || IsCarryingCrate;

    public bool IsInteractHeld { get; private set; }

    public Rigidbody2D RB => rb;
    public float WalkSpeed => walkSpeed;
    public float SprintSpeed => sprintSpeed;
    public Vector2 MoveInput => moveInput;
    public bool IsSprintHeld => isSprintHeld;
    public Vector2 InteractBoxSize => interactBoxSize;
    public LayerMask InteractableLayer => interactableLayer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        inputActions = new InputSystem_Actions();
        inputActions.Player.AddCallbacks(this);

        if (spriteAnimator == null)
            spriteAnimator = GetComponent<DirectionalSpriteAnimator>();

        currentState = new IdleState(this);
        currentState.Enter();
    }

    private void OnEnable()
    {
        if (inputActions != null)
            inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
            inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.RemoveCallbacks(this);
            inputActions.Dispose();
        }
    }

    private void Update()
    {
        if (!IsMenuOpen)
            UpdateInteractDetection();
        if (spriteAnimator != null)
            spriteAnimator.Tick(Time.deltaTime);
        currentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        if (IsMenuOpen)
            rb.linearVelocity = Vector2.zero;
        else
            currentState.PhysicsUpdate();
    }

    public void ChangeState(PlayerState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void UpdateFacingDirection(Vector2 input)
    {
        if (input.magnitude < moveDeadzone) return;

        FacingDirection newFacing = FacingMath.FromVector(input);

        if (newFacing != facingDirection)
        {
            facingDirection = newFacing;
            if (spriteAnimator != null)
                spriteAnimator.SetFacing(facingDirection);
        }
    }

    public void PlayAnimation(string clipName)
    {
        if (spriteAnimator != null)
            spriteAnimator.Play(clipName);
    }

    public void SetAnimatorTrigger(int triggerHash)
    {
    }

    public void SetAnimatorFloat(int paramHash, float value)
    {
    }

    public void SetAnimatorInteger(int paramHash, int value)
    {
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (IsMenuOpen)
        {
            moveInput = Vector2.zero;
            return;
        }
        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
            if (moveInput.magnitude < moveDeadzone)
                moveInput = Vector2.zero;
            currentState.OnMovePerformed(moveInput);
        }
        else if (context.canceled)
        {
            moveInput = Vector2.zero;
            currentState.OnMoveCanceled();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (IsMenuOpen) return;
        if (context.performed)
        {
            isSprintHeld = true;
            currentState.OnSprintPerformed();
        }
        else if (context.canceled)
        {
            isSprintHeld = false;
            currentState.OnSprintCanceled();
        }
    }

    public void OnAttack(InputAction.CallbackContext context) { }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsInteractHeld = true;

            if (IsMenuOpen)
            {
                GameEvents.OnMenuCloseRequested();
                return;
            }
            currentState.OnInteractPerformed();
        }
        else if (context.canceled)
        {
            IsInteractHeld = false;
            currentState.OnInteractCanceled();
        }
    }

    public void OnCrouch(InputAction.CallbackContext context) { }
    public void OnJump(InputAction.CallbackContext context) { }
    public void OnLook(InputAction.CallbackContext context) { }
    public void OnPrevious(InputAction.CallbackContext context) { }
    public void OnNext(InputAction.CallbackContext context) { }

    public void PickUpDebris(Debris debris)
    {
        IsCarrying = true;
        CarriedDebris = debris;
    }

    public void DropDebris()
    {
        IsCarrying = false;
        CarriedDebris = null;
    }

    public void DropDebrisAtFeet()
    {
        if (CarriedDebris != null)
            CarriedDebris.Respawn(rb.position);
        IsCarrying = false;
        CarriedDebris = null;
    }

    public void PickUpCrate(Crate crate)
    {
        IsCarryingCrate = true;
        CarriedCrate = crate;
    }

    public void DropCrate()
    {
        IsCarryingCrate = false;
        CarriedCrate = null;
    }

    public void DropCrateAtFeet()
    {
        if (CarriedCrate != null)
            CarriedCrate.Respawn(rb.position);
        IsCarryingCrate = false;
        CarriedCrate = null;
    }

    public void ForceIdle()
    {
        ChangeState(new IdleState(this));
    }

    public void ShowCarrySprite(bool visible)
    {
        if (carrySpriteRenderer == null) return;

        if (visible && IsCarryingCrate && CarriedCrate != null)
        {
            var crateSr = CarriedCrate.GetComponent<SpriteRenderer>();
            if (crateSr != null)
                carrySpriteRenderer.sprite = crateSr.sprite;
            else if (ContentDb.Instance != null && ContentDb.Instance.CrateCarrySprite != null)
                carrySpriteRenderer.sprite = ContentDb.Instance.CrateCarrySprite;
        }
        carrySpriteRenderer.enabled = visible;
        carrySpriteRenderer.gameObject.SetActive(visible);
    }

    private void UpdateInteractDetection()
    {
        Vector2 origin = rb.position + GetFacingOffset();
        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, interactBoxSize, 0f, interactableLayer);

        CurrentInteractable = null;
        foreach (Collider2D hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable is Building building)
            {
                bool isBoard = building.BoardTrigger != null && hit == building.BoardTrigger;
                bool isDoor = building.DoorTrigger != null && hit == building.DoorTrigger;
                if (!isBoard && !isDoor)
                    continue;
                building.LastHitTrigger = hit;
            }

            if (interactable != null)
            {
                CurrentInteractable = interactable;
                break;
            }
        }
    }

    private Vector2 GetFacingOffset()
    {
        return FacingMath.GetFacingOffset(facingDirection);
    }
}
