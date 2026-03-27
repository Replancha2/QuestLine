using System.Collections;
using UnityEngine;

/// <summary>
/// Genera héroes proceduralmente a intervalos regulares y publica OnHeroArrived.
/// HeroQueue escucha ese evento para asignarlos a slots.
///
/// Intervalo efectivo = SpawnIntervalBase * SpawnIntervalMultiplier (mín: SpawnIntervalMin).
/// SpawnIntervalMultiplier puede ser modificado por DailyEventManager (eventos de spawn).
///
/// Stats generados escalan con DayNumber:
///   heroStat = Random.Range(2 + dayNumber, 5 + dayNumber * 2)
/// </summary>
public class HeroSpawner : MonoBehaviour
{
    public static HeroSpawner Instance { get; private set; }

    [SerializeField] private HeroQueueConfig _config;

    /// <summary>
    /// Modificador de velocidad de spawn. 1.0 = normal.
    /// Valores menores a 1.0 aceleran el spawn; valores mayores lo retrasan.
    /// Modificado por DailyEventManager para eventos de tipo SpawnSpeed.
    /// </summary>
    public float SpawnIntervalMultiplier { get; set; } = 1f;

    private Coroutine _spawnCoroutine;
    private bool _isSpawning;

    // ── Ciclo de vida ──────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnDayStart>(HandleDayStart);
        EventBus.Subscribe<OnDayEnd>(HandleDayEnd);
        EventBus.Subscribe<OnGameOver>(HandleGameOver);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnDayStart>(HandleDayStart);
        EventBus.Unsubscribe<OnDayEnd>(HandleDayEnd);
        EventBus.Unsubscribe<OnGameOver>(HandleGameOver);
        StopSpawning();
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private void HandleDayStart(OnDayStart e) => StartSpawning();
    private void HandleDayEnd(OnDayEnd e)     => StopSpawning();
    private void HandleGameOver(OnGameOver e) => StopSpawning();

    // ── API pública ────────────────────────────────────────────────────────────

    public void StartSpawning()
    {
        if (_isSpawning) return;
        _isSpawning = true;
        _spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        _isSpawning = false;
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    // ── Lógica interna ────────────────────────────────────────────────────────

    private IEnumerator SpawnLoop()
    {
        // 4 heroes spawnean inmediato al arrancar el día
        SpawnHero();
        SpawnHero();
        SpawnHero();
        SpawnHero();

        while (_isSpawning)
        {
            float interval = Mathf.Max(
                _config.SpawnIntervalMin,
                _config.SpawnIntervalBase * SpawnIntervalMultiplier);

            yield return new WaitForSeconds(interval);

            if (_isSpawning)
                SpawnHero();
        }
    }

    /// <summary>
    /// Elige un template al azar, genera un HeroInstance con stats escalados
    /// por DayNumber y publica OnHeroArrived.
    /// </summary>
    public void SpawnHero()
    {
        if (_config == null || _config.HeroTemplates == null || _config.HeroTemplates.Length == 0)
        {
            Debug.LogWarning("[HeroSpawner] No hay HeroTemplates configurados en HeroQueueConfig.");
            return;
        }

        int idx = Random.Range(0, _config.HeroTemplates.Length);
        HeroData template = _config.HeroTemplates[idx];

        HeroInstance hero = HeroInstance.Generate(template);

        // Escalar stats según día: heroStat = Random.Range(2 + day, 5 + day * 2)
        int day = GameManager.Instance != null ? GameManager.Instance.DayNumber : 1;
        int min = 2 + day;
        int max = 5 + day * 2;
        hero.Strength     = Random.Range(min, max + 1);
        hero.Dexterity    = Random.Range(min, max + 1);
        hero.Intelligence = Random.Range(min, max + 1);
        hero.Charisma     = Random.Range(min, max + 1);

        // Escalar paciencia: heroes tienen menos paciencia conforme avanzan los días
        // (dejan de esperar más rápido, forzando decisiones más rápidas)
        float patienceMultiplier = 1f / (1f + (day - 1) * 0.1f); // Día 1: 1x, Día 2: ~0.9x, Día 10: ~0.5x
        hero.Patience *= patienceMultiplier;

        EventBus.Publish(new OnHeroArrived { Hero = hero });
    }
}
