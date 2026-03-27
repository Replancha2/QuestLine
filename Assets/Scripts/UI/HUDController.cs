using UnityEngine;
using TMPro;

/// <summary>
/// Attached to the HUD Canvas. Auto-finds TMP text fields by child GameObject name
/// and keeps them in sync via EventBus events.
/// </summary>
public class HUDController : MonoBehaviour
{
    private TextMeshProUGUI _livesText;
    private TextMeshProUGUI _fameText;
    private TextMeshProUGUI _dayText;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        foreach (TextMeshProUGUI tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            switch (tmp.gameObject.name)
            {
                case "LivesText": _livesText = tmp; break;
                case "FameText":  _fameText  = tmp; break;
                case "DayText":   _dayText   = tmp; break;
            }
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnDayStart>(HandleDayStart);
        EventBus.Subscribe<OnLifeLost>(HandleLifeLost);
        EventBus.Subscribe<OnFameEarned>(HandleFameEarned);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnDayStart>(HandleDayStart);
        EventBus.Unsubscribe<OnLifeLost>(HandleLifeLost);
        EventBus.Unsubscribe<OnFameEarned>(HandleFameEarned);
    }

    private void Start()
    {
        RefreshAll();
    }

    // ── Event handlers ───────────────────────────────────────────────────────

    private void HandleDayStart(OnDayStart e)
    {
        if (_dayText != null)
            _dayText.text = $"Día {e.DayNumber}";
    }

    private void HandleLifeLost(OnLifeLost e)
    {
        if (_livesText != null)
            _livesText.text = BuildHeartsString(e.LivesRemaining);
    }

    private void HandleFameEarned(OnFameEarned e)
    {
        if (_fameText != null)
            _fameText.text = $"{e.TotalFame}/{e.DayThreshold} Fama";
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        if (GameManager.Instance != null)
        {
            if (_dayText != null)
                _dayText.text = $"Día {GameManager.Instance.DayNumber}";

            if (_livesText != null)
                _livesText.text = BuildHeartsString(GameManager.Instance.DayLives);
        }

        if (FameManager.Instance != null && _fameText != null)
        {
            int day       = GameManager.Instance != null ? GameManager.Instance.DayNumber : 1;
            int threshold = FameManager.Instance.GetDayThreshold(day);
            _fameText.text = $"{FameManager.Instance.DayFame}/{threshold} Fama";
        }
    }

    private static string BuildHeartsString(int lives)
    {
        string hearts = "";
        for (int i = 0; i < lives; i++)
            hearts += "♥";
        return hearts;
    }
}
