using UnityEngine;
using UnityEngine.UI;

/// <summary>Main-hand icon plus general E-interaction indicator.</summary>
[DisallowMultipleComponent]
[AddComponentMenu("Game/UI/Equipped Item UI")]
public class EquippedItemUI : MonoBehaviour
{
    [Header("Equipped Item")]
    [SerializeField] private Image equippedItemIcon;

    [Header("General E Interaction Indicator")]
    [SerializeField] private Image pickupIndicator;
    [SerializeField] private Color canInteractColor = Color.green;
    [SerializeField] private Color cannotInteractColor = Color.black;

    private void Awake()
    {
        SetEquippedItem(null);
        SetInteractionAvailable(false);
    }

    public void SetEquippedItem(ItemData item)
    {
        if (equippedItemIcon == null)
        {
            return;
        }

        equippedItemIcon.sprite = item != null ? item.ItemIcon : null;
        equippedItemIcon.enabled = item != null && item.ItemIcon != null;
    }

    public void SetInteractionAvailable(bool available)
    {
        if (pickupIndicator != null)
        {
            pickupIndicator.color = available ? canInteractColor : cannotInteractColor;
        }
    }
}
