using UnityEngine;

public class ClosePanel : MonoBehaviour
{
    [Header("Panel Configs")]
    [Tooltip("Choose the correct panel you want to close with this button")]
    [SerializeField] private GameObject panelToClose;

    public void DeactivatePanel()
    {
        Time.timeScale = 1f;
        panelToClose.SetActive(false);
    }
}
