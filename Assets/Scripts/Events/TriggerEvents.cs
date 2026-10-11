using UnityEngine;

public enum TriggerEventType
{
    EnterHomeGarden,
    EnterNeighborGarden

}

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class TriggerEvents : MonoBehaviour
{
    [SerializeField] private TriggerEventType eventType = TriggerEventType.EnterHomeGarden;
    
    [SerializeField] private bool onEnter = true;
    [SerializeField] private bool onStay = false;
    [SerializeField] private bool onExit = false;

    [SerializeField] private string requiredTag = "Player";

    private void Reset()
    {
        ConfigureCollider();
    }

    private void Awake()
    {
        ValidateCollider();
    }

    private void OnValidate()
    {
        ConfigureCollider();
    }

    private void ConfigureCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void ValidateCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null || !boxCollider.isTrigger)
        {
            Debug.LogWarning("[TriggerEvent] BoxCollider is missing or Is Trigger is disabled!", this);
        }
    }

    private bool IsValidObject(Collider other)
    {
        if (string.IsNullOrWhiteSpace(requiredTag))
        {
            return false;
        }

        return other.tag == requiredTag;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!onEnter || !IsValidObject(other))
        { 
            return;
        }

        ExecuteEvent("Enter");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!onStay || !IsValidObject(other))
        {
            return;
        }

        ExecuteEvent("Stay");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!onExit || !IsValidObject(other))
        {
            return;
        }

        ExecuteEvent("Exit");
    }

    private void ExecuteEvent(string triggerPhase)
    {
        switch (eventType)
        {
            case TriggerEventType.EnterHomeGarden:
                Debug.Log($"[TriggerEvent] Enter Home Garden | On {triggerPhase}", this);
                break;

            case TriggerEventType.EnterNeighborGarden:
                Debug.Log( $"[TriggerEvent] Enter Neighbor Garden | On {triggerPhase}", this);
                break;
        }
    }
}
