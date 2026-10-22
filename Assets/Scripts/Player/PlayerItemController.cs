using System;
using UnityEngine;

/// <summary>One main-hand item at a time. E pickup is delegated by TriggerEvents.</summary>
[DisallowMultipleComponent]
[AddComponentMenu("Game/Items/Player Item Controller")]
public class PlayerItemController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private EquippedItemUI equippedItemUI;

    [Header("Bag")]
    [SerializeField] private PlayerInventory inventory;

    [Header("Drop Settings")]
    [SerializeField] private Transform dropPoint;

    private InteractableItem equippedItem;

    public event Action<ItemData> OnEquippedItemChanged;

    public bool HasEquippedItem => equippedItem != null;
    public InteractableItem EquippedItem => equippedItem;
    public ItemData EquippedItemData => equippedItem != null ? equippedItem.Data : null;
    public EquippedItemUI EquippedItemUI => equippedItemUI;
    public PlayerInventory Inventory => inventory;
    public WaterContainerState EquippedWaterContainer =>
        equippedItem != null ? equippedItem.GetComponent<WaterContainerState>() : null;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
        }
    }

    private void Start() => RefreshEquippedItemUI();

    public bool IsHolding(ItemData item)
    {
        return item != null && EquippedItemData == item;
    }

    public bool CanPickupItem(InteractableItem item)
    {
        if (item == null || item.Data == null || !item.isActiveAndEnabled ||
            !item.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (item.Data.CanStoreInBag)
        {
            return inventory != null && inventory.CanAddItem(item.Data);
        }

        return equippedItem == null;
    }

    public bool TryPickupItem(InteractableItem item)
    {
        if (!CanPickupItem(item))
        {
            return false;
        }

        ItemData data = item.Data;
        if (data.CanStoreInBag)
        {
            if (!inventory.AddItem(data))
            {
                return false;
            }

            item.Pickup();
            Debug.Log($"[Inventory] Picked up 1x {data.ItemName} into bag.", this);
            return true;
        }

        equippedItem = item;
        item.Pickup();
        RefreshEquippedItemUI();
        Debug.Log("Picked up: " + data.ItemName, this);
        return true;
    }

    public bool DropEquippedItem()
    {
        if (equippedItem == null)
        {
            return false;
        }

        InteractableItem item = equippedItem;
        equippedItem = null;

        Vector3 position = dropPoint != null ? dropPoint.position : transform.position;
        item.Drop(position);
        RefreshEquippedItemUI();
        Debug.Log("Dropped: " + item.Data.ItemName, this);
        return true;
    }

    // Convenience methods for the next step (Well and Field controllers).
    // The component on the ACTUAL held item determines whether it can hold water.
    public bool CanFillEquippedWaterContainer =>
        EquippedWaterContainer != null && !EquippedWaterContainer.IsFilled;

    public bool HasFilledWaterContainer =>
        EquippedWaterContainer != null && EquippedWaterContainer.IsFilled;

    public bool TryFillEquippedWaterContainer()
    {
        WaterContainerState container = EquippedWaterContainer;
        return container != null && container.Fill();
    }

    public bool TryUseEquippedWater()
    {
        WaterContainerState container = EquippedWaterContainer;
        return container != null && container.Empty();
    }

    private void RefreshEquippedItemUI()
    {
        ItemData data = EquippedItemData;
        if (equippedItemUI != null)
        {
            equippedItemUI.SetEquippedItem(data);
        }

        OnEquippedItemChanged?.Invoke(data);
    }
}
