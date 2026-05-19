#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-click: make Purly's landing splash use the real water_splash texture.
///
/// Run:  Tools > Snowman > Set Splash Texture
///
/// Builds a valid material from Assets/Textures/water_splash.png (via the Unity
/// API, so serialization is always correct) and assigns it to the
/// ParticleSystemRenderer of the "LandingSplashParticle" object under Purly.
/// Also forces the particle to render the sprite cleanly (white tint, billboard).
/// </summary>
public static class SetSplashTexture
{
    // GUID of Assets/Textures/water_splash.png in this project.
    private const string WaterSplashGuid = "090f1f65d664aec40964f022c8d3f486";
    private const string MaterialPath = "Assets/Materials/M_WaterSplash.mat";

    [MenuItem("Tools/Snowman/Set Splash Texture")]
    public static void Apply()
    {
        // Find the splash particle object in the open scene.
        ParticleSystem splash = FindSplashParticle();
        if (splash == null)
        {
            EditorUtility.DisplayDialog("Set Splash Texture",
                "Could not find the landing splash particle system in the open scene.\n" +
                "Open the gameplay scene (Adventure of A Snowman 2D) and try again.", "OK");
            return;
        }

        Material mat = GetOrCreateSplashMaterial();
        if (mat == null)
        {
            EditorUtility.DisplayDialog("Set Splash Texture",
                "Could not load Assets/Textures/water_splash.png.", "OK");
            return;
        }

        var renderer = splash.GetComponent<ParticleSystemRenderer>();
        Undo.RecordObject(renderer, "Set Splash Texture");
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = mat;
        EditorUtility.SetDirty(renderer);

        // Texture Sheet Animation overrides the material's UV crop. With a 1x1
        // grid it stretches the whole 1024x1024 sheet onto each particle, so the
        // splash reads as a faint blob. Disable it so the material crop wins.
        var tsa = splash.textureSheetAnimation;
        tsa.enabled = false;

        // Make sure nothing tints the texture into invisibility.
        var main = splash.main;
        Color c = main.startColor.color;
        main.startColor = new Color(1f, 1f, 1f, c.a <= 0.01f ? 1f : c.a);

        EditorSceneManager.MarkSceneDirty(splash.gameObject.scene);
        Selection.activeGameObject = splash.gameObject;
        EditorGUIUtility.PingObject(splash.gameObject);
        Debug.Log("Splash now uses the water_splash texture (" + MaterialPath +
                  "). Save the scene to keep it.");
    }

    private static ParticleSystem FindSplashParticle()
    {
        // 1) Direct: the object is named "LandingSplashParticle".
        foreach (var ps in Object.FindObjectsByType<ParticleSystem>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (ps.gameObject.name == "LandingSplashParticle")
                return ps;
        }

        // 2) Fallback: whatever the PurlyMovement.landSplash field points at.
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb == null || mb.GetType().Name != "PurlyMovement") continue;
            var f = mb.GetType().GetField("landSplash",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (f != null && f.GetValue(mb) is ParticleSystem ps) return ps;
        }
        return null;
    }

    private static Material GetOrCreateSplashMaterial()
    {
        string path = AssetDatabase.GUIDToAssetPath(WaterSplashGuid);
        if (string.IsNullOrEmpty(path)) return null;

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return null;

        // Pick the largest sprite in the sheet (the actual splash art, not the
        // tiny thumbnail) so we can map the particle UVs to just that region.
        Sprite best = null;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (obj is Sprite s &&
                (best == null ||
                 s.rect.width * s.rect.height > best.rect.width * best.rect.height))
            {
                best = s;
            }
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            System.IO.Directory.CreateDirectory("Assets/Materials");
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }
        else if (mat.shader == null || mat.shader.name != "Sprites/Default")
        {
            mat.shader = Shader.Find("Sprites/Default");
        }

        mat.mainTexture = tex;
        if (best != null)
        {
            Rect r = best.textureRect;
            mat.mainTextureScale = new Vector2(r.width / tex.width, r.height / tex.height);
            mat.mainTextureOffset = new Vector2(r.x / tex.width, r.y / tex.height);
        }
        else
        {
            mat.mainTextureScale = Vector2.one;
            mat.mainTextureOffset = Vector2.zero;
        }
        mat.color = Color.white;

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return mat;
    }
}
#endif
