using UnityEngine;

/// <summary>
/// Buff comprable en la tienda inter-día.
/// Puede ser permanente (persiste toda la partida) o de efecto puntual.
/// </summary>
[CreateAssetMenu(menuName = "Questline/Buff Data")]
public class BuffData : ScriptableObject
{
    [Header("Identidad")]
    public string BuffName;
    [TextArea] public string Description;
    public Sprite Icon;

    [Header("Economía")]
    public int Cost; // En monedas

    [Header("Tipo y duración")]
    public BuffType Type;
    /// <summary>
    /// true  → el efecto dura toda la partida (ej: StatBoost).
    /// false → efecto de un solo uso que se activa al comprarlo (ej: Reshuffle, HintReveal).
    /// </summary>
    public bool IsPermanent;

    [Header("Parámetros por tipo")]
    // BuffType.HeroStatBoost
    public HeroStat AffectedStat;
    public int StatBoostAmount;        // +N al stat mientras dure la partida

    // BuffType.MissionHintReveal
    public int HintsToReveal;          // Cuántas pistas revela al activarse

    // BuffType.PatienceBoost
    [Range(0f, 2f)]
    public float PatienceBoostPercent; // 0.5 = +50 % de tiempo de paciencia

    // BuffType.FailProtection
    public int FailProtectionCount;    // Absorbe N muertes de héroe
}
