using System;
using System.Collections.Generic;
using System.Linq; // Это нужно для .Select() и .ToArray()

namespace SmartShiftScheduler
{
    public class DemandPredictor
    {
        // Математическое ядро: Интерполяционный многочлен Лагранжа
        public static double InterpolateLagrange(double targetHour, double[] x, double[] y)
        {
            double result = 0;
            int n = x.Length;

            for (int i = 0; i < n; i++)
            {
                double term = y[i];
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                        term *= (targetHour - x[j]) / (x[i] - x[j]);
                }
                result += term;
            }
            return result;
        }

        public static int GetRequiredStaff(DayOfWeek day, ShiftType shift, Department dept)
        {
            // НОВОЕ ПРАВИЛО: Ночная смена всегда требует только 1 человека
            if (shift == ShiftType.Night)
            {
                return 1;
            }

            // Для остальных смен (Утро/День) оставляем логику 2-3 человек
            double occupancyRate = GetOccupancyForecast(day);
            double[] hours = { 8, 14, 20 };

            double[] baseStaffNodes = dept switch
            {
                Department.Reception => new double[] { 2.2, 3.2, 2.2 },
                Department.Housekeeping => new double[] { 3.2, 2.2, 2.2 },
                Department.FoodAndBeverage => new double[] { 2.2, 2.2, 3.2 },
                _ => new double[] { 2, 2, 2 }
            };

            double[] dynamicNodes = baseStaffNodes.Select(n => n * occupancyRate).ToArray();

            double targetHour = (shift == ShiftType.Morning) ? 9.5 : 15.0;

            double prediction = InterpolateLagrange(targetHour, hours, dynamicNodes);

            // Для утра и дня держим планку 2-3
            return (int)Math.Clamp(Math.Round(prediction), 2, 3);
        }
        private static double GetOccupancyForecast(DayOfWeek day)
        {
            if (day == DayOfWeek.Friday || day == DayOfWeek.Saturday) return 0.40;
            if (day == DayOfWeek.Sunday) return 0.75;
            return 0.60;
        }
    }
}
