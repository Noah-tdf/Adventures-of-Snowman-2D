using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// ============================================================
// Regular Kill Zone
// ============================================================
// This is used for simple death trigger areas, such as falling below
// the platform. When Purly touches this trigger, the game ends.
public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PurlyMovement purly = other.GetComponent<PurlyMovement>();
        if (purly != null)
        {
            SnowmanGameManager.Instance.HandlePurlyHit(purly);
        }
    }
}

// ============================================================
// Waterfall Tilemap Hazard
// ============================================================
// This scans Tilemaps for waterfall/water tiles and checks whether
// Purly overlaps those tile areas. If she does, she melts.
public class WaterfallHazardTilemap : MonoBehaviour
{
    private readonly HashSet<Vector3Int> waterfallCells = new HashSet<Vector3Int>();
    private readonly List<Bounds> waterfallWorldBounds = new List<Bounds>();
    private Tilemap tilemap;
    private PurlyMovement purly;
    private Collider2D purlyCollider;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallWaterfallHazards()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddHazardsToCurrentScene();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddHazardsToCurrentScene();
    }

    public static void AddHazardsToCurrentScene()
    {
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        for (int i = 0; i < tilemaps.Length; i++)
        {
            Tilemap foundTilemap = tilemaps[i];
            if (foundTilemap == null || foundTilemap.GetComponent<WaterfallHazardTilemap>() != null)
            {
                continue;
            }

            if (ContainsWaterfallTile(foundTilemap))
            {
                foundTilemap.gameObject.AddComponent<WaterfallHazardTilemap>();
            }
        }

        EnsureKnownWaterfallVolume();
    }

    private static void EnsureKnownWaterfallVolume()
    {
        if (GameObject.Find("Waterfall Hazard Volume") != null)
        {
            return;
        }

        GameObject hazard = new GameObject("Waterfall Hazard Volume");
        hazard.transform.position = new Vector3(6.58f, -3.8f, 0f);

        BoxCollider2D collider = hazard.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(2.9f, 6.4f);

        hazard.AddComponent<WaterfallHazardVolume>();
    }

    private static bool ContainsWaterfallTile(Tilemap tilemapToCheck)
    {
        BoundsInt bounds = tilemapToCheck.cellBounds;
        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (IsWaterfallTile(tilemapToCheck, cell))
            {
                return true;
            }
        }

        return false;
    }

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        CacheWaterfallCells();
    }

    private void Update()
    {
        if (tilemap == null || waterfallCells.Count == 0)
        {
            return;
        }

        if (purly == null)
        {
            purly = FindFirstObjectByType<PurlyMovement>();
            purlyCollider = purly != null ? purly.GetComponent<Collider2D>() : null;
        }

        if (purly == null || purlyCollider == null)
        {
            return;
        }

        Bounds purlyBounds = purlyCollider.bounds;
        for (int i = 0; i < waterfallWorldBounds.Count; i++)
        {
            if (!purlyBounds.Intersects(waterfallWorldBounds[i]))
            {
                continue;
            }

            if (SnowmanGameManager.Instance != null)
            {
                SnowmanGameManager.Instance.HandlePurlyMelted(purly);
                enabled = false;
            }

            return;
        }
    }

    private void CacheWaterfallCells()
    {
        waterfallCells.Clear();
        waterfallWorldBounds.Clear();

        if (tilemap == null)
        {
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (IsWaterfallTile(tilemap, cell))
            {
                waterfallCells.Add(cell);
                waterfallWorldBounds.Add(GetVisibleWaterfallBounds(cell));
            }
        }
    }

    private Bounds GetVisibleWaterfallBounds(Vector3Int cell)
    {
        Sprite sprite = tilemap.GetSprite(cell);
        Vector3 center = tilemap.GetCellCenterWorld(cell);

        if (sprite == null)
        {
            return new Bounds(center, tilemap.cellSize);
        }

        Vector3 size = sprite.bounds.size;
        size.x = Mathf.Max(size.x, tilemap.cellSize.x);
        size.y = Mathf.Max(size.y, tilemap.cellSize.y);

        Bounds bounds = new Bounds(center, size);
        bounds.Expand(0.1f);
        return bounds;
    }

    private static bool IsWaterfallTile(Tilemap sourceTilemap, Vector3Int cell)
    {
        TileBase tile = sourceTilemap.GetTile(cell);
        if (tile == null)
        {
            return false;
        }

        if (NameLooksLikeWaterfall(tile.name))
        {
            return true;
        }

        Sprite sprite = sourceTilemap.GetSprite(cell);
        return sprite != null && NameLooksLikeWaterfall(sprite.name);
    }

    private static bool NameLooksLikeWaterfall(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string lower = value.ToLowerInvariant();
        return lower.Contains("waterfall") || lower.Contains("water fall") || lower.Contains("water");
    }
}

// ============================================================
// Waterfall Backup Trigger Volume
// ============================================================
// This is a hidden backup kill zone placed over the waterfall column.
// It also checks overlap every frame, so Purly still melts even if
// Unity trigger callbacks miss the collision.
public class WaterfallHazardVolume : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private PurlyMovement purly;
    private Collider2D purlyCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (boxCollider == null)
        {
            return;
        }

        if (purly == null)
        {
            purly = FindFirstObjectByType<PurlyMovement>();
            purlyCollider = purly != null ? purly.GetComponent<Collider2D>() : null;
        }

        if (purly == null || purlyCollider == null)
        {
            return;
        }

        Bounds hazardBounds = boxCollider.bounds;
        hazardBounds.Expand(0.15f);
        if (hazardBounds.Intersects(purlyCollider.bounds))
        {
            MeltPurly(purly);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryMelt(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryMelt(other);
    }

    private void TryMelt(Collider2D other)
    {
        PurlyMovement purly = other.GetComponentInParent<PurlyMovement>();
        if (purly == null || SnowmanGameManager.Instance == null)
        {
            return;
        }

        MeltPurly(purly);
    }

    private void MeltPurly(PurlyMovement purly)
    {
        if (purly == null || SnowmanGameManager.Instance == null)
        {
            return;
        }

        SnowmanGameManager.Instance.HandlePurlyMelted(purly);
    }
}
