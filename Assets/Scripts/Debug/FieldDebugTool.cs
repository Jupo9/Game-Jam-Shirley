using UnityEngine;

/// <summary>
/// Optional draggable, collapsible Play Mode readout for a field and elapsed time.
/// No gameplay input: E remains owned by PlayerInteractionController.
/// </summary>
[AddComponentMenu("Game/Debug/Field Debug Tool")]
public class FieldDebugTool : MonoBehaviour
{
    [SerializeField] private FieldController field;
    [SerializeField] private WorldTimeTracker worldTime;
    [SerializeField] private bool expanded = true;
    [SerializeField] private Vector2 startPosition = new Vector2(370f, 15f);

    private Rect windowRect;

    private void Awake()
    {
        windowRect = new Rect(startPosition.x, startPosition.y, 310f, 170f);
    }

    private void OnGUI()
    {
        windowRect.height = expanded ? 170f : 42f;
        windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, string.Empty);
        windowRect.x = Mathf.Clamp(windowRect.x, 0f, Mathf.Max(0f, Screen.width - windowRect.width));
        windowRect.y = Mathf.Clamp(windowRect.y, 0f, Mathf.Max(0f, Screen.height - windowRect.height));
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("FIELD - DEBUG");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(expanded ? "-" : "+", GUILayout.Width(30f)))
            expanded = !expanded;
        GUILayout.EndHorizontal();

        if (expanded)
        {
            GUILayout.Label("Total elapsed slots: " +
                (worldTime != null ? worldTime.TotalElapsedSlots.ToString() : "Tracker not assigned"));

            if (field == null)
            {
                GUILayout.Label("Assign a FieldController.");
            }
            else
            {
                GUILayout.Label("State: " + field.State);
                GUILayout.Label("Plant: " +
                    (field.PlantedPlant != null ? field.PlantedPlant.name : "None"));
                GUILayout.Label("Growth: " + field.ElapsedGrowthSlots + "/" +
                    field.RequiredGrowthSlots + " slots (" +
                    Mathf.RoundToInt(field.GrowthProgress * 100f) + "%)");
            }
        }

        GUI.DragWindow(new Rect(0f, 0f, windowRect.width - 55f, 28f));
    }
}
