#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class CreatePlatformTilemaps
{
    private const string ScenePath = "Assets/Scenes/Adventure of A Snowman 2D.unity";
    private const string RootName = "Manual Platform Tilemaps";
    private const string TopTilemapName = "Top Platform Tilemap";
    private const string BottomTilemapName = "Bottom Platform Tilemap";
    private const string TopDecorationTilemapName = "Top Decoration Tilemap";
    private const string BottomDecorationTilemapName = "Bottom Decoration Tilemap";
    private const string TopColliderItemsTilemapName = "Top Collider Items Tilemap";
    private const string BottomColliderItemsTilemapName = "Bottom Collider Items Tilemap";

    [MenuItem("Tools/Snowman/Create Blank Manual Platform Tilemaps")]
    public static void CreateBlankTilemaps()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath);

        GameObject root = GameObject.Find(RootName);
        if (root == null)
        {
            root = new GameObject(RootName);
            root.transform.position = Vector3.zero;
        }

        Grid grid = root.GetComponent<Grid>();
        if (grid == null)
        {
            grid = root.AddComponent<Grid>();
        }

        grid.cellSize = new Vector3(0.5f, 0.5f, 1f);

        EnsureSolidTilemap(root.transform, TopTilemapName, new Vector3(0f, 0.75f, 0f), false, -2);
        EnsureSolidTilemap(root.transform, BottomTilemapName, new Vector3(0f, -1.25f, 0f), false, -2);
        EnsureVisualTilemap(root.transform, TopDecorationTilemapName, new Vector3(0f, 0.75f, 0f), 3);
        EnsureVisualTilemap(root.transform, BottomDecorationTilemapName, new Vector3(0f, -1.25f, 0f), 3);
        EnsureSolidTilemap(root.transform, TopColliderItemsTilemapName, new Vector3(0f, 0.75f, 0f), false, 2);
        EnsureSolidTilemap(root.transform, BottomColliderItemsTilemapName, new Vector3(0f, -1.25f, 0f), false, 2);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    public static void PaintNow()
    {
        CreateBlankTilemaps();
    }

    private static void EnsureSolidTilemap(Transform root, string name, Vector3 localPosition, bool clearTiles, int sortingOrder)
    {
        Transform existing = root.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        Tilemap tilemap = go.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            tilemap = go.AddComponent<Tilemap>();
        }

        if (clearTiles)
        {
            tilemap.ClearAllTiles();
        }

        TilemapRenderer renderer = go.GetComponent<TilemapRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<TilemapRenderer>();
        }

        renderer.mode = TilemapRenderer.Mode.Individual;
        renderer.sortingOrder = sortingOrder;

        TilemapCollider2D collider = go.GetComponent<TilemapCollider2D>();
        if (collider == null)
        {
            collider = go.AddComponent<TilemapCollider2D>();
        }

        collider.isTrigger = false;
    }

    private static void EnsureVisualTilemap(Transform root, string name, Vector3 localPosition, int sortingOrder)
    {
        Transform existing = root.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        Tilemap tilemap = go.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            tilemap = go.AddComponent<Tilemap>();
        }

        TilemapRenderer renderer = go.GetComponent<TilemapRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<TilemapRenderer>();
        }

        renderer.mode = TilemapRenderer.Mode.Individual;
        renderer.sortingOrder = sortingOrder;

        TilemapCollider2D collider = go.GetComponent<TilemapCollider2D>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }
    }

}
#endif
