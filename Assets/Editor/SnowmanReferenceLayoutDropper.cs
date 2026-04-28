using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SnowmanReferenceLayoutDropper
{
    private const string ScenePath = "Assets/Scenes/Adventure of A Snowman 2D.unity";
    private const string BackgroundSpritePath = "Assets/HobiSoLoved/2D Platformer Snowy/Sprites/Background.png";
    private const string SnowSpritePath = "Assets/HobiSoLoved/2D Platformer Snowy/Sprites/Platform snow2.png";
    private const string RockSpritePath = "Assets/HobiSoLoved/2D Platformer Snowy/Sprites/Platform2.png";
    private const string FloatingSpritePath = "Assets/HobiSoLoved/2D Platformer Snowy/Sprites/Platform3.png";
    private const string TreeSpritePath = "Assets/HobiSoLoved/2D Platformer Snowy/Sprites/tree_snow.png";
    private const string SignSpritePath = "Assets/HobiSoLoved/2D Platformer Snowy/Sprites/Sign.png";
    private const string StumpSpritePath = "Assets/HobiSoLoved/2D Platformer Snowy/Sprites/stump_snow.png";
    private const string GrassSpritePath = "Assets/HobiSoLoved/2D Platformer Snowy/Sprites/Grass.png";
    private const string MushroomSpritePath = "Assets/HobiSoLoved/2D Platformer Snowy/Sprites/Mushroom.png";
    private const string CrystalSpritePath = "Assets/HobiSoLoved/2D Platformer Snowy/Sprites/crytal snow.png";
    private const string BalloonSpritePath = "Assets/Textures/cyan-balloons.png";
    private const string GeneratedFolder = "Assets/GeneratedReference";
    private const string WaterfallTexturePath = "Assets/GeneratedReference/ReferenceWaterfall.png";

    [MenuItem("Tools/Snowman/Drop Reference Platform Layout")]
    public static void DropReferencePlatformLayout()
    {
        EnsureFolder("Assets", "GeneratedReference");

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Bounds arenaBounds = GetArenaBounds();

        Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath);
        Sprite snow = AssetDatabase.LoadAssetAtPath<Sprite>(SnowSpritePath);
        Sprite rock = AssetDatabase.LoadAssetAtPath<Sprite>(RockSpritePath);
        Sprite floating = AssetDatabase.LoadAssetAtPath<Sprite>(FloatingSpritePath);
        Sprite tree = AssetDatabase.LoadAssetAtPath<Sprite>(TreeSpritePath);
        Sprite sign = AssetDatabase.LoadAssetAtPath<Sprite>(SignSpritePath);
        Sprite stump = AssetDatabase.LoadAssetAtPath<Sprite>(StumpSpritePath);
        Sprite grass = AssetDatabase.LoadAssetAtPath<Sprite>(GrassSpritePath);
        Sprite mushroom = AssetDatabase.LoadAssetAtPath<Sprite>(MushroomSpritePath);
        Sprite crystal = AssetDatabase.LoadAssetAtPath<Sprite>(CrystalSpritePath);
        Sprite balloon = AssetDatabase.LoadAssetAtPath<Sprite>(BalloonSpritePath);
        Sprite waterfall = CreateWaterfallSprite();

        GameObject root = FindOrCreateRoot("Assignment04ReferenceObjects");
        ClearChildren(root.transform);

        CreatePanel(root.transform, "ReferencePanelTop", background, WorldPoint(arenaBounds, 0.66f, 0.74f, 0.5f), 0.62f, 0.24f);
        CreatePanel(root.transform, "ReferencePanelBottom", background, WorldPoint(arenaBounds, 0.73f, 0.28f, 0.5f), 0.58f, 0.22f);

        CreateFloatingIsland(root.transform, "UpperIslandA", floating, WorldPoint(arenaBounds, 0.29f, 0.67f), 0.95f);
        CreateFloatingIsland(root.transform, "UpperIslandB", floating, WorldPoint(arenaBounds, 0.38f, 0.57f), 1.05f);
        CreatePlatformGroup(root.transform, "UpperPlatformLeft", snow, rock, WorldPoint(arenaBounds, 0.53f, 0.51f), 5, 2, 0.75f);
        CreatePlatformGroup(root.transform, "UpperBridge", snow, rock, WorldPoint(arenaBounds, 0.64f, 0.66f), 2, 1, 0.75f);
        CreatePlatformGroup(root.transform, "UpperPlatformRight", snow, rock, WorldPoint(arenaBounds, 0.80f, 0.51f), 8, 2, 0.75f);
        CreateFloatingIsland(root.transform, "UpperIslandC", floating, WorldPoint(arenaBounds, 0.95f, 0.58f), 0.95f);

        CreateFloatingIsland(root.transform, "LowerIslandA", floating, WorldPoint(arenaBounds, 0.41f, 0.23f), 0.95f);
        CreateFloatingIsland(root.transform, "LowerIslandB", floating, WorldPoint(arenaBounds, 0.50f, 0.11f), 1.05f);
        CreatePlatformGroup(root.transform, "LowerPlatformMain", snow, rock, WorldPoint(arenaBounds, 0.72f, 0.11f), 9, 2, 0.75f);
        CreateFloatingIsland(root.transform, "LowerIslandC", floating, WorldPoint(arenaBounds, 0.93f, 0.23f), 0.95f);

        CreateWaterfall(root.transform, "WaterfallTop", waterfall, WorldPoint(arenaBounds, 0.64f, 0.47f), 1.25f, 2.9f);
        CreateWaterfall(root.transform, "WaterfallBottom", waterfall, WorldPoint(arenaBounds, 0.96f, 0.13f), 1.2f, 2.7f);

        CreateDecoration(root.transform, "GrassTop", grass, WorldPoint(arenaBounds, 0.48f, 0.60f), 0.42f, 6);
        CreateDecoration(root.transform, "TreeTopLeft", tree, WorldPoint(arenaBounds, 0.57f, 0.61f), 0.42f, 5);
        CreateDecoration(root.transform, "TreeTopRight", tree, WorldPoint(arenaBounds, 0.85f, 0.61f), 0.42f, 5);
        CreateDecoration(root.transform, "SignTop", sign, WorldPoint(arenaBounds, 0.74f, 0.58f), 0.28f, 6);
        CreateDecoration(root.transform, "StumpTop", stump, WorldPoint(arenaBounds, 0.79f, 0.57f), 0.25f, 6);
        CreateDecoration(root.transform, "SignBottom", sign, WorldPoint(arenaBounds, 0.68f, 0.17f), 0.28f, 6);
        CreateDecoration(root.transform, "TreeBottom", tree, WorldPoint(arenaBounds, 0.81f, 0.19f), 0.42f, 5);
        CreateDecoration(root.transform, "MushroomBottom", mushroom, WorldPoint(arenaBounds, 0.87f, 0.16f), 0.25f, 6);
        CreateDecoration(root.transform, "CrystalBottom", crystal, WorldPoint(arenaBounds, 0.94f, 0.18f), 0.33f, 6);

        CreateBalloon(root.transform, "BalloonTopA", balloon, WorldPoint(arenaBounds, 0.46f, 0.77f, -1f));
        CreateBalloon(root.transform, "BalloonTopB", balloon, WorldPoint(arenaBounds, 0.74f, 0.77f, -1f));
        CreateBalloon(root.transform, "BalloonTopC", balloon, WorldPoint(arenaBounds, 0.95f, 0.77f, -1f));
        CreateBalloon(root.transform, "BalloonBottomA", balloon, WorldPoint(arenaBounds, 0.58f, 0.30f, -1f));
        CreateBalloon(root.transform, "BalloonBottomB", balloon, WorldPoint(arenaBounds, 0.72f, 0.30f, -1f));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Reference platform layout dropped into scene.");
    }

    [MenuItem("Tools/Snowman/Drop 5 Platform2")]
    public static void DropFivePlatform2()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Bounds arenaBounds = GetArenaBounds();
        Sprite rock = AssetDatabase.LoadAssetAtPath<Sprite>(RockSpritePath);

        GameObject root = FindOrCreateRoot("Platform2Set");
        ClearChildren(root.transform);

        Vector3[] positions =
        {
            WorldPoint(arenaBounds, 0.38f, 0.62f),
            WorldPoint(arenaBounds, 0.46f, 0.62f),
            WorldPoint(arenaBounds, 0.54f, 0.62f),
            WorldPoint(arenaBounds, 0.62f, 0.62f),
            WorldPoint(arenaBounds, 0.70f, 0.62f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            CreateDecoration(root.transform, $"Platform2_{i + 1}", rock, positions[i], 0.75f, 2);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Dropped 5 Platform2 objects into scene.");
    }

    [MenuItem("Tools/Snowman/Drop Simple Reference Set")]
    public static void DropSimpleReferenceSet()
    {
        EnsureFolder("Assets", "GeneratedReference");

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Bounds arenaBounds = GetArenaBounds();

        Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath);
        Sprite snow = AssetDatabase.LoadAssetAtPath<Sprite>(SnowSpritePath);
        Sprite floating = AssetDatabase.LoadAssetAtPath<Sprite>(FloatingSpritePath);
        Sprite tree = AssetDatabase.LoadAssetAtPath<Sprite>(TreeSpritePath);
        Sprite sign = AssetDatabase.LoadAssetAtPath<Sprite>(SignSpritePath);
        Sprite stump = AssetDatabase.LoadAssetAtPath<Sprite>(StumpSpritePath);
        Sprite grass = AssetDatabase.LoadAssetAtPath<Sprite>(GrassSpritePath);
        Sprite mushroom = AssetDatabase.LoadAssetAtPath<Sprite>(MushroomSpritePath);
        Sprite crystal = AssetDatabase.LoadAssetAtPath<Sprite>(CrystalSpritePath);
        Sprite waterfall = CreateWaterfallSprite();

        GameObject platformSet = GameObject.Find("Platform2Set");
        if (platformSet != null)
        {
            Object.DestroyImmediate(platformSet);
        }

        GameObject root = FindOrCreateRoot("Assignment04ReferenceObjects");
        ClearChildren(root.transform);

        CreatePanel(root.transform, "ReferencePanelTop", background, WorldPoint(arenaBounds, 0.66f, 0.74f, 0.5f), 0.62f, 0.24f);
        CreatePanel(root.transform, "ReferencePanelBottom", background, WorldPoint(arenaBounds, 0.73f, 0.28f, 0.5f), 0.58f, 0.22f);

        CreateFloatingIsland(root.transform, "UpperIslandA", floating, WorldPoint(arenaBounds, 0.29f, 0.67f), 0.95f);
        CreateFloatingIsland(root.transform, "UpperIslandB", floating, WorldPoint(arenaBounds, 0.38f, 0.57f), 1.05f);
        CreateTopOnlyPlatformGroup(root.transform, "UpperPlatformLeft", snow, WorldPoint(arenaBounds, 0.53f, 0.51f), 5, 0.75f);
        CreateTopOnlyPlatformGroup(root.transform, "UpperBridge", snow, WorldPoint(arenaBounds, 0.64f, 0.66f), 2, 0.75f);
        CreateTopOnlyPlatformGroup(root.transform, "UpperPlatformRight", snow, WorldPoint(arenaBounds, 0.80f, 0.51f), 8, 0.75f);
        CreateFloatingIsland(root.transform, "UpperIslandC", floating, WorldPoint(arenaBounds, 0.95f, 0.58f), 0.95f);

        CreateFloatingIsland(root.transform, "LowerIslandA", floating, WorldPoint(arenaBounds, 0.41f, 0.23f), 0.95f);
        CreateFloatingIsland(root.transform, "LowerIslandB", floating, WorldPoint(arenaBounds, 0.50f, 0.11f), 1.05f);
        CreateTopOnlyPlatformGroup(root.transform, "LowerPlatformMain", snow, WorldPoint(arenaBounds, 0.72f, 0.11f), 9, 0.75f);
        CreateFloatingIsland(root.transform, "LowerIslandC", floating, WorldPoint(arenaBounds, 0.93f, 0.23f), 0.95f);

        CreateWaterfall(root.transform, "WaterfallTop", waterfall, WorldPoint(arenaBounds, 0.64f, 0.47f), 1.25f, 2.9f);
        CreateWaterfall(root.transform, "WaterfallBottom", waterfall, WorldPoint(arenaBounds, 0.96f, 0.13f), 1.2f, 2.7f);

        CreateDecoration(root.transform, "GrassTop", grass, WorldPoint(arenaBounds, 0.48f, 0.60f), 0.42f, 6);
        CreateDecoration(root.transform, "TreeTopLeft", tree, WorldPoint(arenaBounds, 0.57f, 0.61f), 0.42f, 5);
        CreateDecoration(root.transform, "TreeTopRight", tree, WorldPoint(arenaBounds, 0.85f, 0.61f), 0.42f, 5);
        CreateDecoration(root.transform, "SignTop", sign, WorldPoint(arenaBounds, 0.74f, 0.58f), 0.28f, 6);
        CreateDecoration(root.transform, "StumpTop", stump, WorldPoint(arenaBounds, 0.79f, 0.57f), 0.25f, 6);
        CreateDecoration(root.transform, "SignBottom", sign, WorldPoint(arenaBounds, 0.68f, 0.17f), 0.28f, 6);
        CreateDecoration(root.transform, "TreeBottom", tree, WorldPoint(arenaBounds, 0.81f, 0.19f), 0.42f, 5);
        CreateDecoration(root.transform, "MushroomBottom", mushroom, WorldPoint(arenaBounds, 0.87f, 0.16f), 0.25f, 6);
        CreateDecoration(root.transform, "CrystalBottom", crystal, WorldPoint(arenaBounds, 0.94f, 0.18f), 0.33f, 6);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Dropped simple reference set into scene.");
    }

    private static Bounds GetArenaBounds()
    {
        GameObject walls = GameObject.Find("Walls");
        if (walls == null)
        {
            return new Bounds(Vector3.zero, new Vector3(22f, 12f, 1f));
        }

        Renderer[] renderers = walls.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(Vector3.zero, new Vector3(22f, 12f, 1f));
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static Vector3 WorldPoint(Bounds arenaBounds, float normalizedX, float normalizedY, float z = 0f)
    {
        float worldX = Mathf.Lerp(arenaBounds.min.x + 1.5f, arenaBounds.max.x - 1.5f, normalizedX);
        float worldY = Mathf.Lerp(arenaBounds.min.y + 1.1f, arenaBounds.max.y - 1.2f, normalizedY);
        return new Vector3(worldX, worldY, z);
    }

    private static void CreatePanel(Transform parent, string name, Sprite sprite, Vector3 position, float widthNormalized, float heightNormalized)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        panel.transform.position = position;

        SpriteRenderer renderer = panel.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = -1;

        float targetWidth = 22f * widthNormalized;
        float targetHeight = 12f * heightNormalized;
        panel.transform.localScale = new Vector3(
            targetWidth / sprite.bounds.size.x,
            targetHeight / sprite.bounds.size.y,
            1f);
    }

    private static void CreatePlatformGroup(Transform parent, string name, Sprite snowSprite, Sprite rockSprite, Vector3 position, int topCount, int depth, float spacing)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        float startX = -((topCount - 1) * spacing * 0.5f);
        for (int x = 0; x < topCount; x++)
        {
            CreateSpriteChild(root.transform, $"Top_{x}", snowSprite, new Vector3(startX + (x * spacing), 0f, 0f), 0.75f, 2);
            for (int y = 1; y <= depth; y++)
            {
                CreateSpriteChild(root.transform, $"Rock_{x}_{y}", rockSprite, new Vector3(startX + (x * spacing), -(y * 0.68f), 0f), 0.75f, 1);
            }
        }
    }

    private static void CreateTopOnlyPlatformGroup(Transform parent, string name, Sprite snowSprite, Vector3 position, int topCount, float spacing)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        float startX = -((topCount - 1) * spacing * 0.5f);
        for (int x = 0; x < topCount; x++)
        {
            CreateSpriteChild(root.transform, $"Top_{x}", snowSprite, new Vector3(startX + (x * spacing), 0f, 0f), 0.75f, 2);
        }
    }

    private static void CreateFloatingIsland(Transform parent, string name, Sprite sprite, Vector3 position, float scale)
    {
        GameObject island = new GameObject(name);
        island.transform.SetParent(parent, false);
        island.transform.position = position;
        SpriteRenderer renderer = island.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 2;
        island.transform.localScale = Vector3.one * scale;
    }

    private static void CreateDecoration(Transform parent, string name, Sprite sprite, Vector3 position, float scale, int sortingOrder)
    {
        GameObject decoration = new GameObject(name);
        decoration.transform.SetParent(parent, false);
        decoration.transform.position = position;
        decoration.transform.localScale = Vector3.one * scale;
        SpriteRenderer renderer = decoration.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }

    private static void CreateBalloon(Transform parent, string name, Sprite sprite, Vector3 position)
    {
        GameObject balloon = new GameObject(name);
        balloon.transform.SetParent(parent, false);
        balloon.transform.position = position;
        balloon.transform.localScale = Vector3.one * 0.6f;
        SpriteRenderer renderer = balloon.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 10;
    }

    private static void CreateWaterfall(Transform parent, string name, Sprite sprite, Vector3 position, float width, float height)
    {
        GameObject waterfall = new GameObject(name);
        waterfall.transform.SetParent(parent, false);
        waterfall.transform.position = position;
        SpriteRenderer renderer = waterfall.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 3;
        waterfall.transform.localScale = new Vector3(
            width / sprite.bounds.size.x,
            height / sprite.bounds.size.y,
            1f);
    }

    private static void CreateSpriteChild(Transform parent, string name, Sprite sprite, Vector3 localPosition, float scale, int sortingOrder)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition;
        child.transform.localScale = Vector3.one * scale;
        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }

    private static Sprite CreateWaterfallSprite()
    {
        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(WaterfallTexturePath);
        if (existing == null)
        {
            Texture2D texture = new Texture2D(48, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    float vertical = (float)y / (texture.height - 1);
                    float shimmer = Mathf.Sin((x * 0.35f) + (y * 0.15f));
                    Color color = Color.Lerp(
                        new Color(0.86f, 0.97f, 1f, 0.92f),
                        new Color(0.52f, 0.82f, 0.93f, 0.82f),
                        vertical);
                    if (shimmer > 0.15f)
                    {
                        color *= 1.07f;
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), WaterfallTexturePath).Replace("\\", "/");
            File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(WaterfallTexturePath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(WaterfallTexturePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(WaterfallTexturePath);
    }

    private static GameObject FindOrCreateRoot(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root == null)
        {
            root = new GameObject(name);
        }

        return root;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static void EnsureFolder(string parent, string child)
    {
        string fullPath = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
