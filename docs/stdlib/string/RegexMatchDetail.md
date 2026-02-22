# RegexMatchDetail

Returns detailed information about the first regex match, including capture groups.

## Signature

```
RegexMatchDetail(string text, string pattern)
```

## Parameters

- **text** (string): The string to search.
- **pattern** (string): The regular expression pattern.

## Returns

- **object**: An object with match details, or `null` if no match is found.

The returned object has the following properties:

- **match** (string): The full matched text.
- **index** (number): The zero-based position where the match begins.
- **groups** (array): An array of capture group values (excluding group 0, the full match).

## Description

Uses .NET `Regex.Match`. Provides access to capture groups defined with parentheses in the pattern. Group 0 (the full match) is excluded from the `groups` array since it duplicates the `match` property.

Returns `null` if the pattern does not match. Throws a runtime error if the pattern is invalid.

## Examples

```jyro
# Extract parts of a date
var result = RegexMatchDetail("2024-01-15", "(\d{4})-(\d{2})-(\d{2})")
# result.match = "2024-01-15"
# result.index = 0
# result.groups = ["2024", "01", "15"]

var year = result.groups[0]
# year = "2024"

# No match
var none = RegexMatchDetail("hello", "\d+")
# none = null

# Single capture group
var tag = RegexMatchDetail("<title>Hello</title>", "<title>(.*?)</title>")
# tag.match = "<title>Hello</title>"
# tag.groups = ["Hello"]
```
