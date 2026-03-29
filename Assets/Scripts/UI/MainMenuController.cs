using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Wires up the Main Menu Play button to load the first gameplay scene.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _muteButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private UnityEngine.UI.Slider _volumeSlider;

    private void Start()
    {
        if (_playButton != null)
            _playButton.onClick.AddListener(() => SceneManager.LoadScene("Gameplay 1"));

        if (_muteButton != null)
            _muteButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.ToggleMute();
            });

        if (_exitButton != null)
            _exitButton.onClick.AddListener(() =>
            {
                Application.Quit();
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            });

        if (_volumeSlider != null)
        {
            _volumeSlider.onValueChanged.AddListener(value =>
            {
                AudioManager.Instance?.SetVolume(value);
                if (AudioManager.Instance != null && AudioManager.Instance.IsMuted())
                    AudioManager.Instance.SetMute(false);
            });

            if (AudioManager.Instance != null)
                _volumeSlider.value = AudioManager.Instance.GetVolume();
        }
    }
}
