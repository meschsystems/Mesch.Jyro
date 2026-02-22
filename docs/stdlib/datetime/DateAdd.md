# DateAdd

Adds a specified amount of time to a date.

## Signature

```
DateAdd(string date, number amount, string unit)
```

## Parameters

- **date** (string): The base date string.
- **amount** (number): The integer amount to add. Negative values subtract.
- **unit** (string): The time unit. Case-insensitive.

## Returns

- **string**: The modified date in ISO 8601 format (`yyyy-MM-ddTHH:mm:ss.fffZ`).

## Description

Parses the date, adds the specified amount, and returns the result. Valid units (singular or plural):

| Unit | Effect |
|------|--------|
| `day` / `days` | Adds calendar days |
| `week` / `weeks` | Adds weeks (7 days per week) |
| `month` / `months` | Adds calendar months |
| `year` / `years` | Adds calendar years |
| `hour` / `hours` | Adds hours |
| `minute` / `minutes` | Adds minutes |
| `second` / `seconds` | Adds seconds |

Throws a runtime error if:
- The date string is invalid.
- The amount is not an integer.
- The unit is not recognised.

## Examples

```jyro
var tomorrow = DateAdd("2025-10-31", 1, "day")
# tomorrow = "2025-11-01T00:00:00.000Z"

var lastWeek = DateAdd("2025-10-31", -1, "week")
# lastWeek = "2025-10-24T00:00:00.000Z"

var nextMonth = DateAdd("2025-10-31", 1, "month")
# nextMonth = "2025-11-30T00:00:00.000Z"

var inTwoHours = DateAdd("2025-10-31T14:00:00.000Z", 2, "hours")
# inTwoHours = "2025-10-31T16:00:00.000Z"
```
