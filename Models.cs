using System;

namespace SmartShiftScheduler
{
    public class ShiftDemand
    {
        public DayOfWeek Day { get; set; }
        public ShiftType Shift { get; set; }
        public Department Dept { get; set; }
        public int RequiredStaffCount { get; set; }
    }

    // Итоговая ячейка расписания
    public class ShiftAssignment
    {
        public DayOfWeek Day { get; set; }
        public ShiftType Shift { get; set; }
        public required Worker AssignedWorker { get; set; }
        public Department Dept { get; set; }

        public override string ToString()
        {
            return $"{Day} | {Shift} | {AssignedWorker.Id} ({Dept})";
        }
    }

    // Контейнер для всей системы
    public class SchedulerContext
    {

        public List<Worker> Workers { get; set; } = new();

        public List<ShiftDemand> Demands { get; set; } = new();

        public List<ShiftAssignment> Schedule { get; set; } = new();

        public List<Worker> GetWorkersByDept(Department dept)
        {
            return Workers.Where(w => w.Dept == dept).ToList();
        }
    }
}
