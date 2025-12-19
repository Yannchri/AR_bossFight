using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonAction : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject aboutPanel;

    void Start()
    {
        aboutPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Bouton PLAY
    public void StartBtn()
    {
        Debug.Log("RAY SELECT OK");
        SceneManager.LoadScene("Main_Quest_Build");
    }

    // Bouton ABOUT
    public void OpenAbout()
    {
        mainMenuPanel.SetActive(false);
        aboutPanel.SetActive(true);
    }

    // Bouton BACK (depuis About)
    public void BackToMenu()
    {
        aboutPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
