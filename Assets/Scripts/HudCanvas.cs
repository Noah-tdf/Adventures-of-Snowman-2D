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
