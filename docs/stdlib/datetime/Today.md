# Today

Returns the current UTC date without a time component.

## Signature

```
Today()
```

## Parameters

None.

## Returns

- **string**: The current UTC date in `yyyy-MM-dd` format.

## Description

Returns `DateTime.UtcNow.Date` formatted as a date-only string. Useful for date comparisons that should ignore time.

## Examples

```jyro
var date = Today()
# date = "2025-10-31"

Data.reportDate = Today()
```
