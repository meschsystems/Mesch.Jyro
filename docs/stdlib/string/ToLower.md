# ToLower

Converts a string to lowercase.

## Signature

```
ToLower(string text)
```

## Parameters

- **text** (string): The string to convert.

## Returns

- **string**: The input string with all characters converted to lowercase.

## Description

Uses culture-invariant conversion (`ToLowerInvariant`). Produces consistent results regardless of the host system's locale.

## Examples

```jyro
var result = ToLower("HELLO")
# result = "hello"

var mixed = ToLower("Hello World")
# mixed = "hello world"

# Useful for case-insensitive comparison
if ToLower(Data.status) == "active" then
    Data.isActive = true
end
```
