using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Representación visual de una carta de misión en la mano del jugador.
///
/// Estados visuales:
///   Normal     — carta en reposo
///   Hovered    — mouse encima (scale up leve)
///   Selected   — siendo arrastrada (elevada, con sombra); lo gestiona CardDragHandler (Tarea 2.3)
///   InTransit  — asignada a un héroe / en animación de salida (no interactuable)
///
/// Las animaciones de entrada y descarte usan Coroutines + AnimationCurve.
/// El layout (posición) es responsabilidad de MissionCardArea.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class MissionCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ── Referencias UI ───────────────────────────────────────────────────────

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private TextMeshProUGUI _fameText;

    [Header("Arte")]
    [SerializeField] private Image   _missionArtImage;
    [SerializeField] private Sprite  _defaultArt;

    [Header("Pistas")]
    [SerializeField] private Transform  _hintsContainer;
    [Tooltip("Prefab con al menos un TextMeshProUGUI para mostrar el texto de la pista.")]
    [SerializeField] private GameObject _hintItemPrefab;

    [Header("Borde de estado")]
    [SerializeField] private Image _borderImage;
    [SerializeField] private Color _colorNormal    = Color.white;
    [SerializeField] private Color _colorHovered   = new Color(1f, 1f, 0.6f, 1f);   // amarillo suave
    [SerializeField] private Color _colorSelected  = new Color(0.6f, 0.9f, 1f, 1f); // azul claro
    [SerializeField] private Color _colorInTransit = new Color(0.5f, 0.5f, 0.5f, 0.6f);

    [Header("Animación")]
    [SerializeField] private AnimationCurve _enterCurve   = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _discardCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private float _enterDuration    = 0.3f;
    [SerializeField] private float _discardDuration  = 0.35f;
    [SerializeField] private float _hoverScaleBoost  = 0.08f;  // +8 % tamaño al hover

    [Header("Descarte")]
    [Tooltip("Botón visible en la esquina inferior de la carta para rechazarla.")]
    [SerializeField] private Button _discardButton;
    [Tooltip("Image de tipo Filled que actúa como indicador de cooldown (fill = 1 → listo, 0 → en espera).")]
    [SerializeField] private Image _discardCooldownFill;
    [Tooltip("Texto de tooltip que aparece al hacer hover sobre el botón de descarte.")]
    [SerializeField] private TextMeshProUGUI _discardTooltipText;
    [SerializeField] private float _pulseScale    = 1.2f;   // escala máxima del pulse al quedar disponible
    [SerializeField] private float _pulseDuration = 0.25f;  // duración total del pulse (ida + vuelta)

    // ── Estado ───────────────────────────────────────────────────────────────

    public enum CardState { Normal, Hovered, Selected, InTransit }

    public MissionInstance Mission { get; private set; }
    public CardState        State  { get; private set; } = CardState.Normal;

    /// <summary>
    /// Se invoca cuando el jugador pulsa el botón "Rechazar".
    /// MissionCardArea se suscribe a este evento al instanciar la carta.
    /// </summary>
    public event Action<MissionCard> OnDiscardRequested;

    private CanvasGroup   _canvasGroup;
    private RectTransform _rectTransform;
    private Vector3       _baseScale;
    private Coroutine     _currentAnimation;
    private Coroutine     _pulseCoroutine;
    private bool          _wasInteractableLastFrame;
    private Vector3       _enterTarget;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _canvasGroup   = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
        _baseScale     = transform.localScale;

        if (_discardButton != null)
        {
            _discardButton.onClick.AddListener(HandleDiscardButtonClicked);

            // Mostrar/ocultar tooltip al hacer hover sobre el botón de descarte
            var trigger = _discardButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>()
                       ?? _discardButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry
                { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => ShowDiscardTooltip(true));
            trigger.triggers.Add(enterEntry);

            var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry
                { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => ShowDiscardTooltip(false));
            trigger.triggers.Add(exitEntry);
        }

        // Ocultar tooltip por defecto
        if (_discardTooltipText != null)
            _discardTooltipText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnConsumableUsed>(OnConsumableUsed);
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Inicializa la carta con los datos de la misión y suscribe al EventBus
    /// para refrescar pistas cuando se usen consumibles.
    /// </summary>
    public void Initialize(MissionInstance mission)
    {
        Mission = mission;
        RefreshDisplay();
        EventBus.Subscribe<OnConsumableUsed>(OnConsumableUsed);
    }

    /// <summary>
    /// Refresca todos los textos, sprites e iconos de pistas con el estado actual.
    /// Llamar explícitamente tras revelar nuevas pistas.
    /// </summary>
    public void RefreshDisplay()
    {
        if (Mission == null) return;

        if (_titleText != null)
            _titleText.text = Mission.Template.MissionTitle;

        if (_descriptionText != null)
            _descriptionText.text = Mission.Template.Description;

        if (_rankText != null)
            _rankText.text = $"R{(int)Mission.Template.Rank}";

        if (_coinsText != null)
            _coinsText.text = Mission.Template.CoinsReward.ToString();

        if (_fameText != null)
            _fameText.text = Mission.Template.FameReward.ToString();

        if (_missionArtImage != null)
            _missionArtImage.sprite = Mission.Template.MissionArt != null
                ? Mission.Template.MissionArt
                : _defaultArt;

        RefreshHints();
    }

    /// <summary>Cambia el estado visual de la carta.</summary>
    public void SetState(CardState newState)
    {
        State = newState;
        ApplyStateVisuals();
    }

    /// <summary>
    /// Actualiza el estado visual del botón de descarte.
    /// </summary>
    /// <param name="interactable">
    ///   True = el jugador puede descartar ahora.
    ///   False = bloqueado (cooldown activo, mazo vacío, o carta InTransit).
    /// </param>
    /// <param name="cooldownFill">
    ///   Valor de relleno para el indicador circular (0 = cooldown máximo, 1 = listo).
    /// </param>
    /// <param name="tooltipText">Texto del tooltip; si es null se mantiene el anterior.</param>
    public void SetDiscardButtonState(bool interactable, float cooldownFill, string tooltipText = null)
    {
        if (_discardButton != null)
            _discardButton.interactable = interactable && State != CardState.InTransit;

        if (_discardCooldownFill != null)
            _discardCooldownFill.fillAmount = cooldownFill;

        if (tooltipText != null && _discardTooltipText != null)
            _discardTooltipText.text = tooltipText;

        // Pulse al transicionar de no-interactable a interactable
        bool nowInteractable = interactable && State != CardState.InTransit;
        if (nowInteractable && !_wasInteractableLastFrame && _discardButton != null)
            TriggerDiscardPulse();
        _wasInteractableLastFrame = nowInteractable;
    }

    /// <summary>Muestra u oculta el tooltip del botón de descarte.</summary>
    public void ShowDiscardTooltip(bool visible)
    {
        if (_discardTooltipText != null)
            _discardTooltipText.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Animación de entrada: la carta parte desde <paramref name="fromLocalPosition"/>
    /// (coordenadas locales del padre) y llega a su posición actual con fade in.
    /// </summary>
    public void PlayEnterAnimation(Vector3 fromLocalPosition)
    {
        if (_currentAnimation != null) StopCoroutine(_currentAnimation);
        _currentAnimation = StartCoroutine(EnterRoutine(fromLocalPosition));
    }

    /// <summary>
    /// Mueve la carta a <paramref name="localPos"/> actualizando también el destino
    /// de la animación de entrada si esta sigue en curso, evitando que el tween
    /// sobreescriba la posición correcta después de un reposicionamiento.
    /// </summary>
    public void SnapToPosition(Vector3 localPos)
    {
        _rectTransform.localPosition = localPos;
        _enterTarget = localPos;
    }

    /// <summary>
    /// Animación de descarte: fade out + caída.
    /// Llama a <paramref name="onComplete"/> al terminar; MissionCardArea
    /// es responsable de llamar Destroy(gameObject) desde ese callback.
    /// </summary>
    public void PlayDiscardAnimation(System.Action onComplete = null)
    {
        SetState(CardState.InTransit);
        if (_currentAnimation != null) StopCoroutine(_currentAnimation);
        _currentAnimation = StartCoroutine(DiscardRoutine(onComplete));
    }

    // ── IPointerHandlers ─────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (State == CardState.Normal)
            SetState(CardState.Hovered);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (State == CardState.Hovered)
            SetState(CardState.Normal);
    }

    // ── Visuales ─────────────────────────────────────────────────────────────

    private void ApplyStateVisuals()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = State != CardState.InTransit;
            _canvasGroup.alpha          = State == CardState.InTransit ? 0.5f : 1f;
        }

        if (_borderImage != null)
        {
            _borderImage.color = State switch
            {
                CardState.Hovered   => _colorHovered,
                CardState.Selected  => _colorSelected,
                CardState.InTransit => _colorInTransit,
                _                   => _colorNormal,
            };
        }

        float scaleMultiplier = State == CardState.Hovered ? 1f + _hoverScaleBoost : 1f;
        transform.localScale  = _baseScale * scaleMultiplier;
    }

    private void RefreshHints()
    {
        if (_hintsContainer == null) return;

        foreach (Transform child in _hintsContainer)
            Destroy(child.gameObject);

        if (Mission == null || _hintItemPrefab == null) return;

        foreach (var hint in Mission.GetRevealedHints())
        {
            GameObject item  = Instantiate(_hintItemPrefab, _hintsContainer);
            var        label = item.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text  = hint.HintText;
                label.color = hint.HintColor;
            }
        }
    }

    // ── Coroutines de animación ───────────────────────────────────────────────

    /// <summary>Slide + fade in desde la posición del mazo hasta la posición en mano.</summary>
    private IEnumerator EnterRoutine(Vector3 fromLocalPosition)
    {
        _enterTarget                  = _rectTransform.localPosition;
        _rectTransform.localPosition  = fromLocalPosition;
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < _enterDuration)
        {
            elapsed += Time.deltaTime;
            float t = _enterCurve.Evaluate(Mathf.Clamp01(elapsed / _enterDuration));
            _rectTransform.localPosition = Vector3.Lerp(fromLocalPosition, _enterTarget, t);
            if (_canvasGroup != null) _canvasGroup.alpha = t;
            yield return null;
        }

        _rectTransform.localPosition = _enterTarget;
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        SetState(CardState.Normal);
        _currentAnimation = null;
    }

    /// <summary>Fade out + caída al descartar la carta.</summary>
    private IEnumerator DiscardRoutine(System.Action onComplete)
    {
        Vector3 startPos = _rectTransform.localPosition;
        Vector3 endPos   = startPos + Vector3.down * 150f;
        float   elapsed  = 0f;

        while (elapsed < _discardDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _discardDuration);
            _rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            if (_canvasGroup != null) _canvasGroup.alpha = _discardCurve.Evaluate(t);
            yield return null;
        }

        _currentAnimation = null;
        onComplete?.Invoke();
    }

    // ── Descarte ─────────────────────────────────────────────────────────────

    private void HandleDiscardButtonClicked()
    {
        if (State == CardState.InTransit) return;
        OnDiscardRequested?.Invoke(this);
    }

    private void TriggerDiscardPulse()
    {
        if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
        if (_discardButton != null)
            _pulseCoroutine = StartCoroutine(PulseDiscardButtonRoutine());
    }

    private IEnumerator PulseDiscardButtonRoutine()
    {
        Transform btnTransform = _discardButton.transform;
        Vector3 originalScale  = btnTransform.localScale;
        Vector3 targetScale    = originalScale * _pulseScale;
        float   halfDuration   = _pulseDuration * 0.5f;
        float   elapsed        = 0f;

        // Escalar hacia arriba
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            btnTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        // Escalar hacia abajo
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            btnTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        btnTransform.localScale = originalScale;
        _pulseCoroutine = null;
    }

    // ── Handler EventBus ─────────────────────────────────────────────────────

    private void OnConsumableUsed(OnConsumableUsed evt)
    {
        // Refrescar si el consumible afecta a esta carta o a toda la mano
        if (evt.TargetMission == null || evt.TargetMission == Mission)
            RefreshDisplay();
    }
}
