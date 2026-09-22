using UnityEngine;
using UnityEngine.UI;

public class EquippedItemUI : MonoBehaviour
{
    [Header("Equipped Item")]
    [Tooltip("UI Image that displays the currently equipped item")]
    [SerializeField] private Image equippedItemIcon;        //change this to Sprite!

    [Header("Pickup Indicator")]
    [Tooltip("Changes color when an item can be picked up")]
    [SerializeField] private Image pickupIndicator;         //change this to Sprite!

    [SerializeField] private Color canPickupColor = Color.green;
    [SerializeField] private Color cannotPickupColor = Color.black;

    private void Awake()
    {
        SetEquippedItem(null);
        SetPickupAvailable(false);
    }

    public void SetEquippedItem(ItemData item)
    {
        if (item == null)
        {
            equippedItemIcon.sprite = null;
            equippedItemIcon.enabled = false;
            return;
        }

        equippedItemIcon.sprite = item.ItemIcon;
        equippedItemIcon.enabled = true;
    }

    public void SetPickupAvailable(bool canPickup)
    {
        if (canPickup)
        {
            pickupIndicator.color = canPickupColor;
        }
        else
        {
            pickupIndicator.color = cannotPickupColor;
        }
    }
}
