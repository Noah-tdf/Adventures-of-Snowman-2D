using UnityEngine;

public class Balloon : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider2D;
    private bool isAvailable = true;

    public BalloonWall WallSide { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        TryRegister();
    }

    private void OnEnable()
    {
        TryRegister();
    }

    public Vector3 GetLocalPosition()
    {
        return transform.localPosition;
    }

    public void SetLocalPosition(Vector3 newLocalPosition)
    {
        transform.localPosition = newLocalPosition;
    }

    public void AssignWall(BalloonWall wallSide)
    {
        WallSide = wallSide;
    }

    public void Hide()
    {
        isAvailable = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (circleCollider2D != null)
        {
            circleCollider2D.enabled = false;
        }
    }

    public void Show()
    {
        isAvailable = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        if (circleCollider2D != null)
        {
            circleCollider2D.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAvailable)
        {
            return;
        }

        PurlyMovement purly = other.GetComponentInParent<PurlyMovement>();
        if (purly != null && SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.PopBalloon(this);
        }
    }

    private void TryRegister()
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.RegisterBalloon(this);
        }
    }
}
