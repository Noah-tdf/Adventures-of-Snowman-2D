using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PurlyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float rotationSpeed = 200.0f;
    [SerializeField] private float bounceMultiplier = 0.9f;
    [SerializeField] private float bounceDuration = 0.12f;

    private InputAction moveAction;
    private InputAction rotateAction;
    private PurlyMovementModel movementModel;
    private PurlyMovementView movementView;

    private void Awake()
    {
        movementModel = GetComponent<PurlyMovementModel>();
        if (movementModel == null)
        {
            movementModel = gameObject.AddComponent<PurlyMovementModel>();
        }

        movementView = GetComponent<PurlyMovementView>();
        if (movementView == null)
        {
            movementView = gameObject.AddComponent<PurlyMovementView>();
        }

        movementView.Initialize(bounceMultiplier, bounceDuration);
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
        movementModel.SetMoveInput(moveAction.ReadValue<Vector2>());
        movementModel.SetRotationInput(rotateAction.ReadValue<float>());
    }

    private void FixedUpdate()
    {
        movementView.ApplyMovement(movementModel, speed, rotationSpeed);
    }

    public Vector3 GetCurrentPosition()
    {
        return transform.position;
    }

    public float GetMiddleSphereRotationY()
    {
        return movementView.GetMiddleSphereRotationY();
    }

    public void RestoreSavedState(Vector3 savedPosition, float middleSphereYRotation)
    {
        transform.position = savedPosition;
        movementView.SetMiddleSphereRotation(middleSphereYRotation);
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
}
