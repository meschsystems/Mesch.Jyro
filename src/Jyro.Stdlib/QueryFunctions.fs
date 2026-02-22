namespace Mesch.Jyro

open System

/// Query and predicate functions for arrays (AnyByField, AllByField, FindByField, WhereByField, Select, Project, CountIf)
module QueryFunctions =

    /// Compares two JyroValues for relational operators.
    /// Returns negative if left < right, zero if equal, positive if left > right.
    /// Throws on incompatible types.
    let private compareValues (left: JyroValue) (right: JyroValue) : int =
        match left, right with
        | :? JyroNumber as ln, (:? JyroNumber as rn) -> ln.Value.CompareTo(rn.Value)
        | :? JyroString as ls, (:? JyroString as rs) -> String.Compare(ls.Value, rs.Value, StringComparison.Ordinal)
        | :? JyroBoolean as lb, (:? JyroBoolean as rb) -> lb.Value.CompareTo(rb.Value)
        | _ ->
            JyroError.raiseRuntime MessageCode.IncomparableTypes [| box (left.GetType().Name); box (right.GetType().Name) |]

    /// Evaluates a comparison between two JyroValues using the specified operator.
    let private evaluateComparison (left: JyroValue) (op: string) (right: JyroValue) : bool =
        match op with
        | "==" -> left.EqualsValue(right)
        | "!=" -> not (left.EqualsValue(right))
        | "<" -> compareValues left right < 0
        | "<=" -> compareValues left right <= 0
        | ">" -> compareValues left right > 0
        | ">=" -> compareValues left right >= 0
        | _ ->
            JyroError.raiseRuntime MessageCode.UnsupportedComparisonOperator [| box op |]

    type AnyByFieldFunction() =
        inherit JyroFunctionBase("AnyByField",
            FunctionSignatures.create "AnyByField"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fieldName", StringParam)
                  Parameter.Required("operator", StringParam)
                  Parameter.Required("value", AnyParam) ]
                BooleanParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fieldName = this.GetStringArgument(args, 1)
            let op = this.GetStringArgument(args, 2)
            let compareValue = args.[3]
            let mutable i = 0
            let mutable found = false
            while i < arr.Length && not found do
                let item = arr.Items.[i]
                match item with
                | :? JyroObject as obj ->
                    let fieldValue = obj.GetProperty(fieldName)
                    if fieldValue :? JyroNull then
                        if op = "!=" && not compareValue.IsNull then
                            found <- true
                    elif evaluateComparison fieldValue op compareValue then
                        found <- true
                | _ -> ()
                i <- i + 1
            JyroBoolean.FromBoolean(found) :> JyroValue

    type AllByFieldFunction() =
        inherit JyroFunctionBase("AllByField",
            FunctionSignatures.create "AllByField"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fieldName", StringParam)
                  Parameter.Required("operator", StringParam)
                  Parameter.Required("value", AnyParam) ]
                BooleanParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fieldName = this.GetStringArgument(args, 1)
            let op = this.GetStringArgument(args, 2)
            let compareValue = args.[3]
            let mutable i = 0
            let mutable allMatch = true
            while i < arr.Length && allMatch do
                let item = arr.Items.[i]
                match item with
                | :? JyroObject as obj ->
                    let fieldValue = obj.GetProperty(fieldName)
                    if fieldValue :? JyroNull then
                        if op = "!=" && not compareValue.IsNull then
                            () // null != non-null is true, continue
                        else
                            allMatch <- false
                    elif not (evaluateComparison fieldValue op compareValue) then
                        allMatch <- false
                | _ ->
                    allMatch <- false // Non-object items fail the "all" check
                i <- i + 1
            JyroBoolean.FromBoolean(allMatch) :> JyroValue

    type FindByFieldFunction() =
        inherit JyroFunctionBase("FindByField",
            FunctionSignatures.create "FindByField"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fieldName", StringParam)
                  Parameter.Required("operator", StringParam)
                  Parameter.Required("value", AnyParam) ]
                AnyParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fieldName = this.GetStringArgument(args, 1)
            let op = this.GetStringArgument(args, 2)
            let compareValue = args.[3]
            let mutable i = 0
            let mutable result: JyroValue = null
            while i < arr.Length && obj.ReferenceEquals(result, null) do
                let item = arr.Items.[i]
                match item with
                | :? JyroObject as jobj ->
                    let fieldValue = jobj.GetProperty(fieldName)
                    if fieldValue :? JyroNull then
                        if op = "!=" && not compareValue.IsNull then
                            result <- item
                    elif evaluateComparison fieldValue op compareValue then
                        result <- item
                | _ -> ()
                i <- i + 1
            if obj.ReferenceEquals(result, null) then JyroNull.Instance :> JyroValue else result

    type WhereByFieldFunction() =
        inherit JyroFunctionBase("WhereByField",
            FunctionSignatures.create "WhereByField"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fieldName", StringParam)
                  Parameter.Required("operator", StringParam)
                  Parameter.Required("value", AnyParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fieldName = this.GetStringArgument(args, 1)
            let op = this.GetStringArgument(args, 2)
            let compareValue = args.[3]
            let result = JyroArray()
            for i = 0 to arr.Length - 1 do
                let item = arr.Items.[i]
                match item with
                | :? JyroObject as jobj ->
                    let fieldValue = jobj.GetProperty(fieldName)
                    if fieldValue :? JyroNull then
                        if op = "!=" && not compareValue.IsNull then
                            result.Add(item)
                    elif evaluateComparison fieldValue op compareValue then
                        result.Add(item)
                | _ -> ()
            result :> JyroValue

    type SelectFunction() =
        inherit JyroFunctionBase("Select",
            FunctionSignatures.create "Select"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fieldName", StringParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fieldName = this.GetStringArgument(args, 1)
            let result = JyroArray()
            for i = 0 to arr.Length - 1 do
                let item = arr.Items.[i]
                match item with
                | :? JyroObject as jobj ->
                    result.Add(jobj.GetProperty(fieldName))
                | _ ->
                    result.Add(JyroNull.Instance)
            result :> JyroValue

    type ProjectFunction() =
        inherit JyroFunctionBase("Project",
            FunctionSignatures.create "Project"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fields", ArrayParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fields = this.GetArrayArgument(args, 1)
            let fieldNames =
                fields.Items
                |> Seq.choose (fun f ->
                    match f with
                    | :? JyroString as s -> Some s.Value
                    | _ -> None)
                |> Seq.toList
            let result = JyroArray()
            for i = 0 to arr.Length - 1 do
                let item = arr.Items.[i]
                match item with
                | :? JyroObject as sourceObj ->
                    let projected = JyroObject()
                    for fieldPath in fieldNames do
                        let value = sourceObj.GetProperty(fieldPath)
                        let key =
                            let lastDot = fieldPath.LastIndexOf('.')
                            if lastDot >= 0 then fieldPath.Substring(lastDot + 1) else fieldPath
                        projected.SetProperty(key, value)
                    result.Add(projected)
                | _ -> () // Skip non-objects
            result :> JyroValue

    type OmitFunction() =
        inherit JyroFunctionBase("Omit",
            FunctionSignatures.create "Omit"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fields", ArrayParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fields = this.GetArrayArgument(args, 1)
            let omitSet =
                fields.Items
                |> Seq.choose (fun f ->
                    match f with
                    | :? JyroString as s -> Some s.Value
                    | _ -> None)
                |> Set.ofSeq
            let result = JyroArray()
            for i = 0 to arr.Length - 1 do
                let item = arr.Items.[i]
                match item with
                | :? JyroObject as sourceObj ->
                    let omitted = JyroObject()
                    for key in sourceObj.Properties.Keys do
                        if not (omitSet.Contains(key)) then
                            omitted.SetProperty(key, sourceObj.Properties.[key])
                    result.Add(omitted)
                | _ -> () // Skip non-objects
            result :> JyroValue

    type CountIfFunction() =
        inherit JyroFunctionBase("CountIf",
            FunctionSignatures.create "CountIf"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fieldName", StringParam)
                  Parameter.Required("operator", StringParam)
                  Parameter.Required("value", AnyParam) ]
                NumberParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fieldName = this.GetStringArgument(args, 1)
            let op = this.GetStringArgument(args, 2)
            let compareValue = args.[3]
            let mutable count = 0
            for i = 0 to arr.Length - 1 do
                let item = arr.Items.[i]
                match item with
                | :? JyroObject as jobj ->
                    let fieldValue = jobj.GetProperty(fieldName)
                    if fieldValue :? JyroNull then
                        if op = "!=" && not compareValue.IsNull then
                            count <- count + 1
                    elif evaluateComparison fieldValue op compareValue then
                        count <- count + 1
                | _ -> ()
            JyroNumber(float count) :> JyroValue

    /// Get all query functions
    let getAll () : IJyroFunction list =
        [ AnyByFieldFunction()
          AllByFieldFunction()
          FindByFieldFunction()
          WhereByFieldFunction()
          SelectFunction()
          ProjectFunction()
          OmitFunction()
          CountIfFunction() ]
