using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(DaySchedule))]
public class DayScheduleEditor : Editor
{
    private DaySchedule schedule;

    private readonly Dictionary<string, Vector2> dayScrollPositions = new();

    private const float NeighborColumnWidth = 145f;
    private const float SlotWidth = 70f;
    private const float RowHeight = 24f;

    private void OnEnable()
    {
        schedule = (DaySchedule)target;

        if (schedule != null && schedule.EnsureDataIntegrity())
        {
            SaveChangedData();
        }
    }

    public override void OnInspectorGUI()
    {
        if (schedule == null)
        { 
            return;
        }

        DrawNeighborSection();
        EditorGUILayout.Space(10);
        DrawTimeSlotSection();
        EditorGUILayout.Space(10);
        DrawSlotStatusSection();
        EditorGUILayout.Space(15);
        DrawDays();
        EditorGUILayout.Space(10);
        DrawAddDayButton();
    }

    // ==================================================
    // NEIGHBORS
    // ==================================================

    private void DrawNeighborSection()
    {
        EditorGUILayout.LabelField("Neighbors", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Name", EditorStyles.miniBoldLabel);
        GUILayout.Label(new GUIContent("Active", "Participates in the game; keeps schedule when disabled."),
                        EditorStyles.miniBoldLabel, GUILayout.Width(48));
        GUILayout.Label(new GUIContent("Board", "Visible on the board while Active is enabled."),
                        EditorStyles.miniBoldLabel, GUILayout.Width(48));
        GUILayout.Space(34);

        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < schedule.Neighbors.Count; i++)
        {
            NeighborDefinition neighbor = schedule.Neighbors[i];
            EditorGUILayout.BeginHorizontal();

            string newName = EditorGUILayout.TextField(neighbor.Name);

            if (newName != neighbor.Name)
            {
                RecordUndo("Rename Neighbor");
                schedule.RenameNeighbor(i, newName);
                MarkDirty();
            }

            bool newActive = EditorGUILayout.Toggle(neighbor.Active, GUILayout.Width(48));
            if (newActive != neighbor.Active)
            {
                RecordUndo("Toggle Neighbor Active");
                schedule.SetNeighborActive(i, newActive);
                MarkDirty();
            }

            bool newBoard = EditorGUILayout.Toggle(neighbor.VisibleOnBoard, GUILayout.Width(48));
            if (newBoard != neighbor.VisibleOnBoard)
            {
                RecordUndo("Toggle Neighbor Board Visibility");
                schedule.SetNeighborVisibleOnBoard(i, newBoard);
                MarkDirty();
            }

            if (GUILayout.Button(new GUIContent("-", "Permanently delete this neighbor and its schedule"),
                GUILayout.Width(30)))
            {
                if (EditorUtility.DisplayDialog("Delete Neighbor?",
                                                "Deleting this neighbor also deletes all of their saved day/slot entries. " +
                                                "Use Active = false to temporarily remove the neighbor from the game.",
                                                "Delete", "Cancel"))
                {
                    RecordUndo("Remove Neighbor");
                    schedule.RemoveNeighborAt(i);
                    MarkDirty();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    GUIUtility.ExitGUI();
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Neighbor"))
        {
            RecordUndo("Add Neighbor");
            schedule.AddNeighbor();
            MarkDirty();
        }

        EditorGUILayout.HelpBox("Active = false: NPC is not active and is hidden from the board. " +
                                "Board can also hide an active NPC. All saved slots remain unchanged.",
                                MessageType.None);
        EditorGUILayout.EndVertical();
    }

    // ==================================================
    // TIMESLOTS
    // ==================================================

    private void DrawTimeSlotSection()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Global Timeslots", EditorStyles.boldLabel);

        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            RecordUndo("Add Timeslot");
            schedule.AddTimeSlot();
            MarkDirty();
        }

        EditorGUI.BeginDisabledGroup(schedule.TimeSlots.Count == 0);
        if (GUILayout.Button("-", GUILayout.Width(30)))
        {
            RecordUndo("Remove Last Timeslot");
            schedule.RemoveLastTimeSlot();
            MarkDirty();
        }

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");
        if (schedule.TimeSlots.Count == 0)
        { 
            EditorGUILayout.HelpBox("No timeslots yet. Press + to add one.", MessageType.Info);
        }

        for (int i = 0; i < schedule.TimeSlots.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Slot " + (i + 1), GUILayout.Width(60));

            string oldName = schedule.TimeSlots[i].Name;
            string newName = EditorGUILayout.TextField(oldName);
            if (newName != oldName)
            {
                RecordUndo("Rename Timeslot");
                schedule.RenameTimeSlot(i, newName);
                MarkDirty();
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    // ==================================================
    // CONFIGURABLE SLOT STATUSES
    // ==================================================

    private void DrawSlotStatusSection()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Slot States (Click Order)", EditorStyles.boldLabel);
        if (GUILayout.Button("+ Add State", GUILayout.Width(110)))
        {
            RecordUndo("Add Slot State");
            schedule.AddSlotStatus();
            MarkDirty();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.HelpBox("Cells cycle from Empty to Garden to the custom states below, then back to Empty. " +
                                "Empty and Garden are protected because older schedules use them.",
                                MessageType.None);

        for (int i = 0; i < schedule.SlotStatus.Count; i++)
        {
            SlotStatusDefinition status = schedule.SlotStatus[i];
            bool builtIn = i < 2;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label((i + 1).ToString() + ".", GUILayout.Width(22));

            if (builtIn)
            {
                GUILayout.Label(status.Name, GUILayout.MinWidth(110), GUILayout.ExpandWidth(true));
            }
            else
            {
                string name = EditorGUILayout.TextField(status.Name, GUILayout.MinWidth(110));
                if (name != status.Name)
                {
                    RecordUndo("Rename Slot State");
                    schedule.RenameSlotStatus(i, name);
                    MarkDirty();
                }
            }

            Color color = EditorGUILayout.ColorField(status.Color, GUILayout.Width(64));
            if (color != status.Color)
            {
                RecordUndo("Change Slot State Color");
                schedule.SetSlotStatusColor(i, color);
                MarkDirty();
            }

            string marker = EditorGUILayout.TextField(status.Marker ?? string.Empty, GUILayout.Width(34));
            if (marker != status.Marker)
            {
                RecordUndo("Change Slot State Marker");
                schedule.SetSlotStatusMarker(i, marker);
                MarkDirty();
            }

            EditorGUI.BeginDisabledGroup(builtIn || i == 2);
            bool moveUp = GUILayout.Button("▲", GUILayout.Width(25));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(builtIn || i == schedule.SlotStatus.Count - 1);
            bool moveDown = GUILayout.Button("▼", GUILayout.Width(25));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(builtIn);
            bool remove = GUILayout.Button("-", GUILayout.Width(25));
            EditorGUI.EndDisabledGroup();

            if (moveUp || moveDown)
            {
                RecordUndo("Reorder Slot State");
                schedule.MoveSlotStatus(i, moveUp ? -1 : 1);
                MarkDirty();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUIUtility.ExitGUI();
                return;
            }

            if (remove && EditorUtility.DisplayDialog("Delete Slot State?",
                                                      "Every existing cell that uses '" + status.Name +
                                                      "' will be reset to Empty. This cannot be undone except via Ctrl+Z.",
                                                      "Delete State", "Cancel"))
            {
                RecordUndo("Remove Slot State");
                schedule.RemoveSlotStatusAt(i);
                MarkDirty();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUIUtility.ExitGUI();
                return;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(27);
            GUILayout.Label("Destination", EditorStyles.miniLabel, GUILayout.Width(76));
            if (builtIn)
            {
                // Fixed meanings: Empty -> Exit, Garden -> that NPC's own garden.
                GUILayout.Label(status.DestinationType.ToString(), EditorStyles.miniLabel);
            }
            else
            {
                SlotDestinationType destination = (SlotDestinationType)EditorGUILayout.EnumPopup(
                                                   status.DestinationType, GUILayout.Width(120));
                if (destination != status.DestinationType)
                {
                    RecordUndo("Change Slot State Destination");
                    schedule.SetSlotStatusDestinationType(i, destination);
                    MarkDirty();
                }

                if (status.DestinationType == SlotDestinationType.SharedLocation)
                {
                    Transform targetTransform = (Transform)EditorGUILayout.ObjectField(status.SharedTarget, typeof(Transform), true);
                    if (targetTransform != status.SharedTarget)
                    {
                        RecordUndo("Assign Shared Location");
                        schedule.SetSlotStatusSharedTarget(i, targetTransform);
                        MarkDirty();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!builtIn && status.DestinationType == SlotDestinationType.SharedLocation && status.SharedTarget == null)
            {
                EditorGUILayout.HelpBox("Assign a scene Transform for this shared location " +
                                        "before NPC navigation is connected.",
                                        MessageType.Info);
            }

            if (i < schedule.SlotStatus.Count - 1)
            { 
                EditorGUILayout.Space(4);
            }
        }

        EditorGUILayout.EndVertical();
    }

    // ==================================================
    // DAYS
    // ==================================================

    private void DrawDays()
    {
        EditorGUILayout.LabelField("Days", EditorStyles.boldLabel);

        if (schedule.Days.Count == 0)
        {
            EditorGUILayout.HelpBox("No days created yet.",MessageType.Info);
            return;
        }

        for (int dayIndex = 0; dayIndex < schedule.Days.Count;dayIndex++)
        {
            DrawDay(dayIndex);
            EditorGUILayout.Space(10);
        }
    }

    private void DrawDay(int dayIndex)
    {
        DayData day = schedule.Days[dayIndex];
        EditorGUILayout.BeginVertical("box");
        DrawDayHeader(dayIndex, day);
        EditorGUILayout.Space(5);

        if (schedule.Neighbors.Count == 0)
        { 
            EditorGUILayout.HelpBox("Add at least one Neighbor.", MessageType.Info);
        }
        else if (schedule.TimeSlots.Count == 0)
        { 
            EditorGUILayout.HelpBox("Add at least one timeslot.", MessageType.Info);
        }
        else
        { 
            DrawScheduleGrid(dayIndex, day);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawDayHeader(int dayIndex,DayData day)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("DAY", EditorStyles.boldLabel, GUILayout.Width(40));

        string newName = EditorGUILayout.TextField(day.Name);
        if (newName != day.Name)
        {
            RecordUndo("Rename Day");
            schedule.RenameDay(dayIndex, newName);
            MarkDirty();
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Delete Day", GUILayout.Width(90)))
        {
            RecordUndo("Delete Day");
            schedule.RemoveDayAt(dayIndex);
            MarkDirty();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUIUtility.ExitGUI();
            return;
        }
        EditorGUILayout.EndHorizontal();
    }

    // ==================================================
    // DAY GRID
    // ==================================================

    private void DrawScheduleGrid(int dayIndex, DayData day)
    {
        Vector2 scrollPosition = GetScrollPosition(day.ID);
        float contentWidth = NeighborColumnWidth + schedule.TimeSlots.Count * SlotWidth + 20f;
        float contentHeight = (schedule.Neighbors.Count + 1) * RowHeight + 15f;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, true, false,
                         GUILayout.Height(Mathf.Min(contentHeight + 20f, 300f)));
        EditorGUILayout.BeginVertical(GUILayout.Width(contentWidth));

        DrawTimeSlotHeader();

        for (int neighborIndex = 0; neighborIndex < schedule.Neighbors.Count; neighborIndex++)
        {
            DrawNeighborRow(dayIndex, neighborIndex);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        dayScrollPositions[day.ID] = scrollPosition;
    }

    private void DrawTimeSlotHeader()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Neighbor", EditorStyles.boldLabel, GUILayout.Width(NeighborColumnWidth), GUILayout.Height(RowHeight));

        foreach (TimeSlotDefinition timeSlot in schedule.TimeSlots)
        {
            GUILayout.Label(timeSlot.Name, EditorStyles.boldLabel, GUILayout.Width(SlotWidth), GUILayout.Height(RowHeight));
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawNeighborRow(int dayIndex, int neighborIndex)
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(schedule.Neighbors[neighborIndex].Name, GUILayout.Width(NeighborColumnWidth), GUILayout.Height(RowHeight));

        for (int slotIndex = 0; slotIndex < schedule.TimeSlots.Count; slotIndex++)
        {
            SlotStatusDefinition status = schedule.GetSlotStatus(dayIndex, neighborIndex, slotIndex);

            Color previousBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = status != null ? status.Color : Color.white;

            string marker = status != null ? status.Marker ?? string.Empty : string.Empty;
            string tooltip = status != null ? status.Name : "Unknown State";

            if (GUILayout.Button(new GUIContent(marker, tooltip), GUILayout.Width(SlotWidth), GUILayout.Height(RowHeight)))
            {
                RecordUndo("Cycle Slot State");
                schedule.CycleSlotStatus(dayIndex, neighborIndex, slotIndex);
                MarkDirty();
            }

            GUI.backgroundColor = previousBackgroundColor;
        }

        EditorGUILayout.EndHorizontal();
    }

    // ==================================================
    // ADD DAY
    // ==================================================

    private void DrawAddDayButton()
    {
        if (GUILayout.Button("+ Add Day", GUILayout.Height(30)))
        {
            RecordUndo("Add Day");
            schedule.AddDay();
            MarkDirty();
        }
    }

    // ==================================================
    // HELPERS
    // ==================================================

    private Vector2 GetScrollPosition(string dayID)
    {
        if (!dayScrollPositions.ContainsKey(dayID))
        {
            dayScrollPositions.Add(dayID, Vector2.zero);
        }

        return dayScrollPositions[dayID];
    }

    private void RecordUndo(string actionName)
    {
        Undo.RecordObject(schedule, actionName);
    }

    private void MarkDirty()
    {
        schedule.EnsureDataIntegrity();

        EditorUtility.SetDirty(schedule);
        PrefabUtility.RecordPrefabInstancePropertyModifications(schedule);

        if (!Application.isPlaying && schedule.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(schedule.gameObject.scene);
        }
    }

    private void SaveChangedData()
    {
        EditorUtility.SetDirty(schedule);
        PrefabUtility.RecordPrefabInstancePropertyModifications(schedule);

        if (!Application.isPlaying && schedule.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(schedule.gameObject.scene);
        }
    }
}
