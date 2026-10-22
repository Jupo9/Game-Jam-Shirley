using System;
using UnityEngine;

[AddComponentMenu("Game/Time/Day Cycle Manager")]
public class DayCycleManager : MonoBehaviour
{
    [Header("Day Schedule Manager")]
    [SerializeField] private DaySchedule daySchedule;

    public event Action OnSlotChanged;
    public event Action OnDayChanged;
    public event Action OnDayCompleted;
    public event Action OnTimeChanged;

    private int currentDayIndex;
    private int consumedSlotsToday;

    public DaySchedule DaySchedule => daySchedule;

    public bool IsConfigured => daySchedule != null 
                                && daySchedule.Days.Count > 0 
                                && daySchedule.TimeSlots.Count > 0;

    public int TotalDays => daySchedule != null ? daySchedule.Days.Count : 0;
    public int TotalSlotsPerDay => daySchedule != null ? daySchedule.TimeSlots.Count : 0;

    // -1 means there is no valid Day/Slot
    public int CurrentDayIndex => IsConfigured ? Mathf.Clamp(currentDayIndex, 0, TotalDays - 1) : -1;
    public int CurrentSlotIndex => IsConfigured ? Mathf.Clamp(consumedSlotsToday, 0, TotalSlotsPerDay - 1) : -1;
    public int NextDayIndex => IsConfigured ? (CurrentDayIndex + 1) % TotalDays : -1;

    public string CurrentDayName => IsConfigured ? daySchedule.Days[CurrentDayIndex].Name : "No Day";
    public string CurrentSlotName => IsConfigured ? daySchedule.TimeSlots[CurrentSlotIndex].Name : "No Slot";
    public string NextDayName => IsConfigured ? daySchedule.Days[NextDayIndex].Name : "No Next Day";

    public int SlotsConsumedToday => IsConfigured ? Mathf.Clamp(consumedSlotsToday, 0, TotalSlotsPerDay) : 0;
    public int SlotsRemainingToday => IsConfigured ? TotalSlotsPerDay - SlotsConsumedToday : 0;

    public bool IsDayComplete => IsConfigured && SlotsConsumedToday >= TotalSlotsPerDay;
    public bool CanConsumeSlot => IsConfigured && !IsDayComplete;

    // For current Version every Run starts on Day one
    private void Awake()
    {
        currentDayIndex = 0;
        consumedSlotsToday = 0;
    }

    private void Start()
    {
        if (!IsConfigured)
        {
            Debug.LogWarning("DayCycleManager: Assign a DaySchedule with at least one Day and global timeslot!!!", this);
        }
    }

    public bool ConsumeSlot()
    {
        if (!CanConsumeSlot)
        {
            return false;
        }

        int previousSlotIndex = CurrentSlotIndex;
        consumedSlotsToday++;

        if (CurrentSlotIndex != previousSlotIndex)
        {
            OnSlotChanged?.Invoke();
        }

        if (IsDayComplete)
        {
            OnDayCompleted?.Invoke();
        }

        OnTimeChanged?.Invoke();
        return true;
    }

    public bool EndDay()
    {
        if (!IsConfigured)
        {
            return false;
        }

        int previousSlotIndex = CurrentSlotIndex;
        currentDayIndex = (CurrentDayIndex + 1) % TotalDays;
        consumedSlotsToday = 0;

        OnDayChanged?.Invoke();

        if (CurrentSlotIndex != previousSlotIndex)
        {
            OnSlotChanged?.Invoke();
        }

        OnTimeChanged?.Invoke();
        return true;
    }
}
