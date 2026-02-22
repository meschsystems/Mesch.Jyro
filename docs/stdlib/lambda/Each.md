# Each

Executes a lambda function for each element of an array.

## Signature

```
Each(array arr, lambda fn)
```

## Parameters

- **arr** (array): The array to iterate.
- **fn** (lambda): A lambda `(item) => void` executed for each element.

## Returns

- **null**: Always returns null. Use for side-effect operations only.

## Description

Calls the lambda once for each element in the array. The lambda's return value is discarded. Returns null. Use `Map` if you need to collect results.

## Examples

```jyro
var items = [1, 2, 3]
Each(items, (x) => SaveNumber(x))
# Calls a hypothetical `SaveNumber()` host function three times with arguments: 1, 2, 3 respectively
```
