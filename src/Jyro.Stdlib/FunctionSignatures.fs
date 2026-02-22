namespace Mesch.Jyro


/// Helper functions for creating common function signatures
module FunctionSignatures =
    /// Create a unary function signature (one required argument)
    let unary (name: string) (argType: ParameterType) (returnType: ParameterType) : JyroFunctionSignature =
        { Name = name
          Parameters = [ Parameter.Required("value", argType) ]
          ReturnType = returnType
          MinArgs = 1
          MaxArgs = 1 }

    /// Create a binary function signature (two required arguments)
    let binary (name: string) (arg1Type: ParameterType) (arg2Type: ParameterType) (returnType: ParameterType) : JyroFunctionSignature =
        { Name = name
          Parameters = [ Parameter.Required("left", arg1Type); Parameter.Required("right", arg2Type) ]
          ReturnType = returnType
          MinArgs = 2
          MaxArgs = 2 }

    /// Create a function signature with specific parameters
    let create (name: string) (parameters: Parameter list) (returnType: ParameterType) : JyroFunctionSignature =
        let required = parameters |> List.filter (fun p -> not p.IsOptional) |> List.length
        { Name = name
          Parameters = parameters
          ReturnType = returnType
          MinArgs = required
          MaxArgs = parameters.Length }
