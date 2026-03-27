using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona el inventario de consumibles de un solo uso del jugador.
///
/// Uso:
///   ConsumableManager.Instance.AddConsumable(consumableData);
///   ConsumableManager.Instance.UseConsumable(ConsumableType.StatRevealHand);
/// </summary>
public class ConsumableManager : MonoBehaviour
{
    public static ConsumableManager Instance { get; private set; }

    // ── Estado ──────────────────────────────────────────────────────────────

    /// <summary>Cantidad disponible por tipo de consumible.</summary>
    private readonly Dictionary<ConsumableType, int> _inventory =
        new Dictionary<ConsumableType, int>();

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Añade un consumible al inventario del jugador.
    /// Llamar desde ShopManager cuando el jugador compra uno.
    /// </summary>
    public void AddConsumable(ConsumableData data)
    {
        if (data == null) return;

        if (!_inventory.ContainsKey(data.Type))
            _inventory[data.Type] = 0;

        _inventory[data.Type]++;
    }

    /// <summary>
    /// Devuelve la cantidad disponible del tipo indicado.
    /// </summary>
    public int GetCount(ConsumableType type)
    {
        _inventory.TryGetValue(type, out int count);
        return count;
    }

    /// <summary>
    /// Usa un consumible del inventario.
    ///
    /// StatRevealHand  → revela el PrimaryStat (pista índice 0) en TODAS las misiones en mano.
    ///                   Se necesita una referencia al mazo activo; se obtiene via MissionDeck.Instance.
    /// FullStatReveal  → revela todas las pistas (índices 0 y 1) de la misión objetivo.
    ///
    /// Publica OnConsumableUsed tras ejecutar el efecto.
    /// </summary>
    /// <param name="type">Tipo de consumible a usar.</param>
    /// <param name="targetMission">
    ///   Misión objetivo para FullStatReveal; null para StatRevealHand.
    /// </param>
    /// <returns>True si el consumible se usó correctamente; false si no hay stock.</returns>
    public bool UseConsumable(ConsumableType type, MissionInstance targetMission = null)
    {
        if (GetCount(type) <= 0)
        {
            Debug.LogWarning($"[ConsumableManager] Intento de usar {type} sin stock.");
            return false;
        }

        // Ejecutar efecto
        switch (type)
        {
            case ConsumableType.StatRevealHand:
                RevealPrimaryStatInHand();
                break;

            case ConsumableType.FullStatReveal:
                if (targetMission == null)
                {
                    Debug.LogWarning("[ConsumableManager] FullStatReveal requiere una misión objetivo.");
                    return false;
                }
                targetMission.RevealHint(0);
                targetMission.RevealHint(1);
                break;

            default:
                Debug.LogWarning($"[ConsumableManager] Tipo de consumible no manejado: {type}");
                return false;
        }

        // Descontar del inventario
        _inventory[type]--;

        // Publicar evento
        EventBus.Publish(new OnConsumableUsed
        {
            Type          = type,
            TargetMission = targetMission,
        });

        return true;
    }

    // ── Helpers privados ────────────────────────────────────────────────────

    /// <summary>
    /// Revela la pista índice 0 (PrimaryStat) de todas las misiones en la mano activa.
    /// Depende de MissionDeck.Instance para obtener la mano actual.
    /// </summary>
    private void RevealPrimaryStatInHand()
    {
        if (MissionDeck.Instance == null)
        {
            Debug.LogWarning("[ConsumableManager] MissionDeck.Instance no disponible.");
            return;
        }

        foreach (var mission in MissionDeck.Instance.CurrentHand)
        {
            mission.RevealHint(0);
        }
    }
}
