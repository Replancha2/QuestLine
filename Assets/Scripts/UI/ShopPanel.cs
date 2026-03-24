using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel visual de la tienda inter-día ("El Tablón de Buffs").
///
/// Escucha los eventos de ShopManager para mostrarse/ocultarse y actualiza
/// los slots de ítems. Se activa cuando ShopManager abre la tienda y se oculta
/// cuando el jugador compra, salta o cierra.
///
/// Setup en Inspector:
///   - Asignar _shopManager (o se busca por Instance en Start)
///   - Asignar 3 referencias de ShopSlotUI en _slots[]
///   - Asignar _continueButton y _coinCountText
///   - Este GameObject debe empezar desactivado (SetActive false)
///
/// Título del panel: "El Tablón de Buffs" (configurable en Inspector)
/// </summary>
public class ShopPanel : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Referencias")]
    [SerializeField] private ShopManager        _shopManager;

    [Header("Slots de ítems")]
    [Tooltip("Exactamente 3 ShopSlotUI; si hay menos se desactivan los sobrantes.")]
    [SerializeField] private ShopSlotUI[]       _slots;

    [Header("Controles del panel")]
    [SerializeField] private Button             _continueButton;
    [SerializeField] private TextMeshProUGUI    _coinCountText;
    [SerializeField] private TextMeshProUGUI    _titleText;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (_shopManager == null)
            _shopManager = ShopManager.Instance;

        if (_shopManager != null)
        {
            _shopManager.OnShopOpened += HandleShopOpened;
            _shopManager.OnShopClosed += HandleShopClosed;
        }

        if (_continueButton != null)
            _continueButton.onClick.AddListener(HandleContinueClicked);

        if (_titleText != null)
            _titleText.text = "El Tablón de Buffs";

        // Iniciar oculto
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_shopManager != null)
        {
            _shopManager.OnShopOpened -= HandleShopOpened;
            _shopManager.OnShopClosed -= HandleShopClosed;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnCoinEarned>(HandleCoinEarned);
        EventBus.Subscribe<OnCoinSpent>(HandleCoinSpent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnCoinEarned>(HandleCoinEarned);
        EventBus.Unsubscribe<OnCoinSpent>(HandleCoinSpent);
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void HandleShopOpened(IReadOnlyList<ShopOffer> offers)
    {
        PopulateSlots(offers);
        RefreshCoinDisplay();
        gameObject.SetActive(true);
    }

    private void HandleShopClosed()
    {
        gameObject.SetActive(false);
    }

    private void HandleContinueClicked()
    {
        _shopManager?.SkipShop();
    }

    private void HandleCoinEarned(OnCoinEarned _)
    {
        RefreshCoinDisplay();
        RefreshSlotAffordability();
    }

    private void HandleCoinSpent(OnCoinSpent _)
    {
        RefreshCoinDisplay();
        RefreshSlotAffordability();
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    private void PopulateSlots(IReadOnlyList<ShopOffer> offers)
    {
        if (_slots == null) return;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null) continue;

            if (i < offers.Count)
            {
                _slots[i].Setup(offers[i]);
                _slots[i].OnBuyClicked -= HandleSlotBuyClicked; // evitar doble suscripción
                _slots[i].OnBuyClicked += HandleSlotBuyClicked;
                _slots[i].RefreshAffordable(CurrentCoins);
            }
            else
            {
                _slots[i].gameObject.SetActive(false);
            }
        }
    }

    private void HandleSlotBuyClicked(ShopSlotUI slot)
    {
        if (_shopManager == null || slot.Offer == null) return;

        bool success = _shopManager.PurchaseOffer(slot.Offer);

        if (success)
        {
            slot.MarkPurchased();
            RefreshCoinDisplay();
            RefreshSlotAffordability();
        }
        // Si no hay monedas suficientes, CoinJar dispara el shake automáticamente
    }

    private void RefreshCoinDisplay()
    {
        if (_coinCountText != null)
            _coinCountText.text = $"Monedas: {CurrentCoins}";
    }

    private void RefreshSlotAffordability()
    {
        if (_slots == null) return;
        int coins = CurrentCoins;
        foreach (var slot in _slots)
            slot?.RefreshAffordable(coins);
    }

    private int CurrentCoins =>
        GameManager.Instance != null ? GameManager.Instance.CurrentCoins : 0;
}
