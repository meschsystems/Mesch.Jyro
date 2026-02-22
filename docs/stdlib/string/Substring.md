# Substring

Extracts a portion of a string by position and optional length.

## Signature

```
Substring(string text, number start, number length?)
```

## Parameters

- **text** (string): The source string.
- **start** (number): The zero-based starting index (truncated to integer).
- **length** (number, optional): The number of characters to extract. If omitted, extracts to the end of the string.

## Returns

- **string**: The extracted substring. Returns `""` if no characters can be extracted.

## Description

Extracts a substring with safe bounds handling:

- Negative `start` is clamped to `0`.
- `start` beyond the string length returns `""`.
- `length` is clamped to the available characters remaining.
- `length <= 0` returns `""`.
- If `length` is omitted, extracts from `start` to the end.

## Examples

```jyro
var a = Substring("Hello World", 0, 5)
# a = "Hello"

var b = Substring("Hello World", 6)
# b = "World"

# Safe bounds - start beyond length
var c = Substring("Hello", 10)
# c = ""

# Safe bounds - length exceeds string
var d = Substring("Hi", 0, 100)
# d = "Hi"

# Negative start clamped to 0
var e = Substring("Hello", -5, 3)
# e = "Hel"
```
