# CountIf

Counts the number of objects in an array that match a condition.

## Signature

```
CountIf(array arr, string fieldName, string operator, any value)
```

## Parameters

- **arr** (array): An array of objects to count.
- **fieldName** (string): The property to compare. Supports dot notation.
- **operator** (string): The comparison operator.
- **value** (any): The value to compare against.

## Returns

- **number**: The count of matching objects.

## Description

Iterates the array and counts objects whose field satisfies the condition. Non-object elements are skipped and not counted.

Supported operators: `==`, `!=`, `<`, `<=`, `>`, `>=`.

Null field handling: A null field matches `!= non-null`. All other comparisons against null fields are skipped.

## Examples

```jyro
var users = [
    { "name": "Alice", "age": 30 },
    { "name": "Bob", "age": 25 },
    { "name": "Carol", "age": 35 }
]

var adults = CountIf(users, "age", ">=", 30)
# adults = 2

var bobs = CountIf(users, "name", "==", "Bob")
# bobs = 1

var total = Length(users)
var activeCount = CountIf(users, "active", "==", true)
```
