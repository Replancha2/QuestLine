using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que define una misión del catálogo (plantilla base).
/// 48 assets en total: 12 rangos × 4 palos (uno por stat).
/// Los requisitos numéricos se calculan en runtime por fórmula — no se hardcodean aquí.
/// </summary>
[CreateAssetMenu(menuName = "Questline/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Identidad")]
    public string MissionTitle;
    [TextArea(2, 5)]
    public string Description;   // Descripción ambigua para el jugador
    public Sprite MissionArt;

    [Header("Rango (sistema naipe inglés, 1–12)")]
    public MissionRank Rank;     // R1 = más fácil, R12 = más difícil
    // El rango escala automáticamente los requisitos y las recompensas.
    // Un héroe con stat promedio puede superar misiones hasta ~Rango 5–6.
    // Rangos 7–12 requieren héroes con stats altos o buffs acumulados.

    [Header("Requisitos (OCULTOS al jugador)")]
    // PrimaryStatRequirement NO se almacena aquí — se calcula en runtime:
    //   effectiveReq(rank, day) = (int)Rank * (1.5f + day * 0.2f)
    // Ejemplo: Rank 4, Día 1 → req = 4*(1.7) = 6.8 ≈ 7
    //          Rank 4, Día 5 → req = 4*(2.5) = 10
    public HeroStat   PrimaryStat;     // El palo de esta misión (Fuerza / Destreza / etc.)
    public HeroStat[] SecondaryStats;  // Stats de apoyo (opcionales, 0–2 elementos)
    // Requisito secundario = 80 % del requisito primario calculado en runtime.

    [Header("Recompensas (base fija en el asset, ajustable en balance)")]
    public int CoinsReward;   // Propina base (~Rank × 3)
    public int FameReward;    // Fama base (~Rank × 5)

    [Header("Modificadores especiales")]
    /// <summary>
    /// Si es true, las monedas ganadas se duplican al completar la misión.
    /// Pensado para misiones de alta dificultad / eventos especiales.
    /// </summary>
    public bool IsBoss;

    [Header("Pistas (desbloqueables por tienda o eventos)")]
    // Las pistas NO se revelan automáticamente.
    // Se compran en la tienda inter-día o se otorgan como recompensa de eventos.
    public MissionHint[] Hints;
}

/// <summary>
/// Pista asociada a una misión específica.
/// El jugador debe comprarla o ganarla para verla.
/// </summary>
[System.Serializable]
public class MissionHint
{
    public string   HintText;     // ej: "Requiere habilidad física" o "Fuerza++"
    public Color    HintColor;    // Verde = confirma, Naranja = precaución / parcial
    public HeroStat RelatedStat;  // Stat al que hace referencia esta pista
}
