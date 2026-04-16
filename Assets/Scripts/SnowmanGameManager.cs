using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SnowmanGameManager : MonoBehaviour
{
    private enum StartMode
    {
        Landing,
        NewGame,
        ResumeSaved
    }

    private enum GameState
    {
        Landing,
        Playing,
        Paused,
        GameOver
    }

    [Serializable]
    private class ScoreEntry
    {
        public string playerName;
        public int score;
        public string date;
    }

    [Serializable]
    private class ScoreHistory
    {
        public List<ScoreEntry> entries = new List<ScoreEntry>();
    }

    [Serializable]
    private class SavedGameData
    {
        public string playerName;
        public int score;
        public float playerX;
        public float playerY;
        public float playerZ;
        public float middleSphereYRotation;
    }

    public static SnowmanGameManager Instance { get; private set; }

    private static StartMode pendingStartMode = StartMode.Landing;
    private static string pendingPlayerName = "Player";
    private const float GunWaitTime = 20f;

    private readonly List<SnowGun> snowGuns = new List<SnowGun>();
    private ScoreHistory scoreHistory = new ScoreHistory();

    private BalloonManager balloonManager;
    private PurlyMovement purly;
    private Coroutine gunRoutine;
    private GameState currentState = GameState.Landing;
    private string currentPlayerName = "Player";
    private int currentScore;

    private Canvas mainCanvas;
    private GameObject menuPanel;
    private GameObject highScorePanel;
    private GameObject hudPanel;
    private Text scoreText;
    private Text menuTitleText;
    private Text highScoreText;
    private Button pauseButton;
    private Button restartButton;
    private Button resumeButton;
    private Button saveGameButton;
    private InputField playerNameField;

    private string ScoreFilePath => Path.Combine(Application.persistentDataPath, "snowman_scores.json");
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "snowman_save.json");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<SnowmanGameManager>() != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("SnowmanGameManager");
        managerObject.AddComponent<SnowmanGameManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        balloonManager = gameObject.AddComponent<BalloonManager>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        LoadScoreHistory();
        if (mainCanvas == null)
        {
            BuildRuntimeUi();
        }

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    public void RegisterSnowGun(SnowGun snowGun)
    {
        if (snowGun == null || snowGuns.Contains(snowGun))
        {
            return;
        }

        snowGuns.Add(snowGun);
        snowGuns.Sort((first, second) => first.transform.position.x.CompareTo(second.transform.position.x));
    }

    public void RegisterBalloon(Balloon balloon)
    {
        balloonManager.RegisterBalloon(balloon);
    }

    public void PopBalloon(Balloon balloon)
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        currentScore += 1;
        UpdateScoreText();
        balloonManager.HandleBalloonPopped(balloon);
    }

    public void HandlePurlyHit(PurlyMovement hitPurly)
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        if (hitPurly != null)
        {
            Destroy(hitPurly.gameObject);
        }

        SaveFinishedScore();
        DeleteSavedGame();
        currentState = GameState.GameOver;
        Time.timeScale = 0f;
        StopGunRoutine();
        menuTitleText.text = "Purly Was Hit";
        menuPanel.SetActive(true);
        highScorePanel.SetActive(false);
        hudPanel.SetActive(true);
        pauseButton.interactable = false;
        resumeButton.interactable = false;
        restartButton.interactable = true;
        saveGameButton.interactable = false;
    }

    private void ApplyPendingStartMode()
    {
        if (pendingStartMode == StartMode.NewGame)
        {
            StartNewGameInternal(pendingPlayerName);
        }
        else if (pendingStartMode == StartMode.ResumeSaved)
        {
            ResumeSavedGameInternal();
        }

        pendingStartMode = StartMode.Landing;
    }

    private void UpdateSceneReferences()
    {
        snowGuns.Clear();

        if (balloonManager == null)
        {
            balloonManager = GetComponent<BalloonManager>();
        }

        if (balloonManager == null)
        {
            balloonManager = gameObject.AddComponent<BalloonManager>();
        }

        purly = FindFirstObjectByType<PurlyMovement>();

        SnowGun[] sceneSnowGuns = FindObjectsByType<SnowGun>(FindObjectsSortMode.None);
        for (int i = 0; i < sceneSnowGuns.Length; i++)
        {
            RegisterSnowGun(sceneSnowGuns[i]);
        }

        Balloon[] sceneBalloons = FindObjectsByType<Balloon>(FindObjectsSortMode.None);
        for (int i = 0; i < sceneBalloons.Length; i++)
        {
            RegisterBalloon(sceneBalloons[i]);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        CreateEventSystem();
        UpdateSceneReferences();
        PositionHudButtons();

        StartMode requestedMode = pendingStartMode;
        ApplyPendingStartMode();

        if (requestedMode == StartMode.Landing)
        {
            ShowLandingPage();
        }
    }

    private void ShowLandingPage()
    {
        currentState = GameState.Landing;
        Time.timeScale = 0f;
        StopGunRoutine();
        menuTitleText.text = string.Empty;
        ShowPanels(true, false, false);
        saveGameButton.interactable = File.Exists(SaveFilePath);
        UpdateScoreText();
    }

    private void StartNewGame()
    {
        string playerNameToUse = GetTypedPlayerName();

        if (purly == null || currentState == GameState.GameOver)
        {
            StopGunRoutine();
            Time.timeScale = 1f;
            ReloadCurrentScene(StartMode.NewGame, playerNameToUse);
            return;
        }

        StartNewGameInternal(playerNameToUse);
    }

    private void StartNewGameInternal(string playerName)
    {
        UpdateSceneReferences();
        currentPlayerName = playerName;
        currentScore = 0;
        currentState = GameState.Playing;
        DeleteSavedGame();
        ShowPlayingUi();
        UpdateScoreText();
        EnsureGunRoutine();
    }

    private void PauseGame()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        SaveCurrentGame();
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        menuTitleText.text = "Game Paused";
        ShowPanels(true, false, true);
        resumeButton.interactable = true;
        pauseButton.interactable = false;
        saveGameButton.interactable = true;
    }

    private void ResumeGame()
    {
        if (currentState != GameState.Paused)
        {
            return;
        }

        currentState = GameState.Playing;
        ShowPlayingUi();
        EnsureGunRoutine();
    }

    private void RestartGame()
    {
        StopGunRoutine();
        Time.timeScale = 1f;
        ReloadCurrentScene(StartMode.NewGame, GetTypedPlayerName());
    }

    private void ResumeSavedGame()
    {
        if (currentState == GameState.Paused)
        {
            ResumeGame();
            return;
        }

        ResumeSavedGameInternal();
    }

    private void ResumeSavedGameInternal()
    {
        UpdateSceneReferences();

        if (!TryLoadSavedGame(out SavedGameData savedGame))
        {
            ShowLandingPage();
            return;
        }

        currentPlayerName = string.IsNullOrWhiteSpace(savedGame.playerName) ? "Player" : savedGame.playerName;
        currentScore = savedGame.score;
        currentState = GameState.Playing;

        if (playerNameField != null)
        {
            playerNameField.text = currentPlayerName;
        }

        if (purly != null)
        {
            purly.RestoreSavedState(
                new Vector3(savedGame.playerX, savedGame.playerY, savedGame.playerZ),
                savedGame.middleSphereYRotation);
        }

        ShowPlayingUi();
        UpdateScoreText();
        EnsureGunRoutine();
    }

    private void ShowHighScores()
    {
        highScorePanel.SetActive(true);
        highScoreText.text = BuildHighScoreText();
    }

    private void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EnsureGunRoutine()
    {
        StopGunRoutine();
        gunRoutine = StartCoroutine(GunLoop());
    }

    private void StopGunRoutine()
    {
        if (gunRoutine != null)
        {
            StopCoroutine(gunRoutine);
            gunRoutine = null;
        }
    }

    private IEnumerator GunLoop()
    {
        int gunIndex = 0;

        while (currentState == GameState.Playing)
        {
            if (snowGuns.Count == 0)
            {
                yield return null;
                continue;
            }

            SnowGun activeGun = snowGuns[gunIndex % snowGuns.Count];
            if (activeGun != null)
            {
                activeGun.Fire();
            }

            gunIndex++;
            yield return new WaitForSeconds(GunWaitTime);
        }
    }

    private void ReloadCurrentScene(StartMode startMode, string playerName)
    {
        pendingPlayerName = playerName;
        pendingStartMode = startMode;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowPlayingUi()
    {
        ShowPanels(false, false, true);
        PositionHudButtons();
        Time.timeScale = 1f;
        pauseButton.interactable = true;
        restartButton.interactable = true;
        resumeButton.interactable = false;
    }

    private void ShowPanels(bool showMenu, bool showHighScores, bool showHud)
    {
        menuPanel.SetActive(showMenu);
        highScorePanel.SetActive(showHighScores);
        hudPanel.SetActive(showHud);
    }

    private void SaveCurrentGame()
    {
        if (purly == null)
        {
            return;
        }

        SavedGameData savedGame = new SavedGameData
        {
            playerName = currentPlayerName,
            score = currentScore,
            playerX = purly.GetCurrentPosition().x,
            playerY = purly.GetCurrentPosition().y,
            playerZ = purly.GetCurrentPosition().z,
            middleSphereYRotation = purly.GetMiddleSphereRotationY()
        };

        File.WriteAllText(SaveFilePath, JsonUtility.ToJson(savedGame, true));
    }

    private bool TryLoadSavedGame(out SavedGameData savedGame)
    {
        savedGame = null;

        if (!File.Exists(SaveFilePath))
        {
            return false;
        }

        string json = File.ReadAllText(SaveFilePath);
        savedGame = JsonUtility.FromJson<SavedGameData>(json);
        return savedGame != null;
    }

    private void DeleteSavedGame()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
        }
    }

    private void SaveFinishedScore()
    {
        ScoreEntry newEntry = new ScoreEntry
        {
            playerName = currentPlayerName,
            score = currentScore,
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

        scoreHistory.entries.Add(newEntry);
        scoreHistory.entries.Sort((first, second) => second.score.CompareTo(first.score));
        File.WriteAllText(ScoreFilePath, JsonUtility.ToJson(scoreHistory, true));
    }

    private void LoadScoreHistory()
    {
        if (!File.Exists(ScoreFilePath))
        {
            scoreHistory = new ScoreHistory();
            return;
        }

        string json = File.ReadAllText(ScoreFilePath);
        scoreHistory = JsonUtility.FromJson<ScoreHistory>(json);

        if (scoreHistory == null)
        {
            scoreHistory = new ScoreHistory();
        }
    }

    private string BuildHighScoreText()
    {
        if (scoreHistory.entries == null || scoreHistory.entries.Count == 0)
        {
            return "No scores saved yet.";
        }

        scoreHistory.entries.Sort((first, second) => second.score.CompareTo(first.score));

        int amountToShow = Mathf.Min(5, scoreHistory.entries.Count);
        string lines = "Top 5 Scores\n";

        for (int i = 0; i < amountToShow; i++)
        {
            ScoreEntry entry = scoreHistory.entries[i];
            lines += $"{i + 1}. {entry.playerName} - {entry.score}\n";
        }

        return lines.TrimEnd();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:00}";
        }
    }

    private string GetTypedPlayerName()
    {
        if (playerNameField == null || string.IsNullOrWhiteSpace(playerNameField.text))
        {
            return "Player";
        }

        return playerNameField.text.Trim();
    }

    private void BuildRuntimeUi()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Sprite landingSprite = LoadSpriteFromResources("UI/landingPage");
        Sprite pauseSprite = LoadSpriteFromResources("UI/pauseBtn");
        Sprite restartSprite = LoadSpriteFromResources("UI/restartBtn");
        Sprite resumeSprite = LoadSpriteFromResources("UI/resumeBtn");

        GameObject canvasObject = new GameObject("RuntimeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        mainCanvas = canvasObject.GetComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1366f, 768f);
        CreateEventSystem();

        hudPanel = CreatePanel("HudPanel", canvasObject.transform, new Color(0f, 0f, 0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        scoreText = CreateText("ScoreText", hudPanel.transform, font, "Score: 00", 26, TextAnchor.MiddleRight, Color.black);
        RectTransform scoreRect = scoreText.rectTransform;
        scoreRect.anchorMin = new Vector2(1f, 1f);
        scoreRect.anchorMax = new Vector2(1f, 1f);
        scoreRect.pivot = new Vector2(1f, 1f);
        scoreRect.sizeDelta = new Vector2(240f, 40f);
        scoreRect.anchoredPosition = new Vector2(-70f, -28f);
        SetTopCenterAnchor(scoreRect);
        scoreRect.sizeDelta = new Vector2(260f, 42f);
        scoreRect.anchoredPosition = new Vector2(0f, -18f);
        scoreText.alignment = TextAnchor.MiddleCenter;
        scoreText.fontStyle = FontStyle.Bold;
        AddTextOutline(scoreText, Color.white, new Vector2(1.5f, -1.5f));

        pauseButton = CreateButton("PauseButton", hudPanel.transform, font, string.Empty, Vector2.zero, new Vector2(42f, 42f), PauseGame);
        restartButton = CreateButton("RestartButton", hudPanel.transform, font, string.Empty, Vector2.zero, new Vector2(42f, 42f), RestartGame);
        resumeButton = CreateButton("ResumeButton", hudPanel.transform, font, string.Empty, Vector2.zero, new Vector2(42f, 42f), ResumeGame);
        PositionHudButtons();
        StyleIconButton(pauseButton, pauseSprite);
        StyleIconButton(restartButton, restartSprite);
        StyleIconButton(resumeButton, resumeSprite);

        menuPanel = CreatePanel("MenuPanel", canvasObject.transform, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image menuPanelImage = menuPanel.GetComponent<Image>();
        menuPanelImage.sprite = landingSprite;
        menuPanelImage.preserveAspect = false;

        GameObject buttonGroup = new GameObject("ButtonGroup", typeof(RectTransform));
        buttonGroup.transform.SetParent(menuPanel.transform, false);
        RectTransform buttonGroupRect = buttonGroup.GetComponent<RectTransform>();
        buttonGroupRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonGroupRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonGroupRect.pivot = new Vector2(0.5f, 0.5f);
        buttonGroupRect.sizeDelta = new Vector2(260f, 330f);
        buttonGroupRect.anchoredPosition = new Vector2(0f, -10f);

        menuTitleText = CreateText("MenuTitle", buttonGroup.transform, font, string.Empty, 24, TextAnchor.MiddleCenter, Color.black);
        menuTitleText.rectTransform.anchoredPosition = new Vector2(0f, 168f);
        menuTitleText.rectTransform.sizeDelta = new Vector2(320f, 40f);

        playerNameField = CreateInputField(buttonGroup.transform, font, new Vector2(0f, 110f), new Vector2(250f, 44f));
        playerNameField.text = "Player";

        Button newGameButton = CreateButton("NewGameButton", buttonGroup.transform, font, "New Game", new Vector2(0f, 42f), new Vector2(210f, 42f), StartNewGame);
        saveGameButton = CreateButton("SaveGameButton", buttonGroup.transform, font, "Saved Game", new Vector2(0f, -14f), new Vector2(210f, 42f), ResumeSavedGame);
        Button highScoresButton = CreateButton("HighScoreButton", buttonGroup.transform, font, "High Scores", new Vector2(0f, -70f), new Vector2(210f, 42f), ShowHighScores);
        Button exitButton = CreateButton("EndGameButton", buttonGroup.transform, font, "Exit", new Vector2(0f, -126f), new Vector2(210f, 42f), EndGame);
        StyleLandingButton(newGameButton);
        StyleLandingButton(saveGameButton);
        StyleLandingButton(highScoresButton);
        StyleLandingButton(exitButton);

        highScorePanel = CreatePanel("HighScorePanel", buttonGroup.transform, new Color(1f, 1f, 1f, 0.9f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250f, 190f), new Vector2(0f, -248f));
        highScoreText = CreateText("HighScoreText", highScorePanel.transform, font, "No scores saved yet.", 17, TextAnchor.UpperLeft, new Color(0.12f, 0.2f, 0.3f, 1f));
        highScoreText.rectTransform.anchorMin = new Vector2(0f, 0f);
        highScoreText.rectTransform.anchorMax = new Vector2(1f, 1f);
        highScoreText.rectTransform.offsetMin = new Vector2(14f, 10f);
        highScoreText.rectTransform.offsetMax = new Vector2(-14f, -10f);
    }

    private void CreateEventSystem()
    {
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();
        if (existingEventSystem != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystemObject.transform.SetParent(transform, false);
        InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();
    }

    private static GameObject CreatePanel(string objectName, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject panelObject = new GameObject(objectName, typeof(Image));
        panelObject.transform.SetParent(parent, false);
        Image image = panelObject.GetComponent<Image>();
        image.color = color;

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;
        return panelObject;
    }

    private static Text CreateText(string objectName, Transform parent, Font font, string value, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text textComponent = textObject.GetComponent<Text>();
        textComponent.font = font;
        textComponent.text = value;
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = color;

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        return textComponent;
    }

    private static Button CreateButton(string objectName, Transform parent, Font font, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(Image), typeof(Outline), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.9f);

        Outline outline = buttonObject.GetComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, -2f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        Text buttonText = CreateText("Label", buttonObject.transform, font, label, 22, TextAnchor.MiddleCenter, Color.black);
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;

        return button;
    }

    private static InputField CreateInputField(Transform parent, Font font, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject inputObject = new GameObject("PlayerNameInput", typeof(Image), typeof(Outline), typeof(InputField));
        inputObject.transform.SetParent(parent, false);

        Image image = inputObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.95f);

        Outline outline = inputObject.GetComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform rectTransform = inputObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        InputField inputField = inputObject.GetComponent<InputField>();

        Text textComponent = CreateText("Text", inputObject.transform, font, string.Empty, 20, TextAnchor.MiddleLeft, Color.black);
        textComponent.rectTransform.anchorMin = Vector2.zero;
        textComponent.rectTransform.anchorMax = Vector2.one;
        textComponent.rectTransform.offsetMin = new Vector2(12f, 6f);
        textComponent.rectTransform.offsetMax = new Vector2(-12f, -6f);

        Text placeholderComponent = CreateText("Placeholder", inputObject.transform, font, "Player", 20, TextAnchor.MiddleLeft, new Color(0.4f, 0.4f, 0.4f, 0.8f));
        placeholderComponent.rectTransform.anchorMin = Vector2.zero;
        placeholderComponent.rectTransform.anchorMax = Vector2.one;
        placeholderComponent.rectTransform.offsetMin = new Vector2(12f, 6f);
        placeholderComponent.rectTransform.offsetMax = new Vector2(-12f, -6f);

        inputField.textComponent = textComponent;
        inputField.placeholder = placeholderComponent;
        return inputField;
    }

    private static void SetTopRightAnchor(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
    }

    private static void SetMiddleRightAnchor(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(1f, 0.5f);
        rectTransform.anchorMax = new Vector2(1f, 0.5f);
        rectTransform.pivot = new Vector2(1f, 0.5f);
    }

    private void PositionHudButtons()
    {
        RectTransform resumeRect = resumeButton.GetComponent<RectTransform>();
        RectTransform pauseRect = pauseButton.GetComponent<RectTransform>();
        RectTransform restartRect = restartButton.GetComponent<RectTransform>();

        SetMiddleRightAnchor(resumeRect);
        SetMiddleRightAnchor(pauseRect);
        SetMiddleRightAnchor(restartRect);

        resumeRect.anchoredPosition = new Vector2(-10f, 60f);
        pauseRect.anchoredPosition = new Vector2(-10f, 0f);
        restartRect.anchoredPosition = new Vector2(-10f, -60f);
    }

    private static void SetTopCenterAnchor(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
    }

    private static void StyleLandingButton(Button button)
    {
        Image image = button.GetComponent<Image>();
        image.color = new Color(0.73f, 0.84f, 0.95f, 0.96f);

        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.color = new Color(0.08f, 0.12f, 0.18f, 1f);
            label.fontSize = 17;
        }
    }

    private static void StyleIconButton(Button button, Sprite sprite)
    {
        Image image = button.GetComponent<Image>();
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

    private static void AddTextOutline(Text textComponent, Color outlineColor, Vector2 outlineDistance)
    {
        if (textComponent == null)
        {
            return;
        }

        Outline outline = textComponent.GetComponent<Outline>();
        if (outline == null)
        {
            outline = textComponent.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = outlineColor;
        outline.effectDistance = outlineDistance;
    }

    private static Sprite LoadSpriteFromResources(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
    }
}
