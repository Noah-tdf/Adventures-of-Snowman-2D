using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PurlyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float rotationSpeed = 200.0f;
    [SerializeField] private float bounceMultiplier = 0.9f;
    [SerializeField] private float bounceDuration = 0.12f;

    private Rigidbody2D rigidBody2D;
    private Transform middleSphere;
    private Vector2 moveInput;
    private float rotationInput;
    private InputAction moveAction;
    private InputAction rotateAction;
    private Vector2 bounceVelocity;
    private float bounceTimer;
    private Vector2 desiredVelocity;

    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        middleSphere = transform.Find("middleSphere");
        EnsureColliderSetup();
        CreateInputActions();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        rotateAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        rotateAction.Disable();
    }

    private void Update()
    {
        moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        rotationInput = Mathf.Clamp(rotateAction.ReadValue<float>(), -1f, 1f);
    }

    private void FixedUpdate()
    {
        if (bounceTimer > 0f)
        {
            bounceTimer -= Time.fixedDeltaTime;
            rigidBody2D.linearVelocity = bounceVelocity;
        }
        else
        {
            desiredVelocity = moveInput * speed;
            rigidBody2D.linearVelocity = desiredVelocity;
        }

        if (middleSphere != null && Mathf.Abs(rotationInput) > 0.01f)
        {
            middleSphere.Rotate(0f, rotationInput * rotationSpeed * Time.fixedDeltaTime, 0f);
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

        rotateAction = new InputAction("Rotate", InputActionType.Value);
        rotateAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/q")
            .With("Positive", "<Keyboard>/e");
        rotateAction.AddBinding("<Gamepad>/rightStick/x");
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
        Vector2 incomingVelocity = desiredVelocity.sqrMagnitude > 0.001f ? desiredVelocity : rigidBody2D.linearVelocity;

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

        bounceVelocity = reflectedVelocity.normalized * Mathf.Max(incomingVelocity.magnitude * bounceMultiplier, 2.4f);
        bounceTimer = Mathf.Max(bounceDuration, 0.12f);
        rigidBody2D.position += normal * 0.08f;
        rigidBody2D.linearVelocity = bounceVelocity;
    }

    private void EnsureColliderSetup()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].enabled)
            {
                return;
            }
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
