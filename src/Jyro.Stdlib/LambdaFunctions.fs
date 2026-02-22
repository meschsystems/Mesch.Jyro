namespace Mesch.Jyro

open System
open System.Collections.Generic

/// Higher-order functions that accept lambda callbacks
module LambdaFunctions =

    let private getCallback (args: IReadOnlyList<JyroValue>) (index: int) (funcName: string) : JyroFunction =
        if index >= args.Count then
            JyroError.raiseRuntime MessageCode.ArgumentNotProvided [| box index |]
        let arg = args.[index]
        if isNull (box arg) then
            JyroError.raiseRuntime MessageCode.CallbackExpected [| box funcName; box "null" |]
        match arg with
        | :? JyroFunction as f -> f
        | other -> JyroError.raiseRuntime MessageCode.CallbackExpected [| box funcName; box other.ValueType |]

    type MapFunction() =
        inherit JyroFunctionBase("Map",
            FunctionSignatures.create "Map"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("callback", AnyParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, ctx) =
            let arr = this.GetArrayArgument(args, 0)
            let callback = getCallback args 1 "Map"
            let result = JyroArray()
            for i = 0 to arr.Length - 1 do
                let item = arr.Items.[i]
                let mapped = callback.Invoke([| item |] :> IReadOnlyList<JyroValue>)
                result.Add(mapped)
            result :> JyroValue

    type WhereFunction() =
        inherit JyroFunctionBase("Where",
            FunctionSignatures.create "Where"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("callback", AnyParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, ctx) =
            let arr = this.GetArrayArgument(args, 0)
            let callback = getCallback args 1 "Where"
            let result = JyroArray()
            for i = 0 to arr.Length - 1 do
                let item = arr.Items.[i]
                let test = callback.Invoke([| item |] :> IReadOnlyList<JyroValue>)
                if test.ToBooleanTruthiness() then
                    result.Add(item)
            result :> JyroValue

    type ReduceFunction() =
        inherit JyroFunctionBase("Reduce",
            FunctionSignatures.create "Reduce"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("callback", AnyParam)
                  Parameter.Required("initial", AnyParam) ]
                AnyParam)
        override this.ExecuteImpl(args, ctx) =
            let arr = this.GetArrayArgument(args, 0)
            let callback = getCallback args 1 "Reduce"
            let mutable acc = args.[2]
            for i = 0 to arr.Length - 1 do
                let item = arr.Items.[i]
                acc <- callback.Invoke([| acc; item |] :> IReadOnlyList<JyroValue>)
            acc

    type EachFunction() =
        inherit JyroFunctionBase("Each",
            FunctionSignatures.create "Each"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("callback", AnyParam) ]
                NullParam)
        override this.ExecuteImpl(args, ctx) =
            let arr = this.GetArrayArgument(args, 0)
            let callback = getCallback args 1 "Each"
            for i = 0 to arr.Length - 1 do
                let item = arr.Items.[i]
                callback.Invoke([| item |] :> IReadOnlyList<JyroValue>) |> ignore
            JyroNull.Instance :> JyroValue

    type FindFunction() =
        inherit JyroFunctionBase("Find",
            FunctionSignatures.create "Find"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("callback", AnyParam) ]
                AnyParam)
        override this.ExecuteImpl(args, ctx) =
            let arr = this.GetArrayArgument(args, 0)
            let callback = getCallback args 1 "Find"
            let mutable found: JyroValue = JyroNull.Instance
            let mutable i = 0
            while i < arr.Length && found.IsNull do
                let item = arr.Items.[i]
                let test = callback.Invoke([| item |] :> IReadOnlyList<JyroValue>)
                if test.ToBooleanTruthiness() then
                    found <- item
                i <- i + 1
            found

    type AllFunction() =
        inherit JyroFunctionBase("All",
            FunctionSignatures.create "All"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("callback", AnyParam) ]
                BooleanParam)
        override this.ExecuteImpl(args, ctx) =
            let arr = this.GetArrayArgument(args, 0)
            let callback = getCallback args 1 "All"
            let mutable result = true
            let mutable i = 0
            while i < arr.Length && result do
                let item = arr.Items.[i]
                let test = callback.Invoke([| item |] :> IReadOnlyList<JyroValue>)
                if not (test.ToBooleanTruthiness()) then
                    result <- false
                i <- i + 1
            JyroBoolean.FromBoolean(result) :> JyroValue

    type AnyFunction() =
        inherit JyroFunctionBase("Any",
            FunctionSignatures.create "Any"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("callback", AnyParam) ]
                BooleanParam)
        override this.ExecuteImpl(args, ctx) =
            let arr = this.GetArrayArgument(args, 0)
            let callback = getCallback args 1 "Any"
            let mutable result = false
            let mutable i = 0
            while i < arr.Length && not result do
                let item = arr.Items.[i]
                let test = callback.Invoke([| item |] :> IReadOnlyList<JyroValue>)
                if test.ToBooleanTruthiness() then
                    result <- true
                i <- i + 1
            JyroBoolean.FromBoolean(result) :> JyroValue

    type SortByFunction() =
        inherit JyroFunctionBase("SortBy",
            FunctionSignatures.create "SortBy"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("callback", AnyParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, ctx) =
            let arr = this.GetArrayArgument(args, 0)
            let callback = getCallback args 1 "SortBy"
            let items = ResizeArray<JyroValue>()
            for i = 0 to arr.Length - 1 do
                items.Add(arr.Items.[i])
            items.Sort(fun a b ->
                let ka = callback.Invoke([| a |] :> IReadOnlyList<JyroValue>)
                let kb = callback.Invoke([| b |] :> IReadOnlyList<JyroValue>)
                match ka, kb with
                | (:? JyroNumber as an), (:? JyroNumber as bn) -> an.Value.CompareTo(bn.Value)
                | (:? JyroString as aStr), (:? JyroString as bStr) -> String.Compare(aStr.Value, bStr.Value, StringComparison.Ordinal)
                | _ -> 0)
            let result = JyroArray()
            for item in items do
                result.Add(item)
            result :> JyroValue

    /// Get all lambda/higher-order functions
    let getAll () : IJyroFunction list =
        [ MapFunction()
          WhereFunction()
          ReduceFunction()
          EachFunction()
          FindFunction()
          AllFunction()
          AnyFunction()
          SortByFunction() ]
