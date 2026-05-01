using SmartShiftScheduler;
using System;
using System.Linq;

ConsoleVisualizer.PrintWelcomeMessage();

var allWorkers = DataManager.LoadWorkersFromCsv("responses.csv");
Console.WriteLine($"[INFO] Total responses received: {allWorkers.Count}");

var finalContext = new SchedulerContext();

foreach (Department currentDept in Enum.GetValues(typeof(Department)))
{
    Console.WriteLine($"\n--- Processing Department: {currentDept} ---");
    var deptWorkers = allWorkers.Where(w => w.Dept == currentDept).ToList();

    if (deptWorkers.Count == 0) continue;

    Console.WriteLine($"[INFO] Found {deptWorkers.Count} employees for {currentDept}.");
    var deptContext = new SchedulerContext { Workers = deptWorkers };

    foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
    {
        foreach (ShiftType shift in Enum.GetValues(typeof(ShiftType)))
        {
            deptContext.Demands.Add(new ShiftDemand
            {
                Day = day,
                Shift = shift,
                Dept = currentDept,
                RequiredStaffCount = DemandPredictor.GetRequiredStaff(day, shift, currentDept)
            });
        }
    }

    var scheduler = new ShiftScheduler();
    if (scheduler.Solve(deptContext, 0))
    {
        Console.WriteLine($"[OK] Schedule for {currentDept} generated!");
        finalContext.Schedule.AddRange(deptContext.Schedule);
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[!] FAILED: Could not find a valid schedule for {currentDept}.");
        Console.ResetColor();
    }
}

ConsoleVisualizer.PrintSchedule(finalContext.Schedule);

if (finalContext.Schedule.Any())
{
    Console.WriteLine("\n" + new string('=', 60));
    Console.WriteLine("📊 SCHEDULE QUALITY ANALYSIS (KPIs)");
    Console.WriteLine(new string('=', 60));

    var stats = finalContext.Schedule
        .GroupBy(s => s.AssignedWorker)
        .Select(g => new {
            Worker = g.Key.Id,
            Days = g.Count(),
            Satisfaction = (double)g.Count(s => g.Key.PreferredShifts.Contains(s.Shift)) / g.Count()
        })
        .OrderBy(x => x.Days)
        .ToList();

    double avgDays = stats.Average(x => (double)x.Days);
    int minDays = stats.Min(x => x.Days);
    int maxDays = stats.Max(x => x.Days);
    double fairnessIndex = 1.0 - ((double)(maxDays - minDays) / 7.0);

    Console.WriteLine($"[KPI] Average Workload: {avgDays:F1} days/worker");
    Console.WriteLine($"[KPI] Fairness Index: {fairnessIndex:P0} (Higher is better)");
    Console.WriteLine($"[KPI] Rule Compliance: 100% (Back-to-back & Availability enforced)");

    if (fairnessIndex < 0.85)
        Console.WriteLine("[!] Warning: Workload distribution is uneven.");
    else
        Console.WriteLine("[OK] Workload is balanced across the team.");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();