# PadLeft

Pads a string on the left to a specified total width.

## Signature

```
PadLeft(string text, number length, string padChar?)
```

## Parameters

- **text** (string): The string to pad.
- **length** (number): The desired total width.
- **padChar** (string, optional): The character to pad with. Only the first character is used. Defaults to space.

## Returns

- **string**: The padded string. If `text` is already at or beyond the target width, it is returned unchanged.

## Description

Uses `String.PadLeft`. If `padChar` is empty or null, defaults to space. Only the first character of `padChar` is used as the padding character.

## Examples

```jyro
var a = PadLeft("42", 5, "0")
# a = "00042"

var b = PadLeft("hi", 10)
# b = "        hi" (padded with spaces)

# Already at target width
var c = PadLeft("hello", 3, "0")
# c = "hello" (unchanged)

var d = PadLeft("x", 4, ".")
# d = "...x"
```
