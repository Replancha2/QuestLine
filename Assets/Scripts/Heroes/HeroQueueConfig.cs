using UnityEngine;

/// <summary>
/// ScriptableObject de configuración para la cola de héroes.
/// Asignar en el Inspector de HeroSpawner / HeroQueue.
/// </summary>
[CreateAssetMenu(menuName = "Questline/Hero Queue Config")]
public class HeroQueueConfig : ScriptableObject
{
    [Header("Timing de spawn")]
    [Tooltip("Segundos entre apariciones de héroes (base).")]
    public float SpawnIntervalBase = 15f;
    [Tooltip("Intervalo mínimo de spawn (en días avanzados).")]
    public float SpawnIntervalMin = 5f;

    [Header("Slots")]
    [Tooltip("Máximo de héroes visibles simultáneamente.")]
    public int MaxSimultaneousHeroes = 4;

    [Header("Templates de héroes disponibles")]
    [Tooltip("Lista de HeroData assets del proyecto. Uno se elige al azar en cada spawn.")]
    public HeroData[] HeroTemplates;
}
