using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(DaySchedule))]
public class DayScheduleEditor : Editor
{
    private DaySchedule schedule;

    private readonly Dictionary<string, Vector2> dayScrollPositions =
        new Dictionary<string, Vector2>();

    private const float PersonColumnWidth = 130f;
    private const float SlotWidth = 70f;
    private const float RowHeight = 24f;

    private void OnEnable()
    {
        schedule = (DaySchedule)target;

        schedule.EnsureDataIntegrity();
    }

    public override void OnInspectorGUI()
    {
        if (schedule == null)
            return;

        DrawPeopleSection();

        EditorGUILayout.Space(10);

        DrawTimeSlotSection();

        EditorGUILayout.Space(15);

        DrawDays();

        EditorGUILayout.Space(10);

        DrawAddDayButton();
    }


    // =========================================================
    // PEOPLE
    // =========================================================

    private void DrawPeopleSection()
    {
        EditorGUILayout.LabelField(
            "People",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginVertical("box");

        for (int i = 0; i < schedule.People.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            string oldName =
                schedule.People[i].Name;

            string newName =
                EditorGUILayout.TextField(oldName);

            if (newName != oldName)
            {
                RecordUndo("Rename Person");

                schedule.RenamePerson(
                    i,
                    newName
                );

                MarkDirty();
            }

            if (GUILayout.Button(
                "-",
                GUILayout.Width(30)))
            {
                RecordUndo("Remove Person");

                schedule.RemovePersonAt(i);

                MarkDirty();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUIUtility.ExitGUI();

                return;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Person"))
        {
            RecordUndo("Add Person");

            schedule.AddPerson();

            MarkDirty();
        }

        EditorGUILayout.EndVertical();
    }


    // =========================================================
    // TIMESLOTS
    // =========================================================

    private void DrawTimeSlotSection()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            "Global Timeslots",
            EditorStyles.boldLabel
        );

        if (GUILayout.Button(
            "+",
            GUILayout.Width(30)))
        {
            RecordUndo("Add Timeslot");

            schedule.AddTimeSlot();

            MarkDirty();
        }

        GUI.enabled =
            schedule.TimeSlots.Count > 0;

        if (GUILayout.Button(
            "-",
            GUILayout.Width(30)))
        {
            RecordUndo("Remove Timeslot");

            schedule.RemoveLastTimeSlot();

            MarkDirty();

            GUIUtility.ExitGUI();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("box");

        if (schedule.TimeSlots.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No timeslots created yet. Press + to create one.",
                MessageType.Info
            );
        }

        for (int i = 0; i < schedule.TimeSlots.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                "Slot " + (i + 1),
                GUILayout.Width(60)
            );

            string oldName =
                schedule.TimeSlots[i].Name;

            string newName =
                EditorGUILayout.TextField(oldName);

            if (newName != oldName)
            {
                RecordUndo("Rename Timeslot");

                schedule.RenameTimeSlot(
                    i,
                    newName
                );

                MarkDirty();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }


    // =========================================================
    // DAYS
    // =========================================================

    private void DrawDays()
    {
        EditorGUILayout.LabelField(
            "Days",
            EditorStyles.boldLabel
        );

        if (schedule.Days.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No days created yet.",
                MessageType.Info
            );

            return;
        }

        for (int dayIndex = 0;
             dayIndex < schedule.Days.Count;
             dayIndex++)
        {
            DrawDay(dayIndex);

            EditorGUILayout.Space(10);
        }
    }

    private void DrawDay(int dayIndex)
    {
        DayData day =
            schedule.Days[dayIndex];

        EditorGUILayout.BeginVertical("box");

        DrawDayHeader(
            dayIndex,
            day
        );

        EditorGUILayout.Space(5);

        if (schedule.People.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Add at least one person.",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();

            return;
        }

        if (schedule.TimeSlots.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Add at least one timeslot.",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();

            return;
        }

        DrawScheduleGrid(
            dayIndex,
            day
        );

        EditorGUILayout.EndVertical();
    }


    private void DrawDayHeader(
        int dayIndex,
        DayData day)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            "DAY",
            EditorStyles.boldLabel,
            GUILayout.Width(40)
        );

        string oldName =
            day.Name;

