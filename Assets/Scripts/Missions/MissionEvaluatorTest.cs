using System.Collections;
using UnityEngine;

/// <summary>
/// MonoBehaviour de prueba manual para MissionEvaluator.
/// Adjuntar a cualquier GameObject en una escena de test y ejecutar en Play Mode.
/// Los resultados se muestran en la consola de Unity.
///
/// NOTA: Este script es solo para QA manual — eliminar antes del build final.
/// </summary>
public class MissionEvaluatorTest : MonoBehaviour
{
    [Header("Iteraciones por caso de prueba")]
    [SerializeField] private int _iterations = 200;

    private IEnumerator Start()
    {
        // Esperar un frame para que los singletons (BuffManager, etc.) estén listos
        yield return null;

        Debug.Log("=== MissionEvaluatorTest BEGIN ===");

        RunCase_HighStatVsLowReq();
        RunCase_LowStatVsHighReq();
        RunCase_SpecialistTrait();
        RunCase_FailProtection();
        RunCase_RecklessDoubleCoins();
        RunCase_ImpulsiveDoubleCoins();
        RunCase_GreedyReject();
        RunCase_DemandingReject();

        Debug.Log("=== MissionEvaluatorTest END ===");
    }

    // ── Caso 1: Héroe fuerte vs requisito bajo → alta probabilidad ─────────
    private void RunCase_HighStatVsLowReq()
    {
        // Rank R4, Día 1 → req = 4*(1.5+0.2) = 6.8 ≈ 7
        // Héroe con Strength 10 → chance = 10/7 → clamp 1.0 → 100%
        var mission = BuildMission(MissionRank.R4, HeroStat.Strength, day: 1);
        var hero    = BuildHero(strength: 10);

        int successes = CountSuccesses(hero, mission);
        float rate = (float)successes / _iterations;
        Debug.Log($"[Caso 1] Alta stat (10) vs req bajo (R4/Día1≈7) | Éxitos: {successes}/{_iterations} ({rate:P0}) — esperado ~100%");
    }

    // ── Caso 2: Héroe débil vs requisito alto → baja probabilidad ──────────
    private void RunCase_LowStatVsHighReq()
    {
        // Rank R4, Día 1 → req ≈ 7; Héroe con Strength 3 → chance = 3/7 ≈ 43%
        var mission = BuildMission(MissionRank.R4, HeroStat.Strength, day: 1);
        var hero    = BuildHero(strength: 3);

        int successes = CountSuccesses(hero, mission);
        float rate = (float)successes / _iterations;
        Debug.Log($"[Caso 2] Baja stat (3) vs req alto (R4/Día1≈7) | Éxitos: {successes}/{_iterations} ({rate:P0}) — esperado ~43%");
    }

    // ── Caso 3: Rasgo Specialist +15% ────────────────────────────────────
    private void RunCase_SpecialistTrait()
    {
        // Strength 5, req ≈ 7 → base chance ≈ 71%; +15% Specialist → ~86%
        var mission = BuildMission(MissionRank.R4, HeroStat.Strength, day: 1);
        var hero    = BuildHero(strength: 5);
        hero.Traits.Clear();
        hero.Traits.Add(HeroTrait.Specialist);

        int successes = CountSuccesses(hero, mission);
        float rate = (float)successes / _iterations;
        Debug.Log($"[Caso 3] Specialist — stat dom. = Strength, PrimaryStat = Strength | Éxitos: {successes}/{_iterations} ({rate:P0}) — esperado ~86%");
    }

    // ── Caso 4: FailProtection absorbe muerte ─────────────────────────────
    private void RunCase_FailProtection()
    {
        if (BuffManager.Instance == null)
        {
            Debug.LogWarning("[Caso 4] BuffManager.Instance es null — omitiendo prueba de FailProtection.");
            return;
        }

        // Stat muy bajo → casi seguro que falla; el escudo debe absorber la muerte
        var mission = BuildMission(MissionRank.R10, HeroStat.Strength, day: 1);
        var hero    = BuildHero(strength: 1);

        // Dar un escudo
        BuffManager.Instance.ConsumeFailProtection(); // asegurar estado limpio
        // Añadir escudo directamente vía campo privado no es posible aquí; usamos ApplyBuff con un BuffData ad-hoc

        // Solo podemos verificar el campo ProtectionUsed cuando el héroe habría muerto
        EventBus.Clear();
        bool heroDied = false;
        EventBus.Subscribe<OnHeroDied>(_ => heroDied = true);

        // Sin escudo → el héroe debe morir en algún intento
        var result = MissionEvaluator.Evaluate(hero, mission);
        if (!result.Success && !result.ProtectionUsed)
        {
            Debug.Log($"[Caso 4] Sin escudo: fallo → heroDied={heroDied} — esperado true (si tuvo mala suerte)");
        }

        EventBus.Unsubscribe<OnHeroDied>(_ => heroDied = true);
        Debug.Log("[Caso 4] FailProtection — verificar manualmente con BuffData en Inspector.");
    }

