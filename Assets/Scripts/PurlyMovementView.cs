using UnityEngine;

public class PurlyMovementView : MonoBehaviour
{
    private Rigidbody2D rigidBody2D;
    private Transform middleSphere;
    private Collider2D activeCollider2D;
    private float bounceMultiplier;
    private float bounceDuration;
    private Vector2 bounceVelocity;
    private float bounceTimer;
    private Vector2 desiredVelocity;

    public void Initialize(float newBounceMultiplier, float newBounceDuration)
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        middleSphere = transform.Find("middleSphere");
        bounceMultiplier = newBounceMultiplier;
        bounceDuration = newBounceDuration;
        EnsureColliderSetup();
    }

    public void ApplyMovement(PurlyMovementModel movementModel, float moveSpeed, float rotateSpeed)
    {
        if (rigidBody2D == null || movementModel == null)
        {
            return;
        }

        if (bounceTimer > 0f)
        {
            bounceTimer -= Time.fixedDeltaTime;
            rigidBody2D.linearVelocity = bounceVelocity;
        }
        else
        {
            desiredVelocity = movementModel.MoveInput * moveSpeed;
            rigidBody2D.linearVelocity = desiredVelocity;
        }

        if (middleSphere != null && Mathf.Abs(movementModel.RotationInput) > 0.01f)
        {
            middleSphere.Rotate(0f, movementModel.RotationInput * rotateSpeed * Time.fixedDeltaTime, 0f);
        }
    }

    public float GetMiddleSphereRotationY()
    {
        if (middleSphere == null)
        {
            return 0f;
        }

        return middleSphere.localEulerAngles.y;
    }

    public void SetMiddleSphereRotation(float yRotation)
    {
        if (middleSphere == null)
        {
            return;
        }

        Vector3 currentRotation = middleSphere.localEulerAngles;
        currentRotation.y = yRotation;
        middleSphere.localEulerAngles = currentRotation;
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
                activeCollider2D = colliders[i];
                return;
            }
        }

        CircleCollider2D generatedCollider = gameObject.GetComponent<CircleCollider2D>();
        if (generatedCollider == null)
        {
            generatedCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        generatedCollider.isTrigger = false;
        generatedCollider.radius = 0.8f;
        generatedCollider.offset = Vector2.zero;
        generatedCollider.enabled = true;
        activeCollider2D = generatedCollider;
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
