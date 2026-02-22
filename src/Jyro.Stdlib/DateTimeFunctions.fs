namespace Mesch.Jyro

open System
open System.Globalization

/// DateTime functions
module DateTimeFunctions =

    /// ISO 8601 output format matching C# implementation
    [<Literal>]
    let private Iso8601Format = "yyyy-MM-ddTHH:mm:ss.fffZ"

    /// Date parsing styles for UTC normalization
    let private utcStyles = DateTimeStyles.AssumeUniversal ||| DateTimeStyles.AdjustToUniversal

    type NowFunction() =
        inherit JyroFunctionBase("Now", FunctionSignatures.create "Now" [] StringParam)
        override _.ExecuteImpl(_, _) =
            JyroString(DateTime.UtcNow.ToString(Iso8601Format)) :> JyroValue

    type TodayFunction() =
        inherit JyroFunctionBase("Today", FunctionSignatures.create "Today" [] StringParam)
        override _.ExecuteImpl(_, _) =
            JyroString(DateTime.UtcNow.Date.ToString("yyyy-MM-dd")) :> JyroValue

    type ParseDateFunction() =
        inherit JyroFunctionBase("ParseDate", FunctionSignatures.unary "ParseDate" StringParam StringParam)
        override this.ExecuteImpl(args, _) =
            let dateStr = this.GetStringArgument(args, 0)
            let supportedFormats = [|
                "yyyy-MM-dd"
                "yyyy-MM-ddTHH:mm:ss"
                "yyyy-MM-ddTHH:mm:ssZ"
                "yyyy-MM-ddTHH:mm:ss.fffZ"
                "MM/dd/yyyy"
                "dd/MM/yyyy"
                "yyyy/MM/dd"
            |]
            match DateTime.TryParseExact(dateStr, supportedFormats, null, utcStyles) with
            | true, dt -> JyroString(dt.ToString(Iso8601Format)) :> JyroValue
            | _ ->
                match DateTime.TryParse(dateStr, null, utcStyles) with
                | true, dt -> JyroString(dt.ToString(Iso8601Format)) :> JyroValue
                | _ ->
                    JyroError.raiseRuntime MessageCode.DateParseError [| box dateStr |]

    type FormatDateFunction() =
        inherit JyroFunctionBase("FormatDate", FunctionSignatures.binary "FormatDate" StringParam StringParam StringParam)
        override this.ExecuteImpl(args, _) =
            let dateStr = this.GetStringArgument(args, 0)
            let format = this.GetStringArgument(args, 1)
            match DateTime.TryParse(dateStr, null, utcStyles) with
            | true, dt ->
                try
                    JyroString(dt.ToString(format)) :> JyroValue
                with :? FormatException ->
                    JyroError.raiseRuntime MessageCode.DateFormatStringInvalid [| box format |]
            | _ ->
                JyroError.raiseRuntime MessageCode.DateFormatInvalid [| box dateStr |]

    type DateAddFunction() =
        inherit JyroFunctionBase("DateAdd",
            FunctionSignatures.create "DateAdd"
                [ Parameter.Required("date", StringParam)
                  Parameter.Required("amount", NumberParam)
                  Parameter.Required("unit", StringParam) ]
                StringParam)
        override this.ExecuteImpl(args, _) =
            let dateStr = this.GetStringArgument(args, 0)
            let amount = this.GetNumberArgument(args, 1)
            let unit = this.GetStringArgument(args, 2).ToLowerInvariant()
            match DateTime.TryParse(dateStr) with
            | true, dt ->
                // Validate integer
                if amount <> System.Math.Floor(amount) then
                    JyroError.raiseRuntime MessageCode.DateAddAmountNotInteger Array.empty<obj>
                let intAmount = int amount
                let result =
                    match unit with
                    | "day" | "days" -> dt.AddDays(float intAmount)
                    | "week" | "weeks" -> dt.AddDays(float (intAmount * 7))
                    | "month" | "months" -> dt.AddMonths(intAmount)
                    | "year" | "years" -> dt.AddYears(intAmount)
                    | "hour" | "hours" -> dt.AddHours(float intAmount)
                    | "minute" | "minutes" -> dt.AddMinutes(float intAmount)
                    | "second" | "seconds" -> dt.AddSeconds(float intAmount)
                    | _ ->
                        JyroError.raiseRuntime MessageCode.DateUnitInvalid [| box unit; box "days, weeks, months, years, hours, minutes, seconds" |]
                JyroString(result.ToString(Iso8601Format)) :> JyroValue
            | _ ->
                JyroError.raiseRuntime MessageCode.DateFormatInvalid [| box dateStr |]

    type DateDiffFunction() =
        inherit JyroFunctionBase("DateDiff",
            FunctionSignatures.create "DateDiff"
                [ Parameter.Required("endDate", StringParam)
                  Parameter.Required("startDate", StringParam)
                  Parameter.Required("unit", StringParam) ]
                NumberParam)
        override this.ExecuteImpl(args, _) =
            let endDateStr = this.GetStringArgument(args, 0)
            let startDateStr = this.GetStringArgument(args, 1)
            let unit = this.GetStringArgument(args, 2).ToLowerInvariant()
            match DateTime.TryParse(endDateStr) with
            | false, _ ->
                JyroError.raiseRuntime MessageCode.DateFormatInvalid [| box endDateStr |]
            | true, endDate ->
                match DateTime.TryParse(startDateStr) with
                | false, _ ->
                    JyroError.raiseRuntime MessageCode.DateFormatInvalid [| box startDateStr |]
                | true, startDate ->
                    let diff = endDate - startDate
                    let result =
                        match unit with
                        | "day" | "days" -> diff.TotalDays
                        | "week" | "weeks" -> diff.TotalDays / 7.0
                        | "hour" | "hours" -> diff.TotalHours
                        | "minute" | "minutes" -> diff.TotalMinutes
                        | "second" | "seconds" -> diff.TotalSeconds
                        | "year" | "years" -> diff.TotalDays / 365.25
                        | "month" | "months" -> diff.TotalDays / 30.44
                        | _ ->
                            JyroError.raiseRuntime MessageCode.DateUnitInvalid [| box unit; box "days, weeks, months, years, hours, minutes, seconds" |]
                    JyroNumber(result) :> JyroValue

    type DatePartFunction() =
        inherit JyroFunctionBase("DatePart", FunctionSignatures.binary "DatePart" StringParam StringParam NumberParam)
        override this.ExecuteImpl(args, _) =
            let dateStr = this.GetStringArgument(args, 0)
            let part = this.GetStringArgument(args, 1).ToLowerInvariant()
            match DateTime.TryParse(dateStr) with
            | true, dt ->
                let result =
                    match part with
                    | "year" -> float dt.Year
                    | "month" -> float dt.Month
                    | "day" -> float dt.Day
                    | "hour" -> float dt.Hour
                    | "minute" -> float dt.Minute
                    | "second" -> float dt.Second
                    | "dayofweek" -> float (int dt.DayOfWeek)
                    | "dayofyear" -> float dt.DayOfYear
                    | _ ->
                        JyroError.raiseRuntime MessageCode.DatePartInvalid [| box part; box "year, month, day, hour, minute, second, dayofweek, dayofyear" |]
                JyroNumber(result) :> JyroValue
            | _ ->
                JyroError.raiseRuntime MessageCode.DateFormatInvalid [| box dateStr |]

    /// Get all datetime functions
    let getAll () : IJyroFunction list =
        [ NowFunction()
          TodayFunction()
          ParseDateFunction()
          FormatDateFunction()
          DateAddFunction()
          DateDiffFunction()
          DatePartFunction() ]
