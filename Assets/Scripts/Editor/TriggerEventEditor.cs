using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(global::TriggerEvents))]
[CanEditMultipleObjects]
public class TriggerEventEditor : Editor
{
    private SerializedProperty eventType;
    private SerializedProperty onEnter;
    private SerializedProperty onStay;
    private SerializedProperty onExit;
    private SerializedProperty requiredTag;
    private SerializedProperty interactionPriority;
    private SerializedProperty targetUIPanel;
    private SerializedProperty closeUIOnExit;
    private SerializedProperty targetItem;
    private SerializedProperty scaleForPickUp;
    private SerializedProperty targetField;
    private SerializedProperty targetWell;
    private SerializedProperty waitSlotCount;
    private SerializedProperty junkToRemove;

    private void OnEnable()
    {
        eventType = serializedObject.FindProperty("eventType");
        onEnter = serializedObject.FindProperty("onEnter");
        onStay = serializedObject.FindProperty("onStay");
        onExit = serializedObject.FindProperty("onExit");
        requiredTag = serializedObject.FindProperty("requiredTag");
        interactionPriority = serializedObject.FindProperty("interactionPriority");
        targetUIPanel = serializedObject.FindProperty("targetUIPanel");
        closeUIOnExit = serializedObject.FindProperty("closeUIOnExit");
        targetItem = serializedObject.FindProperty("targetItem");
        scaleForPickUp = serializedObject.FindProperty("scaleForPickUp");
        targetField = serializedObject.FindProperty("targetField");
        targetWell = serializedObject.FindProperty("targetWell");
        waitSlotCount = serializedObject.FindProperty("waitSlotCount");
        junkToRemove = serializedObject.FindProperty("junkToRemove");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("TRIGGER INTERACTION SYSTEM 2.0", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Event Selection", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(eventType, new GUIContent("Event Type"));
        }

        if (eventType.hasMultipleDifferentValues)
        {
            EditorGUILayout.HelpBox("Select triggers with the same event type to see event-specific settings.", MessageType.Info);
        }
        else
        {
            TriggerEventType selected = (TriggerEventType)eventType.enumValueIndex;
            bool automatic = selected == TriggerEventType.EnterHomeGarden ||
                             selected == TriggerEventType.EnterNeighborGarden;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Activation", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (automatic)
                {
                    EditorGUILayout.PropertyField(onEnter, new GUIContent("On Enter"));
                    EditorGUILayout.PropertyField(onStay, new GUIContent("On Stay"));
                    EditorGUILayout.PropertyField(onExit, new GUIContent("On Exit"));
                    if (!onEnter.hasMultipleDifferentValues && !onStay.hasMultipleDifferentValues &&
                        !onExit.hasMultipleDifferentValues &&
                        !onEnter.boolValue && !onStay.boolValue && !onExit.boolValue)
                    {
                        EditorGUILayout.HelpBox("No automatic phase enabled.", MessageType.Warning);
                    }
                    if (!onStay.hasMultipleDifferentValues && onStay.boolValue)
                    {
                        EditorGUILayout.HelpBox("On Stay is called every physics step. Avoid expensive actions here.", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Mode", "Press E");
                    EditorGUILayout.PropertyField(interactionPriority, new GUIContent("Priority"));
                    EditorGUILayout.HelpBox("Requires PlayerInteractionController on the player.", MessageType.None);
                }
            }

            EditorGUILayout.Space(6);
            DrawEventSettings(selected);
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(requiredTag, new GUIContent("Required Tag"));
            if (!requiredTag.hasMultipleDifferentValues && string.IsNullOrWhiteSpace(requiredTag.stringValue))
            {
                EditorGUILayout.HelpBox("Required Tag must not be empty.", MessageType.Warning);
            }
        }

        foreach (Object selectedTarget in targets)
        {
            TriggerEvents trigger = selectedTarget as TriggerEvents;
            if (trigger == null) continue;
            BoxCollider collider = trigger.GetComponent<BoxCollider>();
            if (collider == null || !collider.isTrigger)
            {
                EditorGUILayout.HelpBox("A BoxCollider with Is Trigger enabled is required.", MessageType.Warning);
                break;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEventSettings(TriggerEventType selected)
    {
        EditorGUILayout.LabelField("Action Settings", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            switch (selected)
            {
                case TriggerEventType.BuildingUI:
                    EditorGUILayout.PropertyField(targetUIPanel, new GUIContent("Target UI Panel"));
                    EditorGUILayout.PropertyField(closeUIOnExit, new GUIContent("Close On Exit"));
                    if (!targetUIPanel.hasMultipleDifferentValues && targetUIPanel.objectReferenceValue == null)
                        EditorGUILayout.HelpBox("Assign a UI panel GameObject (e.g. BoardPanel).", MessageType.Warning);
                    break;

                case TriggerEventType.InteractableItem:
                    EditorGUILayout.PropertyField(targetItem, new GUIContent("Item (optional)"));
                    EditorGUILayout.PropertyField(scaleForPickUp, new GUIContent("Scale for Pick Up"));
                    EditorGUILayout.HelpBox("Pick up is enabled only after the assigned item's LOCAL scale " +
                                            "reaches the configured minimum on X, Y and Z. " +
                                            "Default: 1. Use 0 to allow picking up at any scale. " +
                                            "If Item is empty, InteractableItem is searched on this same GameObject.",
                                            MessageType.Info);
                    break;

                case TriggerEventType.Wait:
                    EditorGUILayout.PropertyField(waitSlotCount, new GUIContent("Slots To Consume"));
                    EditorGUILayout.HelpBox("Wait is only available if enough slots remain today.", MessageType.Info);
                    break;

                case TriggerEventType.ClearJunk:
                    EditorGUILayout.PropertyField(junkToRemove, new GUIContent("Junk To Remove"));
                    EditorGUILayout.HelpBox("If empty, removes this GameObject. One-shot interaction.", MessageType.Info);
                    break;

                case TriggerEventType.Field:
                    EditorGUILayout.PropertyField(targetField, new GUIContent("Field Controller (optional)"));
                    EditorGUILayout.HelpBox("A FieldController manages shovel, selected bag seed, water, " +
                                            "growth and harvest. If empty, the trigger searches its parents.",
                                            MessageType.Info);
                    ShowMissingControllerWarning<FieldController>(targetField, "FieldController");
                    break;

                case TriggerEventType.Well:
                    EditorGUILayout.PropertyField(targetWell, new GUIContent("Well Controller (optional)"));
                    EditorGUILayout.HelpBox("WellController checks the held WaterContainerState and fills " +
                                            "it when E is pressed. If empty, the trigger searches its parents.",
                                            MessageType.Info);
                    ShowMissingControllerWarning<WellController>(targetWell, "WellController");
                    break;

                default:
                    EditorGUILayout.HelpBox("This garden event currently logs the selected trigger phase.", MessageType.None);
                    break;
            }
        }
    }

    private void ShowMissingControllerWarning<T>(SerializedProperty reference, string label)
        where T : Component
    {
        if (reference.hasMultipleDifferentValues || reference.objectReferenceValue != null)
            return;

        foreach (Object selected in targets)
        {
            TriggerEvents trigger = selected as TriggerEvents;
            if (trigger != null && trigger.GetComponentInParent<T>() == null)
            {
                EditorGUILayout.HelpBox("Assign a " + label + " or add it to this GameObject / parent.",
                                        MessageType.Warning);
                return;
            }
        }
    }
}

