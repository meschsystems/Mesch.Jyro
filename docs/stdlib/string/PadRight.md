# PadRight

Pads a string on the right to a specified total width.

## Signature

```
PadRight(string text, number length, string padChar?)
```

## Parameters

- **text** (string): The string to pad.
- **length** (number): The desired total width.
- **padChar** (string, optional): The character to pad with. Only the first character is used. Defaults to space.

## Returns

- **string**: The padded string. If `text` is already at or beyond the target width, it is returned unchanged.

## Description

Uses `String.PadRight`. If `padChar` is empty or null, defaults to space. Only the first character of `padChar` is used as the padding character.

## Examples

```jyro
var a = PadRight("Hi", 10)
# a = "Hi        " (padded with spaces)

var b = PadRight("OK", 5, ".")
# b = "OK..."

# Already at target width
var c = PadRight("hello", 3, "0")
# c = "hello" (unchanged)
```
