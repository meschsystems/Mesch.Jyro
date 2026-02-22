# FindByField

Returns the first object in an array that matches a field condition.

## Signature

```
FindByField(array arr, string fieldName, string operator, any value)
```

## Parameters

- **arr** (array): An array of objects to search.
- **fieldName** (string): The property to compare. Supports dot notation.
- **operator** (string): The comparison operator.
- **value** (any): The value to compare against.

## Returns

- **object**: The first matching object.
- **null**: Returned if no match is found.

## Description

Iterates the array and returns the first object whose field satisfies the condition. Non-object elements are skipped.

Supported operators: `==`, `!=`, `<`, `<=`, `>`, `>=`.

For general-purpose predicate searching, see `Find` which accepts a lambda.

## Examples

```jyro
var users = [
    { "name": "Alice", "age": 30 },
    { "name": "Bob", "age": 25 },
    { "name": "Carol", "age": 35 }
]

var found = FindByField(users, "name", "==", "Bob")
# found = { "name": "Bob", "age": 25 }

var senior = FindByField(users, "age", ">", 30)
# senior = { "name": "Carol", "age": 35 }

var missing = FindByField(users, "name", "==", "Dave")
# missing = null
```
