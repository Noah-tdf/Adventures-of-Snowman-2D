using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PurlyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float jumpForce = 13.86f;
    [SerializeField] private float gravityScale = 3.5f;
    [SerializeField] private float maxFallSpeed = 14.0f;
    [SerializeField] private float groundCheckRadius = 0.18f;
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float bounceMultiplier = 0.9f;
    [SerializeField] private float bounceDuration = 0.12f;
    [SerializeField] private float walkAnimationThreshold = 0.1f;
    [SerializeField] private RuntimeAnimatorController animationController;

    private Rigidbody2D rigidBody2D;
    private Collider2D bodyCollider;
    private Animator animator;
    private Transform middleSphere;
    private float moveInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private float bounceVelocityX;
    private float bounceTimer;
    private float desiredVelocityX;
    private bool jumpQueued;
    private bool isGrounded;
    private float jumpCooldownTimer;
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }

        if (animator.runtimeAnimatorController == null && animationController != null)
        {
            animator.runtimeAnimatorController = animationController;
        }

        middleSphere = transform.Find("middleSphere");
        EnsureColliderSetup();
        bodyCollider = GetComponent<Collider2D>();
        CreateInputActions();
        ConfigurePlatformerBody();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        Vector2 rawMove = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        moveInput = Mathf.Clamp(rawMove.x, -1f, 1f);

        if (jumpAction.WasPressedThisFrame())
        {
            jumpQueued = true;
        }

        UpdateAnimatorState();
    }

    private void FixedUpdate()
    {
        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.fixedDeltaTime;
        }

        RefreshGroundedState();

        if (bounceTimer > 0f)
        {
            bounceTimer -= Time.fixedDeltaTime;
            rigidBody2D.linearVelocity = new Vector2(
                bounceVelocityX,
                Mathf.Max(rigidBody2D.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            desiredVelocityX = moveInput * speed;
            rigidBody2D.linearVelocity = new Vector2(
                desiredVelocityX,
                Mathf.Max(rigidBody2D.linearVelocity.y, -maxFallSpeed));
        }

        if (jumpQueued && CanJump())
        {
            float liftOffOffset = Mathf.Max(groundCheckDistance + groundCheckRadius, 0.05f);
            rigidBody2D.position += Vector2.up * liftOffOffset;

            Vector2 velocity = rigidBody2D.linearVelocity;
            velocity.x = desiredVelocityX;
            velocity.y = jumpForce;
            rigidBody2D.linearVelocity = velocity;
            rigidBody2D.WakeUp();
            isGrounded = false;
            jumpCooldownTimer = 0.18f;
        }

        if (jumpQueued)
        {
            jumpQueued = false;
        }
    }

    public Vector3 GetCurrentPosition()
    {
        return transform.position;
    }

    public float GetMiddleSphereRotationY()
    {
        if (middleSphere == null)
        {
            return 0f;
        }

        return middleSphere.localEulerAngles.y;
    }

    public void RestoreSavedState(Vector3 savedPosition, float middleSphereYRotation)
    {
        transform.position = savedPosition;
        if (middleSphere == null)
        {
            return;
        }

        Vector3 currentRotation = middleSphere.localEulerAngles;
        currentRotation.y = middleSphereYRotation;
        middleSphere.localEulerAngles = currentRotation;
    }

    private void CreateInputActions()
    {
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/leftStick");

        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void UpdateAnimatorState()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        float moveX = Mathf.Abs(moveInput) >= walkAnimationThreshold ? moveInput : 0f;
        animator.SetFloat(MoveXHash, moveX);

        if (jumpQueued)
        {
            animator.SetTrigger(JumpHash);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryBounce(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (bounceTimer > 0f)
        {
            return;
        }

        TryBounce(collision);
    }

    private void TryBounce(Collision2D collision)
    {
        if (rigidBody2D == null || collision.contactCount == 0 || !IsWallCollision(collision))
        {
            return;
        }

        Vector2 normal = collision.GetContact(0).normal;
        Vector2 incomingVelocity = new Vector2(
            Mathf.Abs(desiredVelocityX) > 0.001f ? desiredVelocityX : rigidBody2D.linearVelocity.x,
            rigidBody2D.linearVelocity.y);

        if (incomingVelocity.sqrMagnitude <= 0.001f)
        {
            return;
        }

        if (Vector2.Dot(incomingVelocity.normalized, normal) > -0.05f)
        {
            return;
        }

        Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal);
        if (reflectedVelocity.sqrMagnitude < 0.01f)
        {
            reflectedVelocity = normal;
        }

        bounceVelocityX = reflectedVelocity.normalized.x * Mathf.Max(Mathf.Abs(incomingVelocity.x) * bounceMultiplier, 2.4f);
        bounceTimer = Mathf.Max(bounceDuration, 0.12f);
        rigidBody2D.position += normal * 0.08f;
        rigidBody2D.linearVelocity = new Vector2(bounceVelocityX, rigidBody2D.linearVelocity.y);
    }

    private void EnsureColliderSetup()
    {
        Collider2D rootCollider = GetComponent<Collider2D>();
        if (rootCollider != null)
        {
            rootCollider.enabled = true;
            rootCollider.isTrigger = false;
            return;
        }

        CircleCollider2D generatedCollider = GetComponent<CircleCollider2D>();
        if (generatedCollider == null)
        {
            generatedCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        generatedCollider.isTrigger = false;
        generatedCollider.radius = 0.8f;
        generatedCollider.offset = Vector2.zero;
        generatedCollider.enabled = true;
    }

    private void ConfigurePlatformerBody()
    {
        rigidBody2D.gravityScale = gravityScale;
        rigidBody2D.freezeRotation = true;
        rigidBody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void RefreshGroundedState()
    {
        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<Collider2D>();
        }

        if (bodyCollider == null)
        {
            isGrounded = false;
            return;
        }

        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundLayers,
            useTriggers = false
        };

        int hitCount = bodyCollider.Cast(Vector2.down, filter, groundHits, groundCheckDistance + groundCheckRadius);
        isGrounded = false;

        for (int i = 0; i < hitCount; i++)
        {
            if (groundHits[i].collider == null)
            {
                continue;
            }

            if (groundHits[i].transform.IsChildOf(transform))
            {
                continue;
            }

            if (groundHits[i].normal.y < 0.35f)
            {
                continue;
            }

            isGrounded = true;
            return;
        }

        isGrounded = bodyCollider.IsTouchingLayers(groundLayers);
    }

    private bool CanJump()
    {
        if (jumpCooldownTimer > 0f)
        {
            return false;
        }

        if (isGrounded)
        {
            return true;
        }

        if (bodyCollider != null && bodyCollider.IsTouchingLayers(groundLayers))
        {
            return true;
        }

        return Mathf.Abs(rigidBody2D.linearVelocity.y) < 0.05f;
    }

    private Collider2D GetActiveCollider()
    {
        return GetComponent<Collider2D>();
    }

    private static bool IsWallCollision(Collision2D collision)
    {
        Transform current = collision.transform;
        while (current != null)
        {
            if (current.name == "Walls")
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}
