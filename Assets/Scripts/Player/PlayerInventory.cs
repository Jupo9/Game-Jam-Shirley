using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BagItemStack
{
    [SerializeField] private ItemData item;
    [SerializeField, Min(0)] private int amount;

    public ItemData Item => item;
    public int Amount => amount;

    public BagItemStack(ItemData item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    internal void SetAmount(int value) => amount = value;
}

/// <summary>
/// Runtime bag for stackable items (seeds, crops, miscellaneous items).
/// Starting items can be configured in the Inspector without modifying ItemData assets.
/// All gameplay changes affect a runtime copy, not the Inspector starting list.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Game/Inventory/Player Inventory")]
public class PlayerInventory : MonoBehaviour
{
    [Header("Starting Bag Contents (Debug / Prototype)")]
    [SerializeField] private List<BagItemStack> startingItems = new();

    [Header("Starting Selection (Optional)")]
    [SerializeField] private ItemData startingSelectedSeed;

    private readonly List<BagItemStack> stacks = new();
    private ItemData selectedSeed;

    public event Action OnInventoryChanged;

    public IReadOnlyList<BagItemStack> Stacks => stacks;
    public ItemData SelectedSeed => selectedSeed;
    public bool HasSelectedSeed => selectedSeed != null && GetAmount(selectedSeed) > 0;

    private void Awake()
    {
        foreach (BagItemStack starting in startingItems)
        {
            if (starting != null && starting.Item != null && starting.Amount > 0)
            {
                AddItemInternal(starting.Item, starting.Amount);
            }
        }

        if (startingSelectedSeed != null)
        {
            SelectSeed(startingSelectedSeed);
        }

        // If no explicit choice was provided, preselect the first available seed.
        if (selectedSeed == null)
        {
            foreach (BagItemStack stack in stacks)
            {
                if (stack.Item.Category == ItemCategory.Seed)
                {
                    selectedSeed = stack.Item;
                    break;
                }
            }
        }
    }

    public bool CanAddItem(ItemData item, int amount = 1)
    {
        if (item == null || !item.CanStoreInBag || amount <= 0)
        {
            return false;
        }

        BagItemStack existing = FindStack(item);
        return existing == null || existing.Amount <= int.MaxValue - amount;
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (!CanAddItem(item, amount))
        {
            return false;
        }

        AddItemInternal(item, amount);

        if (selectedSeed == null && item.Category == ItemCategory.Seed)
        {
            selectedSeed = item;
        }

        NotifyChanged();
        return true;
    }

    public bool HasItem(ItemData item, int amount = 1)
    {
        return item != null && amount > 0 && GetAmount(item) >= amount;
    }

    public int GetAmount(ItemData item)
    {
        BagItemStack stack = FindStack(item);
        return stack != null ? stack.Amount : 0;
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (!HasItem(item, amount))
        {
            return false;
        }

        BagItemStack stack = FindStack(item);
        int newAmount = stack.Amount - amount;
        if (newAmount == 0)
        {
            stacks.Remove(stack);
            if (selectedSeed == item)
            {
                selectedSeed = null;
            }
        }
        else
        {
            stack.SetAmount(newAmount);
        }

        NotifyChanged();
        return true;
    }

    public bool SelectSeed(ItemData seed)
    {
        // Null explicitly deselects. Other items must be real owned seeds.
        if (seed != null && (seed.Category != ItemCategory.Seed || !HasItem(seed)))
        {
            return false;
        }

        if (selectedSeed == seed)
        {
            return true;
        }

        selectedSeed = seed;
        NotifyChanged();
        return true;
    }

    public bool TryConsumeSelectedSeed(out ItemData consumedSeed)
    {
        consumedSeed = selectedSeed;
        if (consumedSeed == null || !RemoveItem(consumedSeed))
        {
            consumedSeed = null;
            return false;
        }

        return true;
    }

    private void AddItemInternal(ItemData item, int amount)
    {
        // Used only after validation, including when setting up starting contents.
        if (!CanAddItem(item, amount))
        {
            Debug.LogWarning($"[PlayerInventory] Cannot add {amount}x '{item?.name}'.", this);
            return;
        }

        BagItemStack existing = FindStack(item);
        if (existing == null)
        {
            stacks.Add(new BagItemStack(item, amount));
        }
        else
        {
            existing.SetAmount(existing.Amount + amount);
        }
    }

    private BagItemStack FindStack(ItemData item)
    {
        if (item == null)
        {
            return null;
        }

        // Assets are the canonical identities during gameplay.
        // ItemID is reserved for save/load and must be unique between assets.
        foreach (BagItemStack stack in stacks)
        {
            if (stack.Item == item)
            {
                return stack;
            }
        }

        return null;
    }

    private void NotifyChanged() => OnInventoryChanged?.Invoke();
}
