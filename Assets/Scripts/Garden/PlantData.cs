using UnityEngine;

/// <summary>
/// Shared definition for one crop. Scene fields store references to these
/// assets, while current growth stays per individual FieldController.
/// </summary>
[CreateAssetMenu(fileName = "New Plant", menuName = "Gardening/Plant Definition")]
public class PlantData : ScriptableObject
{
    [Header("Inventory Items")]
    [SerializeField] private ItemData seedItem;
    [SerializeField] private ItemData harvestedItem;
    [SerializeField, Min(1)] private int harvestAmount = 1;

    [Header("Growth")]
    [SerializeField, Min(1)] private int requiredGrowthSlots = 10;
    [SerializeField] private GameObject growingPlantPrefab;

    public ItemData SeedItem => seedItem;
    public ItemData HarvestedItem => harvestedItem;
    public int HarvestAmount => harvestAmount;
    public int RequiredGrowthSlots => Mathf.Max(1, requiredGrowthSlots);
    public GameObject GrowingPlantPrefab => growingPlantPrefab;

    public bool IsValid => seedItem != null && seedItem.Category == ItemCategory.Seed &&
                           harvestedItem != null && harvestedItem.CanStoreInBag;

    private void OnValidate()
    {
        if (seedItem != null && seedItem.Category != ItemCategory.Seed)
            Debug.LogWarning($"[PlantData] '{name}': Seed Item must have category Seed.", this);
        if (harvestedItem != null && !harvestedItem.CanStoreInBag)
            Debug.LogWarning($"[PlantData] '{name}': Harvested Item must be bag-storable.", this);
    }
}
