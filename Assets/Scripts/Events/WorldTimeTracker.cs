using System;
using UnityEngine;

/// <summary>
/// Counts elapsed in-game slots, including those skipped by EndDay().
/// Works with the existing DayCycleManager without changing it.
/// Place on an active GameObject and assign the DayCycleManager.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Game/Time/World Time Tracker")]
public class WorldTimeTracker : MonoBehaviour
{
    [SerializeField] private DayCycleManager dayCycle;

    private long elapsedSlots;
    private int observedConsumedSlots;
    private int observedDayIndex;

    public event Action<int> OnTimeAdvanced;

    public long TotalElapsedSlots => elapsedSlots;
    public DayCycleManager DayCycle => dayCycle;

    private void Awake()
    {
        if (dayCycle == null)
        {
            dayCycle = GetComponent<DayCycleManager>();
        }

        Snapshot();
    }

    private void OnEnable()
    {
        if (dayCycle == null)
        {
            Debug.LogWarning("[WorldTimeTracker] Assign a DayCycleManager.", this);
            return;
        }

        Snapshot();
        dayCycle.OnTimeChanged += HandleTimeChanged;
        dayCycle.OnDayChanged += HandleDayChanged;
    }

    private void OnDisable()
    {
        if (dayCycle == null) return;
        dayCycle.OnTimeChanged -= HandleTimeChanged;
        dayCycle.OnDayChanged -= HandleDayChanged;
    }

    private void Snapshot()
    {
        observedConsumedSlots = dayCycle != null ? dayCycle.SlotsConsumedToday : 0;
        observedDayIndex = dayCycle != null ? dayCycle.CurrentDayIndex : -1;
    }

    private void HandleTimeChanged()
    {
        if (dayCycle == null || !dayCycle.IsConfigured) return;

        // A normal ConsumeSlot() increments SlotsConsumedToday by one.
        // EndDay() is handled in HandleDayChanged(), which is fired first.
        int consumedNow = dayCycle.SlotsConsumedToday;
        int delta = dayCycle.CurrentDayIndex == observedDayIndex
            ? Mathf.Max(0, consumedNow - observedConsumedSlots)
            : 0;

        Snapshot();
        Advance(delta);
    }

    private void HandleDayChanged()
    {
        if (dayCycle == null || !dayCycle.IsConfigured) return;

        // A day always contains TotalSlotsPerDay periods. Already consumed
        // periods were counted by HandleTimeChanged(), so add only the rest.
        // This also works for a schedule containing just ONE day.
        int remaining = Mathf.Max(0, dayCycle.TotalSlotsPerDay - observedConsumedSlots);
        Snapshot();
        Advance(remaining);
    }

    private void Advance(int slots)
    {
        if (slots <= 0) return;
        elapsedSlots += slots;
        OnTimeAdvanced?.Invoke(slots);
    }
}
