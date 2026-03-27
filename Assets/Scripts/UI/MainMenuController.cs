using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Wires up the Main Menu Play button to load the first gameplay scene.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button _playButton;

    private void Start()
    {
        if (_playButton != null)
            _playButton.onClick.AddListener(() => SceneManager.LoadScene("Gameplay 1"));
    }
}
