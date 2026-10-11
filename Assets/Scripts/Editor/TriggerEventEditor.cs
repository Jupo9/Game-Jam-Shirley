using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CustomEditor(typeof(global::TriggerEvents))]
[CanEditMultipleObjects]
public class TriggerEventEditor : Editor
{
    private SerializedProperty eventType;
    private SerializedProperty onEnter;
    private SerializedProperty onStay;
    private SerializedProperty onExit;
    private SerializedProperty requiredTag;

    private void OnEnable()
    {
        eventType = serializedObject.FindProperty("eventType");
        onEnter = serializedObject.FindProperty("onEnter");
        onStay = serializedObject.FindProperty("onStay");
        onExit = serializedObject.FindProperty("onExit");
        requiredTag = serializedObject.FindProperty("requiredTag");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("TRIGGER EVENT SYSTEM", EditorStyles.boldLabel);

        EditorGUILayout.Space(8);

        DrawEventSelection();

        EditorGUILayout.Space(12);

        DrawTriggerSettings();

        EditorGUILayout.Space(12);

        DrawTagFilter();

        EditorGUILayout.Space(12);

        DrawInfoBox();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEventSelection()
    {
        EditorGUILayout.LabelField("Event Selection", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(eventType, new GUIContent("Event Type"));

        EditorGUILayout.EndVertical();
    }

    private void DrawTriggerSettings()
    {
        EditorGUILayout.LabelField("Trigger Settings",EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(onEnter, new GUIContent("On Enter"));

        EditorGUILayout.PropertyField(onStay, new GUIContent("On Stay"));

        EditorGUILayout.PropertyField(onExit, new GUIContent("On Exit"));

        EditorGUILayout.EndVertical();
    }

    private void DrawTagFilter()
    {
        EditorGUILayout.LabelField("Tag Filter", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(requiredTag, new GUIContent("Required Tag"));

        EditorGUILayout.EndVertical();
    }

    private void DrawInfoBox()
    {
        if (onEnter.hasMultipleDifferentValues || onStay.hasMultipleDifferentValues || onExit.hasMultipleDifferentValues)
        {
            return;
        }

        if (!onEnter.boolValue && !onStay.boolValue && !onExit.boolValue)
        {
            EditorGUILayout.HelpBox("No trigger phase is enabled!", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("Trigger Event is configured.", MessageType.Info);
        }

        if (!requiredTag.hasMultipleDifferentValues && string.IsNullOrWhiteSpace(requiredTag.stringValue))
        {
            EditorGUILayout.HelpBox("Required Tag is empty. No object will trigger this event.", MessageType.Warning);
        }

        foreach (Object selectedTarget in targets)
        {
            global::TriggerEvents trigger = (global::TriggerEvents)selectedTarget;

            BoxCollider boxCollider = trigger.GetComponent<BoxCollider>();

            if (boxCollider == null || !boxCollider.isTrigger)
            {
                EditorGUILayout.HelpBox(
                    "BoxCollider must have Is Trigger enabled.",
                    MessageType.Warning
                );

                break;
            }
        }
    }
}

