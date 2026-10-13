using UnityEngine;

[AddComponentMenu("Game/Debug/Day Cycle Debug Tool")]
public class DayCylcleDebugTool : MonoBehaviour
{
    [SerializeField] private DayCycleManager dayCycle;
    [SerializeField] private bool showTestButtons = true;

    private void OnGUI()
    {
        if (dayCycle == null)
        {
            return;
        }

        GUILayout.BeginArea(new Rect(15f, 15f, 330f, 190f), GUI.skin.box);
        GUILayout.Label("DAY CYCLE - DEBUG");

        if (!dayCycle.IsConfigured)
        {
            GUILayout.Label("Assign a DaySchedule with days and time slots.");
            GUILayout.EndArea();
            return;
        }

        GUILayout.Label("Day: " + dayCycle.CurrentDayName + "  (" + (dayCycle.CurrentDayIndex + 1) + "/" + dayCycle.TotalDays + ")");
        GUILayout.Label("Time: " + dayCycle.CurrentSlotName);
        GUILayout.Label("Used slots: " + dayCycle.SlotsConsumedToday + "/" + dayCycle.TotalSlotsPerDay);
        GUILayout.Label(dayCycle.IsDayComplete ? "Day complete - end the day!" : "Remaining: " + dayCycle.SlotsRemainingToday);

        if (showTestButtons)
        {
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

        GUILayout.EndArea();
    }
}
