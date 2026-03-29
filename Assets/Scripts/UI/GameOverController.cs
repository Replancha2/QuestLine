using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Displays the game-over stats stored in GameOverData and lets the player restart.
/// </summary>
public class GameOverController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private TextMeshProUGUI _fameText;
    [SerializeField] private Button          _restartButton;
    [SerializeField] private Button          _quitButton;

    private void Start()
    {
        if (_dayText != null)
            _dayText.text = $"Higher day achieved: {GameOverData.DayReached}";

        if (_fameText != null)
            _fameText.text = $"Total Fame: {GameOverData.TotalFame}";

        if (_restartButton != null)
            _restartButton.onClick.AddListener(() => SceneManager.LoadScene("Gameplay 1"));

        if (_quitButton != null)
            _quitButton.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
    }
}
