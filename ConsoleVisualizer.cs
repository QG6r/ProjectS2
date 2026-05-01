namespace SmartShiftScheduler;

public static class ConsoleVisualizer
{
    public static void PrintSchedule(List<ShiftAssignment> schedule)
    {
        if (schedule == null || schedule.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[!] The schedule is empty. Please run the generation algorithm first.");
            Console.ResetColor();
            return;
        }

        var departments = schedule.Select(s => s.Dept).Distinct();

        foreach (var dept in departments)
        {
            PrintDepartmentHeader(dept.ToString());

            // Расширяем таблицу: колонка для сотрудников теперь 40 символов
            Console.WriteLine("┌────────────┬─────────────┬──────────────────────────────────────────┐");
            Console.WriteLine("│    Day     │    Shift    │                Employees                 │");
            Console.WriteLine("├────────────┼─────────────┼──────────────────────────────────────────┤");

            // Группируем сотрудников по дням и сменам
            var groupedShifts = schedule
                .Where(s => s.Dept == dept)
                .OrderBy(s => s.Day)
                .ThenBy(s => s.Shift)
                .GroupBy(s => new { s.Day, s.Shift });

            foreach (var group in groupedShifts)
            {
                string day = group.Key.Day.ToString().PadRight(10);
                string type = group.Key.Shift.ToString().PadRight(11);

                // Собираем всех "Worker X" этой смены в одну строку
                string workersList = string.Join(", ", group.Select(s => s.AssignedWorker.Id));
                string workersColumn = workersList.PadRight(40);

                Console.WriteLine($"│ {day} │ {type} │ {workersColumn} │");
            }

            Console.WriteLine("└────────────┴─────────────┴──────────────────────────────────────────┘\n");
        }
    }

    private static void PrintDepartmentHeader(string deptName)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('=', 50));
        Console.WriteLine($" DEPARTMENT: {deptName.ToUpper()}");
        Console.WriteLine(new string('=', 50));
        Console.ResetColor();
    }

    public static void PrintWelcomeMessage()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"
   _____                      _     _____ _     _  __ _   
  / ____|                    | |   / ____| |   (_)/ _| |  
 | (___  _ __ ___   __ _ _ __| |_ | (___ | |__  _| |_| |_ 
  \___ \| '_ ` _ \ / _` | '__| __| \___ \| '_ \| |  _| __|
  ____) | | | | | | (_| | |  | |_  ____) | | | | | | | |_ 
 |_____/|_| |_| |_|\__,_|_|   \__| |_____/|_| |_|_|_|  \__|
        ");
        Console.WriteLine("                SMART SHIFT SCHEDULER v1.0");
        Console.WriteLine(new string('-', 60));
        Console.ResetColor();
    }
}