using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SmartShiftScheduler;

public class DataManager
{
    public static List<Worker> LoadWorkersFromCsv(string filePath)
    {
        var workers = new List<Worker>();

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Ошибка: Файл не найден по пути: " + filePath);
            return workers;
        }

        var lines = File.ReadAllLines(filePath).Skip(1);

        foreach (var line in lines)
        {
            var columns = Regex.Matches(line, @"(((?<=\x2c)|(?<=^))(?=\x2c|$))|("".*?""|[^,\r\n]+)")
                               .Cast<Match>()
                               .Select(m => m.Value.Trim('"'))
                               .ToArray();

            if (columns.Length < 8) continue;

            var worker = new Worker
            {
                Id = columns[1],
                Dept = ParseDepartment(columns[2]),
                AvailableDays = ParseDays(columns[3]),
                PreferredShifts = ParseShifts(columns[4]), 
                RequestedOffDays = int.Parse(columns[5]),
                MaxConsecutiveDays = int.Parse(Regex.Match(columns[7], @"\d+").Value),
            };

            workers.Add(worker);
        }

        return workers;
    }

    private static Department ParseDepartment(string value)
    {
        return value.ToLower() switch
        {
            "reception" => Department.Reception,
            "housekeeping" => Department.Housekeeping,
            _ => Department.FoodAndBeverage
        };
    }

    private static List<DayOfWeek> ParseDays(string rawData)
    {
        var days = new List<DayOfWeek>();
        string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        foreach (var day in dayNames)
        {
            if (rawData.Contains(day))
                days.Add(Enum.Parse<DayOfWeek>(day));
        }
        return days;
    }

    private static List<ShiftType> ParseShifts(string rawData)
    {
        var shifts = new List<ShiftType>();
        if (rawData.Contains("Morning")) shifts.Add(ShiftType.Morning);
        if (rawData.Contains("Day")) shifts.Add(ShiftType.Day);
        if (rawData.Contains("Night")) shifts.Add(ShiftType.Night);
        if (rawData.Contains("anytime"))
        {
            shifts.Add(ShiftType.Morning);
            shifts.Add(ShiftType.Day);
            shifts.Add(ShiftType.Night);
        }
        return shifts;
    }
}
