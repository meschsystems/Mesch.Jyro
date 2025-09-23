## Working with the Data Context: Common Patterns and Solutions

Developers new to Jyro's `Data` context paradigm often encounter conceptual challenges when transitioning from traditional programming approaches. The shift from creating and returning new objects to modifying a provided context requires adjusting familiar patterns, but Jyro's design accommodates virtually all data transformation scenarios through direct manipulation of the `Data` structure.

**Building Complex Data Structures**

Traditional approaches involve constructing objects incrementally and returning the final result, but Jyro handles this by treating `Data` as a mutable workspace. Instead of `result = object { "summary": summary, "details": details }`, developers build the same structure with `Data.summary = summary` and `Data.details = details`. The `Data` context can be completely restructured during execution, allowing scripts to transform simple input into complex hierarchical output through successive property assignments and nested object construction.

**Processing Collections and Aggregating Results**

Developers accustomed to mapping operations that produce new arrays can handle transformations through direct manipulation of `Data` properties combined with iterative constructs. A script might receive `Data.rawItems` as input and produce `Data.processedItems` as output by iterating through the raw collection, transforming each item, and building the processed collection incrementally. The `foreach` construct excels at this pattern, allowing scripts to iterate through existing data while populating new structures within the same `Data` context.

**Conditional Data Transformation**

Traditional approaches often involve conditional returns of different object structures, but Jyro handles this through conditional modification of the `Data` context. Scripts can use `if` statements to selectively populate different properties or entirely restructure `Data` based on input conditions. For example, a script might populate `Data.errorMessage` and `Data.hasError = true` when validation fails, or populate `Data.result` and `Data.hasError = false` when processing succeeds, allowing the host to handle different outcomes based on the final `Data` state.

**Modularizing Complex Logic**

While Jyro scripts cannot define custom functions, the host environment can provide specialized functions that operate on `Data` directly or return values that can be assigned to `Data` properties. This approach encourages a clean separation between reusable logic (implemented as host functions) and data transformation logic (implemented in scripts), often resulting in more maintainable and testable code than deeply nested custom function hierarchies. Additionally, Jyro scripts can call other Jyro scripts, passing the same `Data` context to them, and receiving a modified `Data` context back. This allows the logical splitting of functionality and separation of concerns.

**Advanced Host Interoperability**

Jyro scripts can trigger a vast array of interop calls on the host through careful presentation of advanced functionality to the Jyro script. For example, in a trusted scenario a host function could call a REST API once a Jyro script returns, with the `Data` model specifying an endpoint and a payload. This way, ETL pipelines could be changed easily without recompiling the host. Obviously this scenario requires a trusted environment and would not be suitable for running untrusted scripts.

**Performance Optimization**

Jyro's design supports multiple approaches depending on the use case. Scripts can selectively modify specific portions of large data structures without touching unrelated sections, or they can clear existing properties and rebuild `Data` completely for scenarios requiring wholesale transformation. The key insight is that `Data` serves as both input and workspace, allowing scripts to choose the most efficient transformation strategy based on the specific requirements of each use case.

**Establishing Data Contracts**

The key to good data modelling is establishing a contract between the host and script. The contract should state what the script is allowed to see, what it is allowed to modify, what the host should process further once the `Data` is returned, and what it should ignore.