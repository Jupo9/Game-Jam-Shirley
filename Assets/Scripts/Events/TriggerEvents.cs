using System;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerEventType
{
    EnterHomeGarden,
    EnterNeighborGarden,
    BuildingUI,
    InteractableItem,
    Field,      
    Well,       
    Wait,
    ClearJunk
}

/// <summary>
/// Detects a player inside a BoxCollider and delegates E interactions to
/// PlayerInteractionController. The trigger never reads the keyboard itself.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
[AddComponentMenu("Game/Interaction/Trigger Events")]
public class TriggerEvents : MonoBehaviour
{
    [SerializeField] private TriggerEventType eventType = TriggerEventType.EnterHomeGarden;

    // Used by automatic garden events only.
    [SerializeField] private bool onEnter = true;
    [SerializeField] private bool onStay;
    [SerializeField] private bool onExit;

    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private int interactionPriority;

    [Header("Building UI")]
    [SerializeField] private GameObject targetUIPanel;
    [SerializeField] private bool closeUIOnExit = true;

    [Header("Pickup")]
    [SerializeField] private InteractableItem targetItem;
    [Tooltip("Minimum local scale on every axis of the item before it can be picked up. Default: 1. Set to 0 to disable the size requirement.")]
    [SerializeField, Min(0f)] private float scaleForPickUp = 1f;

    [Header("Field")]
    [SerializeField] private FieldController targetField;

    [Header("Well")]
    [SerializeField] private WellController targetWell;

    [Header("Wait")]
    [SerializeField, Min(1)] private int waitSlotCount = 1;

    [Header("Clear Junk")]
    [Tooltip("Leave empty to remove this trigger GameObject.")]
    [SerializeField] private GameObject junkToRemove;

    private readonly HashSet<Collider> touchingColliders = new HashSet<Collider>();
    private PlayerInteractionController registeredPlayer;

    public TriggerEventType EventType => eventType;
    public int InteractionPriority => interactionPriority;
    public bool CloseUIOnExit => closeUIOnExit;
    public Vector3 InteractionPosition => transform.position;

    public bool UsesEInteraction => eventType != TriggerEventType.EnterHomeGarden &&
                                    eventType != TriggerEventType.EnterNeighborGarden;

    private void Reset() => ConfigureCollider();
    private void OnValidate()
    {
        ConfigureCollider();
        waitSlotCount = Mathf.Max(1, waitSlotCount);
    }

    private void Awake() => ConfigureCollider();