    // ── Caso 5: Reckless dobla coins con < 30% chance ─────────────────────
    private void RunCase_RecklessDoubleCoins()
    {
        // Stat 1 vs req ≈ 7 → chance ≈ 14% (< 30%) → si éxito, coins × 2
        var mission = BuildMission(MissionRank.R4, HeroStat.Strength, day: 1, coinsReward: 5);
        var hero    = BuildHero(strength: 1);
        hero.Traits.Clear();
        hero.Traits.Add(HeroTrait.Reckless);

        int doubleCoinsCount = 0;
        for (int i = 0; i < _iterations; i++)
        {
            EventBus.Clear();
            var r = MissionEvaluator.Evaluate(hero, mission);
            if (r.Success && r.CoinsEarned == 10) doubleCoinsCount++;
            // Reset estado misión
            mission.IsCompleted = false;
            mission.IsFailed    = false;
            hero.IsDead         = false;
        }
        Debug.Log($"[Caso 5] Reckless coins×2 con chance<30% | Doble-coins: {doubleCoinsCount}/{_iterations} — esperado ≈14% de {_iterations}");
    }

    // ── Caso 6: Impulsive siempre dobla coins si éxito ──────────────────
    private void RunCase_ImpulsiveDoubleCoins()
    {
        // Stat 10 vs req ≈ 7 → casi siempre éxito; coins base = 5 → debe ser 10
        var mission = BuildMission(MissionRank.R4, HeroStat.Strength, day: 1, coinsReward: 5);
        var hero    = BuildHero(strength: 10);
        hero.Traits.Clear();
        hero.Traits.Add(HeroTrait.Impulsive);

        int doubleCoinsCount = 0;
        for (int i = 0; i < _iterations; i++)
        {
            EventBus.Clear();
            var r = MissionEvaluator.Evaluate(hero, mission);
            if (r.Success && r.CoinsEarned == 10) doubleCoinsCount++;
            mission.IsCompleted = false;
            mission.IsFailed    = false;
            hero.IsDead         = false;
        }
        Debug.Log($"[Caso 6] Impulsive coins×2 (siempre) | Doble-coins en éxitos: {doubleCoinsCount}/{_iterations} — esperado ≈100%");
    }

    // ── Caso 7: Greedy rechaza misiones con coins < 6 ────────────────────
    private void RunCase_GreedyReject()
    {
        var mission = BuildMission(MissionRank.R4, HeroStat.Strength, day: 1, coinsReward: 4);
        var hero    = BuildHero(strength: 10);
        hero.Traits.Clear();
        hero.Traits.Add(HeroTrait.Greedy);

        bool accepts = hero.WillAcceptMission(mission);
        Debug.Log($"[Caso 7] Greedy rechaza coins=4 | WillAccept={accepts} — esperado False");

        var mission2 = BuildMission(MissionRank.R4, HeroStat.Strength, day: 1, coinsReward: 6);
        bool accepts2 = hero.WillAcceptMission(mission2);
        Debug.Log($"[Caso 7] Greedy acepta coins=6 | WillAccept={accepts2} — esperado True");
    }

    // ── Caso 8: Demanding rechaza misiones de Rank <= R3 ──────────────────
    private void RunCase_DemandingReject()
    {
        var missionLow  = BuildMission(MissionRank.R3, HeroStat.Strength, day: 1);
        var missionHigh = BuildMission(MissionRank.R4, HeroStat.Strength, day: 1);
        var hero = BuildHero(strength: 10);
        hero.Traits.Clear();
        hero.Traits.Add(HeroTrait.Demanding);

        bool acceptsLow  = hero.WillAcceptMission(missionLow);
        bool acceptsHigh = hero.WillAcceptMission(missionHigh);
        Debug.Log($"[Caso 8] Demanding rechaza R3={!acceptsLow} acepta R4={acceptsHigh} — esperado True/True");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private int CountSuccesses(HeroInstance hero, MissionInstance baseMission)
    {
        int successes = 0;
        for (int i = 0; i < _iterations; i++)
        {
            EventBus.Clear();
            // Clonar la instancia de misión para no contaminar el estado
            var mission = MissionInstance.FromData(baseMission.Template, baseMission.DayNumber);
            hero.IsDead = false;
            var result = MissionEvaluator.Evaluate(hero, mission);
            if (result.Success) successes++;
        }
        return successes;
    }

    private static MissionInstance BuildMission(MissionRank rank, HeroStat primary, int day,
                                                  int coinsReward = 10, int fameReward = 20)
    {
        var data = ScriptableObject.CreateInstance<MissionData>();
        data.Rank         = rank;
        data.PrimaryStat  = primary;
        data.CoinsReward  = coinsReward;
        data.FameReward   = fameReward;
        return MissionInstance.FromData(data, day);
    }

    private static HeroInstance BuildHero(int strength = 5, int dexterity = 5,
                                           int intelligence = 5, int charisma = 5)
    {
        return new HeroInstance
        {
            Name          = "TestHero",
            Strength      = strength,
            Dexterity     = dexterity,
            Intelligence  = intelligence,
            Charisma      = charisma,
            IsWaiting     = true,
        };
    }
}
