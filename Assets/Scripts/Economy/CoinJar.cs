using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente UI del frasco de monedas visible en pantalla.
/// Es el punto de entrada para añadir y gastar monedas — delega el estado
/// real a GameManager (fuente de verdad) y se encarga de las animaciones.
///
/// Escucha:
///   - OnMissionCompleted → AddCoins(e.CoinsEarned)
///   - OnCoinEarned       → actualiza display + animación moneda entrando
///   - OnCoinSpent        → actualiza display + shake rojo
///
/// API pública:
///   - AddCoins(int)      → usado internamente desde OnMissionCompleted
///   - SpendCoins(int)    → llamado directamente por ShopManager antes de publicar OnBuffPurchased
/// </summary>
public class CoinJar : MonoBehaviour
{
    public static CoinJar Instance { get; private set; }

    // ── Referencias UI ───────────────────────────────────────────────────────

    [Header("Referencias UI")]
    [SerializeField] private TMP_Text      _coinCountText;
    [SerializeField] private Image         _jarImage;
    [SerializeField] private RectTransform _jarRect;

    [Header("Animación — moneda voladora")]
    [SerializeField] private GameObject    _coinFlyPrefab;   // Prefab de moneda que vuela hacia el frasco
    [SerializeField] private RectTransform _coinSpawnPoint;  // Punto de origen de la moneda (ej: centro del tablero)

    [Header("Animación — shake")]
    [SerializeField] private float _shakeDuration  = 0.3f;
    [SerializeField] private float _shakeIntensity = 10f;

    [Header("Animación — coin fly")]
    [SerializeField] private float _coinFlyDuration = 0.5f;

    // ── Estado ───────────────────────────────────────────────────────────────

    /// <summary>Coins actuales — sincronizado con GameManager.</summary>
    public int CurrentCoins => GameManager.Instance != null ? GameManager.Instance.CurrentCoins : 0;

    private Coroutine _shakeCoroutine;

    // ── Ciclo de vida ────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnMissionCompleted>(HandleMissionCompleted);
        EventBus.Subscribe<OnCoinEarned>(HandleCoinEarned);
        EventBus.Subscribe<OnCoinSpent>(HandleCoinSpent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnMissionCompleted>(HandleMissionCompleted);
        EventBus.Unsubscribe<OnCoinEarned>(HandleCoinEarned);
        EventBus.Unsubscribe<OnCoinSpent>(HandleCoinSpent);
    }

    private void Start() => RefreshDisplay();

    // ── Handlers de eventos ──────────────────────────────────────────────────

    private void HandleMissionCompleted(OnMissionCompleted e)
    {
        if (e.CoinsEarned > 0)
            AddCoins(e.CoinsEarned);
    }

    private void HandleCoinEarned(OnCoinEarned e)
    {
        RefreshDisplay();
        PlayEarnAnimation();
    }

    private void HandleCoinSpent(OnCoinSpent e)
    {
        RefreshDisplay();
        PlaySpendAnimation();
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Añade monedas. Delega a GameManager que actualizará CurrentCoins
    /// y publicará OnCoinEarned (que dispara la animación).
    /// </summary>
    public void AddCoins(int amount)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCoins(amount);
    }

    /// <summary>
    /// Intenta gastar monedas. Devuelve false si no hay saldo suficiente
    /// (y dispara animación de rechazo). Llamado directamente por ShopManager.
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (GameManager.Instance == null) return false;

        bool success = GameManager.Instance.SpendCoins(amount);

        if (!success)
            PlayInsufficientCoinsAnimation();
        // Si tiene éxito, la animación se dispara vía OnCoinSpent

        return success;
    }

    // ── Display ──────────────────────────────────────────────────────────────

    private void RefreshDisplay()
    {
        if (_coinCountText != null)
            _coinCountText.text = CurrentCoins.ToString();
    }

    // ── Animaciones ──────────────────────────────────────────────────────────

    private void PlayEarnAnimation()
    {
        if (_coinFlyPrefab != null && _coinSpawnPoint != null)
            StartCoroutine(AnimateCoinFly());
    }

    /// <summary>
    /// Anima una moneda que vuela desde el punto de origen hasta el frasco
    /// siguiendo una trayectoria curva (Bézier cuadrática).
    /// </summary>
    private IEnumerator AnimateCoinFly()
    {
        GameObject coin = Instantiate(_coinFlyPrefab, transform.parent);
        var coinRect = coin.GetComponent<RectTransform>();
        if (coinRect == null)
        {
            Destroy(coin);
            yield break;
        }

        Vector3 startPos  = _coinSpawnPoint.position;
        Vector3 targetPos = _jarRect != null ? _jarRect.position : transform.position;
        Vector3 arcMid    = (startPos + targetPos) * 0.5f + Vector3.up * 60f;

        coinRect.position = startPos;

        float elapsed = 0f;
        while (elapsed < _coinFlyDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _coinFlyDuration);
            coinRect.position   = QuadraticBezier(startPos, arcMid, targetPos, t);
            coinRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.4f, t);
            yield return null;
        }

        Destroy(coin);
    }

    private void PlaySpendAnimation()
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeAndFlashRed());
    }

    private void PlayInsufficientCoinsAnimation()
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeAndFlashRed());
    }

    /// <summary>Shake horizontal + texto rojo temporal cuando se gasta o se intenta gastar sin saldo.</summary>
    private IEnumerator ShakeAndFlashRed()
    {
        if (_coinCountText != null)
            _coinCountText.color = Color.red;

        RectTransform rt = _jarRect != null ? _jarRect : GetComponent<RectTransform>();
        Vector3 originalPos = rt.localPosition;

        float elapsed = 0f;
        while (elapsed < _shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _shakeDuration;
            float x = Mathf.Sin(t * Mathf.PI * 2f * 8f) * _shakeIntensity * (1f - t);
            rt.localPosition = originalPos + new Vector3(x, 0f, 0f);
            yield return null;
        }

        rt.localPosition = originalPos;

        if (_coinCountText != null)
            _coinCountText.color = Color.white;

        _shakeCoroutine = null;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }
}
