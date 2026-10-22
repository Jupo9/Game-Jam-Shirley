using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Game/Day Schedule")]
public class DaySchedule : MonoBehaviour
{
    public const string EmptyStateID = "empty";
    public const string GardenStateID = "garden";

    [SerializeField] private List<NeighborDefinition> neighbors = new();
    [SerializeField] private List<TimeSlotDefinition> timeSlots = new();

    [FormerlySerializedAs("slotStatus")]
    [SerializeField] private List<SlotStatusDefinition> slotStatus = new();
    [SerializeField] private List<DayData> days = new();

    public IReadOnlyList<NeighborDefinition> Neighbors => neighbors;
    public IReadOnlyList<TimeSlotDefinition> TimeSlots => timeSlots;
    public IReadOnlyList<SlotStatusDefinition> SlotStatus => slotStatus;
    public IReadOnlyList<DayData> Days => days;

    // ===================================================
    // NEIGHBORS
    // ==================================================

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

    public void SetNeighborActive(int index, bool isActive)
    {
        if (index < 0 || index >= neighbors.Count)
        {
            return;
        }

        neighbors[index].Active = isActive;
    }

    public void SetNeighborVisibleOnBoard(int index, bool visible)
    {
        if (index < 0 || index >= neighbors.Count)
        {
            return;
        }

        neighbors[index].VisibleOnBoard = visible;
    }

    public bool ShouldShowNeighborOnBoard(int neighborIndex)
    {
        return neighborIndex >= 0 && neighborIndex < neighbors.Count 
                                  && neighbors[neighborIndex].Active 
                                  && neighbors[neighborIndex].VisibleOnBoard; 
    }

    // ==================================================
    // TIMESLOTS
    // ==================================================

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

    // ==================================================
    // SLOTS STATUS
    // ==================================================

    public void AddSlotStatus()
    {
        int number = slotStatus.Count - 1;
        slotStatus.Add(new SlotStatusDefinition(Guid.NewGuid().ToString(),
                     "Location " + number, new Color(0.26f, 0,48f, 0.86f),
                     "L", SlotDestinationType.SharedLocation)); 
    }

    //protect Empty Status (0) and Garden Status(1); for other Slots delete and change is enabled
    public bool RemoveSlotStatusAt(int index)
    {
        if (index < 2 || index >= slotStatus.Count)
        {
            return false;
        }

        string removeID = slotStatus[index].ID;
        slotStatus.RemoveAt(index);

        foreach (DayData day in days)
        {
            foreach (NeighborDayData neighbor in day.NeighborEntries)
            {
                foreach (SlotState slot in neighbor.Slots)
                {
                    if (slot.StateID == removeID)
                    {
                        slot.SetStatus(EmptyStateID);
                    }
                }
            }
        }

        return true;
    }

    // Reorder only custom statuses. Cell assignments stay safe because they store IDs.
    public bool MoveSlotStatus(int index, int direction)
    {
        int destinationIndex = index + direction;
        if (index < 2 || destinationIndex < 2 || index >= slotStatus.Count || destinationIndex >= slotStatus.Count)
        { 
            return false;
        }

        SlotStatusDefinition status = slotStatus[index];
        slotStatus[index] = slotStatus[destinationIndex];
        slotStatus[destinationIndex] = status;

        return true;
    }

    public SlotStatusDefinition FindSlotStatus(string statusID)
    {
        return slotStatus.Find(status => status != null && status.ID == statusID);
    }

    public void RenameSlotStatus(int index, string name)
    {
        if (index < 2 || index >= slotStatus.Count) 
        { 
            return;
        }

        slotStatus[index].Name = name;
    }

    public void SetSlotStatusColor(int index, Color color)
    {
        if (index < 0 || index >= slotStatus.Count) 
        { 
            return;
        }

        slotStatus[index].Color = color;
    }

    public void SetSlotStatusMarker(int index, string marker)
    {
        if (index < 0 || index >= slotStatus.Count) 
        { 
            return;
        }

        slotStatus[index].Marker = marker;
    }

    public void SetSlotStatusDestinationType(int index, SlotDestinationType destinationType)
    {
        if (index < 2 || index >= slotStatus.Count) 
        { 
            return;
        }

        slotStatus[index].DestinationType = destinationType;
    }

    public void SetSlotStatusSharedTarget(int index, Transform target)
    {
        if (index < 2 || index >= slotStatus.Count) 
        { 
            return;
        }

        slotStatus[index].SharedTarget = target;
    }

