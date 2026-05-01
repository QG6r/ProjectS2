# 🏨 SmartShiftScheduler

> An intelligent shift scheduling system for hotel staff, built with C# and powered by Lagrange interpolation for demand forecasting.

---

## 📋 Overview

SmartShiftScheduler automatically generates optimized weekly shift schedules for hotel departments. It reads employee availability from a CSV file, predicts staffing demand using a mathematical forecasting model, and produces a fair, rule-compliant schedule — all from the command line.

---

## ✨ Features

- **AI-driven demand forecasting** using Lagrange polynomial interpolation
- **Constraint-based scheduling** with backtracking (greedy mode)
- **Three departments**: Reception, Housekeeping, Food & Beverage
- **Three shift types**: Morning, Day, Night
- **Rule enforcement**:
  - No back-to-back night → morning/day shifts
  - Maximum consecutive working days
  - Weekly hour limits
  - Availability matching
- **KPI analytics**: Average workload, Fairness Index, Rule Compliance

---

## 🗂️ Project Structure

```
SmartShiftScheduler/
├── Program.cs              # Entry point & orchestration
├── Models.cs               # Core data models (ShiftDemand, ShiftAssignment, SchedulerContext)
├── Enums.cs                # Department and ShiftType enums
├── Worker.cs               # Worker entity with scheduling state
├── ShiftScheduler.cs       # Greedy constraint-based scheduling algorithm
├── DemandPredictor.cs      # Lagrange interpolation demand forecasting
├── CsvParser.cs            # CSV parsing & worker data loading
├── ConsoleVisualizer.cs    # Console table rendering & UI
└── responses.csv           # Employee availability input file
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 7.0+](https://dotnet.microsoft.com/download)

### Installation

```bash
git clone https://github.com/your-username/SmartShiftScheduler.git
cd SmartShiftScheduler
```

### Running

```bash
dotnet run
```

Make sure `responses.csv` is in the working directory before running.

---

## 📄 Input Format (`responses.csv`)

The scheduler reads employee data from a CSV file with the following columns:

| Column | Description | Example |
|--------|-------------|---------|
| 0 | Timestamp | `2024-01-01` |
| 1 | Worker ID | `1267` |
| 2 | Department | `Reception`, `Housekeeping`, `FoodAndBeverage` |
| 3 | Available Days | `Monday, Wednesday, Friday` |
| 4 | Preferred Shifts | `Morning, Day` or `anytime` |
| 5 | Requested Off Days | `2` |
| 6 | *(reserved)* | — |
| 7 | Max Consecutive Days | `3 days` |

---

## 📊 How It Works

### 1. Demand Forecasting (`DemandPredictor.cs`)

Required staff per shift is calculated using **Lagrange polynomial interpolation** over three control points (08:00, 14:00, 20:00), scaled by an occupancy forecast:

| Day | Occupancy Rate |
|-----|---------------|
| Friday / Saturday | 75% |
| Sunday | 90% |
| Weekdays | 85% |

Night shifts always require **1 staff member** per department.

### 2. Scheduling Algorithm (`ShiftScheduler.cs`)

A **greedy constraint-based** algorithm assigns workers to shifts in priority order:

1. Workers with fewer than 5 assigned days
2. Workers with the lowest current workload
3. Workers whose preferred shifts match
4. Alphabetical ID as tiebreaker

Constraints checked per assignment:
- Worker is available on that day
- Worker hasn't already been assigned that day
- Worker hasn't exceeded the 40-hour weekly limit
- No consecutive days beyond `MaxConsecutiveDays`
- Rest rule: no morning/day shift the day after a night shift

### 3. KPI Analysis (`Program.cs`)

After generation, the system outputs:

```
[KPI] Average Workload: X.X days/worker
[KPI] Fairness Index: XX% (Higher is better)
[KPI] Rule Compliance: 100%
```

**Fairness Index** = `1 - (maxDays - minDays) / 7`  
Target: **≥ 85%**

---

## 🖥️ Sample Output

```
==================================================
 DEPARTMENT: FOODANDBEVERAGE
==================================================
┌────────────┬─────────────┬──────────────────────────────────────────┐
│    Day     │    Shift    │                Employees                 │
├────────────┼─────────────┼──────────────────────────────────────────┤
│ Sunday     │ Morning     │ 1267, 3052                               │
│ Sunday     │ Day         │ 179, 7230                                │
│ Sunday     │ Night       │ 4681                                     │
│ Monday     │ Morning     │ 3021, 6970                               │
└────────────┴─────────────┴──────────────────────────────────────────┘
```

---

## ⚙️ Configuration

Key parameters you can tune directly in the source:

| File | Parameter | Default | Effect |
|------|-----------|---------|--------|
| `DemandPredictor.cs` | `occupancyRate` | 0.75–0.90 | Controls staff demand |
| `DemandPredictor.cs` | `Clamp(min, max)` | `(1, 3)` | Min/max staff per shift |
| `ShiftScheduler.cs` | Hour limit | `40` | Max hours per worker/week |
| `ShiftScheduler.cs` | Day threshold | `5` | Target working days per worker |

---

## 📐 Mathematical Background

The Lagrange interpolating polynomial for `n` nodes is:

$$L(x) = \sum_{i=0}^{n} y_i \prod_{j \neq i} \frac{x - x_j}{x_i - x_j}$$

Applied here with three control points representing staff needs at morning, midday, and evening peaks — scaled by the daily occupancy forecast to produce dynamic, realistic demand values.

---

## 📝 License

This project is licensed under the MIT License.
