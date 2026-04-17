using UnityEngine;
using UnityEngine.UI;

public class PauseCanvas : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Text titleText;
    [SerializeField] private Button highScoresButton;
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private Text highScoreText;

    public void ShowPause(string message)
    {
        EnsureHighScoreUi();

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = message;
        }

        HideHighScores();
    }

    public void HidePause()
    {
        HideHighScores();

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void PrepareHighScores(string message)
    {
        EnsureHighScoreUi();

        if (highScoreText != null)
        {
            highScoreText.text = message;
        }
    }

    public void ToggleHighScores()
    {
        EnsureHighScoreUi();

        if (highScorePanel != null)
        {
            highScorePanel.SetActive(!highScorePanel.activeSelf);
        }
    }

    public void HideHighScores()
    {
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }
    }

    private void EnsureHighScoreUi()
    {
        if (pausePanel == null)
        {
            return;
        }

        RectTransform panelRect = pausePanel.GetComponent<RectTransform>();
        if (panelRect != null && panelRect.sizeDelta.y < 380f)
        {
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, 380f);
        }

        if (highScoresButton == null)
        {
            GameObject buttonObject = new GameObject("PauseHighScoresButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            buttonObject.transform.SetParent(pausePanel.transform, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(210f, 40f);
            buttonRect.anchoredPosition = new Vector2(0f, -138f);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.82f, 0.9f, 0.98f, 0.95f);

            Outline buttonOutline = buttonObject.GetComponent<Outline>();
            buttonOutline.effectColor = Color.black;
            buttonOutline.effectDistance = new Vector2(2f, -2f);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(ToggleHighScores);
            highScoresButton = button;

            GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);

            Text label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 20;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.black;
            label.text = "High Scores";

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        if (highScorePanel == null)
        {
            GameObject panelObject = new GameObject("PauseHighScorePanel", typeof(RectTransform), typeof(Image), typeof(Outline));
            panelObject.transform.SetParent(pausePanel.transform, false);

            RectTransform scorePanelRect = panelObject.GetComponent<RectTransform>();
            scorePanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            scorePanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            scorePanelRect.pivot = new Vector2(0.5f, 0.5f);
            scorePanelRect.sizeDelta = new Vector2(240f, 130f);
            scorePanelRect.anchoredPosition = new Vector2(0f, -220f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(1f, 1f, 1f, 0.95f);

            Outline panelOutline = panelObject.GetComponent<Outline>();
            panelOutline.effectColor = Color.black;
            panelOutline.effectDistance = new Vector2(2f, -2f);

            highScorePanel = panelObject;
            highScorePanel.SetActive(false);
        }

        if (highScoreText == null && highScorePanel != null)
        {
            GameObject textObject = new GameObject("HighScoreText", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(highScorePanel.transform, false);

            highScoreText = textObject.GetComponent<Text>();
            highScoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            highScoreText.fontSize = 16;
            highScoreText.alignment = TextAnchor.UpperLeft;
            highScoreText.color = Color.black;
            highScoreText.horizontalOverflow = HorizontalWrapMode.Wrap;
            highScoreText.verticalOverflow = VerticalWrapMode.Overflow;
            highScoreText.text = string.Empty;

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 10f);
            textRect.offsetMax = new Vector2(-10f, -10f);
        }
    }
}
