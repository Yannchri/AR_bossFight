using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class VRSceneFader : MonoBehaviour
{
    public float fadeDuration = 1f;

    static VRSceneFader instance;
    Material fadeMaterial;
    bool isFading = false;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        fadeMaterial = GetComponent<MeshRenderer>().material;

        // Invisible immédiatement
        SetAlpha(1f);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void Start()
    {
        // Fade IN au tout premier lancement
        StartCoroutine(FadeIn());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneUnloaded(Scene scene)
    {
        // Fade OUT AVANT que la scène disparaisse
        if (!isFading)
            StartCoroutine(FadeOut());
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fade IN APRÈS chargement
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        isFading = true;
        float t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime / fadeDuration;
            SetAlpha(t);
            yield return null;
        }

        SetAlpha(0f);
        isFading = false;
    }

    IEnumerator FadeOut()
    {
        isFading = true;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            SetAlpha(t);
            yield return null;
        }

        SetAlpha(1f);
        isFading = false;
    }

    void SetAlpha(float a)
    {
        Color c = fadeMaterial.color;
        c.a = Mathf.Clamp01(a);
        fadeMaterial.color = c;
    }
}
