# EndsWith

Tests whether a string ends with a specified suffix.

## Signature

```
EndsWith(string text, string suffix)
```

## Parameters

- **text** (string): The string to test.
- **suffix** (string): The suffix to search for.

## Returns

- **boolean**: `true` if `text` ends with `suffix`, `false` otherwise.

## Description

Uses `String.EndsWith`. The comparison is case-sensitive.

## Examples

```jyro
var a = EndsWith("Hello, World", "World")
# a = true

var b = EndsWith("document.pdf", ".pdf")
# b = true

var c = EndsWith("Hello", "hello")
# c = false (case-sensitive)
```
