namespace Mesch.Jyro

open System
open System.Collections.Generic
open System.Security.Cryptography

/// Array manipulation functions
module ArrayFunctions =

    type LengthFunction() =
        inherit JyroFunctionBase("Length", FunctionSignatures.unary "Length" AnyParam NumberParam)
        override _.ExecuteImpl(args, _) =
            let value = args.[0]
            let length =
                match value with
                | :? JyroString as s -> s.Length
                | :? JyroArray as a -> a.Length
                | :? JyroObject as o -> o.Count
                | :? JyroNull -> 0
                | _ -> 1
            JyroNumber(float length) :> JyroValue

    type AppendFunction() =
        inherit JyroFunctionBase("Append", FunctionSignatures.binary "Append" ArrayParam AnyParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let value = args.[1]
            let result = JyroArray()
            for item in arr.Items do
                result.Add(item)
            result.Add(value)
            result :> JyroValue

    type PrependFunction() =
        inherit JyroFunctionBase("Prepend", FunctionSignatures.binary "Prepend" ArrayParam AnyParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let value = args.[1]
            let result = JyroArray()
            result.Add(value)
            for item in arr.Items do
                result.Add(item)
            result :> JyroValue

    type FirstFunction() =
        inherit JyroFunctionBase("First", FunctionSignatures.unary "First" ArrayParam AnyParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            if arr.Items.Count > 0 then arr.Items.[0]
            else JyroNull.Instance :> JyroValue

    type LastFunction() =
        inherit JyroFunctionBase("Last", FunctionSignatures.unary "Last" ArrayParam AnyParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            if arr.Items.Count > 0 then arr.Items.[arr.Items.Count - 1]
            else JyroNull.Instance :> JyroValue

    type IndexOfFunction() =
        inherit JyroFunctionBase("IndexOf", FunctionSignatures.binary "IndexOf" AnyParam AnyParam NumberParam)
        override _.ExecuteImpl(args, _) =
            let source = args.[0]
            let search = args.[1]
            match source, search with
            | (:? JyroString as sourceStr), (:? JyroString as searchStr) ->
                let pos = sourceStr.Value.IndexOf(searchStr.Value, StringComparison.Ordinal)
                JyroNumber(float pos) :> JyroValue
            | (:? JyroArray as arr), _ ->
                let mutable index = -1
                let mutable i = 0
                while i < arr.Length && index < 0 do
                    if arr.Items.[i].EqualsValue(search) then
                        index <- i
                    i <- i + 1
                JyroNumber(float index) :> JyroValue
            | _ ->
                JyroError.raiseRuntime MessageCode.StringOrArrayRequired [| box "IndexOf()" |]

    type ReverseFunction() =
        inherit JyroFunctionBase("Reverse", FunctionSignatures.unary "Reverse" ArrayParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let result = JyroArray()
            for i = arr.Items.Count - 1 downto 0 do
                result.Add(arr.Items.[i])
            result :> JyroValue

    type SliceFunction() =
        inherit JyroFunctionBase("Slice",
            FunctionSignatures.create "Slice"
                [ Parameter.Required("arr", ArrayParam)
                  Parameter.Required("start", NumberParam)
                  Parameter.Optional("end", NumberParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let startArg = this.GetArgument<JyroNumber>(args, 1)
            if not startArg.IsInteger then
                JyroError.raiseRuntime MessageCode.IntegerRequired [| box "Slice()"; box "start index"; box startArg.Value |]
            let start = startArg.ToInteger()
            let endIdx =
                if args.Count > 2 then
                    let endArg = this.GetArgument<JyroNumber>(args, 2)
                    if not endArg.IsInteger then
                        JyroError.raiseRuntime MessageCode.IntegerRequired [| box "Slice()"; box "end index"; box endArg.Value |]
                    endArg.ToInteger()
                else arr.Items.Count
            let result = JyroArray()
            let actualStart = max 0 start
            let actualEnd = min arr.Items.Count endIdx
            for i = actualStart to actualEnd - 1 do
                result.Add(arr.Items.[i])
            result :> JyroValue

    type ConcatenateFunction() =
        inherit JyroFunctionBase("Concatenate", FunctionSignatures.binary "Concatenate" ArrayParam ArrayParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr1 = this.GetArrayArgument(args, 0)
            let arr2 = this.GetArrayArgument(args, 1)
            let result = JyroArray()
            for item in arr1.Items do
                result.Add(item)
            for item in arr2.Items do
                result.Add(item)
            result :> JyroValue

    type DistinctFunction() =
        inherit JyroFunctionBase("Distinct", FunctionSignatures.unary "Distinct" ArrayParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let result = JyroArray()
            for item in arr.Items do
                let mutable isDuplicate = false
                for existing in result.Items do
                    if item.EqualsValue(existing) then
                        isDuplicate <- true
                if not isDuplicate then
                    result.Add(item)
            result :> JyroValue

    type SortFunction() =
        inherit JyroFunctionBase("Sort", FunctionSignatures.unary "Sort" ArrayParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let items = ResizeArray<JyroValue>()
            for i = 0 to arr.Length - 1 do
                items.Add(arr.Items.[i])
            items.Sort(fun a b ->
                if a.IsNull && b.IsNull then 0
                elif a.IsNull then -1
                elif b.IsNull then 1
                else
                    match a, b with
                    | (:? JyroNumber as an), (:? JyroNumber as bn) -> an.Value.CompareTo(bn.Value)
                    | (:? JyroString as aStr), (:? JyroString as bStr) -> String.Compare(aStr.Value, bStr.Value, StringComparison.Ordinal)
                    | (:? JyroBoolean as ab), (:? JyroBoolean as bb) -> ab.Value.CompareTo(bb.Value)
                    | _ -> 0)
            let result = JyroArray()
            for item in items do
                result.Add(item)
            result :> JyroValue

    type FlattenFunction() =
        inherit JyroFunctionBase("Flatten", FunctionSignatures.unary "Flatten" ArrayParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let result = JyroArray()
            let rec flatten (items: IReadOnlyList<JyroValue>) =
                for item in items do
                    match item with
                    | :? JyroArray as nested -> flatten nested.Items
                    | other -> result.Add(other)
            flatten arr.Items
            result :> JyroValue

    type RangeFunction() =
        inherit JyroFunctionBase("Range",
            FunctionSignatures.create "Range"
                [ Parameter.Required("start", NumberParam)
                  Parameter.Required("end", NumberParam)
                  Parameter.Optional("step", NumberParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, _) =
            let startVal = this.GetNumberArgument(args, 0)
            let endVal = this.GetNumberArgument(args, 1)
            let step = if args.Count > 2 then this.GetNumberArgument(args, 2) else 1.0
            let result = JyroArray()
            if step > 0.0 then
                let mutable i = startVal
                while i <= endVal do
                    result.Add(JyroNumber(i))
                    i <- i + step
            elif step < 0.0 then
                let mutable i = startVal
                while i >= endVal do
                    result.Add(JyroNumber(i))
                    i <- i + step
            result :> JyroValue

    type SkipFunction() =
        inherit JyroFunctionBase("Skip", FunctionSignatures.binary "Skip" ArrayParam NumberParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let count = int (this.GetNumberArgument(args, 1))
            let result = JyroArray()
            let startIdx = min count arr.Items.Count
            for i = startIdx to arr.Items.Count - 1 do
                result.Add(arr.Items.[i])
            result :> JyroValue

    type InsertFunction() =
        inherit JyroFunctionBase("Insert",
            FunctionSignatures.create "Insert"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("index", NumberParam)
                  Parameter.Required("value", AnyParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let indexArg = this.GetArgument<JyroNumber>(args, 1)
            let value = args.[2]
            if not indexArg.IsInteger then
                JyroError.raiseRuntime MessageCode.IntegerRequired [| box "Insert()"; box "index"; box indexArg.Value |]
            let idx = indexArg.ToInteger()
            if idx < 0 || idx > arr.Length then
                JyroError.raiseRuntime MessageCode.IndexOutOfRange [| box (sprintf "Insert() index %d is out of bounds for array of length %d" idx arr.Length) |]
            let result = JyroArray()
            for i = 0 to idx - 1 do
                result.Add(arr.Items.[i])
            result.Add(value)
            for i = idx to arr.Length - 1 do
                result.Add(arr.Items.[i])
            result :> JyroValue

    type RemoveAtFunction() =
        inherit JyroFunctionBase("RemoveAt", FunctionSignatures.binary "RemoveAt" ArrayParam NumberParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let indexArg = this.GetArgument<JyroNumber>(args, 1)
            if not indexArg.IsInteger then
                JyroError.raiseRuntime MessageCode.IntegerRequired [| box "RemoveAt()"; box "index"; box indexArg.Value |]
            let idx = indexArg.ToInteger()
            let result = JyroArray()
            for i = 0 to arr.Length - 1 do
                if i <> idx then
                    result.Add(arr.Items.[i])
            result :> JyroValue

    type RemoveFirstFunction() =
        inherit JyroFunctionBase("RemoveFirst", FunctionSignatures.unary "RemoveFirst" ArrayParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let result = JyroArray()
            for i = 1 to arr.Length - 1 do
                result.Add(arr.Items.[i])
            result :> JyroValue

    type RemoveLastFunction() =
        inherit JyroFunctionBase("RemoveLast", FunctionSignatures.unary "RemoveLast" ArrayParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let result = JyroArray()
            for i = 0 to arr.Length - 2 do
                result.Add(arr.Items.[i])
            result :> JyroValue

    type FlattenOnceFunction() =
        inherit JyroFunctionBase("FlattenOnce", FunctionSignatures.unary "FlattenOnce" ArrayParam ArrayParam)
        override this.ExecuteImpl(args, _) =
            let input = this.GetArrayArgument(args, 0)
            let result = JyroArray()
            for item in input.Items do
                match item with
                | :? JyroArray as arr ->
                    for inner in arr.Items do
                        result.Add(inner)
                | v when not v.IsNull ->
                    result.Add(v)
                | _ -> ()
            result :> JyroValue

    type RandomChoiceFunction() =
        inherit JyroFunctionBase("RandomChoice", FunctionSignatures.unary "RandomChoice" ArrayParam AnyParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            if arr.Length = 0 then
                JyroNull.Instance :> JyroValue
            else
                let idx = RandomNumberGenerator.GetInt32(0, arr.Length)
                arr.Items.[idx]

    type SortByFieldFunction() =
        inherit JyroFunctionBase("SortByField",
            FunctionSignatures.create "SortByField"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fieldName", StringParam)
                  Parameter.Required("direction", StringParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fieldName = this.GetStringArgument(args, 1)
            let direction = this.GetStringArgument(args, 2)
            let isDesc = String.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase)
            let items = ResizeArray<JyroValue>()
            for i = 0 to arr.Length - 1 do
                items.Add(arr.Items.[i])
            items.Sort(fun a b ->
                match a, b with
                | (:? JyroObject as aObj), (:? JyroObject as bObj) ->
                    let aVal = aObj.GetProperty(fieldName)
                    let bVal = bObj.GetProperty(fieldName)
                    let cmp =
                        match aVal, bVal with
                        | (:? JyroNumber as an), (:? JyroNumber as bn) -> an.Value.CompareTo(bn.Value)
                        | (:? JyroString as aStr), (:? JyroString as bStr) -> String.Compare(aStr.Value, bStr.Value, StringComparison.Ordinal)
                        | _ -> 0
                    if isDesc then -cmp else cmp
                | _ -> 0)
            let result = JyroArray()
            for item in items do
                result.Add(item)
            result :> JyroValue

    type GroupByFunction() =
        inherit JyroFunctionBase("GroupBy",
            FunctionSignatures.create "GroupBy"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fieldName", StringParam) ]
                ObjectParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fieldName = this.GetStringArgument(args, 1)
            let result = JyroObject()
            for item in arr.Items do
                match item with
                | :? JyroObject as obj ->
                    let fieldValue = obj.GetProperty(fieldName)
                    let groupKey = if fieldValue.IsNull then "null" else fieldValue.ToStringValue()
                    let existingGroup = result.GetProperty(groupKey)
                    let groupArray =
                        match existingGroup with
                        | :? JyroArray as existing -> existing
                        | _ ->
                            let newArr = JyroArray()
                            result.SetProperty(groupKey, newArr)
                            newArr
                    groupArray.Add(item)
                | _ -> ()
            result :> JyroValue

    type SelectManyFunction() =
        inherit JyroFunctionBase("SelectMany",
            FunctionSignatures.create "SelectMany"
                [ Parameter.Required("array", ArrayParam)
                  Parameter.Required("fieldName", StringParam) ]
                ArrayParam)
        override this.ExecuteImpl(args, _) =
            let arr = this.GetArrayArgument(args, 0)
            let fieldName = this.GetStringArgument(args, 1)
            let result = JyroArray()
            for item in arr.Items do
                match item with
                | :? JyroObject as obj ->
                    let fieldValue = obj.GetProperty(fieldName)
                    match fieldValue with
                    | :? JyroArray as nested ->
                        for nestedItem in nested.Items do
                            result.Add(nestedItem)
                    | _ -> ()
                | _ -> ()
            result :> JyroValue

    /// Get all array functions
    let getAll () : IJyroFunction list =
        [ LengthFunction()
          AppendFunction()
          PrependFunction()
          FirstFunction()
          LastFunction()
          IndexOfFunction()
          ReverseFunction()
          SliceFunction()
          ConcatenateFunction()
          DistinctFunction()
          SortFunction()
          FlattenFunction()
          RangeFunction()
          SkipFunction()
          InsertFunction()
          RemoveAtFunction()
          RemoveFirstFunction()
          RemoveLastFunction()
          FlattenOnceFunction()
          RandomChoiceFunction()
          SortByFieldFunction()
          GroupByFunction()
          SelectManyFunction() ]
