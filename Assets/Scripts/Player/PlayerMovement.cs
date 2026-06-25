using System;
using UnityEngine;

/// <summary>
/// CharacterController movement: walk, run, jump, crouch.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public enum CrouchMode
    {
        Hold,
        Toggle
    }

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -18f;

    [Header("Crouch")]
    [SerializeField] private CrouchMode crouchMode = CrouchMode.Toggle;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraStandingLocalY = 0.85f;
    [SerializeField] private float cameraCrouchLocalY = 0.45f;

    private CharacterController controller;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float currentHeight;
    private bool isCrouching;
    private bool movementEnabled = true;

    public bool MovementEnabled
    {
        get => movementEnabled;
        set => movementEnabled = value;
    }

    public bool IsCrouching => isCrouching;
    public bool IsGrounded => controller != null && controller.isGrounded;
    public float TotalDistanceMoved { get; private set; }

    public event Action<float> OnDistanceMoved;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        currentHeight = standingHeight;
        ApplyControllerHeight(standingHeight);
    }

    private void Update()
    {
        HandleCrouchInput();
        HandleMovement();
        TrackDistance();
    }

    private void HandleCrouchInput()
    {
        if (!movementEnabled)
            return;

        if (crouchMode == CrouchMode.Toggle)
        {
            if (Input.GetKeyDown(crouchKey))
                SetCrouching(!isCrouching);
        }
        else
        {
            bool shouldCrouch = Input.GetKey(crouchKey);
            if (shouldCrouch != isCrouching)
                SetCrouching(shouldCrouch);
        }
    }

    private void HandleMovement()
    {
        if (controller == null)
            return;

        if (IsGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        Vector3 input = Vector3.zero;
        if (movementEnabled)
        {
            input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);
        }

        bool running = movementEnabled && Input.GetKey(runKey) && !isCrouching && input.sqrMagnitude > 0.01f;
        float targetSpeed = running ? runSpeed : walkSpeed;
        if (isCrouching)
            targetSpeed = walkSpeed * 0.5f;

        Vector3 worldDirection = transform.TransformDirection(input) * targetSpeed;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, worldDirection, acceleration * Time.deltaTime);

        if (movementEnabled && IsGrounded && Input.GetButtonDown("Jump") && !isCrouching)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
        Vector3 oncekiKonum = transform.position;
        controller.Move(motion * Time.deltaTime);
        Vector3 delta = transform.position - oncekiKonum;
        float planarDistance = new Vector3(delta.x, 0f, delta.z).magnitude;
        if (planarDistance > 0.001f)
        {
            TotalDistanceMoved += planarDistance;
            OnDistanceMoved?.Invoke(TotalDistanceMoved);
        }

        UpdateCrouchGeometry();
    }

    private void SetCrouching(bool crouch)
    {
        isCrouching = crouch;
    }

    private void UpdateCrouchGeometry()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        ApplyControllerHeight(currentHeight);

        if (cameraTransform != null)
        {
            float t = Mathf.InverseLerp(crouchHeight, standingHeight, currentHeight);
            float camY = Mathf.Lerp(cameraCrouchLocalY, cameraStandingLocalY, t);
            Vector3 local = cameraTransform.localPosition;
            cameraTransform.localPosition = new Vector3(local.x, camY, local.z);
        }
    }

    private void ApplyControllerHeight(float height)
    {
        controller.height = height;
        controller.center = new Vector3(0f, height * 0.5f, 0f);
    }

    private void TrackDistance()
    {
        // Distance is accumulated from CharacterController.Move delta in HandleMovement.
    }

    public void ResetDistanceTracking()
    {
        TotalDistanceMoved = 0f;
    }
}
