#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class WaterfallAnimatedTileBuilder
{
    private const string FrameFolder = "Assets/HobiSoLoved/WaterFall/Frames";
    private const string TilePath = "Assets/HobiSoLoved/WaterFall/Animated_Waterfall_Tile.asset";
    private const float PixelsPerUnit = 100f;

    [InitializeOnLoadMethod]
    private static void CreateAutomatically()
    {
        EditorApplication.delayCall += CreateAnimatedWaterfallTile;
    }

    [MenuItem("Tools/Snowman/Create Animated Waterfall Tile")]
    public static void CreateAnimatedWaterfallTile()
    {
        if (!Directory.Exists(FrameFolder))
        {
            Debug.LogWarning("Waterfall frames folder was not found: " + FrameFolder);
            return;
        }

        string[] framePaths = Directory.GetFiles(FrameFolder, "Waterfall_Frame_*.png");
        if (framePaths.Length == 0)
        {
            Debug.LogWarning("No waterfall frame PNGs were found in: " + FrameFolder);
            return;
        }

        System.Array.Sort(framePaths);

        Sprite[] sprites = new Sprite[framePaths.Length];
        for (int i = 0; i < framePaths.Length; i++)
        {
            string assetPath = framePaths[i].Replace("\\", "/");
            ConfigureSpriteImport(assetPath);
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        if (System.Array.Exists(sprites, sprite => sprite == null))
        {
            AssetDatabase.Refresh();
            for (int i = 0; i < framePaths.Length; i++)
            {
                string assetPath = framePaths[i].Replace("\\", "/");
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
        }

        AnimatedTile tile = AssetDatabase.LoadAssetAtPath<AnimatedTile>(TilePath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<AnimatedTile>();
            AssetDatabase.CreateAsset(tile, TilePath);
        }

        tile.m_AnimatedSprites = sprites;
        tile.m_MinSpeed = 6f;
        tile.m_MaxSpeed = 6f;
        tile.m_TileColliderType = Tile.ColliderType.None;

        EditorUtility.SetDirty(tile);
        AssetDatabase.SaveAssets();
        Debug.Log("Created animated waterfall tile: " + TilePath);
    }

    private static void ConfigureSpriteImport(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool changed = false;

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (!Mathf.Approximately(importer.spritePixelsPerUnit, PixelsPerUnit))
        {
            importer.spritePixelsPerUnit = PixelsPerUnit;
            changed = true;
        }

        if (importer.filterMode != FilterMode.Point)
        {
            importer.filterMode = FilterMode.Point;
            changed = true;
        }

        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            changed = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

}
#endif
