using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private Text highScoreText;

    public string PlayerName
    {
        get
        {
            if (playerNameInput == null || string.IsNullOrWhiteSpace(playerNameInput.text))
            {
                return "Player";
            }

            return playerNameInput.text.Trim();
        }
    }

    public void ShowMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }

        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }
    }

    public void ShowHighScores(string scoreTextValue)
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }

        if (highScorePanel != null)
        {
            highScorePanel.SetActive(true);
        }

        if (highScoreText != null)
        {
            highScoreText.text = scoreTextValue;
        }
    }

    public void SetPlayerName(string playerName)
    {
        if (playerNameInput != null)
        {
            playerNameInput.text = playerName;
        }
    }
}
