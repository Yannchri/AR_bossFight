using UnityEngine;

public class PlayButtonAction : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject aboutPanel;

    //[Header("Scene")]
    //public string gameSceneName = "Main_Quest_Build";

    void Start()
    {
        aboutPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Bouton PLAY
    public void StartBtn()
    {
        Debug.Log("RAY SELECT OK");

        // Optionnel : cacher l’UI avant le fade
        mainMenuPanel.SetActive(false);

        VRSceneTransitionManager.Instance.FadeToScene("Main_Quest_Build");

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
