using UnityEngine;

public class GameOverController : MonoBehaviour
{
    public GameObject gameOverPanel;

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.PlayerDead)
        {
            ShowGameOver();
        }
    }

    void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // optionnel
    }
}
