using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SnowmanCleanupHelper
{
    public static void KeepOnlyPlatform2Set()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Adventure of A Snowman 2D.unity", OpenSceneMode.Single);
        GameObject extra = GameObject.Find("Assignment04ReferenceObjects");
        if (extra != null)
        {
            Object.DestroyImmediate(extra);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Removed extra reference layout and kept Platform2Set.");
    }
}
