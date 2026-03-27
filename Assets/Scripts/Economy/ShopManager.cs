using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la tienda inter-día: selecciona los ítems a mostrar, procesa compras
/// y controla la apertura/cierre del panel.
///
/// Flujo:
///   OnDayEnd → OpenShop() → jugador compra / omite → CloseShop() → RunManager.StartDay()
///
/// Escucha:
///   - OnDayEnd → abre el shop automáticamente
///
/// Publica:
///   - OnBuffPurchased (vía BuffManager.ApplyBuff)
///   - OnConsumableUsed no se publica aquí; lo hace ConsumableManager al usar el ítem
///
/// Eventos propios (C#):
///   - OnShopOpened
///   - OnShopClosed
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Pools de ítems")]
    [Tooltip("Todos los buffs comprables en la tienda.")]
    [SerializeField] private BuffData[] _availableBuffs;
    [Tooltip("Todos los consumibles comprables en la tienda.")]
    [SerializeField] private ConsumableData[] _availableConsumables;

    [Header("Configuración")]
    [Tooltip("Ítems a mostrar por visita. ⚠️ BALANCE PENDIENTE (TAREA 5.3): puede subir a 5.")]
    [SerializeField] private int _shopSlots = 3;

    // ── Estado ───────────────────────────────────────────────────────────────

    private readonly List<ShopOffer> _currentOffers = new List<ShopOffer>();
    private int _reshuffleCharges = 0;

    public IReadOnlyList<ShopOffer> CurrentOffers => _currentOffers;

    /// <summary>Número de usos de Reshuffle disponibles durante la run activa.</summary>
    public int ReshuffleCharges => _reshuffleCharges;

    /// <summary>True mientras la tienda está abierta.</summary>
    public bool IsShopOpen { get; private set; }

    // ── Eventos C# ───────────────────────────────────────────────────────────

    /// <summary>Se dispara cuando la tienda se abre. Incluye los ítems del turno actual.</summary>
    public event System.Action<IReadOnlyList<ShopOffer>> OnShopOpened;

    /// <summary>Se dispara cuando la tienda se cierra (compra completada o skip).</summary>
    public event System.Action OnShopClosed;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnDayEnd>(HandleDayEnd);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnDayEnd>(HandleDayEnd);
    }

    // ── Handlers de eventos ──────────────────────────────────────────────────

    private void HandleDayEnd(OnDayEnd e)
    {
        OpenShop();
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Abre la tienda: genera una selección aleatoria del pool combinado y
    /// notifica a la UI a través de <see cref="OnShopOpened"/>.
    /// </summary>
    public void OpenShop()
    {
        if (IsShopOpen) return;

        _currentOffers.Clear();
        GenerateOffers();
        IsShopOpen = true;
        OnShopOpened?.Invoke(_currentOffers);
    }

    /// <summary>
    /// Intenta comprar un buff. Verifica coins, gasta, aplica y notifica.
    /// Caso especial: <see cref="BuffType.MissionReshuffle"/> incrementa
    /// <see cref="ReshuffleCharges"/> en lugar de enviarse a BuffManager.
    /// </summary>
    /// <returns>True si la compra fue exitosa.</returns>
    public bool PurchaseBuff(BuffData buff)
    {
        if (buff == null) return false;
        if (CoinJar.Instance == null) return false;
        if (!CoinJar.Instance.SpendCoins(buff.Cost)) return false;

        if (buff.Type == BuffType.MissionReshuffle)
        {
            // El Reshuffle no es permanente; se guarda como carga en el HUD
            _reshuffleCharges++;
            // Publicar igual para que la UI lo pueda escuchar
            EventBus.Publish(new OnBuffPurchased { Buff = buff });
        }
        else
        {
            BuffManager.Instance?.ApplyBuff(buff);
        }

        return true;
    }

    /// <summary>
    /// Intenta comprar un consumible. Verifica coins, gasta y añade al inventario.
    /// </summary>
    /// <returns>True si la compra fue exitosa.</returns>
    public bool PurchaseConsumable(ConsumableData consumable)
    {
        if (consumable == null) return false;
        if (CoinJar.Instance == null) return false;
        if (!CoinJar.Instance.SpendCoins(consumable.Cost)) return false;

        ConsumableManager.Instance?.AddConsumable(consumable);
        // Publicar a través del mismo evento de buff para que la UI pueda reaccionar
        EventBus.Publish(new OnBuffPurchased { Buff = null });

        return true;
    }

    /// <summary>
    /// Compra cualquier oferta del shop (buff o consumible).
    /// </summary>
    /// <returns>True si la compra fue exitosa.</returns>
    public bool PurchaseOffer(ShopOffer offer)
    {
        if (offer == null) return false;
        return offer.IsBuff
            ? PurchaseBuff(offer.Buff)
            : PurchaseConsumable(offer.Consumable);
    }

    /// <summary>
    /// Cierra la tienda sin comprar nada e inicia el siguiente día.
    /// </summary>
    public void SkipShop() => CloseShop();

    /// <summary>
    /// Cierra la tienda e inicia el siguiente día.
    /// </summary>
    public void CloseShop()
    {
        if (!IsShopOpen) return;

        IsShopOpen = false;
        OnShopClosed?.Invoke();
        RunManager.Instance?.StartDay();
    }

    /// <summary>
    /// Usa un cargo de Reshuffle: baraja el pool restante y descarta todas las
    /// cartas en mano para robar una mano nueva.
    /// </summary>
    /// <returns>True si había cargas disponibles y se ejecutó el efecto.</returns>
    public bool UseReshuffle()
    {
        if (_reshuffleCharges <= 0)
        {
            Debug.LogWarning("[ShopManager] UseReshuffle llamado sin cargas disponibles.");
            return false;
        }

        _reshuffleCharges--;

        if (MissionDeck.Instance == null) return false;

        // 1. Barajar el pool restante
        MissionDeck.Instance.Reshuffle();

        // 2. Limpiar cartas visuales
        MissionCardArea.Instance?.ClearAllCards();

        // 3. Limpiar mano lógica (sin activar FillHand por cada carta)
        MissionDeck.Instance.ClearHand();

        // 4. Robar una mano nueva (dispara OnCardDrawn → MissionCardArea instancia las cartas)
        MissionDeck.Instance.FillHand();

        return true;
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Selecciona hasta <see cref="_shopSlots"/> ítems aleatorios del pool combinado
    /// (buffs + consumibles). Si el pool es más pequeño que _shopSlots, se muestran
    /// todos los disponibles.
    /// </summary>
    private void GenerateOffers()
    {
        // Construir lista combinada
        var combined = new List<ShopOffer>();

        if (_availableBuffs != null)
            foreach (var b in _availableBuffs)
                if (b != null) combined.Add(new ShopOffer(b));

        if (_availableConsumables != null)
            foreach (var c in _availableConsumables)
                if (c != null) combined.Add(new ShopOffer(c));

        // Fisher-Yates para selección aleatoria
        for (int i = combined.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (combined[i], combined[j]) = (combined[j], combined[i]);
        }

        int take = Mathf.Min(_shopSlots, combined.Count);
        for (int i = 0; i < take; i++)
            _currentOffers.Add(combined[i]);
    }
}
