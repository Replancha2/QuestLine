using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla una ranura individual de la tienda.
/// Muestra el ítem (buff o consumible), gestiona el botón de compra y
/// reproduce la animación de brillo al comprar.
///
/// Configuración en Inspector:
///   - Asignar las referencias de UI (_iconImage, _nameText, etc.)
///   - El botón _buyButton lanza OnBuyClicked que captura ShopPanel
/// </summary>
public class ShopSlotUI : MonoBehaviour
{
    // ── Referencias UI ───────────────────────────────────────────────────────

    [Header("Contenido")]
    [SerializeField] private Image              _iconImage;
    [SerializeField] private TextMeshProUGUI    _nameText;
    [SerializeField] private TextMeshProUGUI    _descriptionText;
    [SerializeField] private TextMeshProUGUI    _costText;

    [Header("Controles")]
    [SerializeField] private Button             _buyButton;
    [SerializeField] private Image              _soldOverlay;      // Panel oscuro al comprar

    [Header("Animación de compra")]
    [SerializeField] private Image              _glowImage;        // Imagen de brillo
    [SerializeField] private float              _glowDuration = 0.4f;

    // ── Estado ───────────────────────────────────────────────────────────────

    private ShopOffer _offer;
    private bool      _purchased;

    /// <summary>Evento que ShopPanel escucha para procesar la compra.</summary>
    public event System.Action<ShopSlotUI> OnBuyClicked;

    /// <summary>Oferta que este slot representa.</summary>
    public ShopOffer Offer => _offer;

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>Configura el slot con la oferta dada y activa el GameObject.</summary>
    public void Setup(ShopOffer offer)
    {
        _offer     = offer;
        _purchased = false;

        if (_iconImage        != null) _iconImage.sprite  = offer.Icon;
        if (_nameText         != null) _nameText.text     = offer.Name;
        if (_descriptionText  != null) _descriptionText.text = offer.Description;
        if (_costText         != null) _costText.text     = $"{offer.Cost} 🪙";

        SetSoldState(false);
        gameObject.SetActive(true);
    }

    /// <summary>Actualiza el estado interactuable del botón según las monedas actuales.</summary>
    public void RefreshAffordable(int currentCoins)
    {
        if (_purchased || _offer == null) return;
        bool canAfford = currentCoins >= _offer.Cost;
        if (_buyButton != null)
            _buyButton.interactable = canAfford;
    }

    /// <summary>Marca el slot como comprado: gris + animación de brillo.</summary>
    public void MarkPurchased()
    {
        _purchased = true;
        SetSoldState(true);
        StartCoroutine(PlayGlowAnimation());
    }

    // ── Handlers de botón ────────────────────────────────────────────────────

    // Llamado desde el Button onClick en el Inspector
    public void HandleBuyButton()
    {
        if (!_purchased)
            OnBuyClicked?.Invoke(this);
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    private void SetSoldState(bool sold)
    {
        if (_buyButton  != null) _buyButton.interactable = !sold;
        if (_soldOverlay != null) _soldOverlay.gameObject.SetActive(sold);
    }

    private IEnumerator PlayGlowAnimation()
    {
        if (_glowImage == null) yield break;

        _glowImage.gameObject.SetActive(true);

        float elapsed = 0f;
        Color baseColor = _glowImage.color;

        while (elapsed < _glowDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _glowDuration;
            // Fade in rápido → fade out suave
            float alpha = t < 0.3f
                ? Mathf.Lerp(0f, 1f, t / 0.3f)
                : Mathf.Lerp(1f, 0f, (t - 0.3f) / 0.7f);
            _glowImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }

        _glowImage.gameObject.SetActive(false);
        _glowImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
    }
}
