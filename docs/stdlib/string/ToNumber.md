# ToNumber

Converts a string to a number.

## Signature

```
ToNumber(string text)
```

## Parameters

- **text** (string): The string to parse as a number.

## Returns

- **number | null**: The parsed numeric value, or `null` if parsing fails.

## Description

Parses the string using `Double.TryParse` with `NumberStyles.Any` and `CultureInfo.InvariantCulture`. Supports integers, decimals, scientific notation, and leading/trailing whitespace.

Returns `null` on failure rather than throwing an error. Use `is null` or `is not null` to check whether parsing succeeded.

## Examples

```jyro
var a = ToNumber("42")
# a = 42

var b = ToNumber("3.14")
# b = 3.14

var c = ToNumber("1.5e3")
# c = 1500

# Invalid input returns null
var d = ToNumber("abc")
# d = null

var e = ToNumber("")
# e = null

# Check for parse failure
var num = ToNumber(input)
if num is null then
    Data.error = "Invalid number"
end
```
