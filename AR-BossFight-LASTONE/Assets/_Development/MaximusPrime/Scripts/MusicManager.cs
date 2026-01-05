using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip bossMusic;
    public AudioClip gameOverMusic;
    public AudioClip winnerMusic;

    [Header("Scene Names (must match exactly)")]
    public string menuSceneName = "Menu";
    public string bossSceneName = "Main_Quest_Build";
    public string gameOverSceneName = "GameOver";
    public string winnerSceneName = "Winner";

    AudioSource audioSource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.spatialBlend = 0f; // 2D

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    void PlayForScene(string sceneName)
    {
        if (sceneName == bossSceneName)
            PlayMusic(bossMusic, 0.5f, "🔥 Boss music playing");
        else if (sceneName == gameOverSceneName)
            PlayMusic(gameOverMusic, 0.5f, "💀 GameOver music playing");
        else if (sceneName == winnerSceneName)
            PlayMusic(winnerMusic, 0.5f, "🏆 Winner music playing");
        else if (sceneName == menuSceneName)
            PlayMusic(menuMusic, 0.4f, "🎵 Menu music playing");
        else
            PlayMusic(menuMusic, 0.35f, $"🎵 Default music for scene: {sceneName}");
    }

    void PlayMusic(AudioClip clip, float volume, string log)
    {
        if (clip == null)
        {
            Debug.LogWarning("MusicManager: Missing AudioClip for current scene.");
            return;
        }

        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        Debug.Log(log);
    }
}
