using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonAction : MonoBehaviour
{
    public void StartBtn()
    {
        SceneManager.LoadScene("Main_Quest_Build");
        Debug.Log("RAY SELECT OK");
    }
}
