# Values

Returns all property values of an object as an array.

## Signature

```
Values(object obj)
```

## Parameters

- **obj** (object): The object to inspect.

## Returns

- **array**: An array of the object's property values in insertion order.

## Description

Complements `Keys`. Returns values in the same order as their corresponding keys.

## Examples

```jyro
var scores = { "math": 95, "science": 88, "english": 92 }
var vals = Values(scores)
# vals = [95, 88, 92]

var empty = Values({})
# empty = []
```
