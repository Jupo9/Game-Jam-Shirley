using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;

    public ItemData Data => itemData;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player has entered the trigger
        if (!other.transform.root.CompareTag("Player"))
        {
            return;
        }

        PlayerItemController playerItemController = other.GetComponentInParent<PlayerItemController>();

        if (playerItemController == null)
        { 
            return;
        }

        playerItemController.RegisterNearbyItem(this);
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player has exited the trigger
        if (!other.transform.root.CompareTag("Player"))
        {
            return;
        }

        PlayerItemController playerItemController =
            other.GetComponentInParent<PlayerItemController>();

        if (playerItemController == null)
        {
            return;
        }

        playerItemController.UnregisterNearbyItem(this);
    }

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
