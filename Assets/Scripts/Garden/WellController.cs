using UnityEngine;

/// <summary>Refills the water container carried in the player's main hand.</summary>
[DisallowMultipleComponent]
[AddComponentMenu("Game/Garden/Well Controller")]
public class WellController : MonoBehaviour
{
    [Header("Optional: only this exact ItemData may be filled")]
    [SerializeField] private ItemData requiredContainerItem;

    [Header("Time cost (0 = free)")]
    [SerializeField, Min(0)] private int timeslotCost = 0;

    public bool CanInteract(PlayerInteractionController player)
    {
        if (player == null || player.ItemController == null) return false;

        PlayerItemController items = player.ItemController;
        if (!items.CanFillEquippedWaterContainer) return false;
        if (requiredContainerItem != null && !items.IsHolding(requiredContainerItem)) return false;

        return timeslotCost == 0 ||
               (player.DayCycle != null && player.DayCycle.IsConfigured &&
                player.DayCycle.SlotsRemainingToday >= timeslotCost);
    }

    public bool Interact(PlayerInteractionController player)
    {
        if (!CanInteract(player)) return false;

        if (timeslotCost > 0)
        {
            // All slots are validated first; the day may not roll over here.
            for (int i = 0; i < timeslotCost; i++)
            {
                if (!player.DayCycle.ConsumeSlot()) return false;
            }
        }

        if (!player.ItemController.TryFillEquippedWaterContainer()) return false;
        Debug.Log("[Well] Filled the equipped water container.", this);
        return true;
    }
}
