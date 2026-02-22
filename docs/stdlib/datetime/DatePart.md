# DatePart

Extracts a specific component from a date.

## Signature

```
DatePart(string dateStr, string part)
```

## Parameters

- **dateStr** (string): The date string to extract from.
- **part** (string): The component to extract. Case-insensitive.

## Returns

- **number**: The extracted component value.

## Description

Parses the date and returns the requested component. Valid part names:

| Part | Returns |
|------|---------|
| `year` | 4-digit year (e.g., 2025) |
| `month` | Month 1-12 |
| `day` | Day of month 1-31 |
| `hour` | Hour 0-23 |
| `minute` | Minute 0-59 |
| `second` | Second 0-59 |
| `dayofweek` | Day of week 0-6 (Sunday=0) |
| `dayofyear` | Day of year 1-366 |

Throws a runtime error if the date string is invalid or the part name is not recognised.

## Examples

```jyro
var year = DatePart("2025-10-31T14:30:00Z", "year")
# year = 2025

var month = DatePart("2025-10-31", "month")
# month = 10

var dow = DatePart("2025-10-31", "dayofweek")
# dow = 5 (Friday)

var doy = DatePart("2025-03-01", "dayofyear")
# doy = 60
```
