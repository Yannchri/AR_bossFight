using UnityEngine;

public class WinnerController : MonoBehaviour
{
    public GameObject winnerPanel;

    void Start()
    {
        winnerPanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.BossDead)
        {
            ShowWinner();
        }
    }

    void ShowWinner()
    {
        winnerPanel.SetActive(true);
        Time.timeScale = 0f; // optionnel
    }
}
