# Merge

Merges multiple objects into a new object.

## Signature

```
Merge(array objects)
```

## Parameters

- **objects** (array): An array of objects to merge. Non-object items are silently skipped.

## Returns

- **object**: A new object with all properties from the input objects.

## Description

Creates a new object by copying properties from each object in the array in order. Later objects overwrite earlier ones when keys conflict. Non-object items (numbers, strings, arrays, null) are silently skipped.

## Examples

```jyro
var defaults = { "theme": "light", "lang": "en" }
var overrides = { "theme": "dark" }
var config = Merge([defaults, overrides])
# config = { "theme": "dark", "lang": "en" }

# Non-objects are skipped
var result = Merge([{ "a": 1 }, null, "ignored", { "b": 2 }])
# result = { "a": 1, "b": 2 }
```
