using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramienta de editor que genera los BuffData y ConsumableData ScriptableObjects.
/// Menú: Questline → Generate Buff &amp; Consumable Assets
///
/// Idempotente: no sobreescribe assets ya existentes.
/// </summary>
public static class BuffDataGenerator
{
    private const string BUFFS_PATH       = "Assets/Data/Buffs";
    private const string CONSUMABLES_PATH = "Assets/Data/Consumables";

    [MenuItem("Questline/Generate Buff & Consumable Assets")]
    public static void GenerateAll()
    {
        EnsureFolder(BUFFS_PATH);
        EnsureFolder(CONSUMABLES_PATH);

        int created = 0;
        int skipped = 0;

        // ── Buffs ────────────────────────────────────────────────────────────

        created += TryCreateBuff(ref skipped, "Buff_StrBoost",
            "Impulso de Fuerza",
            "Otorga +3 de Fuerza a todos los héroes contratados.",
            cost: 5, type: BuffType.HeroStatBoost, isPermanent: true,
            affectedStat: HeroStat.Strength, statBoost: 3);

        created += TryCreateBuff(ref skipped, "Buff_DexBoost",
            "Impulso de Destreza",
            "Otorga +3 de Destreza a todos los héroes contratados.",
            cost: 5, type: BuffType.HeroStatBoost, isPermanent: true,
            affectedStat: HeroStat.Dexterity, statBoost: 3);

        created += TryCreateBuff(ref skipped, "Buff_IntBoost",
            "Impulso de Inteligencia",
            "Otorga +3 de Inteligencia a todos los héroes contratados.",
            cost: 5, type: BuffType.HeroStatBoost, isPermanent: true,
            affectedStat: HeroStat.Intelligence, statBoost: 3);

        created += TryCreateBuff(ref skipped, "Buff_ChaBoost",
            "Impulso de Carisma",
            "Otorga +3 de Carisma a todos los héroes contratados.",
            cost: 5, type: BuffType.HeroStatBoost, isPermanent: true,
            affectedStat: HeroStat.Charisma, statBoost: 3);

        created += TryCreateBuff(ref skipped, "Buff_Patience",
            "Paciencia Infinita",
            "Los héroes esperan un 50 % más de tiempo antes de irse.",
            cost: 8, type: BuffType.PatienceBoost, isPermanent: true,
            patienceBoost: 0.5f);

        created += TryCreateBuff(ref skipped, "Buff_HintReveal",
            "Magia de Adivinación",
            "Revela el stat primario de una misión aleatoria en la mano actual.",
            cost: 6, type: BuffType.MissionHintReveal, isPermanent: false,
            hintsToReveal: 1);

        created += TryCreateBuff(ref skipped, "Buff_Reshuffle",
            "Barajar de Nuevo",
            "Descarta la mano actual y roba un nuevo conjunto de misiones del pool del día.",
            cost: 3, type: BuffType.MissionReshuffle, isPermanent: false);

        created += TryCreateBuff(ref skipped, "Buff_FailSave",
            "Amuleto Protector",
            "Absorbe 1 muerte de héroe: si un héroe falla su misión, no cuenta como vida perdida.",
            cost: 12, type: BuffType.FailProtection, isPermanent: true,
            failProtection: 1);

        created += TryCreateBuff(ref skipped, "Buff_MegaBoost",
            "Tónico Milagroso",
            "Otorga +10 a un stat aleatorio de todos los héroes contratados.",
            cost: 20, type: BuffType.HeroStatBoost, isPermanent: true,
            // AffectedStat y StatBoostAmount se randomizarán en ShopManager al generar
            // la oferta del día. Aquí usamos Strength como placeholder.
            affectedStat: HeroStat.Strength, statBoost: 10);

        // ── Consumibles ──────────────────────────────────────────────────────

        created += TryCreateConsumable(ref skipped, "Consumable_StatRevealHand",
            "Visión de Conjunto",
            "Revela el stat primario de TODAS las cartas de misión en la mano actual. Un solo uso.",
            cost: 8, type: ConsumableType.StatRevealHand);

        created += TryCreateConsumable(ref skipped, "Consumable_FullStatReveal",
            "Lectura Profunda",
            "Revela TODOS los requisitos (primario y secundarios) de UNA carta de misión a tu elección. Un solo uso.",
            cost: 15, type: ConsumableType.FullStatReveal);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[BuffDataGenerator] Finalizado — {created} assets creados, {skipped} omitidos (ya existían).");
        EditorUtility.DisplayDialog(
            "Buff & Consumable Assets",
            $"Generación completada.\n\n✅ Creados: {created}\n⏭ Omitidos: {skipped}",
            "OK");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static int TryCreateBuff(
        ref int skipped,
        string assetName,
        string buffName,
        string description,
        int cost,
        BuffType type,
        bool isPermanent,
        HeroStat affectedStat       = HeroStat.Strength,
        int statBoost               = 0,
        int hintsToReveal           = 0,
        float patienceBoost         = 0f,
        int failProtection          = 0)
    {
        string path = $"{BUFFS_PATH}/{assetName}.asset";

        if (AssetDatabase.LoadAssetAtPath<BuffData>(path) != null)
        {
            skipped++;
            return 0;
        }

        var data = ScriptableObject.CreateInstance<BuffData>();
        data.BuffName              = buffName;
        data.Description           = description;
        data.Cost                  = cost;
        data.Type                  = type;
        data.IsPermanent           = isPermanent;
        data.AffectedStat          = affectedStat;
        data.StatBoostAmount       = statBoost;
        data.HintsToReveal         = hintsToReveal;
        data.PatienceBoostPercent  = patienceBoost;
        data.FailProtectionCount   = failProtection;

        AssetDatabase.CreateAsset(data, path);
        return 1;
    }

    private static int TryCreateConsumable(
        ref int skipped,
        string assetName,
        string itemName,
        string description,
        int cost,
        ConsumableType type)
    {
        string path = $"{CONSUMABLES_PATH}/{assetName}.asset";

        if (AssetDatabase.LoadAssetAtPath<ConsumableData>(path) != null)
        {
            skipped++;
            return 0;
        }

        var data        = ScriptableObject.CreateInstance<ConsumableData>();
        data.Name       = itemName;
        data.Description = description;
        data.Cost       = cost;
        data.Type       = type;

        AssetDatabase.CreateAsset(data, path);
        return 1;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path);
        string folder = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
