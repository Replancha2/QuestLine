using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona hasta MaxSimultaneousHeroes slots de héroes visibles simultáneamente.
/// Los héroes que llegan cuando todos los slots están ocupados esperan en una cola interna.
/// Al liberarse un slot se promueve automáticamente el siguiente héroe de la cola.
///
/// Escucha:
///   OnHeroArrived     → TryFillSlot
///   OnMissionAssigned → RemoveHero (misión asignada, slot libre)
///   OnHeroDied        → RemoveHero (héroe murió en misión)
///   OnHeroLeft        → RemoveHero (héroe se fue sin misión o voluntariamente)
/// </summary>
public class HeroQueue : MonoBehaviour
{
    public static HeroQueue Instance { get; private set; }

    [SerializeField] private HeroSlotUI[] _slots; // arrastrar los 4 HeroSlotUI desde el Inspector

    /// <summary>
    /// Número total de slots configurados en el Inspector.
    /// </summary>
    public int TotalSlots => _slots != null ? _slots.Length : 0;

    /// <summary>
    /// Límite de slots activos impuesto por un evento diario (DailyEventManager).
    /// -1 = sin límite (usa todos los slots configurados).
    /// </summary>
    public int SlotOverride { get; set; } = -1;

    // Cola de espera off-screen cuando los 4 slots están llenos
    private readonly Queue<HeroInstance>            _waitingQueue = new Queue<HeroInstance>();

    // Mapeo hero → slot para acceso O(1)
    private readonly Dictionary<HeroInstance, HeroSlotUI> _heroSlotMap =
        new Dictionary<HeroInstance, HeroSlotUI>();

    // ── Ciclo de vida ──────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnHeroArrived>    (HandleHeroArrived);
        EventBus.Subscribe<OnMissionAssigned>(HandleMissionAssigned);
        EventBus.Subscribe<OnHeroDied>       (HandleHeroDied);
        EventBus.Subscribe<OnHeroLeft>       (HandleHeroLeft);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnHeroArrived>    (HandleHeroArrived);
        EventBus.Unsubscribe<OnMissionAssigned>(HandleMissionAssigned);
        EventBus.Unsubscribe<OnHeroDied>       (HandleHeroDied);
        EventBus.Unsubscribe<OnHeroLeft>       (HandleHeroLeft);
    }

    // ── Handlers de eventos ────────────────────────────────────────────────────

    private void HandleHeroArrived    (OnHeroArrived e)     => TryFillSlot(e.Hero);
    private void HandleMissionAssigned(OnMissionAssigned e) => RemoveHero(e.Hero);
    private void HandleHeroDied       (OnHeroDied e)        => RemoveHero(e.Hero);
    private void HandleHeroLeft       (OnHeroLeft e)        => RemoveHero(e.Hero);

    // ── API pública ────────────────────────────────────────────────────────────

    /// <summary>
    /// Libera el slot del héroe y promueve el siguiente de la cola de espera.
    /// Si el héroe estaba en la cola off-screen lo elimina directamente.
    /// </summary>
    public void RemoveHero(HeroInstance hero)
    {
        if (hero == null) return;

        if (_heroSlotMap.TryGetValue(hero, out HeroSlotUI slot))
        {
            slot.ClearHero();
            _heroSlotMap.Remove(hero);
            TryPromoteFromQueue();
        }
        else
        {
            // Puede estar aún en la cola off-screen; reconstruirla sin ese héroe
            RemoveFromWaitingQueue(hero);
        }
    }

    /// <summary>Devuelve el slot que contiene el héroe, o null.</summary>
    public HeroSlotUI GetSlotForHero(HeroInstance hero)
    {
        _heroSlotMap.TryGetValue(hero, out HeroSlotUI slot);
        return slot;
    }

    // ── Lógica interna ─────────────────────────────────────────────────────────

    private void TryFillSlot(HeroInstance hero)
    {
        HeroSlotUI freeSlot = GetFreeSlot();
        if (freeSlot != null)
        {
            freeSlot.SetHero(hero);
            _heroSlotMap[hero] = freeSlot;
        }
        else
        {
            // Sin slot libre: el héroe espera en cola interna
            _waitingQueue.Enqueue(hero);
        }
    }

    private void TryPromoteFromQueue()
    {
        if (_waitingQueue.Count == 0) return;
        HeroSlotUI freeSlot = GetFreeSlot();
        if (freeSlot == null) return;

        HeroInstance next = _waitingQueue.Dequeue();
        freeSlot.SetHero(next);
        _heroSlotMap[next] = freeSlot;
    }

    private HeroSlotUI GetFreeSlot()
    {
        if (_slots == null) return null;
        int limit = (SlotOverride > 0) ? Mathf.Min(SlotOverride, _slots.Length) : _slots.Length;
        for (int i = 0; i < limit; i++)
            if (_slots[i] != null && !_slots[i].IsOccupied) return _slots[i];
        return null;
    }

    private void RemoveFromWaitingQueue(HeroInstance hero)
    {
        // Queue<T> no tiene Remove; reconstruir sin el héroe dado
        int count = _waitingQueue.Count;
        for (int i = 0; i < count; i++)
        {
            HeroInstance h = _waitingQueue.Dequeue();
            if (h != hero)
                _waitingQueue.Enqueue(h);
        }
    }
}