    // ==================================================
    // DAYS
    // ==================================================

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

    // ==================================================
    // ACCESS CELLS
    // ==================================================

    private SlotState FindCell(int dayIndex, int neighborIndex, int timeSlotIndex)
    {
        if (!AreIndicesValid(dayIndex, neighborIndex, timeSlotIndex)) 
        { 
            return null;
        }

        NeighborDayData neighbor = days[dayIndex].FindNeighbor(neighbors[neighborIndex].ID);
        return neighbor?.FindSlot(timeSlots[timeSlotIndex].ID);
    }

    public string GetSlotStatusID(int dayIndex, int neighborIndex, int timeSlotIndex)
    {
        SlotState cell = FindCell(dayIndex, neighborIndex, timeSlotIndex);
        return cell == null ? string.Empty : cell.StateID;
    }

    public SlotStatusDefinition GetSlotStatus(int dayIndex, int neighborIndex, int timeSlotIndex)
    {
        SlotState cell = FindCell(dayIndex, neighborIndex, timeSlotIndex);
        return cell == null ? null : FindSlotStatus(cell.StateID);
    }

    public bool SetSlotStatus(int dayIndex, int neighborIndex, int timeSlotIndex, string statusID)
    {
        SlotState cell = FindCell(dayIndex, neighborIndex, timeSlotIndex);

        if (cell == null || FindSlotStatus(statusID) == null) 
        { 
            return false;
        }

        cell.SetStatus(statusID);
        return true;
    }

    // Order Logic: Empty -> Garden -> custom status 1 -> ... -> Empty
    public void CycleSlotStatus(int dayIndex, int neighborIndex, int timeSlotIndex)
    {
        SlotState cell = FindCell(dayIndex, neighborIndex, timeSlotIndex);

        if (cell == null || slotStatus.Count == 0) 
        { 
            return;
        }

        int index = slotStatus.FindIndex(state => state.ID == cell.StateID);
        int nextIndex = (index + 1) % slotStatus.Count;
        cell.SetStatus(slotStatus[nextIndex].ID);
    }

    // ==================================================
    // SYNCHRONIZATION
    // ==================================================

