namespace Mesch.Jyro

open System.Collections.Generic

/// Registry for all standard library functions
module StdlibRegistry =

    /// Get all standard library functions
    let getAll () : IJyroFunction list =
        List.concat [
            StringFunctions.getAll ()
            ArrayFunctions.getAll ()
            MathFunctions.getAll ()
            DateTimeFunctions.getAll ()
            UtilityFunctions.getAll ()
            SchemaFunctions.getAll ()
            QueryFunctions.getAll ()
            LambdaFunctions.getAll ()
        ]

    /// Get functions as a dictionary by name
    let getAsDictionary () : Dictionary<string, IJyroFunction> =
        let dict = Dictionary<string, IJyroFunction>()
        for func in getAll () do
            dict.[func.Name] <- func
        dict

    /// Get a function by name
    let tryGetFunction (name: string) : IJyroFunction option =
        getAll () |> List.tryFind (fun f -> f.Name = name)
