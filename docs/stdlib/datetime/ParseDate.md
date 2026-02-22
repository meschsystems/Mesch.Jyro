# ParseDate

Parses a date string and normalises it to ISO 8601 format.

## Signature

```
ParseDate(string dateStr)
```

## Parameters

- **dateStr** (string): The date string to parse.

## Returns

- **string**: The parsed date in ISO 8601 format (`yyyy-MM-ddTHH:mm:ss.fffZ`).

## Description

Attempts to parse the input using these exact formats first:

1. `yyyy-MM-dd`
2. `yyyy-MM-ddTHH:mm:ss`
3. `yyyy-MM-ddTHH:mm:ssZ`
4. `yyyy-MM-ddTHH:mm:ss.fffZ`
5. `MM/dd/yyyy`
6. `dd/MM/yyyy`
7. `yyyy/MM/dd`

If none match, falls back to general .NET date parsing. All parsing assumes UTC and adjusts to UTC (`AssumeUniversal | AdjustToUniversal`).

Throws a runtime error if the string cannot be parsed by any method.

## Examples

```jyro
var a = ParseDate("2025-10-31")
# a = "2025-10-31T00:00:00.000Z"

var b = ParseDate("10/31/2025")
# b = "2025-10-31T00:00:00.000Z"

var c = ParseDate("2025-10-31T14:30:00.000Z")
# c = "2025-10-31T14:30:00.000Z"

# Invalid date throws error
var d = ParseDate("not-a-date")
# Error: Unable to parse date: 'not-a-date'
```
