using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gestiona el pool de misiones del día y la mano activa del jugador.
///
/// Flujo de uso:
///   1. Al inicio del día: MissionDeck.Instance.BuildDailyPool(dayNumber)
///   2. Llamar FillHand() para poblar la mano inicial
///   3. MissionCardArea escucha OnCardDrawn para instanciar la carta visual
///   4. Al asignar/descartar: RemoveFromHand(mission) → FillHand() automático
/// </summary>
public class MissionDeck : MonoBehaviour
{
    public static MissionDeck Instance { get; private set; }

    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Catálogo de misiones")]
    [Tooltip("Todos los MissionData assets del juego (48 en total: 12 rangos × 4 palos).")]
    [SerializeField] private MissionData[] _allMissions;

    [Header("Configuración del mazo")]
    [Tooltip("Cartas visibles simultáneamente en la mano del jugador.")]
    [SerializeField] private int _handSize = 5;
    [Tooltip("Total de misiones en el pool de cada día (15-20 recomendado).")]
    [SerializeField] private int _dailyPoolSize = 18;

    // ── Estado interno ───────────────────────────────────────────────────────

    private readonly List<MissionInstance> _dailyPool   = new List<MissionInstance>();
    private readonly List<MissionInstance> _currentHand = new List<MissionInstance>();
    private int _currentDay;

    // ── Propiedades públicas ─────────────────────────────────────────────────

    public int HandSize      => _handSize;
    public int DailyPoolSize => _dailyPoolSize;

    /// <summary>Cartas de misión actualmente en la mano del jugador.</summary>
    public IReadOnlyList<MissionInstance> CurrentHand => _currentHand;

    /// <summary>Misiones restantes en el pool (sin contar la mano).</summary>
    public int PoolRemaining => _dailyPool.Count;

    /// <summary>True cuando el pool está vacío y la mano no puede rellenarse.</summary>
    public bool IsPoolEmpty => _dailyPool.Count == 0;

    // ── Eventos ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Se dispara cada vez que DrawCard() roba una carta del pool a la mano.
    /// MissionCardArea escucha esto para instanciar la carta visual correspondiente.
    /// </summary>
    public event System.Action<MissionInstance> OnCardDrawn;

    /// <summary>
    /// Se dispara cuando el pool se agota (el día debería terminar pronto una vez
    /// que también se vacíe la mano).
    /// </summary>
    public event System.Action OnPoolExhausted;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Construye el pool de misiones para el día indicado.
    /// Filtra el catálogo por rango según la progresión del día, baraja y selecciona
    /// hasta <see cref="_dailyPoolSize"/> misiones.
    ///
    /// Usa DayConfig (vía RunManager o FameManager) si está disponible;
    /// en caso contrario usa las fórmulas inline de respaldo.
    /// </summary>
    public void BuildDailyPool(int dayNumber)
    {
        _currentDay = dayNumber;
        _dailyPool.Clear();
        _currentHand.Clear();

        // Preferir DayConfig si está accesible a través de RunManager o FameManager
        DayConfig config = RunManager.Instance != null ? RunManager.Instance.DayConfig
                         : FameManager.Instance != null ? FameManager.Instance.Config
                         : null;

        int minRank = config != null ? config.GetMinRank(dayNumber) : GetMinRankForDay(dayNumber);
        int maxRank = config != null ? config.GetMaxRank(dayNumber) : GetMaxRankForDay(dayNumber);

        var eligible = _allMissions == null
            ? new List<MissionData>()
            : _allMissions
                .Where(m => m != null && (int)m.Rank >= minRank && (int)m.Rank <= maxRank)
                .ToList();

        if (eligible.Count == 0)
        {
            Debug.LogWarning($"[MissionDeck] Sin misiones elegibles para día {dayNumber} " +
                             $"(rango {minRank}-{maxRank}). Usando catálogo completo.");
            eligible = _allMissions != null
                ? _allMissions.Where(m => m != null).ToList()
                : new List<MissionData>();
        }

        Shuffle(eligible);

        int take = Mathf.Min(_dailyPoolSize, eligible.Count);
        for (int i = 0; i < take; i++)
            _dailyPool.Add(MissionInstance.FromData(eligible[i], dayNumber));

        Debug.Log($"[MissionDeck] Pool día {dayNumber}: {_dailyPool.Count} misiones " +
                  $"(rangos {minRank}-{maxRank}).");
    }

    /// <summary>
    /// Extrae la siguiente misión del tope del pool, la añade a la mano y
    /// dispara <see cref="OnCardDrawn"/>.
    /// Devuelve null si el pool está vacío.
    /// </summary>
    public MissionInstance DrawCard()
    {
        if (_dailyPool.Count == 0)
        {
            if (_currentHand.Count == 0)
                OnPoolExhausted?.Invoke();
            return null;
        }

        var mission = _dailyPool[_dailyPool.Count - 1];
        _dailyPool.RemoveAt(_dailyPool.Count - 1);
        _currentHand.Add(mission);

        OnCardDrawn?.Invoke(mission);
        return mission;
    }

    /// <summary>
    /// Rellena la mano hasta <see cref="_handSize"/> robando del pool.
    /// Llamar al inicio del día y después de cada misión asignada/descartada.
    /// </summary>
    public void FillHand()
    {
        while (_currentHand.Count < _handSize)
        {
            if (DrawCard() == null) break;
        }
    }

    /// <summary>
    /// Quita una MissionInstance de la mano (fue asignada a un héroe o descartada).
    /// Tras quitarla llama a <see cref="FillHand"/> para reponer automáticamente.
    /// </summary>
    public void RemoveFromHand(MissionInstance mission)
    {
        if (mission == null || !_currentHand.Contains(mission)) return;
        _currentHand.Remove(mission);
        FillHand();
    }

    /// <summary>
    /// Vacía la mano lógica sin devolver las misiones al pool ni llamar FillHand.
    /// Llamar justo antes de un Reshuffle para limpiar la mano de golpe; después
    /// llamar FillHand() para robar una mano nueva.
    /// </summary>
    public void ClearHand()
    {
        _currentHand.Clear();
    }

    /// <summary>
    /// Baraja las misiones restantes en el pool (consumible Reshuffle).
    /// Publica <see cref="OnMissionsReshuffled"/> en el EventBus.
    /// </summary>
    public void Reshuffle()
    {
        Shuffle(_dailyPool);
        EventBus.Publish(new OnMissionsReshuffled());
        Debug.Log($"[MissionDeck] Pool reshuffleado: {_dailyPool.Count} misiones restantes.");
    }

    // ── Fórmulas de progresión (fallback sin DayConfig) ──────────────────────
    // Valores de referencia: Día 1→(1,4) | Día 5→(4,9) | Día 10→(8,12)
    // Se usan solo si DayConfig no está disponible en RunManager ni FameManager.

    private static int GetMinRankForDay(int day)
    {
        if (day <= 1)  return 1;
        if (day <= 5)  return Mathf.RoundToInt(Mathf.Lerp(1f, 4f, (day - 1) / 4f));
        return          Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(4f, 8f, (day - 5) / 5f)), 1, 12);
    }

    private static int GetMaxRankForDay(int day)
    {
        if (day <= 1)  return 4;
        if (day <= 5)  return Mathf.RoundToInt(Mathf.Lerp(4f, 9f, (day - 1) / 4f));
        return          Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(9f, 12f, (day - 5) / 5f)), 1, 12);
    }

    // ── Fisher-Yates shuffle ─────────────────────────────────────────────────

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
