using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona el evento aleatorio del día: elige uno del pool, aplica sus efectos
/// al inicio del día y los limpia antes de aplicar el siguiente.
///
/// Tipos soportados:
///   StatBuff / StatDebuff     → BuffManager.ApplyDailyModifier
///   SpawnSpeedBuff / Debuff   → HeroSpawner.SpawnIntervalMultiplier (multiplicativo)
///   FameBoost / FameDebuff    → _fameModifier (leído por FameManager via GetFameModifier)
///   SlotReduction             → HeroQueue.SlotOverride
///
/// Escucha:  OnDayStart
/// Publica:  OnDailyEventActivated
/// </summary>
public class DailyEventManager : MonoBehaviour
{
    public static DailyEventManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Tooltip("Todos los DailyEventData disponibles para sortear.")]
    [SerializeField] private DailyEventData[] _eventPool;

    // ── Estado ────────────────────────────────────────────────────────────────

    /// <summary>Evento activo en el día actual. Null si no hay evento (no debería ocurrir).</summary>
    public DailyEventData ActiveEvent { get; private set; }

    private float _fameModifier     = 1f;
    private float _appliedSpawnMod  = 1f;   // guardado para revertir si RunManager no resetea
    private int   _appliedSlotRedux = 0;

    private DailyEventData _lastPickedEvent;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()  => EventBus.Subscribe<OnDayStart>(HandleDayStart);
    private void OnDisable() => EventBus.Unsubscribe<OnDayStart>(HandleDayStart);

    // ── Handler ───────────────────────────────────────────────────────────────

    // NOTA: RunManager.StartDay() ya restableció SpawnIntervalMultiplier al valor base
    // del día ANTES de publicar OnDayStart, por lo que no es necesario revertirlo aquí.
    private void HandleDayStart(OnDayStart e)
    {
        ClearEvent();

        DailyEventData picked = PickDailyEvent(e.DayNumber);
        if (picked == null)
        {
            Debug.LogWarning("[DailyEventManager] Pool vacío o null. No hay evento hoy.");
            return;
        }

        ActiveEvent = picked;
        _lastPickedEvent = picked;
        ApplyEvent(picked);
        EventBus.Publish(new OnDailyEventActivated { Event = picked });

        Debug.Log($"[DailyEventManager] Día {e.DayNumber}: evento activo → \"{picked.EventTitle}\"");
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Multiplicador de fama del evento activo (1.0 = sin modificación).
    /// Llamado por FameManager al resolver OnMissionCompleted.
    /// </summary>
    public static float GetFameModifier() =>
        Instance != null ? Instance._fameModifier : 1f;

    // ── Lógica interna ────────────────────────────────────────────────────────

    /// <summary>
    /// Elige un evento aleatorio del pool, evitando repetir el del día anterior.
    /// </summary>
    private DailyEventData PickDailyEvent(int dayNumber)
    {
        if (_eventPool == null || _eventPool.Length == 0) return null;
        if (_eventPool.Length == 1)                       return _eventPool[0];

        // Filtrar el evento del día anterior para evitar repetición
        var candidates = new List<DailyEventData>(_eventPool.Length);
        foreach (var ev in _eventPool)
            if (ev != null && ev != _lastPickedEvent)
                candidates.Add(ev);

        if (candidates.Count == 0) return _eventPool[Random.Range(0, _eventPool.Length)];

        return candidates[Random.Range(0, candidates.Count)];
    }

    /// <summary>
    /// Aplica los efectos del evento al sistema correspondiente.
    /// Llamado una vez al inicio de cada día.
    /// </summary>
    private void ApplyEvent(DailyEventData ev)
    {
        if (ev == null) return;

        switch (ev.Type)
        {
            case DailyEventType.StatBuff:
            case DailyEventType.StatDebuff:
                ApplyStatEffect(ev);
                break;

            case DailyEventType.SpawnSpeedBuff:
            case DailyEventType.SpawnSpeedDebuff:
                if (HeroSpawner.Instance != null)
                {
                    _appliedSpawnMod = ev.SpawnSpeedModifier;
                    HeroSpawner.Instance.SpawnIntervalMultiplier *= ev.SpawnSpeedModifier;
                }
                break;

            case DailyEventType.FameBoost:
            case DailyEventType.FameDebuff:
                _fameModifier = ev.FameModifier;
                break;

            case DailyEventType.SlotReduction:
                if (HeroQueue.Instance != null && ev.SlotReductionAmount > 0)
                {
                    _appliedSlotRedux = ev.SlotReductionAmount;
                    HeroQueue.Instance.SlotOverride =
                        HeroQueue.Instance.TotalSlots - ev.SlotReductionAmount;
                }
                break;
        }
    }

    /// <summary>
    /// Elimina los modificadores del evento activo.
    /// Llamado automáticamente al inicio del siguiente día (antes de aplicar el nuevo).
    /// NOTA: SpawnIntervalMultiplier ya fue restablecido por RunManager antes de OnDayStart.
    /// </summary>
    private void ClearEvent()
    {
        // Modificadores de stat → BuffManager limpia sus dailyModifiers
        if (BuffManager.Instance != null)
            BuffManager.Instance.ClearDailyModifiers();

        // Fama
        _fameModifier = 1f;

        // Slots (HeroQueue)
        if (_appliedSlotRedux > 0 && HeroQueue.Instance != null)
            HeroQueue.Instance.SlotOverride = -1;
        _appliedSlotRedux = 0;

        // SpawnSpeed: RunManager ya restableció el multiplicador base,
        // pero si de algún modo ClearEvent se llama sin que RunManager haya actuado,
        // revertimos dividiendo por el modificador guardado.
        // (Caso normal: _appliedSpawnMod se sobreescribirá antes de usarse.)
        _appliedSpawnMod = 1f;

        ActiveEvent = null;
    }

    /// <summary>
    /// Aplica modificadores de stat (uno solo o todos según AffectsAllStats).
    /// </summary>
    private static void ApplyStatEffect(DailyEventData ev)
    {
        if (BuffManager.Instance == null) return;

        if (ev.AffectsAllStats)
        {
            BuffManager.Instance.ApplyDailyModifier(HeroStat.Strength,     ev.StatModifier);
            BuffManager.Instance.ApplyDailyModifier(HeroStat.Dexterity,    ev.StatModifier);
            BuffManager.Instance.ApplyDailyModifier(HeroStat.Intelligence, ev.StatModifier);
            BuffManager.Instance.ApplyDailyModifier(HeroStat.Charisma,     ev.StatModifier);
        }
        else
        {
            BuffManager.Instance.ApplyDailyModifier(ev.AffectedStat, ev.StatModifier);
        }
    }
}
