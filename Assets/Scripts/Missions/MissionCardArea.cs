using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona el layout visual de las cartas de misión en la mano del jugador.
///
/// Responsabilidades:
///   - Instanciar un MissionCard prefab por cada carta robada del mazo
///   - Posicionar las cartas en distribución horizontal centrada
///   - Animar la entrada de cada carta nueva (slide desde el área del mazo)
///   - Limpiar la carta visual cuando es asignada a un héroe o descartada
///
/// Conexión con MissionDeck:
///   - Escucha MissionDeck.OnCardDrawn para instanciar la carta visual
///   - Expone RemoveCard() para que CardDragHandler (Tarea 2.3) lo llame al asignar
/// </summary>
public class MissionCardArea : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Referencias")]
    [Tooltip("MissionDeck de la escena. Se busca por Instance si no se asigna.")]
    [SerializeField] private MissionDeck    _deck;
    [Tooltip("Prefab con componente MissionCard.")]
    [SerializeField] private GameObject     _cardPrefab;
    [Tooltip("Transform desde donde parten las animaciones de entrada (posición del mazo).")]
    [SerializeField] private RectTransform  _deckAnchor;

    [Header("Layout")]
    [Tooltip("Separación horizontal entre centros de carta (px).")]
    [SerializeField] private float _cardSpacing = 220f;
    [Tooltip("Posición Y de las cartas en el espacio local del contenedor (negativo = más abajo).")]
    [SerializeField] private float _cardYOffset = 0f;
    [Tooltip("Delay escalonado entre cartas al rellenar la mano de golpe (s).")]
    [SerializeField] private float _dealDelay   = 0.08f;

    [Header("Descarte")]
    [Tooltip("Segundos de espera entre un descarte y el siguiente.")]
    [SerializeField] public float DiscardCooldownSeconds = 10f;

    // ── Estado ───────────────────────────────────────────────────────────────

    private readonly List<MissionCard> _handCards = new List<MissionCard>();
    private int   _pendingDeals;             // cartas pendientes de animar para escalonar el delay
    private float _discardCooldownTimer = 0f;

    /// <summary>True cuando el jugador puede descartar una carta.</summary>
    public bool CanDiscard => _discardCooldownTimer <= 0f;

    // ── Singleton ────────────────────────────────────────────────────────────

    public static MissionCardArea Instance { get; private set; }

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Pivot (0.5, 0.5) → localPosition (0,0) = centro exacto de pantalla
        var rt = GetComponent<RectTransform>();
        if (rt != null)
            rt.pivot = new Vector2(0.5f, 0.5f);

        if (_deck == null)
            _deck = MissionDeck.Instance;
    }

    private void OnEnable()
    {
        if (_deck != null)
        {
            _deck.OnCardDrawn -= HandleCardDrawn; // evitar doble suscripción si se re-activa
            _deck.OnCardDrawn += HandleCardDrawn;
        }
    }

    private void OnDisable()
    {
        if (_deck != null)
            _deck.OnCardDrawn -= HandleCardDrawn;
    }

    private void Update()
    {
        if (_discardCooldownTimer > 0f)
        {
            _discardCooldownTimer -= Time.deltaTime;
            if (_discardCooldownTimer < 0f)
                _discardCooldownTimer = 0f;
            UpdateDiscardButtonStates();
        }
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Descarta la carta indicada: animación de salida, publica OnMissionDiscarded,
    /// roba una nueva carta del mazo (si hay) y activa el cooldown.
    /// No hace nada si el cooldown está activo o la carta ya está InTransit.
    /// </summary>
    public void DiscardMission(MissionCard card)
    {
        if (card == null)                    return;
        if (!CanDiscard)                     return;
        if (card.State == MissionCard.CardState.InTransit) return;

        // Caso límite: mazo vacío → no se puede descartar
        if (_deck != null && _deck.IsPoolEmpty)
        {
            Debug.Log("[MissionCardArea] No se puede descartar: el mazo está vacío.");
            return;
        }

        MissionInstance mission = card.Mission;

        // Quitar de la lista visual antes de la animación para evitar interacción
        _handCards.Remove(card);

        // Animación de salida, luego destruir
        card.PlayDiscardAnimation(() =>
        {
            if (card != null) Destroy(card.gameObject);
            RepositionCards(animated: false);
        });

        // Publicar evento
        EventBus.Publish(new OnMissionDiscarded { Mission = mission });

        // Quitar del mazo lógico (esto llama FillHand → DrawCard → HandleCardDrawn)
        if (_deck != null)
            _deck.RemoveFromHand(mission);

        // Activar cooldown
        _discardCooldownTimer = DiscardCooldownSeconds;
        UpdateDiscardButtonStates();
    }

    /// <summary>
    /// Quita la carta visual de la misión indicada de la mano y reproduce la
    /// animación de descarte. Después de llamar a este método, llamar a
    /// MissionDeck.Instance.RemoveFromHand(mission) para que el mazo reponga.
    ///
    /// Si <paramref name="animate"/> es false, la carta se destruye inmediatamente
    /// (útil al cambiar de día o en resets).
    /// </summary>
    public void RemoveCard(MissionInstance mission, bool animate = true)
    {
        var card = FindCardFor(mission);
        if (card == null) return;

        _handCards.Remove(card);

        if (animate)
        {
            card.PlayDiscardAnimation(() =>
            {
                if (card != null) Destroy(card.gameObject);
                RepositionCards(animated: false);
            });
        }
        else
        {
            Destroy(card.gameObject);
            RepositionCards(animated: false);
        }
    }

    /// <summary>
    /// Devuelve la MissionCard visual asociada a la misión dada, o null si no existe.
    /// Útil para CardDragHandler (Tarea 2.3) al necesitar la carta que se arrastra.
    /// </summary>
    public MissionCard GetCard(MissionInstance mission) => FindCardFor(mission);

    /// <summary>
    /// Destruye inmediatamente todas las cartas visuales de la mano sin animación.
    /// Usado por ShopManager.UseReshuffle() antes de volver a poblar la mano.
    /// </summary>
    public void ClearAllCards()
    {
        StopAllCoroutines();
        _pendingDeals = 0;
        foreach (var card in _handCards)
            if (card != null) Destroy(card.gameObject);
        _handCards.Clear();
    }

    // ── Handler de MissionDeck ────────────────────────────────────────────────

    private void HandleCardDrawn(MissionInstance mission)
    {
        float delay = _pendingDeals * _dealDelay;
        _pendingDeals++;
        StartCoroutine(SpawnCardRoutine(mission, delay));
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    private IEnumerator SpawnCardRoutine(MissionInstance mission, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        _pendingDeals = Mathf.Max(0, _pendingDeals - 1);

        if (_cardPrefab == null)
        {
            Debug.LogError("[MissionCardArea] _cardPrefab no asignado en el Inspector.");
            yield break;
        }

        GameObject go   = Instantiate(_cardPrefab, transform);
        var        card = go.GetComponent<MissionCard>();

        if (card == null)
        {
            Debug.LogError("[MissionCardArea] El prefab no tiene componente MissionCard.");
            Destroy(go);
            yield break;
        }

        card.Initialize(mission);
        card.OnDiscardRequested += DiscardMission;
        _handCards.Add(card);

        // Posicionar en su lugar final antes de animar para que RepositionCards
        // calcule la posición correcta y luego la carta anime desde el mazo.
        RepositionCards(animated: false);

        // Aplicar estado actual del botón de descarte a la nueva carta
        UpdateDiscardButtonStates();

        // Origen de la animación de entrada (posición del mazo en coordenadas locales).
        // Sin deckAnchor, cada carta entra deslizándose desde abajo de su posición final
        // en vez de desde el centro del canvas, evitando que parezca que spawnean a la derecha.
        Vector3 fromLocal;
        if (_deckAnchor != null)
        {
            fromLocal = transform.InverseTransformPoint(_deckAnchor.position);
        }
        else
        {
            var cardRT = card.GetComponent<RectTransform>();
            fromLocal = cardRT.localPosition + Vector3.down * 250f;
        }

        card.PlayEnterAnimation(fromLocal);
    }

    /// <summary>
    /// Sincroniza el estado visual del botón de descarte de todas las cartas en mano
    /// con el cooldown actual y la disponibilidad del mazo.
    /// </summary>
    private void UpdateDiscardButtonStates()
    {
        bool   poolEmpty   = _deck != null && _deck.IsPoolEmpty;
        bool   interactable = CanDiscard && !poolEmpty;
        float  fillAmount  = DiscardCooldownSeconds > 0f
            ? 1f - Mathf.Clamp01(_discardCooldownTimer / DiscardCooldownSeconds)
            : 1f;

        string tooltip;
        if (poolEmpty)
            tooltip = "Mazo vacío";
        else if (!CanDiscard)
            tooltip = $"Rechaza esta misión y roba una nueva. Cooldown: {_discardCooldownTimer:F0}s";
        else
            tooltip = "Rechaza esta misión y roba una nueva";

        foreach (var card in _handCards)
        {
            if (card != null)
                card.SetDiscardButtonState(interactable, fillAmount, tooltip);
        }
    }

    /// <summary>
    /// Reposiciona las cartas centradas en (0,0) = centro de pantalla (pivot 0.5,0.5).
    /// Ajusta el Y con _cardYOffset para mover el grupo arriba/abajo.
    /// </summary>
    private void RepositionCards(bool animated)
    {
        int count = _handCards.Count;
        if (count == 0) return;

        float totalWidth = (count - 1) * _cardSpacing;
        float startX     = -totalWidth * 0.5f;

        for (int i = 0; i < count; i++)
        {
            if (_handCards[i] == null) continue;

            Vector3 pos = new Vector3(startX + i * _cardSpacing, _cardYOffset, 0f);
            _handCards[i].SnapToPosition(pos);
        }
    }

    private MissionCard FindCardFor(MissionInstance mission)
    {
        foreach (var card in _handCards)
        {
            if (card != null && card.Mission == mission)
                return card;
        }
        return null;
    }
}
