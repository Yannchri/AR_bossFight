using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRFadeManager : MonoBehaviour
{
    [Header("Fade")]
    public float fadeDuration = 1f;

    Material _mat;
    Coroutine _running;
    static VRFadeManager _instance;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        var mr = GetComponent<MeshRenderer>();
        _mat = mr.material;

        // Invisible dès la toute première frame (évite le "carré noir" au boot)
        SetAlpha(0f);

        // Fade IN à chaque chargement de scène
        SceneManager.sceneLoaded += (_, __) => StartFadeIn();
    }

    void OnDestroy()
    {
        if (_instance == this)
            SceneManager.sceneLoaded -= (_, __) => StartFadeIn();
    }

    void LateUpdate()
    {
        // Colle le quad devant la caméra active (VR)
        var cam = FindAnyCamera();
        if (!cam) return;

        // Devant les yeux
        transform.position = cam.transform.position + cam.transform.forward * 0.25f;
        transform.rotation = cam.transform.rotation;
        transform.localScale = new Vector3(6f, 6f, 1f); // couvre le FOV
    }

    Camera FindAnyCamera()
    {
        // En XR, Camera.main peut être null. On prend n'importe quelle Camera active.
        var cams = Camera.allCameras;
        if (cams != null && cams.Length > 0) return cams[0];
        return FindObjectOfType<Camera>();
    }

    public void FadeOutAndLoad(string sceneName)
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(FadeOutLoadRoutine(sceneName));
    }

    public void StartFadeIn()
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        // part noir -> transparent
        float t = 1f;
        SetAlpha(1f);
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime / fadeDuration;
            SetAlpha(t);
            yield return null;
        }
        SetAlpha(0f);
        _running = null;
    }

    IEnumerator FadeOutLoadRoutine(string sceneName)
    {
        // part transparent -> noir
        float t = 0f;
        SetAlpha(0f);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            SetAlpha(t);
            yield return null;
        }
        SetAlpha(1f);

        // Charge la scène une fois noir (pas de flash)
        SceneManager.LoadScene(sceneName);
        _running = null;
    }

    void SetAlpha(float a)
    {
        var c = _mat.color;
        c.a = Mathf.Clamp01(a);
        _mat.color = c;
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            SetAlpha(t);
            yield return null;
        }

        SetAlpha(1f); // écran complètement noir
    }

}
