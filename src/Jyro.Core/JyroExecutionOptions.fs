namespace Mesch.Jyro

open System

/// Configuration for execution resource limits
type JyroExecutionOptions =
    { /// Maximum execution time (default: 5 seconds)
      MaxExecutionTime: TimeSpan
      /// Maximum statement count (default: 10,000)
      MaxStatements: int
      /// Maximum cumulative loop iterations (default: 1,000)
      MaxLoopIterations: int
      /// Maximum call stack depth (default: 64)
      MaxCallDepth: int }

    static member Default =
        { MaxExecutionTime = TimeSpan.FromSeconds(5.0)
          MaxStatements = 10_000
          MaxLoopIterations = 1_000
          MaxCallDepth = 64 }

    static member Unlimited =
        { MaxExecutionTime = TimeSpan.MaxValue
          MaxStatements = Int32.MaxValue
          MaxLoopIterations = Int32.MaxValue
          MaxCallDepth = Int32.MaxValue }
