using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente de paciencia para un slot de héroe.
/// Gestiona el timer de cuenta atrás y actualiza la barra visual.
///
/// Cuando el timer llega a 0 publica OnHeroLeft { Hero, WasAngry = true }.
/// GameManager escucha ese evento y llama LoseLife().
///
/// Impulsive: barra roja desde el inicio (señaliza urgencia visual).
/// Nervous threshold (<20%): dispara TriggerNervousAnimation() una sola vez.
/// </summary>
public class HeroPatience : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider _patienceBar;
    [SerializeField] private Image  _patienceBarFill;

    [Header("Colores de la barra")]
    [SerializeField] private Color _colorHigh   = Color.green;
    [SerializeField] private Color _colorMedium = new Color(1f, 0.65f, 0f); // naranja
    [SerializeField] private Color _colorLow    = Color.red;

    private HeroInstance _hero;
    private float        _maxPatience;
    private float        _remaining;
    private bool         _isActive;
    private bool         _expired;
    private bool         _nervousFired;

    private const float NervousThreshold = 0.20f;

    // ── API pública ────────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia el timer para el héroe dado.
    /// Aplica BuffManager.GetPatienceMultiplier() si el singleton existe.
    /// </summary>
    public void Activate(HeroInstance hero)
    {
        _hero     = hero;
        _expired  = false;
        _nervousFired = false;
        _isActive = true;

        float multiplier = BuffManager.Instance != null
            ? BuffManager.Instance.GetPatienceMultiplier()
            : 1f;
        _maxPatience = hero.Patience * multiplier;
        _remaining   = _maxPatience;

        // Impulsive: barra roja desde el inicio para señalizar urgencia
        if (_patienceBarFill != null && hero.Traits.Contains(HeroTrait.Impulsive))
            _patienceBarFill.color = _colorLow;

        UpdateBar();
    }

    /// <summary>Pausa el timer sin publicar evento (ej: al asignar misión).</summary>
    public void Deactivate()
    {
        _isActive = false;
    }

    /// <summary>Porcentaje de paciencia restante [0, 1].</summary>
    public float RatioRemaining =>
        _maxPatience > 0f ? _remaining / _maxPatience : 0f;

    // ── Update ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!_isActive || _expired) return;

        _remaining -= Time.deltaTime;

        if (_remaining <= 0f)
        {
            _remaining = 0f;
            _expired   = true;
            _isActive  = false;
            Expire();
        }
        else
        {
            UpdateBar();
            CheckNervousThreshold();
        }
    }

    // ── Lógica interna ─────────────────────────────────────────────────────────

    private void UpdateBar()
    {
        if (_patienceBar == null) return;

        float ratio = RatioRemaining;
        _patienceBar.value = ratio;

        if (_patienceBarFill == null) return;

        // No sobrescribir si Impulsive (permanece rojo)
        if (_hero != null && _hero.Traits.Contains(HeroTrait.Impulsive)) return;

        if (ratio > 0.5f)
            _patienceBarFill.color = _colorHigh;
        else if (ratio > NervousThreshold)
            _patienceBarFill.color = _colorMedium;
        else
            _patienceBarFill.color = _colorLow;
    }

    private void CheckNervousThreshold()
    {
        if (_nervousFired) return;
        if (RatioRemaining <= NervousThreshold)
        {
            _nervousFired = true;
            TriggerNervousAnimation();
        }
    }

    /// <summary>
    /// Placeholder para animación de nerviosismo.
    /// Implementar en Fase 4/5 con Animator o DOTween.
    /// </summary>
    private void TriggerNervousAnimation()
    {
        // TODO (Fase 4/5): shake o loop de animación de nerviosismo
        Debug.Log($"[HeroPatience] {_hero?.Name} está nervioso (<20% paciencia restante).");
    }

    private void Expire()
    {
        UpdateBar(); // asegurar barra en 0
        EventBus.Publish(new OnHeroLeft { Hero = _hero, WasAngry = true });
    }
}
