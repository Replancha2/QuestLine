using System;
using System.Collections.Generic;

/// <summary>
/// EventBus genérico y estático para comunicación desacoplada entre sistemas.
/// Uso:
///   EventBus.Subscribe&lt;OnMissionCompleted&gt;(handler);
///   EventBus.Publish(new OnMissionCompleted { ... });
///   EventBus.Unsubscribe&lt;OnMissionCompleted&gt;(handler);
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

    /// <summary>Registra un handler para el tipo de evento T.</summary>
    public static void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<Delegate>();

        _handlers[type].Add(handler);
    }

    /// <summary>Elimina el handler del tipo de evento T.</summary>
    public static void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var list))
            list.Remove(handler);
    }

    /// <summary>
    /// Publica un evento a todos los suscriptores registrados.
    /// Itera sobre una copia de la lista para soportar (des)suscripción durante el dispatch.
    /// </summary>
    public static void Publish<T>(T eventData)
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var list) || list.Count == 0)
            return;

        // Copia para evitar errores si un handler se suscribe/desuscribe durante la iteración
        var snapshot = new List<Delegate>(list);
        foreach (var handler in snapshot)
            ((Action<T>)handler)?.Invoke(eventData);
    }

    /// <summary>
    /// Elimina todos los handlers. Útil al cambiar de escena o en tests.
    /// </summary>
    public static void Clear()
    {
        _handlers.Clear();
    }
}
