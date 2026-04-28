using UnityEngine;
using UnityEngine.UI;

public class HudCanvas : MonoBehaviour
{
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button resumeButton;

    public Button PauseButton => pauseButton;
    public Button RestartButton => restartButton;
    public Button ResumeButton => resumeButton;

    private void Awake()
    {
        if (scoreText == null)
        {
            return;
        }

        Outline outline = scoreText.GetComponent<Outline>();
        if (outline == null)
        {
            outline = scoreText.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(1.5f, -1.5f);
    }

    public void ShowHud(bool isVisible)
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(isVisible);
        }
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:00}";
        }
    }
}
