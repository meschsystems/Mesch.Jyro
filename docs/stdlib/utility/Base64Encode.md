# Base64Encode

Encodes a string to Base64.

## Signature

```
Base64Encode(string text)
```

## Parameters

- **text** (string): The string to encode.

## Returns

- **string**: The Base64-encoded string.

## Description

Encodes the input string as UTF-8 bytes, then converts to Base64 using `Convert.ToBase64String`.

## Examples

```jyro
var encoded = Base64Encode("Hello, World!")
# encoded = "SGVsbG8sIFdvcmxkIQ=="

var roundtrip = Base64Decode(Base64Encode("test"))
# roundtrip = "test"
```
