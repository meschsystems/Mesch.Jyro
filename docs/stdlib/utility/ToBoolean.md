# ToBoolean

Converts any value to a boolean using Jyro truthiness rules.

## Signature

```
ToBoolean(any value)
```

## Parameters

- **value** (any): The value to convert.

## Returns

- **boolean**: The truthiness of the value.

## Description

Applies Jyro's truthiness rules:

| Value | Result |
|-------|--------|
| `false` | `false` |
| `null` | `false` |
| `0` | `false` |
| `""` (empty string) | `false` |
| Everything else | `true` |

Note: Empty arrays (`[]`) and empty objects (`{}`) are **truthy**.

## Examples

```jyro
var a = ToBoolean(1)
# a = true

var b = ToBoolean(0)
# b = false

var c = ToBoolean("")
# c = false

var d = ToBoolean([])
# d = true (empty arrays are truthy)
```
