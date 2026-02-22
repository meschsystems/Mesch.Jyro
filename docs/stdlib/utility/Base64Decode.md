# Base64Decode

Decodes a Base64-encoded string.

## Signature

```
Base64Decode(string encoded)
```

## Parameters

- **encoded** (string): The Base64-encoded string to decode.

## Returns

- **string**: The decoded UTF-8 string.

## Description

Converts from Base64 back to a UTF-8 string. Throws a runtime error if the input is not valid Base64.

## Examples

```jyro
var decoded = Base64Decode("SGVsbG8sIFdvcmxkIQ==")
# decoded = "Hello, World!"

# Invalid Base64 throws error
var bad = Base64Decode("not-valid-base64!!!")
# Error: Base64Decode() function requires a valid Base64-encoded string
```
