/// <summary>Tipos de consumibles disponibles en el shop.</summary>
public enum ConsumableType
{
    None = 0,
    /// <summary>Revela el PrimaryStat de UNA carta de misión elegida.</summary>
    StatRevealCard,
    /// <summary>Revela el PrimaryStat de TODAS las cartas en mano.</summary>
    StatRevealHand,
    /// <summary>Revela todos los stats de una carta de misión elegida.</summary>
    FullStatReveal,
    /// <summary>Baraja el pool restante y descarta la mano para robar nueva (ver ShopManager.UseReshuffle).</summary>
    Reshuffle,
}
