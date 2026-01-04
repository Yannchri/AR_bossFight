using UnityEngine;
using UnityEngine.SceneManagement;

public class WinnerMenu : MonoBehaviour
{
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}