using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public sealed class NeighborNPCBinding
{
    // The editor selects this ID by name; do not use the display name as an ID.
    [SerializeField] private string neighborID;
    [SerializeField] private NeighborNPCDefinition definition;
    [SerializeField] private Transform gardenTarget;

    public string NeighborID => neighborID;
    public NeighborNPCDefinition Definition => definition;
    public Transform GardenTarget => gardenTarget;
}

/// <summary>
/// Spawns NPCs at the entrance if their current schedule state is not Empty.
/// Garden -> personal garden; SharedLocation -> status SharedTarget.
/// Empty -> walk back to the entrance/exit, then despawn.
/// On every new day all NPCs are removed and the day's first slot is spawned fresh.
/// </summary>
[AddComponentMenu("Game/NPC/Neighbor NPC World Manager")]
public sealed class NeighborNPCWorldManager : MonoBehaviour
{
    [Header("Existing systems")]
    [SerializeField] private DayCycleManager dayCycle;

    [Header("Scene locations")]
    [SerializeField] private Transform entrance;
    [SerializeField] private Transform exitPoint;

    [Header("Navigation")]
    [SerializeField, Min(0f)] private float spawnScatterRadius = 0.8f;
    [SerializeField, Min(0.25f)] private float navMeshSampleRadius = 2f;
    [SerializeField, Min(0.05f)] private float arrivalTolerance = 0.25f;

    [Header("Schedule neighbor assignments")]
    [SerializeField] private List<NeighborNPCBinding> bindings = new();

    private readonly Dictionary<string, NeighborNPCActor> activeActors = new();

    public DayCycleManager DayCycle => dayCycle;
    public IReadOnlyList<NeighborNPCBinding> Bindings => bindings;
    public int SpawnedCount => activeActors.Count;

    private bool IsReady => dayCycle != null && dayCycle.IsConfigured && entrance != null;

    private void OnEnable()
    {
        if (dayCycle == null)
        {
            return;
        }

        dayCycle.OnSlotChanged += HandleSlotChanged;
        dayCycle.OnDayChanged += HandleDayChanged;
    }

    private void Start()
    {
        if (!IsReady)
        {
            Debug.LogWarning("NeighborNPCWorldManager: Assign DayCycleManager and Entrance, " +
                             "and configure at least one day and timeslot.", this);
            return;
        }

        RefreshAll();
    }

    private void OnDisable()
    {
        if (dayCycle != null)
        {
            dayCycle.OnSlotChanged -= HandleSlotChanged;
            dayCycle.OnDayChanged -= HandleDayChanged;
        }

        if (Application.isPlaying)
        {
            DespawnAllImmediate();
        }
    }

    private void HandleSlotChanged()
    {
        RefreshAll();
    }

    private void HandleDayChanged()
    {
        // Deliberately hard reset on day change, including when the week wraps.
        DespawnAllImmediate();
        RefreshAll();
    }

    /// <summary>
    /// Call this explicitly after runtime Active flags or binding data change.
    /// It is also useful as a play-mode debug action.
    /// </summary>
    public void RefreshAll()
    {
        if (!IsReady)
        {
            return;
        }

        DaySchedule schedule = dayCycle.DaySchedule;
        int dayIndex = dayCycle.CurrentDayIndex;
        int slotIndex = dayCycle.CurrentSlotIndex;
        HashSet<string> knownNeighborIDs = new();

        for (int i = 0; i < schedule.Neighbors.Count; i++)
        {
            NeighborDefinition neighbor = schedule.Neighbors[i];
            if (neighbor == null || string.IsNullOrEmpty(neighbor.ID))
            {
                continue;
            }

            string neighborID = neighbor.ID;
            knownNeighborIDs.Add(neighborID);

            if (!neighbor.Active)
            {
                DespawnImmediate(neighborID);
                continue;
            }

            NeighborNPCBinding binding = FindBinding(neighborID);
            if (binding == null || binding.Definition == null || binding.Definition.Prefab == null)
            {
                DespawnImmediate(neighborID);
                continue;
            }

            SlotStatusDefinition status = schedule.GetSlotStatus(dayIndex, i, slotIndex);
            if (status == null || status.ID == DaySchedule.EmptyStateID)
            {
                SendToExit(neighborID);
                continue;
            }

            if (!TryResolveDestination(binding, status, out Vector3 destination, out bool isExit))
            {
                Debug.LogWarning("NeighborNPCWorldManager: Missing or unreachable location for " +
                                 neighbor.Name + " (state: " + status.Name + ").", this);
                SendToExit(neighborID);
                continue;
            }

            NeighborNPCActor actor = GetOrSpawn(neighborID, i, binding);
            if (actor == null)
            {
                continue;
            }

            if (!actor.MoveTo(destination, status.ID, isExit))
            {
                Debug.LogWarning("NeighborNPCWorldManager: Could not move " + neighbor.Name +
                                 " to " + status.Name + ". Check the NavMesh.", actor);
            }
        }

        // Clean up actors whose neighbor was removed from the Day Schedule.
        List<string> removedIDs = new();
        foreach (string id in activeActors.Keys)
        {
            if (!knownNeighborIDs.Contains(id))
            {
                removedIDs.Add(id);
            }
        }

        foreach (string id in removedIDs)
        {
            DespawnImmediate(id);
        }
    }

