using UnityEngine;

public class Snowball : MonoBehaviour
{
    private Rigidbody2D rigidBody2D;
    private Vector2 moveDirection = Vector2.left;
    private float speed = 7f;
    private float lifeTime = 10f;
    private float spawnTime;

    public void Initialize(Vector2 newDirection, float newSpeed, float newLifeTime)
    {
        moveDirection = newDirection.normalized;
        speed = newSpeed;
        lifeTime = newLifeTime;
    }

    private void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        spawnTime = Time.time;
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (rigidBody2D == null)
        {
            return;
        }

        rigidBody2D.linearVelocity = moveDirection * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitPurly(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitPurly(other);
    }

    private void TryHitPurly(Collider2D other)
    {
        PurlyMovement purly = other.GetComponentInParent<PurlyMovement>();

        if (purly != null && SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.HandlePurlyHit(purly);
            Destroy(gameObject);
            return;
        }

        // Snowballs are trigger hitboxes, so walls and tilemaps should not stop them.
    }
}
