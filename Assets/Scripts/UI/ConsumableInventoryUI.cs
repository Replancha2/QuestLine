using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel de inventario visible durante el juego.
/// Muestra consumibles (usables) y buffs (permanentes o con cargas) comprados.
///
/// Setup en Inspector:
///   - _slotPrefab      : prefab con Button + hijos: IconImage (Image),
///                        NameText (TMP), CountText (TMP)
///   - _slotsContainer  : Transform padre donde se instancian los slots
///   - _allConsumables  : todos los ConsumableData SO del proyecto
///   - _allBuffs        : todos los BuffData SO del proyecto
/// </summary>
public class ConsumableInventoryUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject       _slotPrefab;
    [SerializeField] private Transform        _slotsContainer;

    [Header("Catálogos")]
    [Tooltip("Arrastra aquí todos los ConsumableData del proyecto.")]
    [SerializeField] private ConsumableData[] _allConsumables;
    [Tooltip("Arrastra aquí todos los BuffData del proyecto.")]
    [SerializeField] private BuffData[]       _allBuffs;

    private Action<OnBuffPurchased>    _onBuffPurchased;
    private Action<OnConsumableUsed>   _onConsumableUsed;
    private Action<OnMissionsReshuffled> _onReshuffled;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    private void Awake()
    {
        _onBuffPurchased  = _ => Refresh();
        _onConsumableUsed = _ => Refresh();
        _onReshuffled     = _ => Refresh();
    }

    private void Start() => Refresh();

    private void OnEnable()
    {
        EventBus.Subscribe<OnBuffPurchased>(_onBuffPurchased);
        EventBus.Subscribe<OnConsumableUsed>(_onConsumableUsed);
        EventBus.Subscribe<OnMissionsReshuffled>(_onReshuffled);
        Refresh();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnBuffPurchased>(_onBuffPurchased);
        EventBus.Unsubscribe<OnConsumableUsed>(_onConsumableUsed);
        EventBus.Unsubscribe<OnMissionsReshuffled>(_onReshuffled);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    private void Refresh()
    {
        if (_slotsContainer == null || _slotPrefab == null) return;

        foreach (Transform child in _slotsContainer)
            Destroy(child.gameObject);

        SpawnConsumableSlots();
        SpawnBuffSlots();
    }

    // ── Consumibles ───────────────────────────────────────────────────────────

    private void SpawnConsumableSlots()
    {
        if (_allConsumables == null || ConsumableManager.Instance == null) return;

        foreach (var data in _allConsumables)
        {
            if (data == null) continue;
            int count = ConsumableManager.Instance.GetCount(data.Type);
            if (count <= 0) continue;

            ConsumableType capturedType = data.Type;
            SpawnSlot(
                icon:        data.Icon,
                displayName: data.Name,
                count:       count,
                usable:      true,
                onUse:       () => UseConsumable(capturedType));
        }
    }

    private void UseConsumable(ConsumableType type)
    {
        if (ConsumableManager.Instance == null) return;

        if (type == ConsumableType.FullStatReveal)
        {
            var hand = MissionDeck.Instance != null ? MissionDeck.Instance.CurrentHand : null;
            MissionInstance target = (hand != null && hand.Count > 0) ? hand[0] : null;
            ConsumableManager.Instance.UseConsumable(type, target);
        }
        else
        {
            ConsumableManager.Instance.UseConsumable(type);
        }
    }

    // ── Buffs ─────────────────────────────────────────────────────────────────

    private void SpawnBuffSlots()
    {
        if (_allBuffs == null) return;

        foreach (var buff in _allBuffs)
        {
            if (buff == null) continue;

            int count = GetBuffCount(buff);
            if (count <= 0) continue;

            bool isUsable = buff.Type == BuffType.MissionReshuffle;
            BuffData capturedBuff = buff;

            SpawnSlot(
                icon:        buff.Icon,
                displayName: buff.BuffName,
                count:       count,
                usable:      isUsable,
                onUse:       isUsable ? () => UseBuff(capturedBuff) : (Action)null);
        }
    }

    private int GetBuffCount(BuffData buff)
    {
        switch (buff.Type)
        {
            case BuffType.MissionReshuffle:
                return ShopManager.Instance != null ? ShopManager.Instance.ReshuffleCharges : 0;

            case BuffType.FailProtection:
                return BuffManager.Instance != null ? BuffManager.Instance.FailProtectionCharges : 0;

            default:
                // Buffs permanentes: contar cuántas veces aparece este SO en la lista activa
                if (BuffManager.Instance == null) return 0;
                int n = 0;
                foreach (var b in BuffManager.Instance.PermanentBuffs)
                    if (b == buff) n++;
                return n;
        }
    }

    private void UseBuff(BuffData buff)
    {
        if (buff.Type == BuffType.MissionReshuffle)
            ShopManager.Instance?.UseReshuffle();
    }

    // ── Instanciar slot ───────────────────────────────────────────────────────

    private void SpawnSlot(Sprite icon, string displayName, int count, bool usable, Action onUse)
    {
        GameObject slot = Instantiate(_slotPrefab, _slotsContainer);

        var img = slot.GetComponentInChildren<Image>();
        if (img != null && icon != null)
            img.sprite = icon;

        foreach (var t in slot.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (t.gameObject.name == "NameText")
                t.text = displayName;
            else if (t.gameObject.name == "CountText")
                t.text = count > 1 ? $"×{count}" : "";
        }

        var btn = slot.GetComponentInChildren<Button>();
        if (btn != null)
        {
            btn.interactable = usable;
            if (usable && onUse != null)
                btn.onClick.AddListener(() => onUse());
        }
    }
}
