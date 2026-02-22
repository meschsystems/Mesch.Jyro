# WhereByField

Returns all objects in an array that match a field condition.

## Signature

```
WhereByField(array arr, string fieldName, string operator, any value)
```

## Parameters

- **arr** (array): An array of objects to filter.
- **fieldName** (string): The property to compare. Supports dot notation.
- **operator** (string): The comparison operator.
- **value** (any): The value to compare against.

## Returns

- **array**: A new array containing only matching objects. Returns an empty array if no matches.

## Description

Iterates the array and collects all objects whose field satisfies the condition. Non-object elements are skipped. The original array is not modified.

Supported operators: `==`, `!=`, `<`, `<=`, `>`, `>=`. Comparisons are case-sensitive for strings.

Null field handling: A null field matches `!= non-null`. A null field with `== null` is **not** included (only explicit `!= non-null` matches). All other comparisons against null fields are skipped.

For general-purpose predicate filtering, see `Where` which accepts a lambda.

## Examples

```jyro
var users = [
    { "name": "Alice", "age": 30, "active": true },
    { "name": "Bob", "age": 25, "active": false },
    { "name": "Carol", "age": 35, "active": true }
]

var active = WhereByField(users, "active", "==", true)
# active = [Alice, Carol]

var adults = WhereByField(users, "age", ">=", 30)
# adults = [Alice, Carol]

# Dot notation for nested fields
var items = [
    { "meta": { "priority": "high" } },
    { "meta": { "priority": "low" } }
]
var high = WhereByField(items, "meta.priority", "==", "high")
```
