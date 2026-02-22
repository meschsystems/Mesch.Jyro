# Now

Returns the current UTC date and time.

## Signature

```
Now()
```

## Parameters

None.

## Returns

- **string**: The current UTC date and time in ISO 8601 format (`yyyy-MM-ddTHH:mm:ss.fffZ`).

## Description

Returns `DateTime.UtcNow` formatted as an ISO 8601 string with millisecond precision. The trailing `Z` indicates UTC.

## Examples

```jyro
var timestamp = Now()
# timestamp = "2025-10-31T14:30:15.123Z"

Data.createdAt = Now()
```
