using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Maneja el drag &amp; drop de una carta de misión.
///
/// Flujo:
///   BeginDrag → guarda posición original, reparenta al canvas raíz, desactiva
///               blocksRaycasts para que el drop pase al HeroDropTarget.
///   Drag      → mueve la carta al cursor.
///   EndDrag   → si el drop no fue aceptado, regresa la carta con tween.
///
/// Requiere MissionCard en el mismo GameObject.
/// </summary>
[RequireComponent(typeof(MissionCard))]
[RequireComponent(typeof(CanvasGroup))]
public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// Referencia estática al drag activo. HeroDropTarget la lee para
    /// calcular el highlight verde/rojo durante el hover.
    /// </summary>
    public static CardDragHandler Current { get; private set; }

    [Header("Animación de regreso")]
    [SerializeField] private float          _returnDuration = 0.25f;
    [SerializeField] private AnimationCurve _returnCurve    = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ── Componentes ──────────────────────────────────────────────────────────

    private MissionCard   _missionCard;
    private CanvasGroup   _canvasGroup;
    private RectTransform _rectTransform;

    // ── Estado del drag ───────────────────────────────────────────────────────

    private Canvas    _rootCanvas;
    private Transform _originalParent;
    private Vector3   _originalLocalPosition;
    private int       _originalSiblingIndex;
    private bool      _dropAccepted;
    private Coroutine _returnRoutine;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _missionCard   = GetComponent<MissionCard>();
        _canvasGroup   = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }

    // ── IBeginDragHandler ────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        // No iniciar drag si la carta está en tránsito (animación de descarte)
        if (_missionCard.State == MissionCard.CardState.InTransit) return;

        // Cancelar tween de regreso pendiente
        if (_returnRoutine != null)
        {
            StopCoroutine(_returnRoutine);
            _returnRoutine = null;
        }

        Current       = this;
        _dropAccepted = false;

        // Guardar estado original antes de reparentar
        _originalParent        = transform.parent;
        _originalLocalPosition = _rectTransform.localPosition;
        _originalSiblingIndex  = transform.GetSiblingIndex();

        // Subir al canvas raíz para renderizar por encima de todo
        _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        transform.SetParent(_rootCanvas.transform, worldPositionStays: true);
        transform.SetAsLastSibling();

        // Deshabilitar raycasting propio → los eventos pasan al HeroDropTarget
        _canvasGroup.blocksRaycasts = false;

        _missionCard.SetState(MissionCard.CardState.Selected);
    }

    // ── IDragHandler ─────────────────────────────────────────────────────────

    public void OnDrag(PointerEventData eventData)
    {
        if (_rootCanvas == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            _rectTransform.localPosition = localPoint;
        }
    }

    // ── IEndDragHandler ──────────────────────────────────────────────────────

    public void OnEndDrag(PointerEventData eventData)
    {
        Current = null;

        // Re-habilitar raycasting independientemente del resultado
        _canvasGroup.blocksRaycasts = true;

        if (_dropAccepted) return;

        // Regreso: restaurar jerarquía y animar de vuelta a posición original
        transform.SetParent(_originalParent, worldPositionStays: true);
        transform.SetSiblingIndex(_originalSiblingIndex);
        _missionCard.SetState(MissionCard.CardState.Normal);

        _returnRoutine = StartCoroutine(ReturnRoutine());
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Llamado por HeroDropTarget cuando acepta el drop.
    /// Evita que la carta intente regresar a su posición original.
    /// </summary>
    public void NotifyDropAccepted()
    {
        _dropAccepted = true;
    }

    // ── Tween de regreso ─────────────────────────────────────────────────────

    private IEnumerator ReturnRoutine()
    {
        Vector3 fromPos = _rectTransform.localPosition;
        float   elapsed = 0f;

        while (elapsed < _returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = _returnCurve.Evaluate(Mathf.Clamp01(elapsed / _returnDuration));
            _rectTransform.localPosition = Vector3.Lerp(fromPos, _originalLocalPosition, t);
            yield return null;
        }

        _rectTransform.localPosition = _originalLocalPosition;
        _returnRoutine = null;
    }
}
