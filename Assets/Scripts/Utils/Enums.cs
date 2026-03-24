// ============================================================
// Enums.cs — Enumeraciones globales del juego Questline.
// ============================================================

/// <summary>Estadística de héroe / palo de misión.</summary>
public enum HeroStat { Strength, Dexterity, Intelligence, Charisma }

/// <summary>Rareza del héroe — afecta las varianzas de generación de stats.</summary>
public enum HeroRarity { Common, Rare, Epic }

/// <summary>
/// Rasgos procedurales asignados al spawnear un héroe (2-3 por héroe, sin repetición).
/// </summary>
public enum HeroTrait
{
    Greedy,       // Solo acepta misiones con CoinsReward >= 6
    Specialist,   // +15% chance si el PrimaryStat de la misión es el stat dominante del héroe
    Reckless,     // Acepta cualquier misión; si éxito con chance < 30% → CoinsEarned × 2
    Distrustful,  // 2 de sus 4 stats se ocultan en la UI (elegidos al azar al generar)
    Demanding,    // Rechaza misiones de Rank R1–R3 (dificultad baja)
    Impulsive,    // Patience drena 2× más rápido; CoinsEarned × 2 en cualquier misión
}

/// <summary>
/// Rango de misión (sistema naipe inglés, 1–12).
/// A mayor rango: mayores requisitos y mayor Fama/coins.
/// Un héroe con stat promedio puede superar misiones hasta ~Rango 5-6.
/// </summary>
public enum MissionRank
{
    R1  = 1,
    R2  = 2,
    R3  = 3,
    R4  = 4,
    R5  = 5,
    R6  = 6,
    R7  = 7,
    R8  = 8,
    R9  = 9,
    R10 = 10,
    R11 = 11,
    R12 = 12,
}

/// <summary>Tipos de buff comprables en la tienda inter-día.</summary>
public enum BuffType
{
    HeroStatBoost,
    MissionHintReveal,
    MissionReshuffle,
    PatienceBoost,
    FailProtection,
}

/// <summary>Tipos de evento diario aleatorio.</summary>
public enum DailyEventType
{
    StatBuff,
    StatDebuff,
    SpawnSpeedBuff,
    SpawnSpeedDebuff,
    FameBoost,
    FameDebuff,
    SlotReduction,
}
