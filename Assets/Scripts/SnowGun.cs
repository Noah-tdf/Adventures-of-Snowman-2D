using UnityEngine;

public class SnowGun : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float minSnowballSpeed = 5f;
    [SerializeField] private float maxSnowballSpeed = 10f;
    [SerializeField] private Vector2 targetRandomOffset = new Vector2(0.75f, 1.2f);
    [SerializeField] private float verticalSpawnOffset = -1.45f;
    [SerializeField] private float forwardSpawnOffset = 0.65f;
    [SerializeField] private float minTargetY = -1.25f;
    [SerializeField] private float maxTargetY = 1.35f;
    [SerializeField] private float middleShotChance = 0.8f;
    [SerializeField] private float maxUpwardAim = -0.15f;
    private static Sprite snowballSprite;

    private void Start()
    {
        TryRegister();
    }

    private void OnEnable()
    {
        TryRegister();
    }

    public void Fire()
    {
        FireAt(null);
    }

    public void FireAt(Transform target)
    {
        Vector3 spawnPosition = GetSpawnPosition();
        Vector2 direction = GetDirectionTowardTarget(spawnPosition, target);
        float snowballSpeed = Random.Range(minSnowballSpeed, maxSnowballSpeed);

        spawnPosition.z = 0f;
        GameObject snowball = CreateSimpleSnowball(spawnPosition);
        Snowball snowballScript = snowball.GetComponent<Snowball>();

        if (snowballScript != null)
        {
            snowballScript.Initialize(direction, snowballSpeed, 10f);
        }

        Debug.Log($"Snowball spawned from {name} at {spawnPosition} with direction {direction} and speed {snowballSpeed}");
    }

    private Vector3 GetSpawnPosition()
    {
        float horizontalDirection = GetHorizontalDirection();
        Vector3 basePosition = firePoint != null ? firePoint.position : transform.position;
        basePosition.y += verticalSpawnOffset;
        basePosition.x += horizontalDirection * forwardSpawnOffset;
        return basePosition;
    }

    private Vector2 GetDirectionTowardTarget(Vector3 spawnPosition, Transform target)
    {
        float horizontalDirection = GetHorizontalDirection();
        Vector3 targetPosition;

        if (target != null && Random.value > middleShotChance)
        {
            // Aim near Purly, not exactly at the same point every time.
            targetPosition = target.position + new Vector3(
                Random.Range(-targetRandomOffset.x, targetRandomOffset.x),
                Random.Range(-targetRandomOffset.y, targetRandomOffset.y),
                0f);
        }
        else
        {
            // Most shots cross the playable middle area, which keeps them out of the top wall.
            targetPosition = spawnPosition + new Vector3(
                horizontalDirection * Random.Range(7f, 13f),
                Random.Range(minTargetY, maxTargetY) - spawnPosition.y,
                0f);
        }

        Vector2 direction = targetPosition - spawnPosition;

        // Snow guns sit in the upper corners, so do not allow upward/top-wall shots.
        // This keeps snowballs from scraping along the top wall.
        if (direction.y > maxUpwardAim)
        {
            direction.y = maxUpwardAim;
        }

        if (Mathf.Abs(direction.x) < 0.1f)
        {
            direction.x = horizontalDirection;
        }

        direction.x = Mathf.Abs(direction.x) * horizontalDirection;
        return direction.normalized;
    }

    private float GetHorizontalDirection()
    {
        return transform.position.x < 0f ? 1f : -1f;
    }

    private GameObject CreateSimpleSnowball(Vector3 spawnPosition)
    {
        GameObject snowball = new GameObject("Snowball");
        snowball.name = "Snowball";
        snowball.transform.position = spawnPosition;
        snowball.transform.localScale = Vector3.one;

        CircleCollider2D collider2D = snowball.AddComponent<CircleCollider2D>();
        collider2D.radius = 0.22f;
        collider2D.isTrigger = true;

        Rigidbody2D rigidBody2D = snowball.AddComponent<Rigidbody2D>();
        rigidBody2D.gravityScale = 0f;
        rigidBody2D.freezeRotation = true;
        rigidBody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        SpriteRenderer spriteRenderer = snowball.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSnowballSprite();
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 500;
        snowball.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

        TrailRenderer trailRenderer = snowball.AddComponent<TrailRenderer>();
        trailRenderer.time = 0.28f;
        trailRenderer.startWidth = 0.16f;
        trailRenderer.endWidth = 0.04f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = new Color(1f, 1f, 1f, 0.95f);
        trailRenderer.endColor = new Color(1f, 1f, 1f, 0f);
        trailRenderer.sortingOrder = 499;

        snowball.AddComponent<Snowball>();
        return snowball;
    }

    private static Sprite GetSnowballSprite()
    {
        if (snowballSprite != null)
        {
            return snowballSprite;
        }

        Sprite loadedSprite = Resources.Load<Sprite>("snowball");
        if (loadedSprite != null)
        {
            snowballSprite = loadedSprite;
            return snowballSprite;
        }

        Texture2D texture = new Texture2D(64, 64);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color fill = Color.white;

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = x - 32f;
                float dy = y - 32f;
                bool insideCircle = dx * dx + dy * dy <= 900f;
                texture.SetPixel(x, y, insideCircle ? fill : clear);
            }
        }

        texture.Apply();
        snowballSprite = Sprite.Create(texture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 64f);
        return snowballSprite;
    }

    private void TryRegister()
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.RegisterSnowGun(this);
        }
    }
}