    private NeighborNPCBinding FindBinding(string neighborID)
    {
        foreach (NeighborNPCBinding binding in bindings)
        {
            if (binding != null && binding.NeighborID == neighborID)
            {
                return binding;
            }
        }

        return null;
    }

    private bool TryResolveDestination(NeighborNPCBinding binding, 
                                       SlotStatusDefinition status,
                                       out Vector3 destination,
                                       out bool isExit)
    {
        destination = default;
        isExit = false;
        Transform target;

        switch (status.DestinationType)
        {
            case SlotDestinationType.OwnGarden:
                target = binding.GardenTarget;
                break;
            case SlotDestinationType.SharedLocation:
                target = status.SharedTarget;
                break;
            case SlotDestinationType.ExitPoint:
                target = exitPoint != null ? exitPoint : entrance;
                isExit = true;
                break;
            default:
                return false;
        }

        return target != null && TrySamplePosition(target.position, out destination);
    }

    private NeighborNPCActor GetOrSpawn(string neighborID, int neighborIndex, NeighborNPCBinding binding)
    {
        if (activeActors.TryGetValue(neighborID, out NeighborNPCActor existing))
        {
            if (existing != null)
            {
                return existing;
            }
            activeActors.Remove(neighborID);
        }

        // A small deterministic scatter reduces initial agent overlap.
        float angle = neighborIndex * 2.3999632f;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnScatterRadius;

        if (!TrySamplePosition(entrance.position + offset, out Vector3 spawnPosition))
        {
            Debug.LogWarning("NeighborNPCWorldManager: No NavMesh near the Entrance.", this);
            return null;
        }

        GameObject npcObject = Instantiate(binding.Definition.Prefab, spawnPosition, entrance.rotation, transform);

        NeighborNPCActor actor = npcObject.GetComponent<NeighborNPCActor>();
        if (actor == null || !actor.Initialize(neighborID, arrivalTolerance))
        {
            Debug.LogError("NeighborNPCWorldManager: NPC prefab needs NeighborNPCActor " +
                           "and a configured NavMeshAgent on its root object.", npcObject);
            Destroy(npcObject);
            return null;
        }

        npcObject.name = binding.Definition.Prefab.name + " (" + neighborID + ")";
        actor.OnExitReached += HandleExitReached;
        activeActors.Add(neighborID, actor);
        return actor;
    }

    private void SendToExit(string neighborID)
    {
        if (!activeActors.TryGetValue(neighborID, out NeighborNPCActor actor))
        {
            return; // Empty from the start -> no NPC to spawn.
        }

        if (actor == null)
        {
            activeActors.Remove(neighborID);
            return;
        }

        Transform destination = exitPoint != null ? exitPoint : entrance;
        if (!TrySamplePosition(destination.position, out Vector3 navPoint)
            || !actor.MoveTo(navPoint, DaySchedule.EmptyStateID, true))
        {
            Debug.LogWarning("NeighborNPCWorldManager: NPC cannot reach Exit, removing it.", actor);
            DespawnImmediate(neighborID);
        }
    }

    private bool TrySamplePosition(Vector3 position, out Vector3 sampledPosition)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            sampledPosition = hit.position;
            return true;
        }

        sampledPosition = default;
        return false;
    }

    private void HandleExitReached(NeighborNPCActor actor)
    {
        if (actor == null)
        {
            return;
        }

        if (activeActors.TryGetValue(actor.NeighborID, out NeighborNPCActor current) && current == actor)
        {
            DespawnImmediate(actor.NeighborID);
            RefreshAll();
        }
    }

    private void DespawnImmediate(string neighborID)
    {
        if (!activeActors.TryGetValue(neighborID, out NeighborNPCActor actor))
        {
            return;
        }

        activeActors.Remove(neighborID);
        if (actor != null)
        {
            actor.OnExitReached -= HandleExitReached;
            actor.gameObject.SetActive(false); // Hide instantly; Destroy runs at end of frame.
            Destroy(actor.gameObject);
        }
    }

    private void DespawnAllImmediate()
    {
        List<string> ids = new(activeActors.Keys);
        foreach (string id in ids)
        {
            DespawnImmediate(id);
        }
    }
}
