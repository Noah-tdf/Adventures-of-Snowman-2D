using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class SnowmanGameManager : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const string GameplaySceneName = "Adventure of A Snowman 2D";

    private enum StartMode { Landing, NewGame, ResumeSaved }
    private enum GameState { Landing, Playing, Paused, GameOver }

    [Serializable]
    private class ScoreEntry { public string playerName; public int score; public string date; }
    [Serializable]
    private class ScoreHistory { public List<ScoreEntry> entries = new List<ScoreEntry>(); }
    [Serializable]
    private class SavedGameData { public string playerName; public int score; public float playerX; public float playerY; public float playerZ; public float middleSphereYRotation; }

    public static SnowmanGameManager Instance { get; private set; }
    public bool GameIsPaused => currentState == GameState.Paused;

    private static StartMode pendingStartMode = StartMode.Landing;
    private static string pendingPlayerName = "Player";
    private const float MinGunWaitTime = 2.5f;
    private const float MaxGunWaitTime = 6f;

    private readonly List<SnowGun> snowGuns = new List<SnowGun>();
    private ScoreHistory scoreHistory = new ScoreHistory();

    private BalloonManager balloonManager;
    private PurlyMovement purly;
    private Coroutine gunRoutine;
    private SnowGun lastFiredGun;
    private GameState currentState = GameState.Landing;
    private string currentPlayerName = "Player";
    private int currentScore;

    [Header("Events")]
    public UnityEvent onShowLanding;
    public UnityEvent onStartGame;
    public UnityEvent onPurlyDeath;

    private Canvas mainCanvas;
    private MainMenuCanvas mainMenuCanvas;
    private PauseCanvas pauseCanvas;
    private HudCanvas hudCanvas;
    private GameObject menuPanel, highScorePanel, hudPanel;
    private Image menuPanelImage, buttonGroupImage;
    private Outline buttonGroupOutline;
    private RectTransform buttonGroupRect;
    private Text scoreText, menuTitleText, highScoreText;
    private Button pauseButton, restartButton, resumeButton, newGameButton, saveGameButton, highScoresButton, exitButton;
    private InputField playerNameField;
    private Sprite landingBackgroundSprite;

    private string ScoreFilePath => Path.Combine(Application.persistentDataPath, "snowman_scores.json");
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "snowman_save.json");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<SnowmanGameManager>() != null) return;
        new GameObject("SnowmanGameManager", typeof(SnowmanGameManager));
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        balloonManager = GetComponent<BalloonManager>();
        if (balloonManager == null) balloonManager = gameObject.AddComponent<BalloonManager>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() { if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void Start()
    {
        Time.timeScale = 1f;
        LoadScoreHistory();
        if (mainCanvas == null) BuildRuntimeUi();
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    public void RegisterSnowGun(SnowGun snowGun)
    {
        if (snowGun == null || snowGuns.Contains(snowGun)) return;
        snowGuns.Add(snowGun);
        snowGuns.RemoveAll(g => g == null);
        snowGuns.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
    }

    public void RegisterBalloon(Balloon balloon) => balloonManager.RegisterBalloon(balloon);

    public void PopBalloon(Balloon balloon)
    {
        if (currentState != GameState.Playing) return;
        currentScore += balloon.ScoreValue;
        UpdateScoreText();
        balloonManager.HandleBalloonPopped(balloon);
    }

    public void HandlePurlyHit(PurlyMovement hitPurly) => HandlePurlyDeath(hitPurly, "Purly Was Hit");
    public void HandlePurlyMelted(PurlyMovement meltedPurly) => HandlePurlyDeath(meltedPurly, "Purly Melted");

    private void HandlePurlyDeath(PurlyMovement purlyToDestroy, string gameOverMessage)
    {
        if (currentState != GameState.Playing) return;
        if (purlyToDestroy != null) Destroy(purlyToDestroy.gameObject);

        SaveFinishedScore();
        DeleteSavedGame();
        currentState = GameState.GameOver;
        Time.timeScale = 0f;
        StopGunRoutine();

        onPurlyDeath?.Invoke();

        if (pauseCanvas != null)
        {
            pauseCanvas.ShowPause(gameOverMessage);
            pauseCanvas.PrepareHighScores(BuildHighScoreText());
        }
        else
        {
            SetOverlayMenuLook();
            menuTitleText.text = gameOverMessage;
            menuPanel.SetActive(true);
            highScorePanel.SetActive(false);
            hudPanel.SetActive(true);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        CreateEventSystem();
        UpdateSceneReferences();
        PositionHudButtons();
        
        if (scene.name == MainMenuSceneName)
        {
            ShowLandingPage();
        }
        else
        {
            ApplyPendingStartMode();
            if (currentState == GameState.Landing) ShowLandingPage();
        }
    }

    private void UpdateSceneReferences()
    {
        snowGuns.Clear();
        mainMenuCanvas = FindFirstObjectByType<MainMenuCanvas>(FindObjectsInactive.Include);
        pauseCanvas = FindFirstObjectByType<PauseCanvas>(FindObjectsInactive.Include);
        hudCanvas = FindFirstObjectByType<HudCanvas>(FindObjectsInactive.Include);

        if (hudCanvas != null)
        {
            pauseButton = hudCanvas.PauseButton;
            restartButton = hudCanvas.RestartButton;
            resumeButton = hudCanvas.ResumeButton;
            ApplyHudButtonIcons();
        }

        purly = FindFirstObjectByType<PurlyMovement>();
        if (balloonManager != null && purly != null) balloonManager.Initialize(purly.transform);
        else if (balloonManager != null) balloonManager.ResetBalloons();

        foreach (var gun in FindObjectsByType<SnowGun>(FindObjectsSortMode.None)) RegisterSnowGun(gun);
        foreach (var b in FindObjectsByType<Balloon>(FindObjectsSortMode.None)) RegisterBalloon(b);
    }

    private void ShowLandingPage()
    {
        Debug.Log("GameManager: Transitioning to Landing Page Music (music_1)...");
        currentState = GameState.Landing;
        Time.timeScale = 0f;
        StopGunRoutine();

        // Ensure Music_Landing is playing and Music_Gameplay is stopped
        var musicLanding = transform.Find("Music_Landing")?.GetComponent<AudioSource>();
        var musicGameplay = transform.Find("Music_Gameplay")?.GetComponent<AudioSource>();
        
        if (musicLanding != null) { if (!musicLanding.isPlaying) musicLanding.Play(); }
        if (musicGameplay != null) { musicGameplay.Stop(); }

        // Also trigger events for any other linked behavior
        onShowLanding?.Invoke();

        if (mainMenuCanvas != null)
{
            mainMenuCanvas.gameObject.SetActive(true);
            mainMenuCanvas.ShowMenu();
            mainMenuCanvas.SetPlayerName(currentPlayerName);
        }
        else
        {
            SetLandingMenuLook();
            if (menuTitleText != null) menuTitleText.text = string.Empty;
            ShowPanels(true, false, false);
            if (saveGameButton != null) saveGameButton.interactable = File.Exists(SaveFilePath);
        }
if (pauseCanvas != null) pauseCanvas.HidePause();
        if (hudCanvas != null) hudCanvas.ShowHud(false);
        UpdateScoreText();
    }

    public void StartNewGame()
    {
        string name = GetTypedPlayerName();
        if (SceneManager.GetActiveScene().name != GameplaySceneName || purly == null || currentState == GameState.GameOver)
        {
            LoadGameplayScene(StartMode.NewGame, name);
            return;
        }
        StartNewGameInternal(name);
    }

    private void StartNewGameInternal(string playerName)
    {
        UpdateSceneReferences();
        currentPlayerName = playerName;
        currentScore = 0;
        currentState = GameState.Playing;
        onStartGame?.Invoke();
        DeleteSavedGame();
        HideAllMenuUi();
        ShowPlayingUi();
        UpdateScoreText();
        EnsureGunRoutine();
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        SaveCurrentGame();
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        if (pauseCanvas != null)
        {
            pauseCanvas.ShowPause("Game Paused");
            pauseCanvas.PrepareHighScores(BuildHighScoreText());
        }
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        currentState = GameState.Playing;
        ShowPlayingUi();
        EnsureGunRoutine();
    }

    public void RestartGame() { LoadGameplayScene(StartMode.NewGame, GetTypedPlayerName()); }

    public void LoadSavedGame()
    {
        if (currentState == GameState.Paused) { ResumeGame(); return; }
        if (SceneManager.GetActiveScene().name != GameplaySceneName || purly == null)
        {
            LoadGameplayScene(StartMode.ResumeSaved, GetTypedPlayerName());
            return;
        }
        ResumeSavedGameInternal();
    }

    public void ShowHighScores()
    {
        string text = BuildHighScoreText();
        if (mainMenuCanvas != null) mainMenuCanvas.ShowHighScores(text);
    }

    public void ReturnToMainMenu()
    {
        StopGunRoutine();
        Time.timeScale = 1f;
        pendingStartMode = StartMode.Landing;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void ApplyPendingStartMode()
{
        if (pendingStartMode == StartMode.NewGame) StartNewGameInternal(pendingPlayerName);
        else if (pendingStartMode == StartMode.ResumeSaved) ResumeSavedGameInternal();
        pendingStartMode = StartMode.Landing;
    }

    private void ResumeSavedGameInternal()
    {
        UpdateSceneReferences();
        if (!TryLoadSavedGame(out SavedGameData data)) { ShowLandingPage(); return; }
        currentPlayerName = data.playerName;
        currentScore = data.score;
        currentState = GameState.Playing;
        if (purly != null) purly.RestoreSavedState(new Vector3(data.playerX, data.playerY, data.playerZ), data.middleSphereYRotation);
        ShowPlayingUi();
        UpdateScoreText();
        EnsureGunRoutine();
    }

    public void QuitGame() { 
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#else
        Application.Quit(); 
#endif
    }

    private void EnsureGunRoutine() { StopGunRoutine(); gunRoutine = StartCoroutine(GunLoop()); }
    private void StopGunRoutine() { if (gunRoutine != null) { StopCoroutine(gunRoutine); gunRoutine = null; } }

    private IEnumerator GunLoop()
    {
        while (currentState == GameState.Playing)
        {
            if (snowGuns.Count > 0)
            {
                SnowGun gun = snowGuns[UnityEngine.Random.Range(0, snowGuns.Count)];
                if (gun != lastFiredGun) { gun.FireAt(purly != null ? purly.transform : null); lastFiredGun = gun; }
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(MinGunWaitTime, MaxGunWaitTime));
        }
    }

    private void LoadGameplayScene(StartMode mode, string name) { pendingPlayerName = name; pendingStartMode = mode; SceneManager.LoadScene(GameplaySceneName); }

    private void ShowPlayingUi()
    {
        HideAllMenuUi();
        if (hudCanvas != null) hudCanvas.ShowHud(true);
        Time.timeScale = 1f;
    }

    private void HideAllMenuUi()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.gameObject.SetActive(false);
        if (pauseCanvas != null) pauseCanvas.HidePause();
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    private void ShowPanels(bool menu, bool scores, bool hud) { menuPanel.SetActive(menu); highScorePanel.SetActive(scores); hudPanel.SetActive(hud); }

    private void SaveCurrentGame() { if (purly == null) return; SavedGameData data = new SavedGameData { playerName = currentPlayerName, score = currentScore, playerX = purly.GetCurrentPosition().x, playerY = purly.GetCurrentPosition().y, playerZ = purly.GetCurrentPosition().z, middleSphereYRotation = purly.GetMiddleSphereRotationY() }; File.WriteAllText(SaveFilePath, JsonUtility.ToJson(data, true)); }
    private bool TryLoadSavedGame(out SavedGameData data) { data = null; if (!File.Exists(SaveFilePath)) return false; data = JsonUtility.FromJson<SavedGameData>(File.ReadAllText(SaveFilePath)); return data != null; }
    private void DeleteSavedGame() { if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath); }
    private void SaveFinishedScore() { ScoreEntry entry = new ScoreEntry { playerName = currentPlayerName, score = currentScore, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm") }; scoreHistory.entries.Add(entry); scoreHistory.entries.Sort((a, b) => b.score.CompareTo(a.score)); File.WriteAllText(ScoreFilePath, JsonUtility.ToJson(scoreHistory, true)); }
    private void LoadScoreHistory() { if (File.Exists(ScoreFilePath)) scoreHistory = JsonUtility.FromJson<ScoreHistory>(File.ReadAllText(ScoreFilePath)) ?? new ScoreHistory(); }
    private string BuildHighScoreText() { if (scoreHistory.entries.Count == 0) return "No scores yet."; string s = "Top 5 Scores\n"; for (int i = 0; i < Math.Min(5, scoreHistory.entries.Count); i++) s += $"{i+1}. {scoreHistory.entries[i].playerName} - {scoreHistory.entries[i].score}\n"; return s; }
    private void UpdateScoreText() { if (hudCanvas != null) hudCanvas.SetScore(currentScore); if (scoreText != null) scoreText.text = $"Score: {currentScore:00}"; }
    private string GetTypedPlayerName() => mainMenuCanvas != null ? mainMenuCanvas.PlayerName : (playerNameField != null ? playerNameField.text : "Player");

    private void BuildRuntimeUi()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        landingBackgroundSprite = LoadSpriteFromResources("UI/landingPage");
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
        menuPanelImage = menuPanel.GetComponent<Image>();
        menuPanelImage.sprite = landingBackgroundSprite;
        menuPanelImage.preserveAspect = false;

        GameObject buttonGroup = new GameObject("ButtonGroup", typeof(RectTransform), typeof(Image), typeof(Outline));
        buttonGroup.transform.SetParent(menuPanel.transform, false);
        buttonGroupRect = buttonGroup.GetComponent<RectTransform>();
        buttonGroupRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonGroupRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonGroupRect.pivot = new Vector2(0.5f, 0.5f);
        buttonGroupRect.sizeDelta = new Vector2(260f, 330f);
        buttonGroupRect.anchoredPosition = new Vector2(0f, -10f);
        buttonGroupImage = buttonGroup.GetComponent<Image>();
        buttonGroupOutline = buttonGroup.GetComponent<Outline>();

        menuTitleText = CreateText("MenuTitle", buttonGroup.transform, font, string.Empty, 24, TextAnchor.MiddleCenter, Color.black);
        menuTitleText.rectTransform.anchoredPosition = new Vector2(0f, 168f);
        menuTitleText.rectTransform.sizeDelta = new Vector2(320f, 40f);

        playerNameField = CreateInputField(buttonGroup.transform, font, new Vector2(0f, 110f), new Vector2(250f, 44f));
        playerNameField.text = "Player";

        newGameButton = CreateButton("NewGameButton", buttonGroup.transform, font, "New Game", new Vector2(0f, 42f), new Vector2(210f, 42f), StartNewGame);
        saveGameButton = CreateButton("SaveGameButton", buttonGroup.transform, font, "Saved Game", new Vector2(0f, -14f), new Vector2(210f, 42f), LoadSavedGame);
        highScoresButton = CreateButton("HighScoreButton", buttonGroup.transform, font, "High Scores", new Vector2(0f, -70f), new Vector2(210f, 42f), ShowHighScores);
        exitButton = CreateButton("EndGameButton", buttonGroup.transform, font, "Exit", new Vector2(0f, -126f), new Vector2(210f, 42f), QuitGame);
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
        SetLandingMenuLook();
    }

    private void CreateEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)).GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    private GameObject CreatePanel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject panelObject = new GameObject(name, typeof(Image));
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

    private Text CreateText(string name, Transform parent, Font font, string value, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(Text));
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

    private Button CreateButton(string name, Transform parent, Font font, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(name, typeof(Image), typeof(Outline), typeof(Button));
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

    private InputField CreateInputField(Transform parent, Font font, Vector2 anchoredPosition, Vector2 size)
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

    private static void SetMiddleRightAnchor(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(1f, 0.5f);
        rectTransform.anchorMax = new Vector2(1f, 0.5f);
        rectTransform.pivot = new Vector2(1f, 0.5f);
    }

    private void PositionHudButtons()
    {
        if (resumeButton == null || pauseButton == null || restartButton == null) return;

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
        if (button == null) return;
        Image image = button.GetComponent<Image>();
        if (image == null) return;

        image.color = Color.white;
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;

        Text label = button.GetComponentInChildren<Text>();
        if (label != null) label.text = string.Empty;
    }

    private static void AddTextOutline(Text textComponent, Color outlineColor, Vector2 outlineDistance)
    {
        if (textComponent == null) return;
        Outline outline = textComponent.gameObject.GetComponent<Outline>() ?? textComponent.gameObject.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = outlineDistance;
    }

    private static Sprite LoadSpriteFromResources(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null) return null;
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private void ApplyHudButtonIcons()
    {
        StyleIconButton(pauseButton, LoadSpriteFromResources("UI/pauseBtn"));
        StyleIconButton(restartButton, LoadSpriteFromResources("UI/restartBtn"));
        StyleIconButton(resumeButton, LoadSpriteFromResources("UI/resumeBtn"));
    }

    private void SetLandingMenuLook()
    {
        if (menuPanelImage != null)
        {
            menuPanelImage.sprite = landingBackgroundSprite;
            menuPanelImage.color = Color.white;
            menuPanelImage.raycastTarget = true;
        }

        if (buttonGroupImage != null)
        {
            buttonGroupImage.color = new Color(1f, 1f, 1f, 0f);
            buttonGroupImage.raycastTarget = false;
        }

        if (buttonGroupOutline != null)
        {
            buttonGroupOutline.effectColor = new Color(0f, 0f, 0f, 0f);
            buttonGroupOutline.effectDistance = Vector2.zero;
        }

        if (buttonGroupRect != null)
        {
            buttonGroupRect.sizeDelta = new Vector2(260f, 330f);
            buttonGroupRect.anchoredPosition = new Vector2(0f, -10f);
        }

        if (menuTitleText != null) menuTitleText.rectTransform.anchoredPosition = new Vector2(0f, 168f);

        if (playerNameField != null)
        {
            playerNameField.gameObject.SetActive(true);
            playerNameField.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 110f);
        }

        SetMenuButtonPosition(newGameButton, 42f);
        SetMenuButtonPosition(saveGameButton, -14f);
        SetMenuButtonPosition(highScoresButton, -70f);
        SetMenuButtonPosition(exitButton, -126f);

        if (highScorePanel != null) highScorePanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -248f);
    }

    private void SetOverlayMenuLook()
    {
        if (menuPanelImage != null)
        {
            menuPanelImage.sprite = null;
            menuPanelImage.color = new Color(0f, 0f, 0f, 0f);
            menuPanelImage.raycastTarget = false;
        }

        if (buttonGroupImage != null)
        {
            buttonGroupImage.color = new Color(1f, 1f, 1f, 0.92f);
            buttonGroupImage.raycastTarget = true;
        }

        if (buttonGroupOutline != null)
        {
            buttonGroupOutline.effectColor = Color.black;
            buttonGroupOutline.effectDistance = new Vector2(2f, -2f);
        }

        if (buttonGroupRect != null)
        {
            buttonGroupRect.sizeDelta = new Vector2(280f, 300f);
            buttonGroupRect.anchoredPosition = new Vector2(0f, 8f);
        }

        if (menuTitleText != null) menuTitleText.rectTransform.anchoredPosition = new Vector2(0f, 124f);
        if (playerNameField != null) playerNameField.gameObject.SetActive(false);

        SetMenuButtonPosition(newGameButton, 48f);
        SetMenuButtonPosition(saveGameButton, -4f);
        SetMenuButtonPosition(highScoresButton, -56f);
        SetMenuButtonPosition(exitButton, -108f);

        if (highScorePanel != null) highScorePanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -228f);
    }

    private void SetMenuButtonPosition(Button b, float y) { if (b != null) b.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, y); }
    }
