# DateDiff

Calculates the difference between two dates in a specified unit.

## Signature

```
DateDiff(string endDate, string startDate, string unit)
```

## Parameters

- **endDate** (string): The end date string.
- **startDate** (string): The start date string.
- **unit** (string): The time unit for the result. Case-insensitive.

## Returns

- **number**: The difference as a floating-point value. Positive when `endDate` is later than `startDate`.

## Description

Computes `endDate - startDate` and returns the difference in the requested unit. Valid units (singular or plural):

| Unit | Calculation |
|------|-------------|
| `day` / `days` | `TimeSpan.TotalDays` |
| `week` / `weeks` | `TotalDays / 7.0` |
| `hour` / `hours` | `TimeSpan.TotalHours` |
| `minute` / `minutes` | `TimeSpan.TotalMinutes` |
| `second` / `seconds` | `TimeSpan.TotalSeconds` |
| `year` / `years` | `TotalDays / 365.25` (approximate) |
| `month` / `months` | `TotalDays / 30.44` (approximate) |

Year and month calculations use approximate average values. For exact calendar differences, use `DatePart` to compare individual components.

Throws a runtime error if either date is invalid or the unit is not recognised.

## Examples

```jyro
var days = DateDiff("2025-12-31", "2025-01-01", "days")
# days = 364

var weeks = DateDiff("2025-12-31", "2025-01-01", "weeks")
# weeks = 52.0

var hours = DateDiff("2025-10-31T18:00:00Z", "2025-10-31T14:00:00Z", "hours")
# hours = 4.0

# Negative when endDate is earlier
var neg = DateDiff("2025-01-01", "2025-12-31", "days")
# neg = -364
```
