# RegexMatch

Returns the first match of a regular expression pattern in a string.

## Signature

```
RegexMatch(string text, string pattern)
```

## Parameters

- **text** (string): The string to search.
- **pattern** (string): The regular expression pattern.

## Returns

- **string**: The matched text of the first match.
- **null**: Returned if no match is found.

## Description

Uses .NET `Regex.Match`. Returns the value of the first match, or `null` if the pattern does not match. Only the overall match text is returned; use `RegexMatchDetail` to access capture groups.

Throws a runtime error if the pattern is invalid.

## Examples

```jyro
var a = RegexMatch("order-12345-item", "\d+")
# a = "12345"

var b = RegexMatch("no digits here", "\d+")
# b = null

var c = RegexMatch("2024-01-15", "\d{4}-\d{2}-\d{2}")
# c = "2024-01-15"
```
