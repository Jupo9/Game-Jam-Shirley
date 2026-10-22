using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The ONLY place reading E. Chooses the highest-priority available trigger,
/// then the closest among triggers with equal priority.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Game/Interaction/Player Interaction Controller")]
public class PlayerInteractionController : MonoBehaviour
{
    [Header("Required dependencies")]
    [SerializeField] private PlayerItemController itemController;
    [SerializeField] private DayCycleManager dayCycle;
    [SerializeField] private GameUIManager uiManager;

    [Header("Interaction indicator")]
    [SerializeField] private EquippedItemUI equippedItemUI;

    private readonly List<TriggerEvents> nearbyTriggers = new List<TriggerEvents>();
    private TriggerEvents selectedTrigger;

    public PlayerItemController ItemController => itemController;
    public DayCycleManager DayCycle => dayCycle;
    public GameUIManager UIManager => uiManager;
    public TriggerEvents SelectedTrigger => selectedTrigger;

    private void Awake()
    {
        if (itemController == null)
        {
            itemController = GetComponent<PlayerItemController>();
        }
        if (equippedItemUI == null && itemController != null)
        {
            equippedItemUI = itemController.EquippedItemUI;
        }
    }

    private void Update()
    {
        bool blocked = uiManager != null && uiManager.IsOpen;
        selectedTrigger = blocked ? null : FindBestTrigger();

        if (equippedItemUI != null)
        {
            equippedItemUI.SetInteractionAvailable(selectedTrigger != null);
        }

        if (!blocked && selectedTrigger != null && Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            selectedTrigger.Interact(this);
            // Refresh next frame; pickup can disable the selected trigger immediately.
        }
    }

    public void RegisterTrigger(TriggerEvents trigger)
    {
        if (trigger != null && !nearbyTriggers.Contains(trigger))
        {
            nearbyTriggers.Add(trigger);
        }
    }

    public void UnregisterTrigger(TriggerEvents trigger)
    {
        nearbyTriggers.Remove(trigger);
        if (selectedTrigger == trigger)
        {
            selectedTrigger = null;
        }
        if (uiManager != null && trigger != null && trigger.CloseUIOnExit)
        {
            uiManager.CloseIfOwner(trigger);
        }
    }

    private TriggerEvents FindBestTrigger()
    {
        TriggerEvents best = null;
        int bestPriority = int.MinValue;
        float bestDistance = float.PositiveInfinity;

        for (int i = nearbyTriggers.Count - 1; i >= 0; i--)
        {
            TriggerEvents trigger = nearbyTriggers[i];
            if (trigger == null || !trigger.isActiveAndEnabled)
            {
                nearbyTriggers.RemoveAt(i);
                continue;
            }

            if (!trigger.CanInteract(this))
            {
                continue;
            }

            float distance = (trigger.InteractionPosition - transform.position).sqrMagnitude;
            int priority = trigger.InteractionPriority;
            if (priority > bestPriority || (priority == bestPriority && distance < bestDistance))
            {
                best = trigger;
                bestPriority = priority;
                bestDistance = distance;
            }
        }

        return best;
    }

    private void OnDisable()
    {
        nearbyTriggers.Clear();
        selectedTrigger = null;
        if (equippedItemUI != null)
        {
            equippedItemUI.SetInteractionAvailable(false);
        }
    }
}
