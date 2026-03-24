using UnityEngine;

/// <summary>
/// ScriptableObject que describe un evento aleatorio diario.
/// Cada evento tiene un tipo (StatBuff, SpawnSpeedBuff, FameBoost, etc.) y magnitudes
/// según el tipo. El DailyEventManager aplica y limpia sus efectos cada día.
/// </summary>
[CreateAssetMenu(menuName = "Questline/Daily Event")]
public class DailyEventData : ScriptableObject
{
    [Header("Presentación")]
    public string EventTitle;
    [TextArea] public string EventDescription;
    public Sprite EventIcon;

    [Header("Efecto")]
    public DailyEventType Type;

    [Header("Stat (solo StatBuff / StatDebuff)")]
    public HeroStat AffectedStat;
    [Tooltip("Valor positivo o negativo. Ej: +3 para buff, -2 para debuff.")]
    public int StatModifier;
    [Tooltip("Si es true, aplica StatModifier a TODOS los stats (ignora AffectedStat).")]
    public bool AffectsAllStats;

    [Header("Velocidad de spawn (solo SpawnSpeedBuff / SpawnSpeedDebuff)")]
    [Tooltip("Multiplicador de intervalo. <1 = más rápido, >1 = más lento. 1 = neutro.")]
    public float SpawnSpeedModifier = 1f;

    [Header("Fama (solo FameBoost / FameDebuff)")]
    [Tooltip("Multiplicador de fama. 1.2 = +20%, 0.8 = -20%, 1.0 = neutro.")]
    public float FameModifier = 1f;

    [Header("Reducción de slots (solo SlotReduction)")]
    [Tooltip("Cuántos slots se eliminan del máximo disponible. Ej: 1 → 4-1 = 3 slots.")]
    public int SlotReductionAmount;
}
