using UnityEngine;

public enum ItemCategory
{
    MainHand = 0,
    Seed = 1,
    Crop = 2,
    Misc = 3
}

[CreateAssetMenu(fileName = "New Interactable Item", menuName = "Interactable Items/Interactable Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Information")]
    [Tooltip("Every item needs a unique ID for future save/load support.")]
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;

    [Header("Inventory")]
    [Tooltip("MainHand = physical object. Seed, Crop and Misc = stackable bag items.")]
    [SerializeField] private ItemCategory itemCategory = ItemCategory.MainHand;

    [Header("UI")]
    [SerializeField] private Sprite itemIcon;

    public string ItemID => itemID;
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;
    public ItemCategory Category => itemCategory;
    public bool CanStoreInBag => itemCategory != ItemCategory.MainHand;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(itemID))
        {
            Debug.LogWarning($"[ItemData] '{name}' has no Item ID. Assign a stable unique ID.", this);
        }
    }
}
