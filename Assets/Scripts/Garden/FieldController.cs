using System;
using System.Collections.Generic;
using UnityEngine;

public enum FieldState
{
    Empty,
    Dug,
    Seeded,
    Watered,
    Growing,
    ReadyToHarvest
}

/// <summary>
/// Per-field planting state and visual growth. Interactions come only from
/// TriggerEvents; all item and time requirements are checked before spending.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Game/Garden/Field Controller")]
public class FieldController : MonoBehaviour
{
    [Header("Time (WorldTimeTracker must be active)")]
    [SerializeField] private WorldTimeTracker worldTime;

    [Header("Tools")]
    [SerializeField] private ItemData requiredShovel;
    [Tooltip("Optional. If empty, any filled WaterContainerState can water the field.")]
    [SerializeField] private ItemData requiredWaterContainer;

    [Header("Allowed Plants")]
    [SerializeField] private List<PlantData> allowedPlants = new();

    [Header("Actions cost this many time slots")]
    [SerializeField, Min(1)] private int digCost = 1;
    [SerializeField, Min(1)] private int sowCost = 1;
    [SerializeField, Min(1)] private int waterCost = 1;
    [SerializeField, Min(1)] private int harvestCost = 1;

    [Header("Optional Scene Visuals")]
    [SerializeField] private Transform plantAnchor;
    [SerializeField] private GameObject emptyVisual;
    [SerializeField] private GameObject dugVisual;
    [SerializeField] private GameObject seededVisual;

    private FieldState state = FieldState.Empty;
    private PlantData plantedPlant;
    private long wateredAtElapsedSlot;
    private GameObject plantInstance;
    private Vector3 fullPlantScale = Vector3.one;

    public event Action OnFieldChanged;

    public FieldState State => state;
    public PlantData PlantedPlant => plantedPlant;
    public int RequiredGrowthSlots => plantedPlant != null ? plantedPlant.RequiredGrowthSlots : 0;
    public int ElapsedGrowthSlots => plantedPlant == null || worldTime == null ||
                                     (state != FieldState.Watered && state != FieldState.Growing &&
                                      state != FieldState.ReadyToHarvest)
        ? 0
        : (int)Math.Min((long)RequiredGrowthSlots,
            Math.Max(0L, worldTime.TotalElapsedSlots - wateredAtElapsedSlot));
    public float GrowthProgress => RequiredGrowthSlots > 0
        ? Mathf.Clamp01((float)ElapsedGrowthSlots / RequiredGrowthSlots)
        : 0f;

    private void Awake()
    {
        RefreshVisuals();
    }

    private void OnEnable()
    {
        if (worldTime != null) worldTime.OnTimeAdvanced += HandleTimeAdvanced;
    }

    private void OnDisable()
    {
        if (worldTime != null) worldTime.OnTimeAdvanced -= HandleTimeAdvanced;
    }

    private void Start()
    {
        if (worldTime == null)
            Debug.LogWarning("[Field] Assign a WorldTimeTracker to enable watering and growth.", this);
    }

    public bool CanInteract(PlayerInteractionController player)
    {
        if (player == null || player.ItemController == null || player.DayCycle == null ||
            !player.DayCycle.IsConfigured) return false;

        PlayerItemController items = player.ItemController;
        PlayerInventory inventory = items.Inventory;

        switch (state)
        {
            case FieldState.Empty:
                return HasTime(player, digCost) && requiredShovel != null &&
                       items.IsHolding(requiredShovel);

            case FieldState.Dug:
                return HasTime(player, sowCost) && inventory != null &&
                       inventory.HasSelectedSeed && FindPlant(inventory.SelectedSeed) != null;

            case FieldState.Seeded:
                return HasTime(player, waterCost) && worldTime != null && worldTime.isActiveAndEnabled &&
                       items.HasFilledWaterContainer &&
                       (requiredWaterContainer == null || items.IsHolding(requiredWaterContainer));

            case FieldState.ReadyToHarvest:
                return HasTime(player, harvestCost) && inventory != null &&
                       plantedPlant != null && plantedPlant.IsValid &&
                       inventory.CanAddItem(plantedPlant.HarvestedItem, plantedPlant.HarvestAmount);

            // A growing plant cannot be interacted with before harvest.
            default:
                return false;
        }
    }

