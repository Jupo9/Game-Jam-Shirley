using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Minimal UI panel manager for phase A. One open panel at a time.
/// Escape closes the active panel. Optional pause preserves the previous time scale.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Game/UI/Game UI Manager")]
public class GameUIManager : MonoBehaviour
{
    [Header("Panels to hide at startup")]
    [SerializeField] private GameObject[] managedPanels;

    [Header("Opening a panel")]
    [SerializeField] private bool pauseWorldWhileOpen = true;

    private GameObject activePanel;
    private TriggerEvents owner;
    private float previousTimeScale = 1f;
    private bool hasOpenPanel;
    private bool appliedPause;

    public bool IsOpen => hasOpenPanel;

    private void Awake()
    {
        if (managedPanels == null)
        {
            return;
        }
        foreach (GameObject panel in managedPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!hasOpenPanel)
        {
            return;
        }

        // A panel can also be hidden/destroyed by another UI script.
        if (activePanel == null || !activePanel.activeInHierarchy)
        {
            CloseCurrentPanel();
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseCurrentPanel();
        }
    }

    public bool OpenPanel(GameObject panel, TriggerEvents source)
    {
        if (panel == null)
        {
            return false;
        }

        if (hasOpenPanel)
        {
            CloseCurrentPanel();
        }

        activePanel = panel;
        owner = source;
        hasOpenPanel = true;

        // The Board's OnEnable automatically refreshes its UI.
        panel.SetActive(true);
        if (pauseWorldWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            appliedPause = true;
        }
        return true;
    }

    public void CloseIfOwner(TriggerEvents source)
    {
        if (hasOpenPanel && source != null && owner == source)
        {
            CloseCurrentPanel();
        }
    }

    public void CloseCurrentPanel()
    {
        if (!hasOpenPanel)
        {
            return;
        }

        GameObject panel = activePanel;
        activePanel = null;
        owner = null;
        hasOpenPanel = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (appliedPause)
        {
            Time.timeScale = previousTimeScale;
            appliedPause = false;
        }
    }

    private void OnDisable() => CloseCurrentPanel();
}
