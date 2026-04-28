using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-450)]
public sealed class Assignment4PlatformSurfaceBuilder : MonoBehaviour
{
    private const string GameplaySceneName = "Adventure of A Snowman 2D";
    private const string RootName = "Assignment04ReferenceObjects";
    private const string TopPlatformName = "Top Platform";
    private const string BottomPlatformName = "Bottom Platform";
    private const string GeneratedRootName = "GeneratedPlatformColliders";

    private static bool sceneHooked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!sceneHooked)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            sceneHooked = true;
        }

        TryCreate(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreate(scene);
    }

    private static void TryCreate(Scene scene)
    {
        if (!scene.IsValid() || scene.name != GameplaySceneName)
        {
            return;
        }

        if (FindFirstObjectByType<Assignment4PlatformSurfaceBuilder>() != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject(nameof(Assignment4PlatformSurfaceBuilder));
        bootstrap.AddComponent<Assignment4PlatformSurfaceBuilder>();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != GameplaySceneName)
        {
            Destroy(gameObject);
            return;
        }

        BuildPlatformColliders();
    }

    private void BuildPlatformColliders()
    {
        GameObject rootObject = GameObject.Find(RootName);
        if (rootObject == null)
        {
            return;
        }

        Transform root = rootObject.transform;
        Transform generatedRoot = FindChild(root, GeneratedRootName);
        if (generatedRoot != null)
        {
            Destroy(generatedRoot.gameObject);
        }

        BuildRowsForPlatform(FindChild(root, TopPlatformName), "Top");
        BuildRowsForPlatform(FindChild(root, BottomPlatformName), "Bottom");
    }

    private static void BuildRowsForPlatform(Transform platformRoot, string prefix)
    {
        if (platformRoot == null)
        {
            return;
        }

        ClearGeneratedColliders(platformRoot, prefix);

        List<SpriteRenderer> platformTiles = new List<SpriteRenderer>();
        SpriteRenderer[] renderers = platformRoot.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && IsWalkableRenderer(renderers[i]))
            {
                platformTiles.Add(renderers[i]);
            }
        }

        if (platformTiles.Count == 0)
        {
            return;
        }

        List<List<SpriteRenderer>> rows = GroupIntoRows(platformTiles, 0.2f);
        int colliderIndex = 0;
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            List<Bounds> clusters = GroupRowIntoClusters(rows[rowIndex], 0.15f);
            for (int clusterIndex = 0; clusterIndex < clusters.Count; clusterIndex++)
            {
                CreateColliderStrip(platformRoot, $"{prefix}PlatformCollider_{colliderIndex}", clusters[clusterIndex]);
                colliderIndex++;
            }
        }
    }

    private static void ClearGeneratedColliders(Transform platformRoot, string prefix)
    {
        string generatedPrefix = $"{prefix}PlatformCollider_";
        for (int i = platformRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = platformRoot.GetChild(i);
            if (!child.name.StartsWith(generatedPrefix))
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static List<List<SpriteRenderer>> GroupIntoRows(List<SpriteRenderer> renderers, float tolerance)
    {
        renderers.Sort((left, right) => right.bounds.center.y.CompareTo(left.bounds.center.y));

        List<List<SpriteRenderer>> rows = new List<List<SpriteRenderer>>();
        for (int i = 0; i < renderers.Count; i++)
        {
            SpriteRenderer renderer = renderers[i];
            bool added = false;

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                float rowY = rows[rowIndex][0].bounds.center.y;
                if (Mathf.Abs(renderer.bounds.center.y - rowY) <= tolerance)
                {
                    rows[rowIndex].Add(renderer);
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                rows.Add(new List<SpriteRenderer> { renderer });
            }
        }

        return rows;
    }

    private static List<Bounds> GroupRowIntoClusters(List<SpriteRenderer> row, float gapTolerance)
    {
        row.Sort((left, right) => left.bounds.min.x.CompareTo(right.bounds.min.x));

        List<Bounds> clusters = new List<Bounds>();
        if (row.Count == 0)
        {
            return clusters;
        }

        Bounds current = row[0].bounds;
        for (int i = 1; i < row.Count; i++)
        {
            Bounds next = row[i].bounds;
            float gap = next.min.x - current.max.x;

            if (gap <= gapTolerance)
            {
                current.Encapsulate(next);
            }
            else
            {
                clusters.Add(current);
                current = next;
            }
        }

        clusters.Add(current);
        return clusters;
    }

    private static void CreateColliderStrip(Transform parent, string name, Bounds bounds)
    {
        float thickness = Mathf.Clamp(bounds.size.y * 0.4f, 0.18f, 0.45f);
        Vector3 position = new Vector3(bounds.center.x, bounds.max.y - (thickness * 0.5f), 0f);

        GameObject colliderObject = new GameObject(name);
        colliderObject.transform.SetParent(parent, false);
        colliderObject.transform.position = position;

        BoxCollider2D collider = colliderObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(bounds.size.x, thickness);
        collider.offset = Vector2.zero;
    }

    private static bool IsWalkableRenderer(SpriteRenderer renderer)
    {
        string name = renderer.name;
        if (name.StartsWith("Platform"))
        {
            return true;
        }

        return name.Contains("Island");
    }

    private static Transform FindChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
            {
                return child;
            }
        }

        return null;
    }
}
