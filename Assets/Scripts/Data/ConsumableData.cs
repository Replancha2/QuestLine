using UnityEngine;

/// <summary>
/// Consumible de un solo uso comprable en la tienda.
/// Al usarse se consume inmediatamente del inventario del jugador.
/// </summary>
[CreateAssetMenu(menuName = "Questline/Consumable")]
public class ConsumableData : ScriptableObject
{
    [Header("Identidad")]
    public string Name;
    [TextArea] public string Description;
    public Sprite Icon;

    [Header("Economía")]
    public int Cost; // En monedas

    [Header("Tipo")]
    public ConsumableType Type;
    // StatRevealHand : revela el PrimaryStat de TODAS las cartas en mano
    // FullStatReveal : revela todos los stats de UNA carta elegida por el jugador
}
