using UnityEngine;

/// <summary>
/// Orquesta el inicio de cada día/run coordinando todos los subsistemas.
///
/// Es el punto de entrada para comenzar o reiniciar un día. Se encarga de:
///   1. Limpiar modificadores del día anterior (BuffManager).
///   2. Resetear la Fama del día (FameManager).
///   3. Construir el pool de misiones con los rangos correctos (MissionDeck + DayConfig).
///   4. Ajustar el intervalo de spawn de héroes (HeroSpawner + DayConfig).
///   5. Rellenar la mano inicial de cartas (MissionDeck.FillHand).
///   6. Delegar el inicio formal del día a GameManager (resetea vidas, publica OnDayStart).
///
/// Nota: La lógica de Game Over y fin de día (OnDayThresholdReached) la gestiona
/// GameManager; RunManager solo orquesta la preparación del día.
///
/// Escucha:  — (inicia desde GameManager o llamada directa a StartDay)
/// Publica:  — (delega en GameManager: OnDayStart / OnDayEnd / OnGameOver)
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    // ── Inspector ────────────────────────────────────────────────────────────

    [Tooltip("ScriptableObject con las curvas de progresión del día. " +
             "Si no se asigna, cada sistema usa sus valores por defecto.")]
    [SerializeField] private DayConfig _dayConfig;

    // ── Propiedad pública ────────────────────────────────────────────────────

    /// <summary>DayConfig activo en este run. Puede ser null si no se asignó en Inspector.</summary>
    public DayConfig DayConfig => _dayConfig;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartDay();
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia (o reinicia) el día actual. Llama a este método al comenzar la partida
    /// y al regresar de la tienda inter-día.
    ///
    /// Orden de operaciones:
    ///   1. ClearDailyModifiers  (BuffManager)
    ///   2. ResetDayFame         (FameManager)
    ///   3. BuildDailyPool       (MissionDeck, rangos desde DayConfig)
    ///   4. Ajustar SpawnInterval (HeroSpawner, desde DayConfig)
    ///   5. FillHand             (MissionDeck)
    ///   6. StartDay             (GameManager → resetea vidas + publica OnDayStart)
    /// </summary>
    public void StartDay()
    {
        int day = GameManager.Instance != null ? GameManager.Instance.DayNumber : 1;

        // 1. Limpiar modificadores diarios de buffs del evento anterior
        if (BuffManager.Instance != null)
            BuffManager.Instance.ClearDailyModifiers();

        // 2. Resetear la fama del día
        if (FameManager.Instance != null)
            FameManager.Instance.ResetDayFame();

        // 3a. Limpiar las cartas visuales de la mano del día anterior
        if (MissionCardArea.Instance != null)
            MissionCardArea.Instance.ClearAllCards();

        // 3b. Construir el pool de misiones con rangos del día
        if (MissionDeck.Instance != null)
        {
            // Si DayConfig está disponible, inyectarlo para que MissionDeck lo use
            // (MissionDeck lee DayConfig.Instance internamente en BuildDailyPool)
            MissionDeck.Instance.BuildDailyPool(day);
        }

        // 4. Ajustar velocidad de spawn según DayConfig
        if (HeroSpawner.Instance != null && _dayConfig != null)
        {
            float baseInterval = _dayConfig.GetSpawnInterval(day);
            // SpawnIntervalMultiplier se aplica sobre SpawnIntervalBase en HeroSpawner.
            // Como RunManager no tiene acceso directo a SpawnIntervalBase, ajustamos el
            // multiplicador para que el intervalo efectivo coincida con la curva del día.
            // Si HeroQueueConfig.SpawnIntervalBase == 15s (valor por defecto):
            //   multiplier = dayInterval / 15s
            // El resultado queda clampado a SpawnIntervalMin por HeroSpawner.
            const float DefaultBaseInterval = 15f;
            HeroSpawner.Instance.SpawnIntervalMultiplier = baseInterval / DefaultBaseInterval;
        }

        // 5. Rellenar la mano inicial de cartas
        if (MissionDeck.Instance != null)
            MissionDeck.Instance.FillHand();

        // 6. Iniciar formalmente el día (resetea vidas, publica OnDayStart)
        if (GameManager.Instance != null)
            GameManager.Instance.StartDay();

        Debug.Log($"[RunManager] Día {day} iniciado. " +
                  $"Umbral Fama: {(FameManager.Instance != null ? FameManager.Instance.GetDayThreshold(day) : -1)}. " +
                  $"Pool: {(MissionDeck.Instance != null ? MissionDeck.Instance.PoolRemaining : 0)} misiones.");
    }
}
