#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-click setup for the gameplay rainfall.
///
/// Run:  Tools > Snowman > Add Rainfall
///
/// Creates a native ParticleSystem GameObject named "Rainfall", parented to the
/// Main Camera so the rain always fills the gameplay window, using the
/// rain_drop sprite at a Medium emission frequency. Re-running it just
/// reconfigures the existing object instead of duplicating it.
/// </summary>
public static class AddRainfall
{
    // GUID of Assets/Textures/rain_drop.png in this project.
    private const string RainDropGuid = "09d348490a3ff164f809f4f77376a58b";
    private const string MaterialPath = "Assets/Materials/M_Rain.mat";

    // Medium frequency (drops emitted per second).
    private const float MediumEmissionRate = 108f;

    [MenuItem("Tools/Snowman/Add Rainfall")]
    public static void AddRainfallToScene()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            EditorUtility.DisplayDialog("Add Rainfall",
                "No camera found in the open scene. Open the gameplay scene first.", "OK");
            return;
        }

        // Reuse an existing Rainfall object if the menu is run again.
        GameObject rainGo = null;
        var existing = cam.GetComponentsInChildren<Transform>(true);
        foreach (var t in existing)
        {
            if (t.name == "Rainfall") { rainGo = t.gameObject; break; }
        }

        bool created = false;
        if (rainGo == null)
        {
            rainGo = new GameObject("Rainfall");
            Undo.RegisterCreatedObjectUndo(rainGo, "Add Rainfall");
            rainGo.transform.SetParent(cam.transform, false);
            created = true;
        }

        // Counteract the camera's non-uniform scale so the rain box is sized in
        // real world units even though we are parented under the camera.
        Vector3 camScale = cam.transform.lossyScale;
        float invX = camScale.x != 0f ? 1f / camScale.x : 1f;
        float invY = camScale.y != 0f ? 1f / camScale.y : 1f;

        float halfHeight = cam.orthographic ? cam.orthographicSize : 6f;
        float halfWidth = halfHeight * cam.aspect + 3f; // a little past the edges

        // Sit on the gameplay (z = 0) plane, just above the top of the view.
        float camLocalZ = cam.transform.position.z; // typically -10
        rainGo.transform.localPosition = new Vector3(
            0f,
            (halfHeight + 1.5f) * invY,
            -camLocalZ);
        rainGo.transform.localRotation = Quaternion.identity;

        var ps = rainGo.GetComponent<ParticleSystem>();
        if (ps == null) ps = Undo.AddComponent<ParticleSystem>(rainGo);

        // Configure while stopped.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = 1.4f;
        main.startSpeed = 0f;            // velocity handled explicitly below
        main.startSize = 0.35f;
        main.startColor = new Color(0.75f, 0.85f, 1f, 0.55f);
        main.gravityModifier = 1.1f;
        main.maxParticles = 2000;
        main.playOnAwake = true;
        main.loop = true;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = MediumEmissionRate;

        // Wide thin slab spanning the top of the camera view.
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(halfWidth * 2f * invX, 0.5f * invY, 1f);
        shape.position = Vector3.zero;
        shape.rotation = Vector3.zero;

        // Constant downward + slight wind velocity so drops always fall straight
        // regardless of how the emitter ends up oriented.
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-2.5f);
        vel.y = new ParticleSystem.MinMaxCurve(-18f);
        vel.z = new ParticleSystem.MinMaxCurve(0f);

        // Stretch the billboards into rain streaks and apply the rain_drop sprite.
        var renderer = rainGo.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2.5f;
        renderer.velocityScale = 0.06f;
        renderer.cameraVelocityScale = 0f;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sortingOrder = 100;
        renderer.sharedMaterial = GetOrCreateRainMaterial();

        ps.Play();

        EditorSceneManager.MarkSceneDirty(rainGo.scene);
        Selection.activeGameObject = rainGo;
        EditorGUIUtility.PingObject(rainGo);
        Debug.Log(created
            ? "Rainfall added under the Main Camera (Medium frequency). Save the scene to keep it."
            : "Rainfall reconfigured (Medium frequency). Save the scene to keep it.");
    }

    private static Material GetOrCreateRainMaterial()
    {
        string spritePath = AssetDatabase.GUIDToAssetPath(RainDropGuid);
        Sprite sprite = !string.IsNullOrEmpty(spritePath)
            ? AssetDatabase.LoadAssetAtPath<Sprite>(spritePath)
            : null;
        Texture2D tex = !string.IsNullOrEmpty(spritePath)
            ? AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath)
            : null;

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            System.IO.Directory.CreateDirectory("Assets/Materials");
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        if (tex != null)
        {
            mat.mainTexture = tex;

            // The sprite is a sub-rect of the 1024x1024 source texture; map the
            // particle UVs to just the drop.
            if (sprite != null)
            {
                Rect r = sprite.textureRect;
                mat.mainTextureScale = new Vector2(r.width / tex.width, r.height / tex.height);
                mat.mainTextureOffset = new Vector2(r.x / tex.width, r.y / tex.height);
            }
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
        }

        return mat;
    }
}
#endif
