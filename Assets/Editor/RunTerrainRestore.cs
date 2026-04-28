#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

public static class RunTerrainRestore
{
    public static void RestoreTilemaps()
    {
        const string scenePath = "Assets/Scenes/Adventure of A Snowman 2D.unity";
        EditorSceneManager.OpenScene(scenePath);
        SnowmanTerrainTilemapConverter.ConvertOpenSceneFromMenu();
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
    }
}
#endif
