using System.Collections.Generic;
using UnityEngine;

internal class PlayerItemController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private EquippedItemUI equippedItemUI;

    [Header("Drop Settings")]
    [Tooltip("Optional position where the item gets dropped")]
    [SerializeField] private Transform dropPoint;

    private readonly List<InteractableItem> nearbyItems = new List<InteractableItem>();
    private InteractableItem equippedItem;

    public bool HasEquippedItem => equippedItem != null;

    private void Start()
    {
        RefreshUI();
    }

    public void RegisterNearbyItem(InteractableItem item)
    {
        if (item == null)
            return;

        if (!nearbyItems.Contains(item))
        {
            nearbyItems.Add(item);
        }

        RefreshUI();
    }

    public void UnregisterNearbyItem(InteractableItem item)
    {
        if (item == null)
            return;

        nearbyItems.Remove(item);

        RefreshUI();
    }

    public void TryPickupItem()
    {
        if (equippedItem != null)
        {
            return;
        }

        InteractableItem itemToPickup = GetClosestNearbyItem();

        if (itemToPickup == null)
        {
            return;
        }

        nearbyItems.Remove(itemToPickup);
        equippedItem = itemToPickup;

        itemToPickup.Pickup();
        RefreshUI();

        Debug.Log("Picked up: " + equippedItem.Data.ItemName);
    }


    public void DropEquippedItem()
    {
        if (equippedItem == null)
            return;

        InteractableItem itemToDrop = equippedItem;
        equippedItem = null;
        Vector3 dropPosition;

        if (dropPoint != null)
        {
            dropPosition = dropPoint.position;
        }
        else
        {
            dropPosition = transform.position;
        }

        itemToDrop.Drop(dropPosition);

        RefreshUI();

        Debug.Log("Dropped: " +itemToDrop.Data.ItemName);
    }

    private InteractableItem GetClosestNearbyItem()
    {
        InteractableItem closestItem = null;

        float closestDistance = float.MaxValue;

        for (int i = nearbyItems.Count - 1; i >= 0; i--)
        {
            if (nearbyItems[i] == null)
            {
                nearbyItems.RemoveAt(i);
            }
        }

        foreach (InteractableItem item in nearbyItems)
        {
            float distance = (item.transform.position - transform.position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        return closestItem;
    }

    private void RefreshUI()
    {
        if (equippedItemUI == null)
        {
            return;
        }

        if (equippedItem != null)
        {
            equippedItemUI.SetEquippedItem(equippedItem.Data);
        }
        else
        {
            equippedItemUI.SetEquippedItem(null);
        }

        bool canPickup =
            equippedItem == null &&
            GetClosestNearbyItem() != null;

        equippedItemUI.SetPickupAvailable(canPickup);
    }
}
