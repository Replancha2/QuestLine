using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Instancia en runtime de una misión concreta.
/// Creada por <see cref="MissionInstance.FromData"/> al construir la mano del día.
/// </summary>
public class MissionInstance
{
    public MissionData Template;
    public int         DayNumber;    // Día en que fue generada (afecta el requisito calculado)
    public bool        IsCompleted;
    public bool        IsFailed;

    // Índices de las pistas de Template.Hints que ya han sido reveladas al jugador.
    // Empieza vacío; se puebla con RevealHint() al usar un consumible o recibir un evento.
    public List<int> RevealedHintIndices;

    // -----------------------------------------------------------------------
    // Requisitos calculados por fórmula (NO hardcodeados en el SO)
    // -----------------------------------------------------------------------

    /// <summary>Requisito efectivo del stat primario para el día en que fue generada la misión.</summary>
    public int GetEffectivePrimaryReq()
        => Mathf.RoundToInt((int)Template.Rank * (1.5f + DayNumber * 0.2f));

    /// <summary>Requisito efectivo de los stats secundarios (80 % del primario).</summary>
    public int GetEffectiveSecondaryReq()
        => Mathf.RoundToInt(GetEffectivePrimaryReq() * 0.8f);

    // -----------------------------------------------------------------------
    // Gestión de pistas
    // -----------------------------------------------------------------------

    /// <summary>
    /// Revela la pista en el índice dado.
    /// Llama a este método al usar un consumible de revelación o al recibir un evento.
    /// </summary>
    public void RevealHint(int hintIndex)
    {
        if (Template.Hints == null || hintIndex < 0 || hintIndex >= Template.Hints.Length)
            return;

        if (!RevealedHintIndices.Contains(hintIndex))
            RevealedHintIndices.Add(hintIndex);
    }

    /// <summary>Revela todas las pistas de la misión a la vez (consumible FullStatReveal).</summary>
    public void RevealAllHints()
    {
        if (Template.Hints == null) return;
        for (int i = 0; i < Template.Hints.Length; i++)
            RevealHint(i);
    }

    /// <summary>Devuelve solo las pistas actualmente reveladas al jugador.</summary>
    public List<MissionHint> GetRevealedHints()
    {
        var result = new List<MissionHint>();
        if (Template.Hints == null) return result;

        foreach (int index in RevealedHintIndices)
        {
            if (index >= 0 && index < Template.Hints.Length)
                result.Add(Template.Hints[index]);
        }
        return result;
    }

    /// <summary>True si hay al menos una pista revelada.</summary>
    public bool HasAnyRevealedHint() => RevealedHintIndices.Count > 0;

    // -----------------------------------------------------------------------
    // Fábrica
    // -----------------------------------------------------------------------

    /// <summary>Crea una MissionInstance lista para usar a partir de un MissionData asset.</summary>
    public static MissionInstance FromData(MissionData data, int dayNumber)
    {
        return new MissionInstance
        {
            Template             = data,
            DayNumber            = dayNumber,
            IsCompleted          = false,
            IsFailed             = false,
            RevealedHintIndices  = new List<int>(),
        };
    }
}
