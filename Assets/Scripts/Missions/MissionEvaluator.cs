using UnityEngine;

/// <summary>
/// Resultado completo de la evaluación de una misión.
/// </summary>
public class MissionResult
{
    public bool Success;
    public bool ProtectionUsed;   // true si un escudo de FailProtection absorbió la muerte
    public float SuccessChance;   // Probabilidad final calculada (0–1), para mostrar en UI post-misión
    public int CoinsEarned;
    public int FameEarned;        // Ya incluye el multiplicador del evento del día
    public int XPEarned;
    public HeroInstance Hero;
    public MissionInstance Mission;
}

/// <summary>
/// Lógica central de resolución de misiones.
/// Dado un héroe y una misión calcula si hay éxito o fallo, aplica rasgos,
/// consume buffs relevantes y publica los eventos correspondientes en el EventBus.
///
/// Dependencias:
///   - BuffManager.Instance   (bonos de stats y FailProtection)
///   - DailyEventManager      (multiplicador de Fama del día — stub hasta Tarea 3.4)
/// </summary>
public static class MissionEvaluator
{
    // XP fija por misión completada (ajustar en balance)
    private const int BaseXPReward = 10;

    /// <summary>
    /// Evalúa la misión, publica los eventos correspondientes y devuelve el resultado.
    ///
    /// Flujo:
    ///   1. Calcular effectiveStat (hero stat + buff bonus)
    ///   2. Calcular baseChance = effectiveStat / primaryReq
    ///   3. bonusChance por stats secundarios (+5 % c/u que supere su umbral)
    ///   4. bonusChance por rasgo Specialist (+15 % si el PrimaryStat es el stat dominante)
    ///   5. finalChance = Clamp01(baseChance + bonusChance)
    ///   6. Tirar Random.value — success si <= finalChance
    ///   7. Si falla: comprobar FailProtection
    ///   8. Calcular recompensas (rasgos Impulsive y Reckless modifican coins)
    ///   9. Publicar OnMissionCompleted / OnMissionFailed / OnHeroDied
    /// </summary>
    public static MissionResult Evaluate(HeroInstance hero, MissionInstance mission)
    {
        // ── 1. Stat efectivo del héroe (incluye bonos del BuffManager) ─────────
        HeroStat primaryStat = mission.Template.PrimaryStat;
        int heroStatValue = GetStatValue(hero, primaryStat);

        if (BuffManager.Instance != null)
            heroStatValue += BuffManager.Instance.GetActiveStatBonus(primaryStat);

        // ── 2. Requisito primario efectivo y chance base ───────────────────────
        int primaryReq = mission.GetEffectivePrimaryReq();
        float baseChance = primaryReq > 0 ? (float)heroStatValue / primaryReq : 1f;

        // ── 3. Bonus por stats secundarios (+5 % cada uno que supere el umbral) ─
        float bonusChance = 0f;
        if (mission.Template.SecondaryStats != null)
        {
            int secondaryReq = mission.GetEffectiveSecondaryReq();
            foreach (HeroStat secondary in mission.Template.SecondaryStats)
            {
                int secValue = GetStatValue(hero, secondary);
                if (BuffManager.Instance != null)
                    secValue += BuffManager.Instance.GetActiveStatBonus(secondary);

                if (secValue >= secondaryReq)
                    bonusChance += 0.05f;
            }
        }

        // ── 4. Rasgo Specialist (+15 % si el PrimaryStat de la misión coincide ──
        //       con el stat dominante del héroe)
        if (hero.Traits.Contains(HeroTrait.Specialist) &&
            mission.Template.PrimaryStat == hero.GetDominantStat())
        {
            bonusChance += 0.15f;
        }

        // ── 5. Chance final clampada ───────────────────────────────────────────
        float finalChance = Mathf.Clamp01(baseChance + bonusChance);

        // ── 6. Tirada de dado ─────────────────────────────────────────────────
        bool success = Random.value <= finalChance;

        // ── 7. FailProtection si falló ─────────────────────────────────────────
        bool protectionUsed = false;
        if (!success && BuffManager.Instance != null && BuffManager.Instance.HasFailProtection())
        {
            BuffManager.Instance.ConsumeFailProtection();
            protectionUsed = true;
            // El escudo absorbe la muerte del héroe; la misión sigue fallida
            // (el héroe sobrevive pero la misión no tiene éxito)
        }

        // ── 8. Recompensas ────────────────────────────────────────────────────
        float fameModifier = DailyEventManager.GetFameModifier();
        int fameEarned = success ? Mathf.RoundToInt(mission.Template.FameReward * fameModifier) : 0;
        int xpEarned   = success ? BaseXPReward : 0;

        // Coins base + bonus por nivel del héroe (+1 moneda por cada 10 niveles)
        int levelBonus  = hero.Level / 10;
        int coinsEarned = success ? (mission.Template.CoinsReward + levelBonus) : 0;

        // Boss: 2× coins al completar con éxito
        if (success && mission.Template.IsBoss)
            coinsEarned *= 2;

        // Rasgo Impulsive: dobla coins (independientemente de si tiene éxito o no,
        // pero solo hay coins en caso de éxito — se aplica sobre coinsEarned)
        if (success && hero.Traits.Contains(HeroTrait.Impulsive))
            coinsEarned *= 2;

        // Rasgo Reckless: dobla coins si tuvo éxito con < 30 % de probabilidad
        if (success && hero.Traits.Contains(HeroTrait.Reckless) && finalChance < 0.30f)
            coinsEarned *= 2;

        // Progressive scaling: coins increase with day number to make later days more rewarding,
        // but start lower to nerf early game
        int day = GameManager.Instance != null ? GameManager.Instance.DayNumber : 1;
        float dayMultiplier = 0.2f + (day - 1) * 0.1f; // Day 1: 0.2x, Day 2: 0.3x, Day 10: 1.0x
        coinsEarned = Mathf.RoundToInt(coinsEarned * dayMultiplier);

        // ── 9. Actualizar estado de la instancia ──────────────────────────────
        if (success)
            mission.IsCompleted = true;
        else
            mission.IsFailed = true;

        // ── 10. Construir resultado ───────────────────────────────────────────
        var result = new MissionResult
        {
            Success         = success,
            ProtectionUsed  = protectionUsed,
            SuccessChance   = finalChance,
            CoinsEarned     = coinsEarned,
            FameEarned      = fameEarned,
            XPEarned        = xpEarned,
            Hero            = hero,
            Mission         = mission,
        };

        // ── 11. Publicar eventos y actualizar progresión ──────────────────────
        if (success)
        {
            hero.GainXP(xpEarned);

            EventBus.Publish(new OnMissionCompleted
            {
                Mission     = mission.Template,
                Hero        = hero,
                CoinsEarned = coinsEarned,
                FameEarned  = fameEarned,
            });
        }
        else
        {
            EventBus.Publish(new OnMissionFailed
            {
                Mission = mission.Template,
                Hero    = hero,
            });

            // Solo muere si no hay escudo activo que lo proteja
            if (!protectionUsed)
            {
                hero.IsDead = true;
                EventBus.Publish(new OnHeroDied { Hero = hero });
            }
        }

        return result;
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static int GetStatValue(HeroInstance hero, HeroStat stat)
    {
        return stat switch
        {
            HeroStat.Strength     => hero.Strength,
            HeroStat.Dexterity    => hero.Dexterity,
            HeroStat.Intelligence => hero.Intelligence,
            HeroStat.Charisma     => hero.Charisma,
            _                     => 0,
        };
    }
}
