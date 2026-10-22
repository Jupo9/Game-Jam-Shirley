using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Lives on an NPC prefab together with NavMeshAgent.
/// Does not read the schedule or decide where to go; it only moves.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public sealed class NeighborNPCActor : MonoBehaviour
{
    private NavMeshAgent agent;
    private bool movingToExit;
    private float arrivalTolerance;
    private Vector3 exitDestination;

    public string NeighborID { get; private set; }
    public string CurrentStateID { get; private set; }
    public bool IsLeaving => movingToExit;

    public event Action<NeighborNPCActor> OnExitReached;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public bool Initialize(string neighborID, float tolerance)
    {
        NeighborID = neighborID;
        arrivalTolerance = Mathf.Max(0.05f, tolerance);
        return agent != null && agent.isOnNavMesh;
    }

    public bool MoveTo(Vector3 destination, string stateID, bool isExit)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return false;
        }

        if (!agent.SetDestination(destination))
        {
            return false;
        }

        CurrentStateID = stateID;
        movingToExit = isExit;
        if (isExit)
        {
            exitDestination = destination;
        }
        return true;
    }

    private void Update()
    {
        if (!movingToExit || agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        Vector3 currentPosition = agent.nextPosition;
        float distanceX = currentPosition.x - exitDestination.x;
        float distanceZ = currentPosition.z - exitDestination.z;
        float distanceLimit = agent.stoppingDistance + arrivalTolerance;

        if (distanceX * distanceX + distanceZ * distanceZ > distanceLimit * distanceLimit)
        {
            return;
        }

        movingToExit = false;
        agent.ResetPath();
        OnExitReached?.Invoke(this);
    }
}
