# RegexMatchAll

Returns all matches of a regular expression pattern in a string.

## Signature

```
RegexMatchAll(string text, string pattern)
```

## Parameters

- **text** (string): The string to search.
- **pattern** (string): The regular expression pattern.

## Returns

- **array**: An array of strings, one per match. Returns an empty array if no matches are found.

## Description

Uses .NET `Regex.Matches`. Returns the matched text of every non-overlapping match. Only overall match values are returned, not capture groups.

Throws a runtime error if the pattern is invalid.

## Examples

```jyro
var nums = RegexMatchAll("a1b2c3", "\d")
# nums = ["1", "2", "3"]

var words = RegexMatchAll("hello world foo", "\w+")
# words = ["hello", "world", "foo"]

var none = RegexMatchAll("no match", "\d+")
# none = []

# Extract all email addresses
var emails = RegexMatchAll(Data.text, "[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}")
```
