# StartsWith

Tests whether a string begins with a specified prefix.

## Signature

```
StartsWith(string text, string prefix)
```

## Parameters

- **text** (string): The string to test.
- **prefix** (string): The prefix to search for.

## Returns

- **boolean**: `true` if `text` starts with `prefix`, `false` otherwise.

## Description

Uses `String.StartsWith`. The comparison is case-sensitive.

## Examples

```jyro
var a = StartsWith("Hello, World", "Hello")
# a = true

var b = StartsWith("Hello", "hello")
# b = false (case-sensitive)

var c = StartsWith("Hello", "")
# c = true (empty prefix matches any string)
```
