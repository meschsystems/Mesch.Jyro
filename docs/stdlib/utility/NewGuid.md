# NewGuid

Generates a new globally unique identifier.

## Signature

```
NewGuid()
```

## Parameters

None.

## Returns

- **string**: A lowercase UUID in standard format (e.g., `"a1b2c3d4-e5f6-7890-abcd-ef1234567890"`).

## Description

Generates a version 4 UUID using `Guid.NewGuid()`. Output uses the "D" format (lowercase, hyphen-separated). Each call produces a unique value.

## Examples

```jyro
var id = NewGuid()
# id = "f47ac10b-58cc-4372-a567-0e02b2c3d479" (random)

Data.transactionId = NewGuid()
```
