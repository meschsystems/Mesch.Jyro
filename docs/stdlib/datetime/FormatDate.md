# FormatDate

Formats a date string according to a .NET format pattern.

## Signature

```
FormatDate(string dateStr, string format)
```

## Parameters

- **dateStr** (string): The date string to parse and format.
- **format** (string): A .NET date/time format pattern.

## Returns

- **string**: The formatted date string.

## Description

Parses the input date using UTC styles (`AssumeUniversal | AdjustToUniversal`), then formats it with the specified pattern. Common format specifiers:

| Specifier | Meaning |
|-----------|---------|
| `yyyy` | 4-digit year |
| `MM` | 2-digit month |
| `dd` | 2-digit day |
| `HH` | 24-hour hour |
| `mm` | Minute |
| `ss` | Second |
| `fff` | Milliseconds |
| `ddd` | Abbreviated day name |
| `MMMM` | Full month name |

Throws a runtime error if the date string or the format pattern is invalid.

## Examples

```jyro
var a = FormatDate("2025-10-31T14:30:00.000Z", "yyyy-MM-dd")
# a = "2025-10-31"

var b = FormatDate("2025-10-31T14:30:00.000Z", "dd/MM/yyyy HH:mm")
# b = "31/10/2025 14:30"

var c = FormatDate("2025-10-31", "MMMM dd, yyyy")
# c = "October 31, 2025"
```
