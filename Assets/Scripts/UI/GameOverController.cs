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

    private void Start()
    {
        if (_dayText != null)
            _dayText.text = $"Llegaste al Día {GameOverData.DayReached}";

        if (_fameText != null)
            _fameText.text = $"Fama Total: {GameOverData.TotalFame}";

        if (_restartButton != null)
            _restartButton.onClick.AddListener(() => SceneManager.LoadScene("Gameplay 1"));
    }
}
