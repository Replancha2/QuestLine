using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI del evento diario:
///   - Panel de anuncio: aparece al inicio del día durante 2 segundos o hasta click.
///   - Icono HUD: visible en el top-bar durante todo el día.
///
/// Setup en Inspector (conectar en Tarea 4.1):
///   _announcementPanel  → Panel con título + descripción del evento
///   _titleText          → Text / TMP del título
///   _descriptionText    → Text / TMP de la descripción
///   _hudIcon            → Image del icono en el top-bar
///   _hudIconImage       → Image que muestra el sprite del evento
///   _closeButton        → Button para cerrar el panel antes de los 2s (opcional)
///
/// Escucha: OnDailyEventActivated, OnDayEnd
/// </summary>
public class DailyEventUI : MonoBehaviour
{
    // ── Inspector (asignar en Tarea 4.1 al construir el HUD) ─────────────────

    [Header("Panel de anuncio (inicio del día)")]
    [SerializeField] private GameObject _announcementPanel;
    [SerializeField] private TMP_Text   _titleText;
    [SerializeField] private TMP_Text   _descriptionText;
    [SerializeField] private Image      _announcementIcon;
    [SerializeField] private Button     _closeButton;

    [Header("Icono HUD (visible todo el día)")]
    [SerializeField] private GameObject _hudIconRoot;
    [SerializeField] private Image      _hudIconImage;

    // ── Constantes ────────────────────────────────────────────────────────────

    private const float AnnouncementDuration = 10f;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        HideAll();

        Debug.Log("_closeButton is " + (_closeButton != null ? "assigned" : "null"));
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(HideAnnouncement);
            Debug.Log("Listener added to _closeButton");
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnDailyEventActivated>(HandleEventActivated);
        EventBus.Subscribe<OnDayEnd>(HandleDayEnd);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnDailyEventActivated>(HandleEventActivated);
        EventBus.Unsubscribe<OnDayEnd>(HandleDayEnd);
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private void HandleEventActivated(OnDailyEventActivated e)
    {
        Debug.Log("HandleEventActivated called with event: " + (e.Event != null ? e.Event.EventTitle : "null"));
        if (e.Event == null) return;

        // Actualizar textos e ícono del panel de anuncio
        if (_titleText        != null) _titleText.text       = e.Event.EventTitle;
        if (_descriptionText  != null) _descriptionText.text = e.Event.EventDescription;
        if (_announcementIcon != null) _announcementIcon.sprite = e.Event.EventIcon;

        // Actualizar ícono HUD
        if (_hudIconImage != null) _hudIconImage.sprite = e.Event.EventIcon;
        if (_hudIconRoot  != null) _hudIconRoot.SetActive(true);

        // Mostrar panel de anuncio y pausar el juego hasta que el panel se cierre
        if (_announcementPanel != null)
        {
            _announcementPanel.SetActive(true);
            Time.timeScale = 0f;
            StopAllCoroutines();
            StartCoroutine(AutoHideAnnouncement());
        }
    }

    private void HandleDayEnd(OnDayEnd e)
    {
        HideAll();
        Time.timeScale = 1f;
    }

    // ── Coroutine ─────────────────────────────────────────────────────────────

    private IEnumerator AutoHideAnnouncement()
    {
        yield return new WaitForSeconds(AnnouncementDuration);
        HideAnnouncement();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void HideAnnouncement()
    {
        Debug.Log("HideAnnouncement called");
        StopAllCoroutines();
        if (_announcementPanel != null) _announcementPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void HideAll()
    {
        if (_announcementPanel != null) _announcementPanel.SetActive(false);
        if (_hudIconRoot       != null) _hudIconRoot.SetActive(false);
        Time.timeScale = 1f;
    }
}
