using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    public ItemData Data => itemData;

    public void Pickup()
    {
        gameObject.SetActive(false);
    }

    public void Drop(Vector3 dropPosition)
    {
        transform.position = dropPosition;
        gameObject.SetActive(true);
    }
}
