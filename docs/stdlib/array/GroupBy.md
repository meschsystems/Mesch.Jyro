# GroupBy

Groups array elements by a field value.

## Signature

```
GroupBy(array arr, string fieldName)
```

## Parameters

- **arr** (array): An array of objects to group.
- **fieldName** (string): The property to group by. Supports dot notation for nested properties.

## Returns

- **object**: An object where each key is a group value and each value is an array of matching elements.

## Description

Iterates the array and groups objects by the string representation of the specified field. Non-object elements are skipped. Objects with a null or missing field are grouped under the key `"null"`.

## Examples

```jyro
var employees = [
    { "name": "Alice", "dept": "Engineering" },
    { "name": "Bob", "dept": "Sales" },
    { "name": "Carol", "dept": "Engineering" }
]

var groups = GroupBy(employees, "dept")
# groups.Engineering = [Alice, Carol]
# groups.Sales = [Bob]

# Access a group
var engineers = groups["Engineering"]
var count = Length(engineers)
# count = 2

# Nested field
var people = [
    { "name": "Alice", "address": { "city": "Boston" } },
    { "name": "Bob", "address": { "city": "Boston" } }
]
var byCity = GroupBy(people, "address.city")
```
