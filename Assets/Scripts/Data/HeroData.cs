using UnityEngine;

/// <summary>
/// ScriptableObject plantilla de héroe. Define los rangos de stats y la personalidad
/// base que usa HeroInstance.Generate() para crear héroes procedurales en runtime.
/// </summary>
[CreateAssetMenu(menuName = "Questline/Hero Data")]
public class HeroData : ScriptableObject
{
    [Header("Identidad")]
    public string HeroName;
    public Sprite Portrait;
    public HeroRarity Rarity;

    [Header("Rangos de stats para generación aleatoria")]
    public Vector2Int StrengthRange;        // ej: (7, 12) para un Knight
    public Vector2Int DexterityRange;
    public Vector2Int IntelligenceRange;
    public Vector2Int CharismaRange;

    [Header("Personalidad — afecta el timer de paciencia")]
    [Tooltip("Multiplicador sobre BasePatience. 0.5=muy impaciente, 2.0=muy paciente.")]
    public float PatienceMultiplier = 1f;
    [Tooltip("Segundos base de espera antes de irse sin misión.")]
    public float BasePatience = 30f;

    [Header("Descripción de sabor")]
    [TextArea(2, 4)]
    public string FlavorText;
}
