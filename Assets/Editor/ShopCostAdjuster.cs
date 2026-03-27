using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to increase shop item costs by a multiplier.
/// Run this from the Unity Editor menu: Tools > Increase Shop Costs
/// </summary>
public class ShopCostAdjuster : EditorWindow
{
    private float multiplier = 3f; // 300% increase

    [MenuItem("Questline/Increase Shop Costs")]
    static void Init()
    {
        ShopCostAdjuster window = (ShopCostAdjuster)EditorWindow.GetWindow(typeof(ShopCostAdjuster));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Increase Shop Costs", EditorStyles.boldLabel);
        multiplier = EditorGUILayout.FloatField("Multiplier (e.g., 3 for 300%)", multiplier);

        if (GUILayout.Button("Apply to Buffs"))
        {
            ApplyToBuffs(multiplier);
        }

        if (GUILayout.Button("Apply to Consumables"))
        {
            ApplyToConsumables(multiplier);
        }

        if (GUILayout.Button("Apply to All"))
        {
            ApplyToBuffs(multiplier);
            ApplyToConsumables(multiplier);
        }
    }

    private void ApplyToBuffs(float mult)
    {
        string[] guids = AssetDatabase.FindAssets("t:BuffData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BuffData buff = AssetDatabase.LoadAssetAtPath<BuffData>(path);
            if (buff != null)
            {
                buff.Cost = Mathf.RoundToInt(buff.Cost * mult);
                EditorUtility.SetDirty(buff);
                Debug.Log($"Updated {buff.BuffName} cost to {buff.Cost}");
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void ApplyToConsumables(float mult)
    {
        string[] guids = AssetDatabase.FindAssets("t:ConsumableData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ConsumableData consumable = AssetDatabase.LoadAssetAtPath<ConsumableData>(path);
            if (consumable != null)
            {
                consumable.Cost = Mathf.RoundToInt(consumable.Cost * mult);
                EditorUtility.SetDirty(consumable);
                Debug.Log($"Updated {consumable.Name} cost to {consumable.Cost}");
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}