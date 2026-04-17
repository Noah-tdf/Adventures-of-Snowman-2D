using UnityEngine;

public class SnowGun : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float snowballSpeed = 7f;
    [SerializeField] private float upwardDirectionRange = 0.05f;
    [SerializeField] private float downwardDirectionRange = 0.28f;
    [SerializeField] private float verticalSpawnOffset = -1.6f;
    [SerializeField] private float forwardSpawnOffset = 0.95f;
    [SerializeField] private float extraVerticalTravelOffset = 0.75f;
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
        Vector2 direction = GetRandomStraightDirection();
        Vector3 basePosition = transform.position;
        basePosition.y += verticalSpawnOffset;
        Vector3 spawnPosition = basePosition + new Vector3(
            direction.x * forwardSpawnOffset,
            direction.y * extraVerticalTravelOffset,
            0f);
        spawnPosition.z = 0f;
        GameObject snowball = CreateSimpleSnowball(spawnPosition);
        Snowball snowballScript = snowball.GetComponent<Snowball>();

        if (snowballScript != null)
        {
            snowballScript.Initialize(direction, snowballSpeed, 10f);
        }

        Debug.Log($"Snowball spawned from {name} at {spawnPosition} with direction {direction} and speed {snowballSpeed}");
    }

    private Vector2 GetRandomStraightDirection()
    {
        float horizontalDirection = transform.position.x < 0f ? 1f : -1f;
        float randomVertical = Random.Range(-downwardDirectionRange, upwardDirectionRange);
        return new Vector2(horizontalDirection, randomVertical).normalized;
    }

    private GameObject CreateSimpleSnowball(Vector3 spawnPosition)
    {
        GameObject snowball = new GameObject("Snowball");
        snowball.name = "Snowball";
        snowball.transform.position = spawnPosition;
        snowball.transform.localScale = Vector3.one;

        CircleCollider2D collider2D = snowball.AddComponent<CircleCollider2D>();
        collider2D.radius = 0.22f;
        collider2D.isTrigger = false;

        Rigidbody2D rigidBody2D = snowball.AddComponent<Rigidbody2D>();
        rigidBody2D.gravityScale = 0f;
        rigidBody2D.freezeRotation = true;
        rigidBody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        SpriteRenderer spriteRenderer = snowball.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSnowballSprite();
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 220;
        snowball.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

        TrailRenderer trailRenderer = snowball.AddComponent<TrailRenderer>();
        trailRenderer.time = 0.28f;
        trailRenderer.startWidth = 0.16f;
        trailRenderer.endWidth = 0.04f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = new Color(1f, 1f, 1f, 0.95f);
        trailRenderer.endColor = new Color(1f, 1f, 1f, 0f);
        trailRenderer.sortingOrder = 219;

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
