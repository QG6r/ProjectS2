using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartShiftScheduler
{
    public class ShiftScheduler
    {
        public bool Solve(SchedulerContext context, int demandIndex)
        {
            if (demandIndex >= context.Demands.Count) return true;

            var currentDemand = context.Demands[demandIndex];

            // ЛОГ ДЛЯ ТЕБЯ: Выводим прогресс раз в 10 смен
            if (demandIndex % 10 == 0)
            {
                Console.WriteLine($"[DEBUG] Processing: {currentDemand.Day} {currentDemand.Shift} (Step {demandIndex}/{context.Demands.Count})");
            }

            int currentlyAssigned = context.Schedule.Count(s =>
                s.Day == currentDemand.Day &&
                s.Shift == currentDemand.Shift &&
                s.Dept == currentDemand.Dept);

            if (currentlyAssigned >= currentDemand.RequiredStaffCount)
                return Solve(context, demandIndex + 1);

            // Оптимизация: Сначала те, кто хочет работать, и те, у кого меньше смен
            var bestWorkers = context.Workers
    .Where(w => w.Dept == currentDemand.Dept)
    // Приоритет 1: Те, кто еще не отработал 5 дней
    .OrderBy(w => w.AssignedDaysCount >= 5 ? 1 : 0)
    // Приоритет 2: У кого в принципе меньше дней (балансировка)
    .ThenBy(w => w.AssignedDaysCount)
    .ThenBy(w => Guid.NewGuid())
    .ToList();

            foreach (var worker in bestWorkers)
            {
                if (CanAssign(worker, currentDemand, context.Schedule))
                {
                    Assign(worker, currentDemand, context.Schedule);
                    if (Solve(context, demandIndex)) return true;
                    Unassign(worker, currentDemand, context.Schedule);
                }
            }
            return false;
        }

        private bool CanAssign(Worker w, ShiftDemand d, List<ShiftAssignment> schedule)
        {
            if (!w.AvailableDays.Contains(d.Day)) return false;
            if (w.AssignedDays.Contains(d.Day)) return false;
            if (w.TotalWorkHours + 8 > 60) return false;

            // Проверка на максимальное количество дней подряд
            if (IsStreakBroken(w, d.Day, schedule)) return false;

            // Правило отдыха после ночной смены
            if (d.Shift == ShiftType.Morning || d.Shift == ShiftType.Day)
            {
                var yesterday = GetPreviousDay(d.Day);
                if (schedule.Any(s => s.AssignedWorker == w && s.Day == yesterday && s.Shift == ShiftType.Night))
                    return false;
            }

            return true;
        }

        private bool IsStreakBroken(Worker w, DayOfWeek day, List<ShiftAssignment> schedule)
        {
            int consecutive = 0;
            // Проверяем предыдущие дни
            for (int i = 1; i <= w.MaxConsecutiveDays; i++)
            {
                // Вычисляем предыдущий день с учетом цикла недели
                var checkDay = (DayOfWeek)(((int)day - i + 7) % 7);
                if (w.AssignedDays.Contains(checkDay)) consecutive++;
                else break;
            }
            return consecutive >= w.MaxConsecutiveDays;
        }

        // НЕДОСТАЮЩИЙ МЕТОД
        private DayOfWeek GetPreviousDay(DayOfWeek current)
        {
            return (DayOfWeek)(((int)current + 6) % 7);
        }

        private void Assign(Worker w, ShiftDemand d, List<ShiftAssignment> schedule)
        {
            w.AssignedDaysCount++;
            w.AssignedDays.Add(d.Day);
            schedule.Add(new ShiftAssignment { Day = d.Day, Shift = d.Shift, AssignedWorker = w, Dept = w.Dept });
        }

        private void Unassign(Worker w, ShiftDemand d, List<ShiftAssignment> schedule)
        {
            w.AssignedDaysCount--;
            w.AssignedDays.Remove(d.Day);
            var last = schedule.LastOrDefault(s => s.AssignedWorker == w && s.Day == d.Day);
            if (last != null) schedule.Remove(last);
        }
    }
}
