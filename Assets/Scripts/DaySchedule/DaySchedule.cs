using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/Day Schedule")]
public class DaySchedule : MonoBehaviour
{
    [SerializeField] private List<NeighborDefinition> neighbors = new();
    [SerializeField] private List<TimeSlotDefinition> timeSlots = new();
    [SerializeField] private List<DayData> days = new();

    public IReadOnlyList<NeighborDefinition> Neighbor => neighbors;
    public IReadOnlyList<TimeSlotDefinition> TimeSlots => timeSlots;
    public IReadOnlyList<DayData> Days => days;

    // =========================================================
    // NEIGHBORS
    // =========================================================

    public void AddNeighbor()
    {
        string defaultName = "Neighbor " + (neighbors.Count + 1);

        neighbors.Add(new NeighborDefinition(defaultName));

        EnsureDataIntegrity();
    }

    public void RemoveNeighborAt(int index)
    {
        if (index < 0 || index >= neighbors.Count)
        {
            return;
        }

        neighbors.RemoveAt(index);

        EnsureDataIntegrity();
    }

    public void RenameNeighbor(int index, string newName)
    {
        if (index < 0 || index >= neighbors.Count)
        { 
            return;
        }

        neighbors[index].Name = newName;
    }

    // =========================================================
    // TIMESLOTS
    // =========================================================

    public void AddTimeSlot()
    {
        string defaultName = "Slot " + (timeSlots.Count + 1);

        timeSlots.Add(new TimeSlotDefinition(defaultName));

        EnsureDataIntegrity();
    }

    public void RemoveLastTimeSlot()
    {
        if (timeSlots.Count == 0)
        { 
            return;
        }

        timeSlots.RemoveAt(timeSlots.Count - 1);

        EnsureDataIntegrity();
    }

    public void RenameTimeSlot(int index, string newName)
    {
        if (index < 0 || index >= timeSlots.Count)
        { 
            return;
        }

        timeSlots[index].Name = newName;
    }

    // =========================================================
    // DAYS
    // =========================================================

    public void AddDay()
    {
        string defaultName = "Day " + (days.Count + 1);

        days.Add(new DayData(defaultName));

        EnsureDataIntegrity();
    }

    public void RemoveDayAt(int index)
    {
        if (index < 0 || index >= days.Count)
        { 
            return;
        }

        days.RemoveAt(index);
    }

    public void RenameDay(int index, string newName)
    {
        if (index < 0 || index >= days.Count)
        { 
            return;
        }

        days[index].Name = newName;
    }

    // =========================================================
    // OCCUPIED / FREE
    // =========================================================

    public bool IsOccupied(int dayIndex, int neighborIndex, int timeSlotIndex)
    {
        if (!AreIndicesValid(dayIndex, neighborIndex, timeSlotIndex))
        { 
            return false;
        }

        string neighborID = neighbors[neighborIndex].ID;
        string timeSlotID = timeSlots[timeSlotIndex].ID;

        NeighborDayData neighborData = days[dayIndex].FindNeighbor(neighborID);

        if (neighborData == null)
        { 
            return false;
        }

        SlotState slotState = neighborData.FindSlot(timeSlotID);

        if (slotState == null)
        { 
            return false;
        }

        return slotState.Occupied;
    }

    public void ToggleOccupied(int dayIndex, int neighborIndex, int timeSlotIndex)
    {
        if (!AreIndicesValid(dayIndex, neighborIndex, timeSlotIndex))
        { 
            return;
        }

        string neighborID = neighbors[neighborIndex].ID;
        string timeSlotID = timeSlots[timeSlotIndex].ID;

        NeighborDayData neighborData = days[dayIndex].FindNeighbor(neighborID);

        if (neighborData == null)
        {
            return;
        }

        SlotState slotState = neighborData.FindSlot(timeSlotID);

        if (slotState == null)
        {
            return;
        }

        slotState.Occupied = !slotState.Occupied;
    }

    public void SetOccupied(int dayIndex, int neighborIndex, int timeSlotIndex, bool occupied)
    {
        if (!AreIndicesValid(dayIndex, neighborIndex, timeSlotIndex))
        {
            return;
        }

        string neighborID = neighbors[neighborIndex].ID;
        string timeSlotID = timeSlots[timeSlotIndex].ID;

        NeighborDayData neighborData = days[dayIndex].FindNeighbor(neighborID);

        if (neighborData == null)
        {
            return;
        }

        SlotState slotState = neighborData.FindSlot(timeSlotID);

        if (slotState == null)
        {
            return;
        }

        slotState.Occupied = occupied;
    }

    // =========================================================
    // DATA SYNCHRONIZATION
    // =========================================================

    public void EnsureDataIntegrity()
    {
        EnsureIDs();

        foreach (DayData day in days)
        {
            day.SyncWith(neighbors, timeSlots);
        }
    }

