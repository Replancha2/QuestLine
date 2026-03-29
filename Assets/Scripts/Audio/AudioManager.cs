using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip gameOverMusic;

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnDayStart>(OnDayStart);
        EventBus.Subscribe<OnGameOver>(OnGameOver);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnDayStart>(OnDayStart);
        EventBus.Unsubscribe<OnGameOver>(OnGameOver);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("MainMenu"))
        {
            PlayMenuMusic();
            return;
        }

        if (scene.name.Contains("GameOver"))
        {
            PlayGameOverMusic();
            return;
        }

        // Any other scene (including gameplay) uses game music
        PlayGameMusic();
    }

    private bool _isMuted;
    private float _previousVolume = .5f;

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    public void PlayGameOverMusic()
    {
        PlayMusic(gameOverMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
            ApplyMuteState();
        }
    }

    public void SetVolume(float volume)
    {
        if (audioSource == null) return;

        volume = Mathf.Clamp01(volume);
        audioSource.volume = volume;

        if (!_isMuted)
        {
            _previousVolume = volume;
        }
    }

    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume : 0f;
    }

    public void ToggleMute()
    {
        SetMute(!_isMuted);
    }

    public void SetMute(bool mute)
    {
        _isMuted = mute;
        if (mute)
        {
            _previousVolume = audioSource != null ? audioSource.volume : _previousVolume;
            if (audioSource != null)
                audioSource.volume = 0f;
        }
        else
        {
            if (audioSource != null)
                audioSource.volume = _previousVolume;
        }
    }

    public bool IsMuted() => _isMuted;

    private void ApplyMuteState()
    {
        if (audioSource == null) return;
        audioSource.volume = _isMuted ? 0f : Mathf.Clamp01(_previousVolume);
    }

    private void OnDayStart(OnDayStart e)
    {
        PlayGameMusic();
    }

    private void OnGameOver(OnGameOver e)
    {
        PlayGameOverMusic();
    }
}