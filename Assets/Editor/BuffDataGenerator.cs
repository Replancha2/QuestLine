using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool that generates BuffData and ConsumableData ScriptableObjects.
/// Menu: Questline → Generate Buff & Consumable Assets
///
/// Idempotent: does not overwrite existing assets.
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
            "Strength Boost",
            "Grants +3 Strength to all hired heroes.",
            cost: 5, type: BuffType.HeroStatBoost, isPermanent: true,
            affectedStat: HeroStat.Strength, statBoost: 3);

        created += TryCreateBuff(ref skipped, "Buff_DexBoost",
            "Dexterity Boost",
            "Grants +3 Dexterity to all hired heroes.",
            cost: 5, type: BuffType.HeroStatBoost, isPermanent: true,
            affectedStat: HeroStat.Dexterity, statBoost: 3);

        created += TryCreateBuff(ref skipped, "Buff_IntBoost",
            "Intelligence Boost",
            "Grants +3 Intelligence to all hired heroes.",
            cost: 5, type: BuffType.HeroStatBoost, isPermanent: true,
            affectedStat: HeroStat.Intelligence, statBoost: 3);

        created += TryCreateBuff(ref skipped, "Buff_ChaBoost",
            "Charisma Boost",
            "Grants +3 Charisma to all hired heroes.",
            cost: 5, type: BuffType.HeroStatBoost, isPermanent: true,
            affectedStat: HeroStat.Charisma, statBoost: 3);

        created += TryCreateBuff(ref skipped, "Buff_Patience",
            "Infinite Patience",
            "Heroes wait 50% longer before leaving.",
            cost: 8, type: BuffType.PatienceBoost, isPermanent: true,
            patienceBoost: 0.5f);

        created += TryCreateBuff(ref skipped, "Buff_HintReveal",
            "Divination Magic",
            "Reveals the primary stat of a random mission in the current hand.",
            cost: 6, type: BuffType.MissionHintReveal, isPermanent: false,
            hintsToReveal: 1);

        created += TryCreateBuff(ref skipped, "Buff_Reshuffle",
            "Reshuffle",
            "Discard the current hand and draw a new set of missions from the day's pool.",
            cost: 3, type: BuffType.MissionReshuffle, isPermanent: false);

        created += TryCreateBuff(ref skipped, "Buff_FailSave",
            "Protective Amulet",
            "Absorbs 1 hero death: if a hero fails their mission, it does not count as a lost life.",
            cost: 12, type: BuffType.FailProtection, isPermanent: true,
            failProtection: 1);

        created += TryCreateBuff(ref skipped, "Buff_MegaBoost",
            "Miraculous Tonic",
            "Grants +10 to a random stat of all hired heroes.",
            cost: 20, type: BuffType.HeroStatBoost, isPermanent: true,
            // AffectedStat and StatBoostAmount will be randomized in ShopManager when
            // generating the daily offer. Using Strength as placeholder here.
            affectedStat: HeroStat.Strength, statBoost: 10);

        // ── Consumables ──────────────────────────────────────────────────────

        created += TryCreateConsumable(ref skipped, "Consumable_StatRevealHand",
            "Overview",
            "Reveals the primary stat of ALL mission cards in the current hand. Single use.",
            cost: 8, type: ConsumableType.StatRevealHand);

        created += TryCreateConsumable(ref skipped, "Consumable_FullStatReveal",
            "Deep Reading",
            "Reveals ALL requirements (primary and secondary) of ONE mission card of your choice. Single use.",
            cost: 15, type: ConsumableType.FullStatReveal);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[BuffDataGenerator] Done — {created} assets created, {skipped} skipped (already existed).");
        EditorUtility.DisplayDialog(
            "Buff & Consumable Assets",
            $"Generation complete.\n\n✅ Created: {created}\n⏭ Skipped: {skipped}",
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
