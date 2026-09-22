using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Interactable Item", menuName = "Interactable Items/Interactable Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Information")]
    [Tooltip("Every item needs an unique ID!")]
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;

    [Header("UI")]
    [Tooltip("Add here the correct item icon")]
    [SerializeField] private Sprite itemIcon;

    public string ItemID => itemID;
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;             
}
