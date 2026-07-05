using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float moveDeadzone = 0.1f;
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private PlayerState currentState;
    public FacingDirection facingDirection;

    private Vector2 moveInput;
    private bool isSprintHeld;
    
    [SerializeField] private Vector2 interactBoxSize = new Vector2(1, 1);
    [SerializeField] private LayerMask interactableLayer;

    public IInteractable CurrentInteractable { get; private set; }

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

        currentState = new IdleState(this);
        currentState.Enter();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Player.RemoveCallbacks(this);
        inputActions.Dispose();
    }

    private void Update()
    {
        UpdateInteractDetection();
        currentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
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

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        FacingDirection newFacing;
        if (angle < 45f || angle >= 315f)
            newFacing = FacingDirection.Right;
        else if (angle < 135f)
            newFacing = FacingDirection.Up;
        else if (angle < 225f)
            newFacing = FacingDirection.Left;
        else
            newFacing = FacingDirection.Down;

        if (newFacing != facingDirection)
        {
            facingDirection = newFacing;
            SetAnimatorInteger(AnimatorParams.FacingDirection, (int)facingDirection);
        }
    }

    public void SetAnimatorTrigger(int triggerHash)
    {
        if (animator != null && animator.isActiveAndEnabled)
            animator.SetTrigger(triggerHash);
    }

    public void SetAnimatorFloat(int paramHash, float value)
    {
        if (animator != null && animator.isActiveAndEnabled)
            animator.SetFloat(paramHash, value);
    }

    public void SetAnimatorInteger(int paramHash, int value)
    {
        if (animator != null && animator.isActiveAndEnabled)
            animator.SetInteger(paramHash, value);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
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
            currentState.OnInteractPerformed();
        }
    }
    public void OnCrouch(InputAction.CallbackContext context) { }
    public void OnJump(InputAction.CallbackContext context) { }
    public void OnLook(InputAction.CallbackContext context) { }
    public void OnPrevious(InputAction.CallbackContext context) { }
    public void OnNext(InputAction.CallbackContext context) { }

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
        return facingDirection switch
        {
            FacingDirection.Down => new Vector2(0, -1),
            FacingDirection.Up => new Vector2(0, 1),
            FacingDirection.Left => new Vector2(-1, 0),
            FacingDirection.Right => new Vector2(1, 0),
            _ => Vector2.zero
        };
    }
}
