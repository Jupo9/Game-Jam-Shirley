using UnityEngine;

/// <summary>
/// State of THIS physical item, not of its shared ItemData asset.
/// Attach to a bucket that also has InteractableItem.
/// The filled state survives pickup/drop because the item object is deactivated,
/// not destroyed, while it is held.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(InteractableItem))]
[AddComponentMenu("Game/Items/Water Container State")]
public class WaterContainerState : MonoBehaviour
{
    [SerializeField] private bool startsFilled;
    private bool isFilled;

    public bool IsFilled => isFilled;

    private void Awake() => isFilled = startsFilled;

    public bool Fill()
    {
        if (isFilled)
        {
            return false;
        }

        isFilled = true;
        return true;
    }

    public bool Empty()
    {
        if (!isFilled)
        {
            return false;
        }

        isFilled = false;
        return true;
    }
}
