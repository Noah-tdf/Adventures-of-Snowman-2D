using UnityEngine;

public class SnowmanGameUiBridge : MonoBehaviour
{
    public void StartNewGame()
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.StartNewGame();
        }
    }

    public void LoadSavedGame()
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.LoadSavedGame();
        }
    }

    public void ShowHighScores()
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.ShowHighScores();
        }
    }

    public void PauseGame()
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.PauseGame();
        }
    }

    public void ResumeGame()
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.ResumeGame();
        }
    }

    public void RestartGame()
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.RestartGame();
        }
    }

    public void ReturnToMainMenu()
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.ReturnToMainMenu();
        }
    }

    public void QuitGame()
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.QuitGame();
        }
    }
}
