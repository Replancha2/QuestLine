using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona dos capas de modificadores de stats:
///   - Buffs permanentes: comprados en tienda, duran toda la partida.
///   - Modificadores diarios: aplicados por el DailyEventManager, se limpian al inicio de cada día.
///
/// Uso:
///   BuffManager.Instance.ApplyBuff(buffData);
///   int bonus = BuffManager.Instance.GetActiveStatBonus(HeroStat.Strength);
/// </summary>
public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    // ── Estado ──────────────────────────────────────────────────────────────

    private readonly List<BuffData> _permanentBuffs = new List<BuffData>();
    private readonly List<StatModifier> _dailyModifiers = new List<StatModifier>();

    // Protección contra fallos: número de escudos activos
    private int _failProtectionCharges = 0;

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

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica un buff comprado en la tienda.
    /// Los buffs permanentes se acumulan; los de efecto puntual se resuelven inmediatamente.
    /// </summary>
    public void ApplyBuff(BuffData buff)
    {
        if (buff == null) return;

        switch (buff.Type)
        {
            case BuffType.HeroStatBoost:
                if (buff.IsPermanent)
                    _permanentBuffs.Add(buff);
                // Si no es permanente se aplica como modificador único de aquí al fin del día
                else
                    _dailyModifiers.Add(new StatModifier
                    {
                        Stat   = buff.AffectedStat,
                        Amount = buff.StatBoostAmount,
                    });
                break;

            case BuffType.FailProtection:
                _failProtectionCharges += buff.FailProtectionCount;
                if (buff.IsPermanent)
                    _permanentBuffs.Add(buff);
                break;

            case BuffType.PatienceBoost:
                if (buff.IsPermanent)
                    _permanentBuffs.Add(buff);
                break;

            // MissionHintReveal y MissionReshuffle son efectos puntuales;
            // su lógica la ejecuta el sistema que los compra (ShopManager → MissionDeck / etc.).
            case BuffType.MissionHintReveal:
            case BuffType.MissionReshuffle:
                // Efecto inmediato resuelto externamente; no se almacena.
                break;
        }

        EventBus.Publish(new OnBuffPurchased { Buff = buff });
    }

    /// <summary>
    /// Aplica un modificador de stat temporal proveniente del evento diario.
    /// Se limpia automáticamente al inicio del siguiente día (ver ClearDailyModifiers).
    /// </summary>
    public void ApplyDailyModifier(HeroStat stat, int amount)
    {
        _dailyModifiers.Add(new StatModifier { Stat = stat, Amount = amount });
    }

    /// <summary>
    /// Elimina todos los modificadores del evento del día.
    /// Llamar desde RunManager.StartDay() antes de aplicar el nuevo evento.
    /// </summary>
    public void ClearDailyModifiers()
    {
        _dailyModifiers.Clear();
    }

    /// <summary>
    /// Suma el bonus total activo (permanentes + diarios) para el stat indicado.
    /// </summary>
    public int GetActiveStatBonus(HeroStat stat)
    {
        int total = 0;

        foreach (var buff in _permanentBuffs)
            if (buff.Type == BuffType.HeroStatBoost && buff.AffectedStat == stat)
                total += buff.StatBoostAmount;

        foreach (var mod in _dailyModifiers)
            if (mod.Stat == stat)
                total += mod.Amount;

        return total;
    }

    /// <summary>
    /// Multiplicador de paciencia acumulado de todos los buffs activos.
    /// 1.0 = sin modificación; 1.5 = +50 %.
    /// </summary>
    public float GetPatienceMultiplier()
    {
        float multiplier = 1f;

        foreach (var buff in _permanentBuffs)
            if (buff.Type == BuffType.PatienceBoost)
                multiplier += buff.PatienceBoostPercent;

        return multiplier;
    }

    /// <summary>True si hay al menos un escudo de FailProtection activo.</summary>
    public bool HasFailProtection() => _failProtectionCharges > 0;

    /// <summary>
    /// Consume un escudo de FailProtection.
    /// Llamar cuando un héroe moriría pero se quiere absorber la muerte.
    /// </summary>
    public void ConsumeFailProtection()
    {
        if (_failProtectionCharges > 0)
            _failProtectionCharges--;
    }

    // ── Acceso de solo lectura (útil para UI/debug) ──────────────────────────

    /// <summary>Número de escudos de FailProtection restantes.</summary>
    public int FailProtectionCharges => _failProtectionCharges;

    /// <summary>Lista de lectura de buffs permanentes activos.</summary>
    public IReadOnlyList<BuffData> PermanentBuffs => _permanentBuffs;

    // ── Tipos privados ───────────────────────────────────────────────────────

    private struct StatModifier
    {
        public HeroStat Stat;
        public int Amount;
    }
}