        string newName =
            EditorGUILayout.TextField(oldName);

        if (newName != oldName)
        {
            RecordUndo("Rename Day");

            schedule.RenameDay(
                dayIndex,
                newName
            );

            MarkDirty();
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(
            "Delete Day",
            GUILayout.Width(90)))
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


    // =========================================================
    // GRID
    // =========================================================

    private void DrawScheduleGrid(
        int dayIndex,
        DayData day)
    {
        Vector2 scrollPosition =
            GetScrollPosition(day.ID);

        float contentWidth =
            PersonColumnWidth +
            schedule.TimeSlots.Count * SlotWidth +
            20f;

        float contentHeight =
            (schedule.People.Count + 1)
            * RowHeight
            + 15f;

        scrollPosition =
            EditorGUILayout.BeginScrollView(
                scrollPosition,
                true,
                false,
                GUILayout.Height(
                    Mathf.Min(
                        contentHeight + 20f,
                        300f
                    )
                )
            );

        EditorGUILayout.BeginVertical(
            GUILayout.Width(contentWidth)
        );

        DrawTimeSlotHeader();

        for (int personIndex = 0;
             personIndex < schedule.People.Count;
             personIndex++)
        {
            DrawPersonRow(
                dayIndex,
                personIndex
            );
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();

        dayScrollPositions[day.ID] =
            scrollPosition;
    }


    private void DrawTimeSlotHeader()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(
            "Person",
            EditorStyles.boldLabel,
            GUILayout.Width(PersonColumnWidth),
            GUILayout.Height(RowHeight)
        );

        foreach (TimeSlotDefinition timeSlot
                 in schedule.TimeSlots)
        {
            GUILayout.Label(
                timeSlot.Name,
                EditorStyles.boldLabel,
                GUILayout.Width(SlotWidth),
                GUILayout.Height(RowHeight)
            );
        }

        EditorGUILayout.EndHorizontal();
    }


    private void DrawPersonRow(
        int dayIndex,
        int personIndex)
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(
            schedule.People[personIndex].Name,
            GUILayout.Width(PersonColumnWidth),
            GUILayout.Height(RowHeight)
        );

        for (int slotIndex = 0;
             slotIndex < schedule.TimeSlots.Count;
             slotIndex++)
        {
            bool occupied =
                schedule.IsOccupied(
                    dayIndex,
                    personIndex,
                    slotIndex
                );

            Color oldBackgroundColor =
                GUI.backgroundColor;

            GUI.backgroundColor =
                occupied
                    ? new Color(1f, 0.35f, 0.35f)
                    : Color.white;

            string buttonText =
                occupied ? "X" : "";

            if (GUILayout.Button(
                buttonText,
                GUILayout.Width(SlotWidth),
                GUILayout.Height(RowHeight)))
            {
                RecordUndo(
                    occupied
                        ? "Free Timeslot"
                        : "Occupy Timeslot"
                );

                schedule.ToggleOccupied(
                    dayIndex,
                    personIndex,
                    slotIndex
                );

                MarkDirty();
            }

            GUI.backgroundColor =
                oldBackgroundColor;
        }

        EditorGUILayout.EndHorizontal();
    }


    // =========================================================
    // ADD DAY
    // =========================================================

    private void DrawAddDayButton()
    {
        if (GUILayout.Button(
            "+ Add Day",
            GUILayout.Height(30)))
        {
            RecordUndo("Add Day");

            schedule.AddDay();

            MarkDirty();
        }
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private Vector2 GetScrollPosition(
        string dayID)
    {
        if (!dayScrollPositions.ContainsKey(dayID))
        {
            dayScrollPositions.Add(
                dayID,
                Vector2.zero
            );
        }

        return dayScrollPositions[dayID];
    }

    private void RecordUndo(string actionName)
    {
        Undo.RecordObject(
            schedule,
            actionName
        );
    }

    private void MarkDirty()
    {
        schedule.EnsureDataIntegrity();

        EditorUtility.SetDirty(schedule);

        PrefabUtility
            .RecordPrefabInstancePropertyModifications(
                schedule
            );

        if (!Application.isPlaying &&
            schedule.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(
                schedule.gameObject.scene
            );
        }
    }
}
