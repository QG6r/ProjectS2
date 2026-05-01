using System;
using System.Collections.Generic;
using SmartShiftScheduler;
public class Worker
{
    public string Id { get; set; }
    public Department Dept { get; set; }
    public List<DayOfWeek> AvailableDays { get; set; } = new();
    public List<ShiftType> PreferredShifts { get; set; } = new();
    public int RequestedOffDays { get; set; }
    public int MaxConsecutiveDays { get; set; }

    public int TotalWorkHours => AssignedDaysCount * 8;

    public int AssignedDaysCount { get; set; } = 0;
    public List<DayOfWeek> AssignedDays { get; set; } = new();

    public void ResetCounters()
    {
        AssignedDaysCount = 0;
        AssignedDays.Clear();
    }
}