    private void EnsureIDs()
    {
        foreach (NeighborDefinition neighbor in neighbors)
        {
            neighbor.EnsureID();
        }

        foreach (TimeSlotDefinition timeSlot in timeSlots)
        {
            timeSlot.EnsureID();
        }

        foreach (DayData day in days)
        {
            day.EnsureID();
        }
    }

    private bool AreIndicesValid(int dayIndex, int neighborIndex, int timeSlotIndex)
    {
        if (dayIndex < 0 || dayIndex >= days.Count ||
            neighborIndex < 0 || neighborIndex >= neighbors.Count ||
            timeSlotIndex < 0 || timeSlotIndex >= timeSlots.Count)
        { 
            return false;
        }

        return true;
    }

    private void OnValidate()
    {
        EnsureDataIntegrity();
    }
}

// =============================================================
// Neighbor DEFINITION
// =============================================================

[Serializable]
public class NeighborDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string neighborName;

    public string ID => id;

    public string Name
    {
        get => neighborName;
        set => neighborName = value;
    }

    public NeighborDefinition(string name)
    {
        id = Guid.NewGuid().ToString();
        neighborName = name;
    }

    public void EnsureID()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
        }
    }
}

// =============================================================
// TIMESLOT DEFINITION
// =============================================================

[Serializable]
public class TimeSlotDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string slotName;

    public string ID => id;

    public string Name
    {
        get => slotName;
        set => slotName = value;
    }

    public TimeSlotDefinition(string name)
    {
        id = Guid.NewGuid().ToString();
        slotName = name;
    }

    public void EnsureID()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
        }
    }
}

// =============================================================
// DAY
// =============================================================

[Serializable]
public class DayData
{
    [SerializeField] private string id;
    [SerializeField] private string dayName;

    [SerializeField] private List<NeighborDayData> neighbors = new();

    public string ID => id;

    public string Name
    {
        get => dayName;
        set => dayName = value;
    }

    public DayData(string name)
    {
        id = Guid.NewGuid().ToString();
        dayName = name;
    }

    public void EnsureID()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
        }
    }

    public NeighborDayData FindNeighbor(string neighborID)
    {
        return neighbors.Find(neighbor => neighbor.NeighborID == neighborID);
    }

    public void SyncWith(IReadOnlyList<NeighborDefinition> globalNeighbor, IReadOnlyList<TimeSlotDefinition> globalTimeSlots)
    {
        // Remove Neighbor that no longer exist
        for (int i = neighbors.Count - 1; i >= 0; i--)
        {
            bool neighborStillExists = false;

            foreach (NeighborDefinition neighbor in globalNeighbor)
            {
                if (neighbor.ID == neighbors[i].NeighborID)
                {
                    neighborStillExists = true;
                    break;
                }
            }

            if (!neighborStillExists)
            {
                neighbors.RemoveAt(i);
            }
        }

        // Add missing Neighbor
        foreach (NeighborDefinition neighbor in globalNeighbor)
        {
            NeighborDayData neighborData = FindNeighbor(neighbor.ID);

            if (neighborData == null)
            {
                neighborData = new NeighborDayData(neighbor.ID);

                neighbors.Add(neighborData);
            }

            neighborData.SyncTimeSlots(globalTimeSlots);
        }
    }
}

// =============================================================
// NEIGHBOR DATA INSIDE A DAY
// =============================================================

[Serializable]
public class NeighborDayData
{
    [SerializeField] private string neighborID;
    [SerializeField] private List<SlotState> slots = new();

    public string NeighborID => neighborID;

    public NeighborDayData(string id)
    {
        neighborID = id;
    }

    public SlotState FindSlot(string timeSlotID)
    {
        return slots.Find(slot => slot.TimeSlotID == timeSlotID);
    }

    public void SyncTimeSlots(IReadOnlyList<TimeSlotDefinition> globalTimeSlots)
    {
        // Remove deleted timeslots
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            bool slotStillExists = false;

            foreach (TimeSlotDefinition timeSlot in globalTimeSlots)
            {
                if (timeSlot.ID == slots[i].TimeSlotID)
                {
                    slotStillExists = true;
                    break;
                }
            }

            if (!slotStillExists)
            {
                slots.RemoveAt(i);
            }
        }

        // Add new timeslots
        foreach (TimeSlotDefinition timeSlot in globalTimeSlots)
        {
            if (FindSlot(timeSlot.ID) == null)
            {
                slots.Add( new SlotState(timeSlot.ID));
            }
        }
    }
}

// =============================================================
// SINGLE SLOT STATE
// =============================================================

[Serializable]
public class SlotState
{
    [SerializeField] private string timeSlotID;
    [SerializeField] private bool occupied;

    public string TimeSlotID => timeSlotID;

    public bool Occupied
    {
        get => occupied;
        set => occupied = value;
    }

    public SlotState(string id)
    {
        timeSlotID = id;
        occupied = false;
    }
}
