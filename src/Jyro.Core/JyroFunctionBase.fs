namespace Mesch.Jyro

open System.Collections.Generic

/// Base class for implementing Jyro functions with helper methods
[<AbstractClass>]
type JyroFunctionBase(name: string, signature: JyroFunctionSignature) =

    abstract member ExecuteImpl: args: IReadOnlyList<JyroValue> * ctx: JyroExecutionContext -> JyroValue

    interface IJyroFunction with
        member _.Name = name
        member _.Signature = signature
        member this.Execute(args, ctx) = this.ExecuteImpl(args, ctx)

    // Argument retrieval helpers
    member _.GetArgument<'T when 'T :> JyroValue>(args: IReadOnlyList<JyroValue>, index: int) : 'T =
        if index >= args.Count then
            JyroError.raiseRuntime MessageCode.ArgumentNotProvided [| box index |]
        match args.[index] with
        | :? 'T as value -> value
        | other -> JyroError.raiseRuntime MessageCode.ArgumentTypeMismatch [| box typeof<'T>.Name; box other.ValueType |]

    member _.GetOptionalArgument<'T when 'T :> JyroValue>(args: IReadOnlyList<JyroValue>, index: int, defaultValue: 'T) : 'T =
        if index >= args.Count then defaultValue
        else
            match args.[index] with
            | :? 'T as value -> value
            | _ -> defaultValue

    member _.GetStringArgument(args: IReadOnlyList<JyroValue>, index: int) : string =
        if index >= args.Count then
            JyroError.raiseRuntime MessageCode.ArgumentNotProvided [| box index |]
        match args.[index] with
        | :? JyroString as s -> s.Value
        | other -> other.ToStringValue()

    member _.GetNumberArgument(args: IReadOnlyList<JyroValue>, index: int) : float =
        if index >= args.Count then
            JyroError.raiseRuntime MessageCode.ArgumentNotProvided [| box index |]
        match args.[index] with
        | :? JyroNumber as n -> n.Value
        | other -> JyroError.raiseRuntime MessageCode.ArgumentTypeMismatch [| box "number"; box other.ValueType |]

    member _.GetBooleanArgument(args: IReadOnlyList<JyroValue>, index: int) : bool =
        if index >= args.Count then
            JyroError.raiseRuntime MessageCode.ArgumentNotProvided [| box index |]
        match args.[index] with
        | :? JyroBoolean as b -> b.Value
        | other -> other.ToBooleanTruthiness()

    member _.GetArrayArgument(args: IReadOnlyList<JyroValue>, index: int) : JyroArray =
        if index >= args.Count then
            JyroError.raiseRuntime MessageCode.ArgumentNotProvided [| box index |]
        match args.[index] with
        | :? JyroArray as a -> a
        | :? JyroNull -> JyroArray()
        | other -> JyroError.raiseRuntime MessageCode.ArgumentTypeMismatch [| box "array"; box other.ValueType |]

    member _.GetObjectArgument(args: IReadOnlyList<JyroValue>, index: int) : JyroObject =
        if index >= args.Count then
            JyroError.raiseRuntime MessageCode.ArgumentNotProvided [| box index |]
        match args.[index] with
        | :? JyroObject as o -> o
        | :? JyroNull -> JyroObject()
        | other -> JyroError.raiseRuntime MessageCode.ArgumentTypeMismatch [| box "object"; box other.ValueType |]
