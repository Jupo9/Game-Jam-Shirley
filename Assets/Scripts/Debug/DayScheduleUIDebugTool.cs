using UnityEngine;

[AddComponentMenu("Game/Debug/Day Schedule UI Debug Toggle")]
public class DayScheduleUIDebugTool : MonoBehaviour
{
    [SerializeField] private DayScheduleUI dayScheduleUI;

    private void OnGUI()
    {
        if (dayScheduleUI == null)
        {
            return;
        }

        GUILayout.BeginArea(new Rect(Screen.width - 205f, 15f, 190f, 55f));
        if (GUILayout.Button(dayScheduleUI.gameObject.activeSelf ? "Close UI (Debug)" : "Open UI (Debug)",
            GUILayout.Height(36f)))
        {
            dayScheduleUI.Toggle();
        }
        GUILayout.EndArea();
    }
}