    public bool Interact(PlayerInteractionController player)
    {
        if (!CanInteract(player)) return false;

        PlayerItemController items = player.ItemController;
        PlayerInventory bag = items.Inventory;

        switch (state)
        {
            case FieldState.Empty:
                if (!SpendTime(player, digCost)) return false;
                state = FieldState.Dug;
                Debug.Log("[Field] Dug the field.", this);
                break;

            case FieldState.Dug:
                ItemData selectedSeed = bag.SelectedSeed;
                PlantData plant = FindPlant(selectedSeed);
                if (plant == null || !SpendTime(player, sowCost)) return false;
                if (!bag.RemoveItem(selectedSeed))
                {
                    Debug.LogError("[Field] Seed disappeared after time was spent.", this);
                    return false;
                }
                plantedPlant = plant;
                state = FieldState.Seeded;
                Debug.Log("[Field] Sowed " + selectedSeed.ItemName + ".", this);
                break;

            case FieldState.Seeded:
                // ConsumeSlot() notifies time listeners synchronously. Assign
                // Watered AFTER spending time so this action does not also grow it.
                if (!SpendTime(player, waterCost)) return false;
                if (!items.TryUseEquippedWater())
                {
                    Debug.LogError("[Field] Water container changed after time was spent.", this);
                    return false;
                }
                wateredAtElapsedSlot = worldTime.TotalElapsedSlots;
                state = FieldState.Watered;
                CreatePlantVisual();
                Debug.Log("[Field] Watered. Growth starts on the NEXT elapsed slot.", this);
                break;

            case FieldState.ReadyToHarvest:
                ItemData harvestItem = plantedPlant.HarvestedItem;
                int amount = plantedPlant.HarvestAmount;
                if (!SpendTime(player, harvestCost)) return false;
                if (!bag.AddItem(harvestItem, amount))
                {
                    Debug.LogError("[Field] Failed to add harvest after time was spent.", this);
                    return false;
                }
                Debug.Log($"[Field] Harvested {amount}x {harvestItem.ItemName}.", this);
                ClearField();
                break;

            default:
                return false;
        }

        RefreshVisuals();
        OnFieldChanged?.Invoke();
        return true;
    }

    private PlantData FindPlant(ItemData seed)
    {
        if (seed == null) return null;
        foreach (PlantData plant in allowedPlants)
        {
            if (plant != null && plant.IsValid && plant.SeedItem == seed)
                return plant;
        }
        return null;
    }

    private static bool HasTime(PlayerInteractionController player, int amount)
    {
        return player.DayCycle != null && player.DayCycle.IsConfigured &&
               player.DayCycle.SlotsRemainingToday >= amount;
    }

    private static bool SpendTime(PlayerInteractionController player, int amount)
    {
        if (!HasTime(player, amount)) return false;
        for (int i = 0; i < amount; i++)
        {
            if (!player.DayCycle.ConsumeSlot()) return false;
        }
        return true;
    }

    private void HandleTimeAdvanced(int _)
    {
        if (state != FieldState.Watered && state != FieldState.Growing) return;
        if (plantedPlant == null) return;

        if (ElapsedGrowthSlots >= plantedPlant.RequiredGrowthSlots)
        {
            state = FieldState.ReadyToHarvest;
            Debug.Log("[Field] Plant is ready to harvest!", this);
        }
        else if (ElapsedGrowthSlots > 0)
        {
            state = FieldState.Growing;
        }

        RefreshVisuals();
        OnFieldChanged?.Invoke();
    }

    private void CreatePlantVisual()
    {
        DestroyPlantVisual();
        if (plantedPlant == null || plantedPlant.GrowingPlantPrefab == null) return;

        Transform parent = plantAnchor != null ? plantAnchor : transform;
        plantInstance = Instantiate(plantedPlant.GrowingPlantPrefab, parent);
        plantInstance.transform.localPosition = Vector3.zero;
        plantInstance.transform.localRotation = Quaternion.identity;
        fullPlantScale = plantInstance.transform.localScale;
        ApplyGrowthScale();
    }

    private void ApplyGrowthScale()
    {
        if (plantInstance != null)
            plantInstance.transform.localScale = fullPlantScale * GrowthProgress;
    }

    private void ClearField()
    {
        DestroyPlantVisual();
        plantedPlant = null;
        wateredAtElapsedSlot = 0;
        state = FieldState.Empty;
    }

    private void DestroyPlantVisual()
    {
        if (plantInstance == null) return;
        Destroy(plantInstance);
        plantInstance = null;
    }

    private void RefreshVisuals()
    {
        if (emptyVisual != null) emptyVisual.SetActive(state == FieldState.Empty);
        if (dugVisual != null) dugVisual.SetActive(state != FieldState.Empty);
        if (seededVisual != null)
            seededVisual.SetActive(state == FieldState.Seeded || state == FieldState.Watered);
        ApplyGrowthScale();
    }
}
