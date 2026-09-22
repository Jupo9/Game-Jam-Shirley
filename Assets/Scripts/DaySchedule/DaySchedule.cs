using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Game/Day Schedule")]
public class DaySchedule : MonoBehaviour
{
    [SerializeField] private List<PersonDefinition> people = new();
    [SerializeField] private List<TimeSlotDefinition> timeSlots = new();
    [SerializeField] private List<DayData> days = new();

    public IReadOnlyList<PersonDefinition> People => people;
    public IReadOnlyList<TimeSlotDefinition> TimeSlots => timeSlots;
    public IReadOnlyList<DayData> Days => days;


    // =========================================================
    // PERSONS
    // =========================================================

    public void AddPerson()
    {
        string defaultName = "Person " + (people.Count + 1);

        people.Add(new PersonDefinition(defaultName));

        EnsureDataIntegrity();
    }

    public void RemovePersonAt(int index)
    {
        if (index < 0 || index >= people.Count)
            return;

        people.RemoveAt(index);

        EnsureDataIntegrity();
    }

    public void RenamePerson(int index, string newName)
    {
        if (index < 0 || index >= people.Count)
            return;

        people[index].Name = newName;
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
            return;

        timeSlots.RemoveAt(timeSlots.Count - 1);

        EnsureDataIntegrity();
    }

    public void RenameTimeSlot(int index, string newName)
    {
        if (index < 0 || index >= timeSlots.Count)
            return;

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
            return;

        days.RemoveAt(index);
    }

    public void RenameDay(int index, string newName)
    {
        if (index < 0 || index >= days.Count)
            return;

        days[index].Name = newName;
    }


    // =========================================================
    // OCCUPIED / FREE
    // =========================================================

    public bool IsOccupied(
        int dayIndex,
        int personIndex,
        int timeSlotIndex)
    {
        if (!AreIndicesValid(dayIndex, personIndex, timeSlotIndex))
            return false;

        string personID = people[personIndex].ID;
        string timeSlotID = timeSlots[timeSlotIndex].ID;

        PersonDayData personData =
            days[dayIndex].FindPerson(personID);

        if (personData == null)
            return false;

        SlotState slotState =
            personData.FindSlot(timeSlotID);

        if (slotState == null)
            return false;

        return slotState.Occupied;
    }

    public void ToggleOccupied(
        int dayIndex,
        int personIndex,
        int timeSlotIndex)
    {
        if (!AreIndicesValid(dayIndex, personIndex, timeSlotIndex))
            return;

        string personID = people[personIndex].ID;
        string timeSlotID = timeSlots[timeSlotIndex].ID;

        PersonDayData personData =
            days[dayIndex].FindPerson(personID);

        if (personData == null)
            return;

        SlotState slotState =
            personData.FindSlot(timeSlotID);

        if (slotState == null)
            return;

        slotState.Occupied = !slotState.Occupied;
    }

    public void SetOccupied(
        int dayIndex,
        int personIndex,
        int timeSlotIndex,
        bool occupied)
    {
        if (!AreIndicesValid(dayIndex, personIndex, timeSlotIndex))
            return;

        string personID = people[personIndex].ID;
        string timeSlotID = timeSlots[timeSlotIndex].ID;

        PersonDayData personData =
            days[dayIndex].FindPerson(personID);

        if (personData == null)
            return;

        SlotState slotState =
            personData.FindSlot(timeSlotID);

        if (slotState == null)
            return;

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
            day.SyncWith(people, timeSlots);
        }
    }

    private void EnsureIDs()
    {
        foreach (PersonDefinition person in people)
        {
            person.EnsureID();
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

    private bool AreIndicesValid(
        int dayIndex,
        int personIndex,
        int timeSlotIndex)
    {
        if (dayIndex < 0 || dayIndex >= days.Count)
            return false;

        if (personIndex < 0 || personIndex >= people.Count)
            return false;

        if (timeSlotIndex < 0 || timeSlotIndex >= timeSlots.Count)
            return false;

        return true;
    }

    private void OnValidate()
    {
        EnsureDataIntegrity();
    }
}


// =============================================================
// PERSON DEFINITION
// =============================================================

[Serializable]
public class PersonDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string personName;

    public string ID => id;

    public string Name
    {
        get => personName;
        set => personName = value;
    }

    public PersonDefinition(string name)
    {
        id = Guid.NewGuid().ToString();
        personName = name;
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

    [SerializeField]
    private List<PersonDayData> persons = new();

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

    public PersonDayData FindPerson(string personID)
    {
        return persons.Find(
            person => person.PersonID == personID
        );
    }

    public void SyncWith(
        IReadOnlyList<PersonDefinition> globalPeople,
        IReadOnlyList<TimeSlotDefinition> globalTimeSlots)
    {
        // Remove persons that no longer exist
        for (int i = persons.Count - 1; i >= 0; i--)
        {
            bool personStillExists = false;

            foreach (PersonDefinition person in globalPeople)
            {
                if (person.ID == persons[i].PersonID)
                {
                    personStillExists = true;
                    break;
                }
            }

            if (!personStillExists)
            {
                persons.RemoveAt(i);
            }
        }

        // Add missing persons
        foreach (PersonDefinition person in globalPeople)
        {
            PersonDayData personData =
                FindPerson(person.ID);

            if (personData == null)
            {
                personData =
                    new PersonDayData(person.ID);

                persons.Add(personData);
            }

            personData.SyncTimeSlots(globalTimeSlots);
        }
    }
}


// =============================================================
// PERSON DATA INSIDE A DAY
// =============================================================

[Serializable]
public class PersonDayData
{
    [SerializeField] private string personID;

    [SerializeField]
    private List<SlotState> slots = new();

    public string PersonID => personID;

    public PersonDayData(string id)
    {
        personID = id;
    }

    public SlotState FindSlot(string timeSlotID)
    {
        return slots.Find(
            slot => slot.TimeSlotID == timeSlotID
        );
    }

    public void SyncTimeSlots(
        IReadOnlyList<TimeSlotDefinition> globalTimeSlots)
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
                slots.Add(
                    new SlotState(timeSlot.ID)
                );
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
