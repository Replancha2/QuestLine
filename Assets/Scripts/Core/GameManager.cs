using UnityEngine;

/// <summary>
/// Singleton persistente que mantiene el estado global de la run.
/// Gestiona vidas del día, monedas, día actual y fama total.
///
/// Escucha:
///   - OnHeroLeft (WasAngry=true) → LoseLife()
///   - OnDayThresholdReached      → EndDay()
///
/// Publica:
///   - OnLifeLost, OnGameOver, OnDayStart, OnDayEnd, OnCoinEarned, OnCoinSpent
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Estado global ───────────────────────────────────────────────────────
    public int DayNumber      { get; private set; } = 1;
    public int DayLives       { get; private set; } = 3;
    public int CurrentCoins   { get; private set; } = 0;
    public int TotalFameEarned{ get; private set; } = 0;

    private const int MaxDayLives = 3;

    // ── Ciclo de vida de Unity ──────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnHeroLeft>(HandleHeroLeft);
        EventBus.Subscribe<OnDayThresholdReached>(HandleDayThresholdReached);
        EventBus.Subscribe<OnMissionCompleted>(HandleMissionCompleted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnHeroLeft>(HandleHeroLeft);
        EventBus.Unsubscribe<OnDayThresholdReached>(HandleDayThresholdReached);
        EventBus.Unsubscribe<OnMissionCompleted>(HandleMissionCompleted);
    }

    // ── Handlers de eventos ────────────────────────────────────────────────

    private void HandleHeroLeft(OnHeroLeft e)
    {
        if (e.WasAngry)
            LoseLife();
    }

    private void HandleDayThresholdReached(OnDayThresholdReached e)
    {
        EndDay();
    }

    private void HandleMissionCompleted(OnMissionCompleted e)
    {
        TotalFameEarned += e.FameEarned;
    }

    // ── API pública ────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia el día: resetea vidas y publica OnDayStart.
    /// Llamar al principio de cada nueva jornada.
    /// </summary>
    public void StartDay()
    {
        DayLives = MaxDayLives;
        EventBus.Publish(new OnDayStart { DayNumber = DayNumber });
    }

    /// <summary>
    /// Cierra el día actual, incrementa DayNumber y resetea las vidas.
    /// Lo llama HandleDayThresholdReached automáticamente.
    /// </summary>
    public void EndDay()
    {
        EventBus.Publish(new OnDayEnd { DayNumber = DayNumber });
        DayNumber++;
        DayLives = MaxDayLives;
    }

    /// <summary>
    /// Descuenta 1 vida. Si llega a 0 → publica OnGameOver.
    /// </summary>
    public void LoseLife()
    {
        DayLives = Mathf.Max(0, DayLives - 1);
        EventBus.Publish(new OnLifeLost { LivesRemaining = DayLives });

        if (DayLives <= 0)
            EventBus.Publish(new OnGameOver
            {
                DayReached      = DayNumber,
                TotalFameEarned = TotalFameEarned
            });
    }

    /// <summary>Agrega monedas al total y publica OnCoinEarned.</summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        CurrentCoins += amount;
        EventBus.Publish(new OnCoinEarned { Amount = amount });
    }

    /// <summary>
    /// Intenta gastar monedas. Devuelve false si no hay suficiente saldo.
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || CurrentCoins < amount) return false;
        CurrentCoins -= amount;
        EventBus.Publish(new OnCoinSpent { Amount = amount });
        return true;
    }
}