    public bool EnsureDataIntegrity()
    {
        bool changed = EnsureStatusDefinitions();

        foreach (NeighborDefinition neighbor in neighbors)
        {
            if (neighbor == null) continue;
            changed |= neighbor.EnsureID();
            changed |= neighbor.EnsureFlagsInitialized();
        }

        foreach (TimeSlotDefinition timeSlot in timeSlots)
        { 
            if (timeSlot != null) 
            { 
                changed |= timeSlot.EnsureID();
            }
        }

        foreach (DayData day in days)
        {
            if (day == null) 
            { 
                continue;
            }

            changed |= day.EnsureID();
            changed |= day.SyncWith(neighbors, timeSlots);

            foreach (NeighborDayData neighbor in day.NeighborEntries)
            {
                foreach (SlotState cell in neighbor.Slots)
                {
                    // Converts old serialized bool occupied=true into the Garden state.
                    changed |= cell.MigrateLegacyState();
                    if (FindSlotStatus(cell.StateID) == null)
                    {
                        cell.SetStatus(EmptyStateID);
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }

    private bool EnsureStatusDefinitions()
    {
        bool changed = false;

        // Ensure the old red X gets a stable target even on existing components.
        if (FindSlotStatus(EmptyStateID) == null)
        {
            slotStatus.Insert(0, new SlotStatusDefinition(EmptyStateID,
                             "Empty", new Color(0.63f, 0.63f, 0.63f),
                             string.Empty, SlotDestinationType.ExitPoint));
            changed = true;
        }

        if (FindSlotStatus(GardenStateID) == null)
        {
            slotStatus.Insert(Mathf.Min(1, slotStatus.Count),
                             new SlotStatusDefinition(GardenStateID, "Garden",
                             new Color(0.95f, 0.32f, 0.32f), "X",
                             SlotDestinationType.OwnGarden));
            changed = true;
        }

        // Repair ordering if a list has been changed through raw serialization.
        int emptyIndex = slotStatus.FindIndex(s => s.ID == EmptyStateID);
        if (emptyIndex != 0)
        {
            SlotStatusDefinition empty = slotStatus[emptyIndex];
            slotStatus.RemoveAt(emptyIndex);
            slotStatus.Insert(0, empty);
            changed = true;
        }

        int gardenIndex = slotStatus.FindIndex(s => s.ID == GardenStateID);
        if (gardenIndex != 1)
        {
            SlotStatusDefinition garden = slotStatus[gardenIndex];
            slotStatus.RemoveAt(gardenIndex);
            slotStatus.Insert(1, garden);
            changed = true;
        }

        // Empty and Garden retain fixed navigation semantics.
        if (slotStatus[0].DestinationType != SlotDestinationType.ExitPoint)
        {
            slotStatus[0].DestinationType = SlotDestinationType.ExitPoint;
            changed = true;
        }
        if (slotStatus[1].DestinationType != SlotDestinationType.OwnGarden)
        {
            slotStatus[1].DestinationType = SlotDestinationType.OwnGarden;
            changed = true;
        }

        return changed;
    }

    private bool AreIndicesValid(int dayIndex, int neighborIndex, int timeSlotIndex)
    {
        return dayIndex >= 0 && dayIndex < days.Count 
                             && neighborIndex >= 0 && neighborIndex < neighbors.Count 
                             && timeSlotIndex >= 0 && timeSlotIndex < timeSlots.Count;
    }

    private void Awake() => EnsureDataIntegrity();
    private void OnValidate() => EnsureDataIntegrity();
}

// ==================================================
// NEIGHBOR DEFINITION
// ==================================================

[Serializable]
public class NeighborDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string neighborName;
    [SerializeField] private bool active = true;
    [SerializeField] private bool visibleOnBoard = true;
    // Needed because absent bool fields in older serialized objects would otherwise be false.
    [SerializeField, HideInInspector] private bool flagsInitialized;

    public string ID => id;
    public string Name { get => neighborName; set => neighborName = value; }
    public bool Active { get => active; set => active = value; }
    public bool VisibleOnBoard { get => visibleOnBoard; set => visibleOnBoard = value; }

    public NeighborDefinition(string name)
    {
        id = Guid.NewGuid().ToString();
        neighborName = name;
        active = true;
        visibleOnBoard = true;
        flagsInitialized = true;
    }

    public bool EnsureID()
    {
        if (!string.IsNullOrEmpty(id)) 
        {
            return false;
        }

        id = Guid.NewGuid().ToString();
        return true;
    }

    public bool EnsureFlagsInitialized()
    {
        if (flagsInitialized) 
        { 
            return false;
        }

        active = true;
        visibleOnBoard = true;
        flagsInitialized = true;
        return true;
    }
}

// ==================================================
// TIMESLOT DEFINITION
// ==================================================

[Serializable]
public class TimeSlotDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string slotName;

    public string ID => id;
    public string Name { get => slotName; set => slotName = value; }

    public TimeSlotDefinition(string name)
    {
        id = Guid.NewGuid().ToString();
        slotName = name;
    }

    public bool EnsureID()
    {
        if (!string.IsNullOrEmpty(id)) 
        { 
            return false;
        }
        id = Guid.NewGuid().ToString();
        return true;
    }
}

// ==================================================
// SLOT STATUS CONFIGURATION
// ==================================================

public enum SlotDestinationType
{
    ExitPoint,
    OwnGarden,
    SharedLocation
}

[Serializable]
public class SlotStatusDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string statusName;
    [SerializeField] private Color color;
    [SerializeField] private string marker;
    [SerializeField] private SlotDestinationType destinationType;
    [SerializeField] private Transform sharedTarget;

    public string ID => id;
    public string Name { get => statusName; set => statusName = value; }
    public Color Color { get => color; set => color = value; }
    public string Marker { get => marker; set => marker = value; }
    public SlotDestinationType DestinationType { get => destinationType; set => destinationType = value; }
    public Transform SharedTarget { get => sharedTarget; set => sharedTarget = value; }

    public SlotStatusDefinition(string newID, string name, Color statusColor,
        string statusMarker, SlotDestinationType destination)
    {
        id = newID;
        statusName = name;
        color = statusColor;
        marker = statusMarker;
        destinationType = destination;
    }
}

// ==================================================
// DAY DEFINITION
// ==================================================

[Serializable]
public class DayData
{
    [SerializeField] private string id;
    [SerializeField] private string dayName;
    [SerializeField] private List<NeighborDayData> neighbors = new();

    public string ID => id;
    public string Name { get => dayName; set => dayName = value; }
    public IReadOnlyList<NeighborDayData> NeighborEntries => neighbors;

