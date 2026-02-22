# FlattenOnce

Flattens an array of arrays by one level into a single array.

## Signature

```
FlattenOnce(array arrays)
```

## Parameters

- **arrays** (array): An array of arrays to flatten. Non-array, non-null items are added as single elements. Null items are skipped.

## Returns

- **array**: A new array containing all elements from all input arrays in order.

## Description

Takes an array of arrays and flattens them by one level into a single new array. Each array item has its elements added to the result. Non-array, non-null items are added as individual elements. Null items are skipped.

For recursive flattening of deeply nested arrays, see `Flatten`.

## Examples

```jyro
var a = FlattenOnce([[1, 2], [3, 4], [5, 6]])
# a = [1, 2, 3, 4, 5, 6]

var b = FlattenOnce([[1, 2], [3]])
# b = [1, 2, 3]
```
