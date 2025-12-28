using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class VRSceneTransitionManager : MonoBehaviour
{
    public static VRSceneTransitionManager Instance;

    [Header("Fade Settings")]
    public float fadeDuration = 0.8f;

    Material fadeMat;
    Coroutine currentRoutine;
    Camera xrCamera;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        fadeMat = GetComponent<MeshRenderer>().material;
        SetAlpha(1f); // noir immédiat

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        CacheCamera();
        StartFadeIn();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void LateUpdate()
    {
        if (!xrCamera) CacheCamera();
        if (!xrCamera) return;

        transform.position = xrCamera.transform.position + xrCamera.transform.forward * 0.25f;
        transform.rotation = xrCamera.transform.rotation;
        transform.localScale = Vector3.one * 6f;
    }

    void CacheCamera()
    {
        xrCamera = Camera.main ?? FindObjectOfType<Camera>();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CacheCamera();
        StartFadeIn();
    }

    public void FadeToScene(string sceneName)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeOutAndLoad(sceneName));
    }

    void StartFadeIn()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            SetAlpha(Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetAlpha(to);
        currentRoutine = null;
    }

    void SetAlpha(float a)
    {
        Color c = fadeMat.color;
        c.a = Mathf.Clamp01(a);
        fadeMat.color = c;
    }
}
