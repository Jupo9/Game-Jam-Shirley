using UnityEngine;

[AddComponentMenu("Game/Debug/Day Cycle Debug Tool")]
public class DayCylcleDebugTool : MonoBehaviour
{
    [SerializeField] private DayCycleManager dayCycle;
    [SerializeField] private bool showTestButtons = true;

    [Header("Debug Window")]
    [SerializeField] private bool isExpanded = true;

    [SerializeField] private Vector2 startPosition = new Vector2(15f, 15f);

    private Rect windowRect;

    private void Awake()
    {
        windowRect = new Rect(startPosition.x, startPosition.y, 330f, 205f);
    }

    private void OnGUI()
    {
        if (dayCycle == null)
            return;

        // Adjust window height depending on its content
        windowRect.height = isExpanded
            ? (dayCycle.IsConfigured
                ? (showTestButtons ? 205f : 155f)
                : 90f)
            : 42f;

        // Draw movable window
        windowRect = GUI.Window(
            GetInstanceID(),
            windowRect,
            DrawDebugWindow,
            ""
        );

        // Keep window on screen
        windowRect.x = Mathf.Clamp(
            windowRect.x,
            0f,
            Mathf.Max(0f, Screen.width - windowRect.width)
        );

        windowRect.y = Mathf.Clamp(
            windowRect.y,
            0f,
            Mathf.Max(0f, Screen.height - windowRect.height)
        );
    }

    private void DrawDebugWindow(int windowID)
    {
        // HEADER
        GUILayout.BeginHorizontal();

        GUILayout.Label("DAY CYCLE - DEBUG");
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(isExpanded ? "-" : "+", GUILayout.Width(30f)))
        {
            isExpanded = !isExpanded;
        }

        GUILayout.EndHorizontal();

        if (isExpanded)
        {
            DrawDebugContent();
        }

        // Allow dragging using the header area.
        // The right side is reserved for the +/- button.
        GUI.DragWindow(new Rect(0f, 0f, windowRect.width - 55f, 28f));
    }

    private void DrawDebugContent()
    {
        if (!dayCycle.IsConfigured)
        {
            GUILayout.Label("Assign a DaySchedule with days and time slots.");
            return;
        }

        GUILayout.Label(
            "Day: " + dayCycle.CurrentDayName +
            "  (" + (dayCycle.CurrentDayIndex + 1) +
            "/" + dayCycle.TotalDays + ")"
        );

        GUILayout.Label("Time: " + dayCycle.CurrentSlotName);

        GUILayout.Label(
            "Used slots: " +
            dayCycle.SlotsConsumedToday +
            "/" + dayCycle.TotalSlotsPerDay
        );

        GUILayout.Label(
            dayCycle.IsDayComplete
                ? "Day complete - end the day!"
                : "Remaining: " + dayCycle.SlotsRemainingToday
        );

        if (!showTestButtons)
            return;

        bool oldGuiEnabled = GUI.enabled;

        GUI.enabled = dayCycle.CanConsumeSlot;

        if (GUILayout.Button("Consume Slot (Debug)"))
        {
            dayCycle.ConsumeSlot();
        }

        GUI.enabled = oldGuiEnabled;

        if (GUILayout.Button("End Day (Debug - any time)"))
        {
            dayCycle.EndDay();
        }
    }
}