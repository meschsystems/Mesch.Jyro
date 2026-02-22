# AllByField

Tests whether all objects in an array match a field condition.

## Signature

```
AllByField(array arr, string fieldName, string operator, any value)
```

## Parameters

- **arr** (array): An array of objects to test.
- **fieldName** (string): The property to compare. Supports dot notation.
- **operator** (string): The comparison operator.
- **value** (any): The value to compare against.

## Returns

- **boolean**: `true` if every element matches the condition.

## Description

Iterates the array and evaluates the condition against each object's field. Stops at the first non-match. Non-object elements cause the function to return `false` (they fail the "all" check).

Supported operators: `==`, `!=`, `<`, `<=`, `>`, `>=`.

Null field handling: A null field passes `!= non-null` and `== null`. All other comparisons against null fields fail.

For general-purpose predicate testing, see `All` which accepts a lambda.

## Examples

```jyro
var users = [
    { "name": "Alice", "active": true },
    { "name": "Bob", "active": true }
]

var allActive = AllByField(users, "active", "==", true)
# allActive = true

var allAdult = AllByField(users, "age", ">=", 18)
# allAdult = false (age field is null)
```
