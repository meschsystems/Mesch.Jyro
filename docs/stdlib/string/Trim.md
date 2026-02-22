# Trim

Removes leading and trailing whitespace from a string.

## Signature

```
Trim(string text)
```

## Parameters

- **text** (string): The string to trim.

## Returns

- **string**: The input string with leading and trailing whitespace removed.

## Description

Removes all leading and trailing white-space characters using .NET's `String.Trim()`. Includes spaces, tabs, newlines, and other Unicode whitespace. Interior whitespace is preserved.

## Examples

```jyro
var result = Trim("  hello  ")
# result = "hello"

var tabs = Trim("\thello\t")
# tabs = "hello"

var inner = Trim("  hello world  ")
# inner = "hello world"
```