    private void ConfigureCollider()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            box.isTrigger = true;
        }
    }

    private bool HasRequiredTag(Collider other)
    {
        return other != null 
               && !string.IsNullOrWhiteSpace(requiredTag) 
               && other.transform.root.CompareTag(requiredTag);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasRequiredTag(other) || !touchingColliders.Add(other))
        {
            return;
        }

        if (UsesEInteraction)
        {
            PlayerInteractionController player = other.GetComponentInParent<PlayerInteractionController>();
            if (player != null && (registeredPlayer == null || registeredPlayer == player))
            {
                registeredPlayer = player;
                player.RegisterTrigger(this);
            }
        }
        else if (touchingColliders.Count == 1 && onEnter)
        {
            ExecuteAutomaticEvent("Enter");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!UsesEInteraction && onStay && HasRequiredTag(other))
        {
            ExecuteAutomaticEvent("Stay");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!touchingColliders.Remove(other) || touchingColliders.Count != 0)
        {
            return;
        }

        if (UsesEInteraction)
        {
            UnregisterPlayer();
        }
        else if (onExit)
        {
            ExecuteAutomaticEvent("Exit");
        }
    }

    private void OnDisable()
    {
        UnregisterPlayer();
        touchingColliders.Clear();
    }

    private void UnregisterPlayer()
    {
        if (registeredPlayer == null)
        {
            return;
        }

        PlayerInteractionController previous = registeredPlayer;
        registeredPlayer = null;
        previous.UnregisterTrigger(this);
    }

    public bool CanInteract(PlayerInteractionController player)
    {
        if (!isActiveAndEnabled || !UsesEInteraction || player == null)
        {
            return false;
        }

        switch (eventType)
        {
            case TriggerEventType.BuildingUI:
                return targetUIPanel != null && player.UIManager != null &&
                       !player.UIManager.IsOpen;

            case TriggerEventType.InteractableItem:
                InteractableItem item = GetTargetItem();
                return player.ItemController != null && player.ItemController.CanPickupItem(item) && HasRequiredPickUpScale(item);

            case TriggerEventType.Wait:
                return player.DayCycle != null && player.DayCycle.IsConfigured &&
                       player.DayCycle.SlotsRemainingToday >= waitSlotCount;

            case TriggerEventType.ClearJunk:
                return junkToRemove != null || gameObject.activeInHierarchy;

            case TriggerEventType.Field:
                FieldController field = GetTargetField();
                return field != null && field.CanInteract(player);

            case TriggerEventType.Well:
                WellController well = GetTargetWell();
                return well != null && well.CanInteract(player);

            default:
                return false;
        }
    }

    public bool Interact(PlayerInteractionController player)
    {
        if (!CanInteract(player))
        {
            return false;
        }

        switch (eventType)
        {
            case TriggerEventType.BuildingUI:
                return player.UIManager.OpenPanel(targetUIPanel, this);

            case TriggerEventType.InteractableItem:
                return player.ItemController.TryPickupItem(GetTargetItem());

            case TriggerEventType.Wait:
                // All-or-nothing: available slots were checked in CanInteract().
                for (int i = 0; i < waitSlotCount; i++)
                {
                    if (!player.DayCycle.ConsumeSlot())
                    {
                        return false;
                    }
                }
                return true;

            case TriggerEventType.Field:
                return GetTargetField().Interact(player);

            case TriggerEventType.Well:
                return GetTargetWell().Interact(player);

            case TriggerEventType.ClearJunk:
                GameObject target = junkToRemove != null ? junkToRemove : gameObject;
                // Don't allow a misconfigured Clear Junk trigger to remove the player.
                if (target == player.gameObject ||
                    target.transform.IsChildOf(player.transform) ||
                    player.transform.IsChildOf(target.transform))
                {
                    Debug.LogWarning("Clear Junk target must not be the Player.", this);
                    return false;
                }
                UnregisterPlayer();
                // Remove the trigger as well if it is separate from the junk object.
                // This guarantees a one-shot interaction.
                if (target != gameObject)
                {
                    gameObject.SetActive(false);
                }
                Destroy(target);
                return true;

            default:
                return false;
        }
    }

    private FieldController GetTargetField()
    {
        return targetField != null ? targetField : GetComponentInParent<FieldController>();
    }

    private WellController GetTargetWell()
    {
        return targetWell != null ? targetWell : GetComponentInParent<WellController>();
    }

    private InteractableItem GetTargetItem()
    {
        return targetItem != null ? targetItem : GetComponent<InteractableItem>();
    }

    private void ExecuteAutomaticEvent(string phase)
    {
        switch (eventType)
        {
            case TriggerEventType.EnterHomeGarden:
                Debug.Log("[TriggerEvents] Enter Home Garden | On " + phase, this);
                break;
            case TriggerEventType.EnterNeighborGarden:
                Debug.Log("[TriggerEvents] Enter Neighbor Garden | On " + phase, this);
                break;
        }
    }

    private bool HasRequiredPickUpScale(InteractableItem item)
    {
        if (item == null)
        {
            return false;
        }

        Vector3 scale = item.transform.localScale;
        const float epsilon = 0.0001f;

        return Mathf.Abs(scale.x) + epsilon >= scaleForPickUp &&
               Mathf.Abs(scale.y) + epsilon >= scaleForPickUp &&
               Mathf.Abs(scale.z) + epsilon >= scaleForPickUp;
    }

}
