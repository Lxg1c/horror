using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float staminaRegenDelay = 1.5f;

    public float CurrentStamina { get; private set; }
    public bool IsSprinting { get; private set; }

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool sprintHeld;
    private float staminaRegenTimer;

    private InputAction moveAction;
    private InputAction sprintAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        CurrentStamina = maxStamina;
    }

    private void OnEnable()
    {
        moveAction = InputManager.Instance.actions.Player.Move;
        sprintAction = InputManager.Instance.actions.Player.Sprint;
        moveAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        sprintAction.Disable();
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        sprintHeld = sprintAction.IsPressed();
        UpdateStamina();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void UpdateStamina()
    {
        IsSprinting = sprintHeld && moveInput.sqrMagnitude > 0.01f && CurrentStamina > 0f;

        if (IsSprinting)
        {
            CurrentStamina -= staminaDrainRate * Time.deltaTime;
            CurrentStamina = Mathf.Max(CurrentStamina, 0f);
            staminaRegenTimer = staminaRegenDelay;
        }
        else
        {
            staminaRegenTimer -= Time.deltaTime;
            if (staminaRegenTimer <= 0f)
            {
                CurrentStamina += staminaRegenRate * Time.deltaTime;
                CurrentStamina = Mathf.Min(CurrentStamina, maxStamina);
            }
        }
    }

    private void ApplyMovement()
    {
        float speed = IsSprinting ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= speed;
        move.y = rb.linearVelocity.y;
        rb.linearVelocity = move;
    }
}
