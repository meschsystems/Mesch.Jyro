# AnyByField

Tests whether any object in an array matches a field condition.

## Signature

```
AnyByField(array arr, string fieldName, string operator, any value)
```

## Parameters

- **arr** (array): An array of objects to test.
- **fieldName** (string): The property to compare. Supports dot notation.
- **operator** (string): The comparison operator.
- **value** (any): The value to compare against.

## Returns

- **boolean**: `true` if at least one object matches the condition.

## Description

Iterates the array and evaluates the condition against each object's field. Stops at the first match. Non-object elements are skipped.

Supported operators: `==`, `!=`, `<`, `<=`, `>`, `>=`.

Null field handling: A null field matches `!= non-null` and `== null`. All other comparisons against null fields are skipped.

For general-purpose predicate testing, see `Any` which accepts a lambda.

## Examples

```jyro
var users = [
    { "name": "Alice", "age": 30 },
    { "name": "Bob", "age": 25 }
]

var hasAdult = AnyByField(users, "age", ">=", 30)
# hasAdult = true

var hasCarol = AnyByField(users, "name", "==", "Carol")
# hasCarol = false

# Dot notation
var orders = [
    { "customer": { "vip": true } },
    { "customer": { "vip": false } }
]
var hasVip = AnyByField(orders, "customer.vip", "==", true)
# hasVip = true
```
