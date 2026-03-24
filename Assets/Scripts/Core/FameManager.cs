using UnityEngine;

/// <summary>
/// Gestiona la Fama acumulada durante el día.
///
/// Responsabilidades:
///   - Acumula fama al completar misiones (escucha OnMissionCompleted).
///   - Aplica el multiplicador del DailyEventManager (eventos de Fama).
///   - Publica OnFameEarned con el total del día y el umbral del día actual.
///   - Publica OnDayThresholdReached cuando se supera el umbral.
///   - Se resetea al inicio de cada día mediante ResetDayFame().
///
/// Escucha:  OnMissionCompleted
/// Publica:  OnFameEarned, OnDayThresholdReached
/// </summary>
public class FameManager : MonoBehaviour
{
    public static FameManager Instance { get; private set; }

    // ── Inspector ────────────────────────────────────────────────────────────

    [Tooltip("Referencia al ScriptableObject DayConfig. " +
             "Si no se asigna se usa la fórmula placeholder.")]
    [SerializeField] private DayConfig _dayConfig;

    // ── Estado ───────────────────────────────────────────────────────────────

    /// <summary>Fama total acumulada en toda la run (nunca se resetea).</summary>
    public int TotalFame { get; private set; }

    /// <summary>Fama acumulada en el día actual (se resetea cada día).</summary>
    public int DayFame { get; private set; }

    private bool _thresholdReachedThisDay;

    // ── Unity ────────────────────────────────────────────────────────────────

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
        EventBus.Subscribe<OnMissionCompleted>(HandleMissionCompleted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnMissionCompleted>(HandleMissionCompleted);
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void HandleMissionCompleted(OnMissionCompleted e)
    {
        if (e.FameEarned <= 0) return;

        // Aplicar modificador de evento diario (1.0 = sin cambio)
        float modifier = DailyEventManager.GetFameModifier();
        int fameGained = Mathf.RoundToInt(e.FameEarned * modifier);
        if (fameGained <= 0) return;

        AddFame(fameGained);
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Añade fama al contador del día, publica OnFameEarned y comprueba el umbral.
    /// </summary>
    public void AddFame(int amount)
    {
        if (amount <= 0) return;

        TotalFame += amount;
        DayFame   += amount;
        int day       = GameManager.Instance != null ? GameManager.Instance.DayNumber : 1;
        int threshold = GetDayThreshold(day);

        EventBus.Publish(new OnFameEarned
        {
            Amount       = amount,
            TotalFame    = DayFame,   // total del día para comparar con el umbral en HUD
            DayThreshold = threshold,
        });

        if (!_thresholdReachedThisDay && DayFame >= threshold)
        {
            _thresholdReachedThisDay = true;
            EventBus.Publish(new OnDayThresholdReached { DayNumber = day });
        }
    }

    /// <summary>
    /// Resetea la fama del día. Llamado por RunManager.StartDay() al inicio de cada jornada.
    /// </summary>
    public void ResetDayFame()
    {
        DayFame = 0;
        _thresholdReachedThisDay = false;
    }

    /// <summary>
    /// Devuelve el umbral de Fama para el día indicado.
    /// Usa DayConfig si está asignado; en caso contrario aplica la fórmula placeholder.
    /// </summary>
    public int GetDayThreshold(int day)
    {
        if (_dayConfig != null)
            return _dayConfig.GetFameThreshold(day);

        // Fórmula placeholder (sin DayConfig): 30 + day * 20
        return 30 + day * 20;
    }

    // ── Acceso de solo lectura ────────────────────────────────────────────────

    /// <summary>Expone el DayConfig para que RunManager pueda distribuirlo a otros sistemas.</summary>
    public DayConfig Config => _dayConfig;
}
