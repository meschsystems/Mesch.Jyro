# Split

Splits a string into an array of substrings using a delimiter.

## Signature

```
Split(string text, string delimiter)
```

## Parameters

- **text** (string): The string to split.
- **delimiter** (string): The separator to split on.

## Returns

- **array**: An array of string segments.

## Description

Splits the string using `String.Split` with `StringSplitOptions.None`. Empty strings are preserved when consecutive delimiters appear or when the delimiter is at the start or end of the string.

## Examples

```jyro
var result = Split("a,b,c", ",")
# result = ["a", "b", "c"]

# Consecutive delimiters produce empty strings
var gaps = Split("a,,b", ",")
# gaps = ["a", "", "b"]

# Delimiter at edges
var edges = Split(",a,b,", ",")
# edges = ["", "a", "b", ""]

# Multi-character delimiter
var parts = Split("one::two::three", "::")
# parts = ["one", "two", "three"]
```
