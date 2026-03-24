using UnityEngine;

/// <summary>
/// ScriptableObject con curvas de progresión por día.
/// Permite ajustar el balance del juego visualmente desde el Inspector de Unity
/// sin necesidad de recompilar.
///
/// Crear: Assets → Create → Questline → Day Config
/// Recomendado: guardar en Assets/Data/DayConfig.asset
///
/// Curvas de referencia:
///   FameThreshold : Día 1→50,  Día 5→150, Día 10→500
///   MinRank       : Día 1→1,   Día 5→4,   Día 10→8
///   MaxRank       : Día 1→4,   Día 5→9,   Día 10→12
///   SpawnInterval : Día 1→15s, Día 5→10s, Día 10→6s
/// </summary>
[CreateAssetMenu(menuName = "Questline/Day Config")]
public class DayConfig : ScriptableObject
{
    [Header("Umbral de Fama para terminar el día")]
    [Tooltip("X = DayNumber, Y = Fama requerida. Día 1→50, Día 5→150, Día 10→500")]
    public AnimationCurve FameThresholdCurve = AnimationCurve.Linear(1, 50, 10, 500);

    [Header("Rango de misiones disponibles")]
    [Tooltip("X = DayNumber, Y = rango mínimo de misiones en el pool")]
    public AnimationCurve MinRankCurve = AnimationCurve.Linear(1, 1, 10, 8);

    [Tooltip("X = DayNumber, Y = rango máximo de misiones en el pool")]
    public AnimationCurve MaxRankCurve = AnimationCurve.Linear(1, 4, 10, 12);

    [Header("Velocidad de spawn de héroes")]
    [Tooltip("X = DayNumber, Y = segundos entre spawns. A mayor día → menor intervalo.")]
    public AnimationCurve SpawnIntervalCurve = AnimationCurve.Linear(1, 15, 10, 6);

    // ── Getters ──────────────────────────────────────────────────────────────

    /// <summary>Fama necesaria para completar el día indicado.</summary>
    public int GetFameThreshold(int day)  => Mathf.RoundToInt(FameThresholdCurve.Evaluate(day));

    /// <summary>Rango mínimo de misiones que aparecen ese día.</summary>
    public int GetMinRank(int day)        => Mathf.Clamp(Mathf.RoundToInt(MinRankCurve.Evaluate(day)), 1, 12);

    /// <summary>Rango máximo de misiones que aparecen ese día.</summary>
    public int GetMaxRank(int day)        => Mathf.Clamp(Mathf.RoundToInt(MaxRankCurve.Evaluate(day)), 1, 12);

    /// <summary>Segundos entre spawns de héroe para el día indicado.</summary>
    public float GetSpawnInterval(int day) => Mathf.Max(1f, SpawnIntervalCurve.Evaluate(day));
}
