using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NeighborNPCWorldManager))]
public sealed class NeighborNPCWorldManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("NPC World / Day Schedule", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dayCycle"));
        EditorGUILayout.Space(6f);

        EditorGUILayout.LabelField("Scene Locations", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("entrance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitPoint"));
        EditorGUILayout.HelpBox("Exit Point is optional; without it, Entrance is also the exit.", MessageType.None);
        EditorGUILayout.Space(6f);

        EditorGUILayout.LabelField("Navigation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnScatterRadius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("navMeshSampleRadius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("arrivalTolerance"));
        EditorGUILayout.Space(6f);

        NeighborNPCWorldManager manager = (NeighborNPCWorldManager)target;
        DaySchedule schedule = manager.DayCycle != null ? manager.DayCycle.DaySchedule : null;
        SerializedProperty entries = serializedObject.FindProperty("bindings");

        EditorGUILayout.LabelField("Neighbor Assignments", EditorStyles.boldLabel);
        if (schedule == null)
        {
            EditorGUILayout.HelpBox("Assign a DayCycleManager that references your DaySchedule to select neighbors by name.",
                                    MessageType.Info);
            EditorGUILayout.PropertyField(entries, true);
        }
        else
        {
            DrawBindings(entries, schedule);
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(8f);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Refresh NPCs (Play Mode)"))
            {
                manager.RefreshAll();
            }
        }

        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Spawned NPCs", manager.SpawnedCount.ToString());
        }
    }

    private void DrawBindings(SerializedProperty entries, DaySchedule schedule)
    {
        for (int i = 0; i < entries.arraySize; i++)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            SerializedProperty idProp = entry.FindPropertyRelative("neighborID");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("NPC " + (i + 1), EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(70f)))
            {
                entries.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUIUtility.ExitGUI();
                return;
            }
            EditorGUILayout.EndHorizontal();

            string[] choices = new string[schedule.Neighbors.Count + 1];
            choices[0] = "(Unassigned)";
            int selected = 0;
            for (int n = 0; n < schedule.Neighbors.Count; n++)
            {
                NeighborDefinition neighbor = schedule.Neighbors[n];
                choices[n + 1] = neighbor.Name + " (" + (n + 1) + ")";

                if (idProp.stringValue == neighbor.ID)
                {
                    selected = n + 1;
                }
            }

            int picked = EditorGUILayout.Popup("Schedule Neighbor", selected, choices);
            string pickedID = picked == 0 ? string.Empty : schedule.Neighbors[picked - 1].ID;

            if (pickedID != idProp.stringValue)
            {
                idProp.stringValue = pickedID;
            }

            EditorGUILayout.PropertyField(entry.FindPropertyRelative("definition"), new GUIContent("NPC Definition"));
            EditorGUILayout.PropertyField(entry.FindPropertyRelative("gardenTarget"), new GUIContent("Own Garden Target"));
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Binding"))
        {
            int index = entries.arraySize;
            entries.InsertArrayElementAtIndex(index);
            SerializedProperty entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("neighborID").stringValue = string.Empty;
            entry.FindPropertyRelative("definition").objectReferenceValue = null;
            entry.FindPropertyRelative("gardenTarget").objectReferenceValue = null;
        }

        if (GUILayout.Button("+ Add Missing Neighbors"))
        {
            HashSet<string> assigned = new();
            for (int i = 0; i < entries.arraySize; i++)
            {
                string id = entries.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("neighborID").stringValue;
                if (!string.IsNullOrEmpty(id))
                {
                    assigned.Add(id);
                }
            }

            foreach (NeighborDefinition neighbor in schedule.Neighbors)
            {
                if (neighbor == null || assigned.Contains(neighbor.ID))
                {
                    continue;
                }

                int index = entries.arraySize;
                entries.InsertArrayElementAtIndex(index);
                SerializedProperty entry = entries.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("neighborID").stringValue = neighbor.ID;
                entry.FindPropertyRelative("definition").objectReferenceValue = null;
                entry.FindPropertyRelative("gardenTarget").objectReferenceValue = null;
                assigned.Add(neighbor.ID);
            }
        }

        EditorGUILayout.EndHorizontal();
    }
}
