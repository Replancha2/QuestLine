// ============================================================
// GameEvents.cs — Catálogo centralizado de todos los eventos del juego.
//
// NOTA: Los tipos HeroInstance, MissionData, MissionInstance, BuffData,
// DailyEventData y ConsumableType son stubs temporales definidos en sus
// respectivos archivos (ver carpetas Data/ y Economy/).
// Se irán implementando en las tareas 1.1, 1.2, 1.3, 3.2 y 3.4.
// ============================================================

// ── HÉROES ─────────────────────────────────────────────────────────────────

/// <summary>Un nuevo héroe entra a la guild y ocupa un slot.</summary>
public class OnHeroArrived
{
    public HeroInstance Hero;
}

/// <summary>
/// Un héroe abandona la guild. WasAngry=true significa que se fue sin recibir
/// misión (timer expirado) → el GameManager descuenta 1 vida.
/// </summary>
public class OnHeroLeft
{
    public HeroInstance Hero;
    public bool WasAngry; // true → -1 vida
}

/// <summary>Un héroe murió en misión.</summary>
public class OnHeroDied
{
    public HeroInstance Hero;
}

// ── VIDAS ──────────────────────────────────────────────────────────────────

/// <summary>Se pierde una vida (héroe se va sin misión).</summary>
public class OnLifeLost
{
    public int LivesRemaining;
}

// ── MISIONES ───────────────────────────────────────────────────────────────

/// <summary>El jugador asignó una misión a un héroe.</summary>
public class OnMissionAssigned
{
    public MissionData Mission;
    public HeroInstance Hero;
}

/// <summary>Una misión se completó con éxito.</summary>
public class OnMissionCompleted
{
    public MissionData Mission;
    public HeroInstance Hero;
    public int CoinsEarned;
    public int FameEarned;
}

/// <summary>Una misión falló.</summary>
public class OnMissionFailed
{
    public MissionData Mission;
    public HeroInstance Hero;
}

/// <summary>El jugador descartó manualmente una carta de misión.</summary>
public class OnMissionDiscarded
{
    public MissionInstance Mission;
}

/// <summary>El mazo de misiones del día se ha rebarajado (consumible).</summary>
public class OnMissionsReshuffled { }

// ── ECONOMÍA ───────────────────────────────────────────────────────────────

/// <summary>Se ganaron monedas.</summary>
public class OnCoinEarned
{
    public int Amount;
}

/// <summary>Se gastaron monedas.</summary>
public class OnCoinSpent
{
    public int Amount;
}

// ── FAMA Y PROGRESIÓN ──────────────────────────────────────────────────────

/// <summary>Se ganó fama. Incluye el total acumulado y el umbral del día actual.</summary>
public class OnFameEarned
{
    public int Amount;
    public int TotalFame;
    public int DayThreshold;
}

/// <summary>La fama del día superó el umbral → el día termina exitosamente.</summary>
public class OnDayThresholdReached
{
    public int DayNumber;
}

// ── DÍAS ───────────────────────────────────────────────────────────────────

/// <summary>Inicio de un nuevo día.</summary>
public class OnDayStart
{
    public int DayNumber;
}

/// <summary>Fin del día actual.</summary>
public class OnDayEnd
{
    public int DayNumber;
}

// ── GAME OVER ──────────────────────────────────────────────────────────────

/// <summary>El jugador perdió las 3 vidas del día → Game Over.</summary>
public class OnGameOver
{
    public int DayReached;
    public int TotalFameEarned;
}

// ── SHOP / BUFFS ───────────────────────────────────────────────────────────

/// <summary>El jugador compró un buff en el shop.</summary>
public class OnBuffPurchased
{
    public BuffData Buff;
}

/// <summary>El jugador usó un consumible (pista de misión, rebarajar, etc.).</summary>
public class OnConsumableUsed
{
    public ConsumableType Type;
    public MissionInstance TargetMission; // puede ser null si no aplica a una misión concreta
}

// ── EVENTOS DIARIOS ────────────────────────────────────────────────────────

/// <summary>Se activó el evento aleatorio del día (buff o debuff).</summary>
public class OnDailyEventActivated
{
    public DailyEventData Event;
}
