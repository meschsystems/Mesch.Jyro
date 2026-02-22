# HasProperty

Tests whether an object has a specific property defined.

## Signature

```
HasProperty(object obj, string key)
```

## Parameters

- **obj** (object): The object to check.
- **key** (string): The property name to look for.

## Returns

- **boolean**: `true` if the property exists (even if its value is null).

## Description

Checks for property existence using `ContainsKey`. Unlike `Exists`, which checks whether a value is non-null, `HasProperty` returns `true` even when the property's value is null.

## Examples

```jyro
var user = { "name": "Alice", "age": null }

var a = HasProperty(user, "name")
# a = true

var b = HasProperty(user, "age")
# b = true (property exists, even though value is null)

var c = HasProperty(user, "email")
# c = false
```
