using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SnowmanTeacherUiSetup
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameplayScenePath = "Assets/Scenes/Adventure of A Snowman 2D.unity";

    [MenuItem("Tools/Snowman/Build Teacher Style UI")]
    public static void BuildTeacherStyleUi()
    {
        CreateMainMenuScene();
        UpdateGameplayScene();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Snowman UI Setup", "Main menu scene, HUD canvas, and pause canvas are ready.", "OK");
    }

    private static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "MainMenu";

        EnsureEventSystem();
        SnowmanGameUiBridge bridge = new GameObject("SnowmanGameUiBridge").AddComponent<SnowmanGameUiBridge>();
        new GameObject("SnowmanGameManager").AddComponent<SnowmanGameManager>();

        Canvas canvas = CreateCanvas("MainMenuCanvas");
        MainMenuCanvas mainMenuCanvas = canvas.gameObject.AddComponent<MainMenuCanvas>();

        Image background = CreateStretchImage("Background", canvas.transform, Color.white);
        background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/landingPage.png");
        background.preserveAspect = false;

        GameObject menuPanel = CreateUiObject("MenuPanel", canvas.transform, typeof(Image));
        RectTransform menuPanelRect = menuPanel.GetComponent<RectTransform>();
        menuPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuPanelRect.pivot = new Vector2(0.5f, 0.5f);
        menuPanelRect.sizeDelta = new Vector2(320f, 430f);
        menuPanelRect.anchoredPosition = Vector2.zero;
        Image menuPanelImage = menuPanel.GetComponent<Image>();
        menuPanelImage.color = new Color(1f, 1f, 1f, 0.18f);

        InputField playerInput = CreateInputField("PlayerNameInput", menuPanel.transform, new Vector2(0f, 120f), new Vector2(240f, 42f), "Player");
        Button newGameButton = CreateButton("NewGameButton", menuPanel.transform, "New Game", new Vector2(0f, 52f), new Vector2(210f, 40f));
        Button savedGameButton = CreateButton("SavedGameButton", menuPanel.transform, "Saved Game", new Vector2(0f, 0f), new Vector2(210f, 40f));
        Button highScoresButton = CreateButton("HighScoresButton", menuPanel.transform, "High Scores", new Vector2(0f, -52f), new Vector2(210f, 40f));
        Button exitButton = CreateButton("ExitButton", menuPanel.transform, "Exit", new Vector2(0f, -104f), new Vector2(210f, 40f));

        GameObject highScorePanel = CreateUiObject("HighScorePanel", menuPanel.transform, typeof(Image));
        RectTransform highScoreRect = highScorePanel.GetComponent<RectTransform>();
        highScoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        highScoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        highScoreRect.pivot = new Vector2(0.5f, 0.5f);
        highScoreRect.sizeDelta = new Vector2(250f, 165f);
        highScoreRect.anchoredPosition = new Vector2(0f, -230f);
        highScorePanel.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.88f);

        Text highScoreText = CreateText("HighScoreText", highScorePanel.transform, "Top 5 Scores", 18, TextAnchor.UpperLeft);
        RectTransform highScoreTextRect = highScoreText.GetComponent<RectTransform>();
        highScoreTextRect.anchorMin = Vector2.zero;
        highScoreTextRect.anchorMax = Vector2.one;
        highScoreTextRect.offsetMin = new Vector2(12f, 10f);
        highScoreTextRect.offsetMax = new Vector2(-12f, -10f);
        highScorePanel.SetActive(false);

        UnityEventTools.AddPersistentListener(newGameButton.onClick, bridge.StartNewGame);
        UnityEventTools.AddPersistentListener(savedGameButton.onClick, bridge.LoadSavedGame);
        UnityEventTools.AddPersistentListener(highScoresButton.onClick, bridge.ShowHighScores);
        UnityEventTools.AddPersistentListener(exitButton.onClick, bridge.QuitGame);

        SerializedObject mainMenuSerialized = new SerializedObject(mainMenuCanvas);
        mainMenuSerialized.FindProperty("menuPanel").objectReferenceValue = menuPanel;
        mainMenuSerialized.FindProperty("highScorePanel").objectReferenceValue = highScorePanel;
        mainMenuSerialized.FindProperty("playerNameInput").objectReferenceValue = playerInput;
        mainMenuSerialized.FindProperty("highScoreText").objectReferenceValue = highScoreText;
        mainMenuSerialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
    }

    private static void UpdateGameplayScene()
    {
        Scene scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        EnsureEventSystem();

        SnowmanGameUiBridge bridge = Object.FindFirstObjectByType<SnowmanGameUiBridge>();
        if (bridge == null)
        {
            bridge = new GameObject("SnowmanGameUiBridge").AddComponent<SnowmanGameUiBridge>();
        }

        CreateHudCanvas(bridge);
        CreatePauseCanvas(bridge);
        EditorSceneManager.SaveScene(scene, GameplayScenePath);
    }

    private static void CreateHudCanvas(SnowmanGameUiBridge bridge)
    {
        HudCanvas existingHud = Object.FindFirstObjectByType<HudCanvas>(FindObjectsInactive.Include);
        if (existingHud != null)
        {
            return;
        }

        Canvas canvas = CreateCanvas("HudCanvas");
        HudCanvas hudCanvas = canvas.gameObject.AddComponent<HudCanvas>();

        GameObject hudPanel = CreateUiObject("HudPanel", canvas.transform, typeof(Image));
        RectTransform hudPanelRect = hudPanel.GetComponent<RectTransform>();
        hudPanelRect.anchorMin = Vector2.zero;
        hudPanelRect.anchorMax = Vector2.one;
        hudPanelRect.offsetMin = Vector2.zero;
        hudPanelRect.offsetMax = Vector2.zero;
        hudPanel.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

        Text scoreText = CreateText("ScoreText", hudPanel.transform, "Score: 00", 28, TextAnchor.MiddleCenter);
        RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 1f);
        scoreRect.anchorMax = new Vector2(0.5f, 1f);
        scoreRect.pivot = new Vector2(0.5f, 1f);
        scoreRect.sizeDelta = new Vector2(280f, 44f);
        scoreRect.anchoredPosition = new Vector2(0f, -18f);

        Button resumeButton = CreateButton("ResumeButton", hudPanel.transform, "Play", new Vector2(-18f, 60f), new Vector2(44f, 44f));
        Button pauseButton = CreateButton("PauseButton", hudPanel.transform, "Pause", new Vector2(-18f, 0f), new Vector2(44f, 44f));
        Button restartButton = CreateButton("RestartButton", hudPanel.transform, "Restart", new Vector2(-18f, -60f), new Vector2(44f, 44f));

        SetRightMiddleAnchor(resumeButton.GetComponent<RectTransform>());
        SetRightMiddleAnchor(pauseButton.GetComponent<RectTransform>());
        SetRightMiddleAnchor(restartButton.GetComponent<RectTransform>());
        SetButtonIcon(resumeButton, "Assets/Resources/UI/resumeBtn.png");
        SetButtonIcon(pauseButton, "Assets/Resources/UI/pauseBtn.png");
        SetButtonIcon(restartButton, "Assets/Resources/UI/restartBtn.png");

        UnityEventTools.AddPersistentListener(resumeButton.onClick, bridge.ResumeGame);
        UnityEventTools.AddPersistentListener(pauseButton.onClick, bridge.PauseGame);
        UnityEventTools.AddPersistentListener(restartButton.onClick, bridge.RestartGame);

        SerializedObject hudSerialized = new SerializedObject(hudCanvas);
        hudSerialized.FindProperty("hudPanel").objectReferenceValue = hudPanel;
        hudSerialized.FindProperty("scoreText").objectReferenceValue = scoreText;
        hudSerialized.FindProperty("pauseButton").objectReferenceValue = pauseButton;
        hudSerialized.FindProperty("restartButton").objectReferenceValue = restartButton;
        hudSerialized.FindProperty("resumeButton").objectReferenceValue = resumeButton;
        hudSerialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreatePauseCanvas(SnowmanGameUiBridge bridge)
    {
        PauseCanvas existingPause = Object.FindFirstObjectByType<PauseCanvas>(FindObjectsInactive.Include);
        if (existingPause != null)
        {
            return;
        }

        Canvas canvas = CreateCanvas("PauseCanvas");
        PauseCanvas pauseCanvas = canvas.gameObject.AddComponent<PauseCanvas>();

        GameObject pausePanel = CreateUiObject("PausePanel", canvas.transform, typeof(Image));
        RectTransform pauseRect = pausePanel.GetComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(0.5f, 0.5f);
        pauseRect.anchorMax = new Vector2(0.5f, 0.5f);
        pauseRect.pivot = new Vector2(0.5f, 0.5f);
        pauseRect.sizeDelta = new Vector2(280f, 250f);
        pauseRect.anchoredPosition = Vector2.zero;
        pausePanel.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);

        Text titleText = CreateText("PauseTitle", pausePanel.transform, "Game Paused", 26, TextAnchor.MiddleCenter);
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(240f, 40f);
        titleRect.anchoredPosition = new Vector2(0f, 78f);

        Button resumeButton = CreateButton("PauseResumeButton", pausePanel.transform, "Resume", new Vector2(0f, 18f), new Vector2(210f, 40f));
        Button restartButton = CreateButton("PauseRestartButton", pausePanel.transform, "Restart", new Vector2(0f, -34f), new Vector2(210f, 40f));
        Button menuButton = CreateButton("PauseMenuButton", pausePanel.transform, "Main Menu", new Vector2(0f, -86f), new Vector2(210f, 40f));

        UnityEventTools.AddPersistentListener(resumeButton.onClick, bridge.ResumeGame);
        UnityEventTools.AddPersistentListener(restartButton.onClick, bridge.RestartGame);
        UnityEventTools.AddPersistentListener(menuButton.onClick, bridge.ReturnToMainMenu);

        SerializedObject pauseSerialized = new SerializedObject(pauseCanvas);
        pauseSerialized.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        pauseSerialized.FindProperty("titleText").objectReferenceValue = titleText;
        pauseSerialized.ApplyModifiedPropertiesWithoutUndo();

        pausePanel.SetActive(false);
    }

    private static void UpdateBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MainMenuScenePath, true),
            new EditorBuildSettingsScene(GameplayScenePath, true)
        };

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static Canvas CreateCanvas(string canvasName)
    {
        GameObject canvasObject = CreateUiObject(canvasName, null, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1366f, 768f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static Image CreateStretchImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = CreateUiObject(name, parent, typeof(Image));
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return image;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = CreateUiObject(name, parent, typeof(Image), typeof(Button), typeof(Outline));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.82f, 0.9f, 0.98f, 0.95f);

        Outline outline = buttonObject.GetComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, -2f);

        Text buttonText = CreateText("Text", buttonObject.transform, label, 20, TextAnchor.MiddleCenter);
        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return buttonObject.GetComponent<Button>();
    }

    private static void SetButtonIcon(Button button, string spriteAssetPath)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetPath);
        if (sprite == null)
        {
            return;
        }

        image.color = Color.white;
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;

        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = string.Empty;
        }
    }

    private static InputField CreateInputField(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string placeholder)
    {
        GameObject inputObject = CreateUiObject(name, parent, typeof(Image), typeof(InputField), typeof(Outline));
        RectTransform rect = inputObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = inputObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.95f);

        Outline outline = inputObject.GetComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, -2f);

        InputField inputField = inputObject.GetComponent<InputField>();
        Text text = CreateText("Text", inputObject.transform, string.Empty, 18, TextAnchor.MiddleLeft);
        Text placeholderText = CreateText("Placeholder", inputObject.transform, placeholder, 18, TextAnchor.MiddleLeft);
        placeholderText.color = new Color(0.35f, 0.35f, 0.35f, 0.8f);

        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 6f);
        textRect.offsetMax = new Vector2(-10f, -6f);

        RectTransform placeholderRect = placeholderText.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10f, 6f);
        placeholderRect.offsetMax = new Vector2(-10f, -6f);

        inputField.textComponent = text;
        inputField.placeholder = placeholderText;
        return inputField;
    }

    private static Text CreateText(string name, Transform parent, string value, int size, TextAnchor alignment)
    {
        GameObject textObject = CreateUiObject(name, parent, typeof(Text));
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.black;
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220f, 40f);
        return text;
    }

    private static GameObject CreateUiObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject gameObject = new GameObject(name, components);
        if (parent != null)
        {
            gameObject.transform.SetParent(parent, false);
        }

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        return gameObject;
    }

    private static void SetRightMiddleAnchor(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(1f, 0.5f);
        rectTransform.anchorMax = new Vector2(1f, 0.5f);
        rectTransform.pivot = new Vector2(1f, 0.5f);
    }
}
