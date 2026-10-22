using UnityEngine;

/// <summary>
/// Asset configuration for one NPC. Schedule IDs and scene-specific garden
/// transforms are intentionally kept in NeighborNPCWorldManager.
/// </summary>
[CreateAssetMenu(fileName = "NeighborNPC", menuName = "Game/NPC/Neighbor NPC Definition")]
public sealed class NeighborNPCDefinition : ScriptableObject
{
    [SerializeField] private GameObject prefab;

    public GameObject Prefab => prefab;
}
