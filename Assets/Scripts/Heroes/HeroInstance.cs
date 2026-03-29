using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Instancia en runtime de un héroe con stats modificables.
/// Se crea llamando a HeroInstance.Generate(heroData).
/// No es un ScriptableObject ni MonoBehaviour — es una clase C# pura.
/// </summary>
public class HeroInstance
{
    // ── Stats base (generados al instanciar) ────────────────────────────────
    public string Name;
    public int Strength;
    public int Dexterity;
    public int Intelligence;
    public int Charisma;
    public HeroRarity Rarity;
    public Sprite Portrait;

    /// <summary>
    /// Segundos antes de irse sin misión (ya aplicado PatienceMultiplier e Impulsive).
    /// </summary>
    public float Patience;

    // ── Rasgos procedurales (2-3 por héroe) ────────────────────────────────
    public List<HeroTrait> Traits = new List<HeroTrait>();

    /// <summary>
    /// Stats ocultos al jugador. Solo relevante si tiene Distrustful;
    /// 2 stats elegidos al azar en Generate().
    /// </summary>
    public List<HeroStat> HiddenStats = new List<HeroStat>();

    // ── Progresión (XP y Nivel) ─────────────────────────────────────────────
    private const int XPPerLevel = 50;

    public int XP    { get; private set; } = 0;
    /// <summary>Nivel del héroe derivado del XP acumulado (0 al inicio).</summary>
    public int Level => XP / XPPerLevel;

    /// <summary>Acumula XP al héroe. Normalmente llamado por MissionEvaluator tras éxito.</summary>
    public void GainXP(int amount)
    {
        if (amount > 0) XP += amount;
    }

    // ── Estado en runtime ───────────────────────────────────────────────────
    public bool IsWaiting;
    public bool IsDead;

    // ── Construcción ────────────────────────────────────────────────────────

    /// <summary>
    /// Genera un HeroInstance con stats aleatorios dentro de los rangos del template.
    /// Asigna 2-3 rasgos al azar (sin repetición).
    /// </summary>
    public static HeroInstance Generate(HeroData template)
    {
        var hero = new HeroInstance
        {
            Name       = template.HeroName,
            Portrait   = template.Portrait,
            Rarity     = template.Rarity,
            Strength     = UnityEngine.Random.Range(template.StrengthRange.x,     template.StrengthRange.y     + 1),
            Dexterity    = UnityEngine.Random.Range(template.DexterityRange.x,    template.DexterityRange.y    + 1),
            Intelligence = UnityEngine.Random.Range(template.IntelligenceRange.x, template.IntelligenceRange.y + 1),
            Charisma     = UnityEngine.Random.Range(template.CharismaRange.x,     template.CharismaRange.y     + 1),
            IsWaiting    = true,
        };

        // Asignar 2-3 rasgos al azar (sin repetición)
        var pool = new List<HeroTrait>((HeroTrait[])Enum.GetValues(typeof(HeroTrait)));
        int traitCount = UnityEngine.Random.Range(2, 4); // 2 o 3
        for (int i = 0; i < traitCount && pool.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            hero.Traits.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        // Paciencia: BasePatience * PatienceMultiplier; Impulsive → la mitad
        float basePatience = template.BasePatience * template.PatienceMultiplier;
        hero.Patience = hero.Traits.Contains(HeroTrait.Impulsive) ? basePatience / 2f : basePatience;

        // Distrustful: elige 2 stats al azar para ocultarlos en la UI
        if (hero.Traits.Contains(HeroTrait.Distrustful))
        {
            var allStats = new List<HeroStat> { HeroStat.Strength, HeroStat.Dexterity, HeroStat.Intelligence, HeroStat.Charisma };
            int s = UnityEngine.Random.Range(0, allStats.Count);
            hero.HiddenStats.Add(allStats[s]);
            allStats.RemoveAt(s);
            hero.HiddenStats.Add(allStats[UnityEngine.Random.Range(0, allStats.Count)]);
        }

        return hero;
    }

    // ── Lógica de misiones ─────────────────────────────────────────────────

    /// <summary>
    /// Determina si el héroe acepta voluntariamente la misión según sus rasgos.
    ///   Greedy    → false si CoinsReward de la misión es menor a 6
    ///   Demanding → false si el Rank de la misión es R3 o inferior
    ///   Reckless  → true siempre (no filtra misiones)
    ///   Resto     → true
    ///
    /// Softlock: Hasta el día 5, los héroes deben aceptar misiones de rango 1-3.
    /// </summary>
    public bool WillAcceptMission(MissionInstance mission)
    {
        // Softlock: En los primeros 5 días, forzar aceptación de misiones de rango 1-3
        if (GameManager.Instance != null && GameManager.Instance.DayNumber <= 5 && (int)mission.Template.Rank <= 3)
            return true;

        if (Traits.Contains(HeroTrait.Greedy) && mission.Template.CoinsReward < 6)
            return false;

        if (Traits.Contains(HeroTrait.Demanding) && mission.Template.Rank <= MissionRank.R3)
            return false;

        return true;
    }

    /// <summary>
    /// Calcula la probabilidad de éxito en la misión (0.0 a 1.0).
    ///   Fórmula base: heroStat / primaryRequirement
    ///   +5% por cada stat secundario que supera el umbral secundario
    ///   +15% si tiene Specialist y el PrimaryStat de la misión es el stat dominante del héroe
    /// </summary>
    public float CalculateSuccessChance(MissionInstance mission)
    {
        int heroStatValue = GetStatValue(mission.Template.PrimaryStat);
        int primaryReq    = mission.GetEffectivePrimaryReq();

        float chance = primaryReq > 0 ? (float)heroStatValue / primaryReq : 1f;

        // Bonus por stats secundarios
        if (mission.Template.SecondaryStats != null)
        {
            int secondaryReq = mission.GetEffectiveSecondaryReq();
            foreach (HeroStat secondary in mission.Template.SecondaryStats)
            {
                if (GetStatValue(secondary) >= secondaryReq)
                    chance += 0.05f;
            }
        }

        // Specialist: +15% si el PrimaryStat de la misión coincide con el stat dominante
        if (Traits.Contains(HeroTrait.Specialist) && mission.Template.PrimaryStat == GetDominantStat())
            chance += 0.15f;

        return Mathf.Clamp01(chance);
    }

    /// <summary>Devuelve el stat más alto del héroe (para el cálculo de Specialist).</summary>
    public HeroStat GetDominantStat()
    {
        if (Strength >= Dexterity && Strength >= Intelligence && Strength >= Charisma)
            return HeroStat.Strength;
        if (Dexterity >= Intelligence && Dexterity >= Charisma)
            return HeroStat.Dexterity;
        if (Intelligence >= Charisma)
            return HeroStat.Intelligence;
        return HeroStat.Charisma;
    }

    // ── Helpers privados ───────────────────────────────────────────────────

    private int GetStatValue(HeroStat stat)
    {
        return stat switch
        {
            HeroStat.Strength     => Strength,
            HeroStat.Dexterity    => Dexterity,
            HeroStat.Intelligence => Intelligence,
            HeroStat.Charisma     => Charisma,
            _                     => 0,
        };
    }
}