    public DayData(string name)
    {
        id = Guid.NewGuid().ToString();
        dayName = name;
    }

    public bool EnsureID()
    {
        if (!string.IsNullOrEmpty(id)) 
        { 
            return false;
        }

        id = Guid.NewGuid().ToString();
        return true;
    }

    public NeighborDayData FindNeighbor(string neighborID)
    {
        return neighbors.Find(neighbor => neighbor.NeighborID == neighborID);
    }

    public bool SyncWith(IReadOnlyList<NeighborDefinition> globalNeighbors,
                         IReadOnlyList<TimeSlotDefinition> globalTimeSlots)
    {
        bool changed = false;

        // Only actually deleted neighbors are removed. Inactive ones retain their data.
        for (int i = neighbors.Count - 1; i >= 0; i--)
        {
            bool exists = false;
            foreach (NeighborDefinition neighbor in globalNeighbors)
            {
                if (neighbor != null && neighbor.ID == neighbors[i].NeighborID)
                {
                    exists = true;
                    break;
                }
            }
            if (exists)
            { 
                continue;
            }

            neighbors.RemoveAt(i);
            changed = true;
        }

        foreach (NeighborDefinition neighbor in globalNeighbors)
        {
            if (neighbor == null) 
            { 
                continue;
            }

            NeighborDayData data = FindNeighbor(neighbor.ID);
            if (data == null)
            {
                data = new NeighborDayData(neighbor.ID);
                neighbors.Add(data);
                changed = true;
            }

            changed |= data.SyncTimeSlots(globalTimeSlots);
        }

        return changed;
    }
}

// ==================================================
// NEIGHBOR DATA INSIDE DAY
// ==================================================

[Serializable]
public class NeighborDayData
{
    [SerializeField] private string neighborID;
    [SerializeField] private List<SlotState> slots = new();

    public string NeighborID => neighborID;
    public IReadOnlyList<SlotState> Slots => slots;

    public NeighborDayData(string id) => neighborID = id;

    public SlotState FindSlot(string timeSlotID)
    {
        return slots.Find(slot => slot.TimeSlotID == timeSlotID);
    }

    public bool SyncTimeSlots(IReadOnlyList<TimeSlotDefinition> globalTimeSlots)
    {
        bool changed = false;
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            bool exists = false;
            foreach (TimeSlotDefinition timeSlot in globalTimeSlots)
            {
                if (timeSlot != null && timeSlot.ID == slots[i].TimeSlotID)
                {
                    exists = true;
                    break;
                }
            }

            if (exists) 
            { 
                continue;
            }
            slots.RemoveAt(i);
            changed = true;
        }

        foreach (TimeSlotDefinition timeSlot in globalTimeSlots)
        {
            if (timeSlot == null || FindSlot(timeSlot.ID) != null) 
            { 
                continue;
            }
            slots.Add(new SlotState(timeSlot.ID));
            changed = true;
        }
        return changed;
    }
}

// ==================================================
// ONE TIMESLOT CELL
// ==================================================

[Serializable]
public class SlotState
{
    [SerializeField] private string timeSlotID;
    [SerializeField] private string stateID;

    // NEVER rename/remove this field until all old scenes/prefabs were migrated and saved.
    // Unity deserializes the old bool here; stateID is empty until first migration.
    [SerializeField, HideInInspector] private bool occupied;

    public string TimeSlotID => timeSlotID;
    public string StateID => stateID;

    // Old API, for scripts that still use SlotState.Occupied.
    public bool Occupied
    {
        get => stateID == DaySchedule.GardenStateID || (string.IsNullOrEmpty(stateID) && occupied);
        set => SetStatus(value ? DaySchedule.GardenStateID : DaySchedule.EmptyStateID);
    }

    public SlotState(string timeSlotID)
    {
        this.timeSlotID = timeSlotID;
        stateID = DaySchedule.EmptyStateID;
        occupied = false;
    }

    public void SetStatus(string newStateID)
    {
        stateID = newStateID;
        // Mirror Garden for legacy consumers but only the state ID is authoritative.
        occupied = newStateID == DaySchedule.GardenStateID;
    }

    public bool MigrateLegacyState()
    {
        if (!string.IsNullOrEmpty(stateID)) 
        { 
            return false;
        }

        SetStatus(occupied ? DaySchedule.GardenStateID : DaySchedule.EmptyStateID);
        return true;
    }
}
