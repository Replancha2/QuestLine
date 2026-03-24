using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Representación visual de un slot de héroe en pantalla.
///
/// Muestra:
///   - Retrato del héroe
///   - Stats (con "?" para stats ocultos si el héroe tiene rasgo Distrustful)
///   - Barra de paciencia (via componente HeroPatience)
///   - Iconos de rasgos (2-3 iconos pequeños)
///
/// Efectos:
///   - Tween de entrada al asignarse un héroe
///   - Tween de salida al liberarse el slot
///
/// Importante: HeroSlotUI no gestiona la lógica de cola — eso es responsabilidad
/// de HeroQueue. Este componente solo se encarga de la presentación.
/// </summary>
public class HeroSlotUI : MonoBehaviour
{
    [Header("Contenedor raíz del slot (se desactiva cuando está vacío)")]
    [SerializeField] private GameObject _slotRoot;

    [Header("Retrato")]
    [SerializeField] private Image _portraitImage;
    [SerializeField] private Sprite _defaultPortrait;

    [Header("Nombre y stats")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _strengthText;
    [SerializeField] private TextMeshProUGUI _dexterityText;
    [SerializeField] private TextMeshProUGUI _intelligenceText;
    [SerializeField] private TextMeshProUGUI _charismaText;

    [Header("Rasgos")]
    [SerializeField] private Transform     _traitsContainer;
    [SerializeField] private GameObject    _traitIconPrefab;  // prefab: Image + TextMeshProUGUI (abreviatura)

    [Header("Paciencia")]
    [SerializeField] private HeroPatience _patience;

    [Header("Animación")]
    [Tooltip("Animator con triggers 'Enter' y 'Exit'. Opcional — si es null se omite.")]
    [SerializeField] private Animator _animator;

    private static readonly int TriggerEnter = Animator.StringToHash("Enter");
    private static readonly int TriggerExit  = Animator.StringToHash("Exit");

    // ── Estado ─────────────────────────────────────────────────────────────────

    public bool         IsOccupied  { get; private set; }
    public HeroInstance CurrentHero { get; private set; }

    // ── API pública ────────────────────────────────────────────────────────────

    /// <summary>
    /// Asigna un héroe al slot: actualiza visuales, activa el timer de paciencia
    /// y reproduce la animación de entrada.
    /// </summary>
    public void SetHero(HeroInstance hero)
    {
        CurrentHero = hero;
        IsOccupied  = true;

        if (_slotRoot != null) _slotRoot.SetActive(true);

        RefreshDisplay();

        if (_patience != null)
            _patience.Activate(hero);

        if (_animator != null)
            _animator.SetTrigger(TriggerEnter);
    }

    /// <summary>
    /// Libera el slot: pausa el timer y reproduce la animación de salida.
    /// Al terminar la animación se llama FinishClear() (vía AnimationEvent o fallback inmediato).
    /// </summary>
    public void ClearHero()
    {
        if (_patience != null)
            _patience.Deactivate();

        if (_animator != null)
            _animator.SetTrigger(TriggerExit);
        else
            FinishClear();
    }

    /// <summary>
    /// Llamado por AnimationEvent al terminar el clip de salida,
    /// o directamente por ClearHero() si no hay Animator.
    /// </summary>
    public void FinishClear()
    {
        CurrentHero = null;
        IsOccupied  = false;
        if (_slotRoot != null) _slotRoot.SetActive(false);
        ClearTraitIcons();
    }

    // ── Visuales ───────────────────────────────────────────────────────────────

    private void RefreshDisplay()
    {
        if (CurrentHero == null) return;

        // Retrato
        if (_portraitImage != null)
            _portraitImage.sprite = CurrentHero.Portrait != null
                ? CurrentHero.Portrait
                : _defaultPortrait;

        // Nombre
        if (_nameText != null)
            _nameText.text = CurrentHero.Name;

        // Stats — Distrustful oculta 2 stats al azar (ya definidos en HeroInstance.HiddenStats)
        var hidden = new HashSet<HeroStat>(CurrentHero.HiddenStats);
        SetStatLabel(_strengthText,     HeroStat.Strength,     CurrentHero.Strength,     hidden);
        SetStatLabel(_dexterityText,    HeroStat.Dexterity,    CurrentHero.Dexterity,    hidden);
        SetStatLabel(_intelligenceText, HeroStat.Intelligence, CurrentHero.Intelligence, hidden);
        SetStatLabel(_charismaText,     HeroStat.Charisma,     CurrentHero.Charisma,     hidden);

        // Iconos de rasgos
        RefreshTraitIcons();
    }

    private void SetStatLabel(TextMeshProUGUI label, HeroStat stat, int value, HashSet<HeroStat> hidden)
    {
        if (label == null) return;
        label.text = hidden.Contains(stat) ? "?" : value.ToString();
    }

    private void RefreshTraitIcons()
    {
        ClearTraitIcons();

        if (_traitsContainer == null || _traitIconPrefab == null || CurrentHero == null) return;

        foreach (HeroTrait trait in CurrentHero.Traits)
        {
            GameObject icon = Instantiate(_traitIconPrefab, _traitsContainer);
            // El prefab debe tener al menos un TextMeshProUGUI para la abreviatura (3 letras)
            var label = icon.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                string name = trait.ToString();
                label.text = name.Length > 3 ? name[..3].ToUpper() : name.ToUpper();
            }

            // Tooltip (TooltipManager — Fase 4)
            // var tooltip = icon.GetComponent<TooltipTrigger>();
            // if (tooltip != null) tooltip.SetText(GetTraitDescription(trait));
        }
    }

    private void ClearTraitIcons()
    {
        if (_traitsContainer == null) return;
        foreach (Transform child in _traitsContainer)
            Destroy(child.gameObject);
    }
}
