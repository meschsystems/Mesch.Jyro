# ToUpper

Converts a string to uppercase.

## Signature

```
ToUpper(string text)
```

## Parameters

- **text** (string): The string to convert.

## Returns

- **string**: The input string with all characters converted to uppercase.

## Description

Uses culture-invariant conversion (`ToUpperInvariant`). Produces consistent results regardless of the host system's locale.

## Examples

```jyro
var result = ToUpper("hello")
# result = "HELLO"

var mixed = ToUpper("Hello World")
# mixed = "HELLO WORLD"

var already = ToUpper("ABC")
# already = "ABC"
```
