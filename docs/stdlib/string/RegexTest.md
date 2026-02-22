# RegexTest

Tests whether a string matches a regular expression pattern.

## Signature

```
RegexTest(string text, string pattern)
```

## Parameters

- **text** (string): The string to test.
- **pattern** (string): The regular expression pattern.

## Returns

- **boolean**: `true` if the pattern matches anywhere in the string, `false` otherwise.

## Description

Uses .NET `Regex.IsMatch`. The pattern does not need to match the entire string; a partial match returns `true`. Use anchors (`^` and `$`) to require a full match.

Throws a runtime error if the pattern is invalid.

## Examples

```jyro
var a = RegexTest("hello123", "\d+")
# a = true

var b = RegexTest("hello", "\d+")
# b = false

# Full match with anchors
var c = RegexTest("abc", "^[a-z]+$")
# c = true

var d = RegexTest("abc123", "^[a-z]+$")
# d = false

# Email-like pattern
var e = RegexTest("user@example.com", "^[^@]+@[^@]+\.[^@]+$")
# e = true
```
