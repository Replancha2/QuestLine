using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componente que se coloca en cada HeroSlotUI para recibir drops de cartas de misión.
///
/// Comportamiento durante el drag (IPointerEnterHandler / IPointerExitHandler):
///   - Borde verde  → el héroe del slot acepta la misión arrastrada
///   - Borde rojo   → el héroe la rechazaría
///
/// Al soltar la carta (IDropHandler.OnDrop):
///   - Si el héroe acepta:
///       1. Notifica a CardDragHandler que el drop fue aceptado
///       2. Publica OnMissionAssigned en el EventBus
///       3. Llama a MissionCardArea.RemoveCard + MissionDeck.RemoveFromHand
///       4. Llama a MissionEvaluator.Evaluate
///   - Si rechaza:
///       → Shake del slot + texto "¡Me niego!" durante ~0.4 s
///       → La carta regresa a su posición (CardDragHandler lo gestiona)
///
/// Requiere HeroSlotUI en el mismo GameObject.
/// </summary>
[RequireComponent(typeof(HeroSlotUI))]
public class HeroDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Highlight de drop")]
    [Tooltip("Image que se usa como borde de highlight al arrastrar sobre el slot.")]
    [SerializeField] private Image _highlightBorder;
    [SerializeField] private Color _colorAccept = new Color(0.2f, 1f, 0.2f, 0.8f);
    [SerializeField] private Color _colorReject = new Color(1f, 0.2f, 0.2f, 0.8f);

    [Header("Feedback de rechazo")]
    [Tooltip("TextMeshProUGUI flotante para '¡Me niego!'. Puede ser null.")]
    [SerializeField] private TextMeshProUGUI _rejectFeedbackText;
    [SerializeField] private float _rejectShakeDuration  = 0.4f;
    [SerializeField] private float _rejectShakeMagnitude = 12f;

    // ── Referencias ──────────────────────────────────────────────────────────

    private HeroSlotUI     _heroSlot;
    private MissionCardArea _cardArea;

    // ── Estado interno ────────────────────────────────────────────────────────

    private Coroutine _shakeRoutine;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _heroSlot = GetComponent<HeroSlotUI>();
        _cardArea = Object.FindAnyObjectByType<MissionCardArea>();

        // Si este GameObject no tiene ningún Graphic, Unity no puede detectar raycasts
        // sobre él y OnDrop / OnPointerEnter nunca se dispararían. Añadimos una Image
        // invisible que sirve como superficie de detección.
        if (GetComponent<Graphic>() == null)
        {
            var img = gameObject.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);
            img.raycastTarget = true;
        }

        if (_highlightBorder != null)
            _highlightBorder.enabled = false;

        if (_rejectFeedbackText != null)
            _rejectFeedbackText.enabled = false;
    }

    // ── IPointerEnterHandler / IPointerExitHandler ────────────────────────────
    // Unity los dispara cuando blocksRaycasts está desactivado en la carta arrastrada,
    // lo que permite detectar sobre qué hero slot está el cursor.

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CardDragHandler.Current == null) return;
        if (!_heroSlot.IsOccupied) return;

        var missionCard = CardDragHandler.Current.GetComponent<MissionCard>();
        if (missionCard == null || missionCard.Mission == null) return;

        bool accepts = _heroSlot.CurrentHero.WillAcceptMission(missionCard.Mission);
        ShowHighlight(accepts ? _colorAccept : _colorReject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideHighlight();
    }

    // ── IDropHandler ─────────────────────────────────────────────────────────

    public void OnDrop(PointerEventData eventData)
    {
        HideHighlight();

        // Obtener el CardDragHandler de la carta que se soltó
        var dragHandler = eventData.pointerDrag?.GetComponent<CardDragHandler>();
        if (dragHandler == null) return;

        var missionCard = dragHandler.GetComponent<MissionCard>();
        if (missionCard == null || missionCard.Mission == null) return;

        // El slot debe estar ocupado para poder asignar
        if (!_heroSlot.IsOccupied)
        {
            Debug.Log("[HeroDropTarget] Slot vacío, no se puede asignar misión.");
            return;
        }

        HeroInstance    hero    = _heroSlot.CurrentHero;
        MissionInstance mission = missionCard.Mission;

        // ── Verificar aceptación ─────────────────────────────────────────────
        if (!hero.WillAcceptMission(mission))
        {
            // Rechazo visual; la carta volverá sola (CardDragHandler._dropAccepted = false)
            TriggerRejectFeedback();
            return;
        }

        // ── Aceptado ─────────────────────────────────────────────────────────

        // 1. Evitar que CardDragHandler regrese la carta
        dragHandler.NotifyDropAccepted();

        // 2. Publicar evento de asignación (HeroQueue lo escucha y libera el slot)
        EventBus.Publish(new OnMissionAssigned
        {
            Mission = mission.Template,
            Hero    = hero,
        });

        // 3. Retirar la carta de la mano visual y del mazo (el mazo roba carta nueva)
        if (_cardArea != null)
            _cardArea.RemoveCard(mission);

        if (MissionDeck.Instance != null)
            MissionDeck.Instance.RemoveFromHand(mission);

        // 4. Evaluar la misión: éxito / fallo / muerte de héroe
        //    El MissionResult queda disponible para Tarea 4.2 (pantalla de resultado).
        MissionResult result = MissionEvaluator.Evaluate(hero, mission);

        Debug.Log($"[HeroDropTarget] Misión '{mission.Template.MissionTitle}' " +
                  $"asignada a '{hero.Name}'. " +
                  $"Resultado: {(result.Success ? "ÉXITO" : "FALLO")} " +
                  $"(chance={result.SuccessChance:P0})");
    }

    // ── Visuales de highlight ─────────────────────────────────────────────────

    private void ShowHighlight(Color color)
    {
        if (_highlightBorder == null) return;
        _highlightBorder.color   = color;
        _highlightBorder.enabled = true;
    }

    private void HideHighlight()
    {
        if (_highlightBorder == null) return;
        _highlightBorder.enabled = false;
    }

    // ── Feedback de rechazo ───────────────────────────────────────────────────

    private void TriggerRejectFeedback()
    {
        if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
        _shakeRoutine = StartCoroutine(RejectRoutine());
    }

    private IEnumerator RejectRoutine()
    {
        // Activar texto de rechazo
        if (_rejectFeedbackText != null)
        {
            _rejectFeedbackText.text    = "¡Me niego!";
            _rejectFeedbackText.enabled = true;
        }

        // Shake horizontal con magnitud decreciente
        RectTransform rt        = GetComponent<RectTransform>();
        Vector3       originPos = rt.localPosition;
        float         elapsed   = 0f;

        while (elapsed < _rejectShakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress  = elapsed / _rejectShakeDuration;
            float magnitude = _rejectShakeMagnitude * (1f - progress); // suaviza al final
            float offsetX   = Mathf.Sin(elapsed * 40f) * magnitude;
            rt.localPosition = originPos + new Vector3(offsetX, 0f, 0f);
            yield return null;
        }

        rt.localPosition = originPos;

        // Mantener el texto visible un momento antes de ocultarlo
        if (_rejectFeedbackText != null)
        {
            yield return new WaitForSeconds(0.6f);
            _rejectFeedbackText.enabled = false;
        }

        _shakeRoutine = null;
    }
}
