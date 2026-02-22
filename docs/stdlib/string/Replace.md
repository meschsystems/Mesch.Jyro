# Replace

Replaces all occurrences of a substring with another string.

## Signature

```
Replace(string source, string oldValue, string newValue)
```

## Parameters

- **source** (string): The original string.
- **oldValue** (string): The substring to find.
- **newValue** (string): The replacement string.

## Returns

- **string**: A new string with all occurrences of `oldValue` replaced by `newValue`.

## Description

Uses `String.Replace`. The comparison is case-sensitive. If `oldValue` is not found, the original string is returned unchanged. All occurrences are replaced, not just the first.

## Examples

```jyro
var result = Replace("Hello, World", "World", "Jyro")
# result = "Hello, Jyro"

var multi = Replace("aabaa", "a", "x")
# multi = "xxbxx"

var noMatch = Replace("hello", "xyz", "abc")
# noMatch = "hello"

# Remove a substring
var cleaned = Replace("foo-bar-baz", "-", "")
# cleaned = "foobarbaz"
```
