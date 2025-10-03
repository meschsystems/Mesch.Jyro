using System.Diagnostics;

namespace Mesch.Jyro;

/// <summary>
/// Enforces runtime resource quotas and execution limits to protect against runaway scripts
/// and ensure system stability during Jyro script execution. This limiter tracks and enforces
/// limits on statement execution, loop iterations, call depth, and wall-clock execution time.
/// </summary>
/// <remarks>
/// The resource limiter serves as a critical security and stability component by preventing
/// scripts from consuming excessive system resources through infinite loops, recursive calls,
/// or long-running operations. It operates through active monitoring and immediate termination
/// when any configured limit is exceeded.
/// 
/// <para><strong>Protected Resources:</strong></para>
/// <list type="bullet">
/// <item><description>Statement execution count: Prevents scripts with excessive complexity</description></item>
/// <item><description>Loop iteration count: Guards against infinite or excessive loops</description></item>
/// <item><description>Call stack depth: Prevents stack overflow from deep recursion</description></item>
/// <item><description>Execution time: Enforces maximum wall-clock time limits</description></item>
/// </list>
/// 
/// <para><strong>Integration:</strong></para>
/// The limiter is designed to be called at strategic points during script execution by the
/// interpreter or executor, providing fine-grained control over resource consumption without
/// requiring external process monitoring or termination.
/// </remarks>
public sealed class JyroResourceLimiter
{
    private readonly int _maxStatementCount;
    private readonly int _maxLoopIterations;
    private readonly int _maxCallStackDepth;
    private readonly int _maxScriptCallDepth;
    private readonly TimeSpan _maxExecutionTime;
    private readonly Stopwatch _executionTimer;

    private int _currentStatementCount;
    private int _currentLoopIterations;
    private int _currentCallStackDepth;
    private int _currentScriptCallDepth;

    /// <summary>
    /// Initializes a new instance of the <see cref="JyroResourceLimiter"/> class with the specified execution options.
    /// The limiter begins timing immediately upon construction to track total execution time.
    /// </summary>
    /// <param name="executionOptions">
    /// The execution options containing the resource limits to enforce.
    /// These limits define the maximum allowed resource consumption for the script execution.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when executionOptions is null.</exception>
    public JyroResourceLimiter(JyroExecutionOptions executionOptions)
    {
        ArgumentNullException.ThrowIfNull(executionOptions);

        _maxStatementCount = executionOptions.MaxStatements;
        _maxLoopIterations = executionOptions.MaxLoops;
        _maxCallStackDepth = executionOptions.MaxCallDepth;
        _maxScriptCallDepth = executionOptions.MaxScriptCallDepth;
        _maxExecutionTime = executionOptions.MaxExecutionTime;
        _executionTimer = Stopwatch.StartNew();
    }

    /// <summary>
    /// Checks and increments the statement execution counter, enforcing the maximum statement limit.
    /// This method should be called before executing each statement in the script to ensure
    /// compliance with the configured statement execution limit.
    /// </summary>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the statement count exceeds the configured maximum or when execution time is exceeded.
    /// </exception>
    public void CheckAndCountStatement()
    {
        _currentStatementCount++;
        if (_currentStatementCount > _maxStatementCount)
        {
            throw new JyroRuntimeException($"Script execution exceeded maximum statement limit of {_maxStatementCount}");
        }

        CheckExecutionTime();
    }

    /// <summary>
    /// Checks and increments the loop iteration counter when entering a loop iteration.
    /// This method should be called at the beginning of each loop iteration to track
    /// total loop activity across all active loops in the script.
    /// </summary>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the loop iteration count exceeds the configured maximum or when execution time is exceeded.
    /// </exception>
    public void CheckAndEnterLoop()
    {
        _currentLoopIterations++;
        if (_currentLoopIterations > _maxLoopIterations)
        {
            throw new JyroRuntimeException($"Script execution exceeded maximum loop iteration limit of {_maxLoopIterations}");
        }

        CheckExecutionTime();
    }

    /// <summary>
    /// Decrements the loop iteration counter when exiting a loop iteration.
    /// This method maintains accurate tracking of active loop iterations and should
    /// be called when a loop iteration completes normally.
    /// </summary>
    /// <remarks>
    /// This method does not perform limit checking as it represents resource release
    /// rather than resource consumption. It maintains the accuracy of the loop
    /// iteration counter for subsequent limit checks.
    /// </remarks>
    public void ExitLoop() => _currentLoopIterations--;

    /// <summary>
    /// Checks and increments the call stack depth when entering a function or method call.
    /// This method should be called at the beginning of each function invocation to prevent
    /// stack overflow conditions from excessive recursion or deeply nested calls.
    /// </summary>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the call depth exceeds the configured maximum or when execution time is exceeded.
    /// </exception>
    public void CheckAndEnterCall()
    {
        _currentCallStackDepth++;
        if (_currentCallStackDepth > _maxCallStackDepth)
        {
            throw new JyroRuntimeException($"Script execution exceeded maximum call depth limit of {_maxCallStackDepth}");
        }

        CheckExecutionTime();
    }

    /// <summary>
    /// Decrements the call stack depth when exiting a function or method call.
    /// This method maintains accurate tracking of the call stack depth and should
    /// be called when a function call completes normally or due to an early return.
    /// </summary>
    /// <remarks>
    /// This method does not perform limit checking as it represents resource release.
    /// It maintains the accuracy of the call depth counter for subsequent limit checks.
    /// </remarks>
    public void ExitCall() => _currentCallStackDepth--;

    /// <summary>
    /// Checks and increments the script chain depth when entering a function or method call.
    /// This method should be called at the beginning of each script invocation to prevent
    /// stack overflow conditions from excessive chaining.
    /// </summary>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the script chain depth exceeds the configured maximum or when execution time is exceeded.
    /// </exception>
    public void CheckAndEnterScriptCall()
    {
        _currentScriptCallDepth++;
        if (_currentScriptCallDepth > _maxScriptCallDepth)
        {
            throw new JyroRuntimeException($"Script execution exceeded maximum script call depth limit of {_maxScriptCallDepth}");
        }
        CheckExecutionTime();
    }

    /// <summary>
    /// Decrements the script call chain depth when exiting a script call.
    /// This method maintains accurate tracking of the script chain depth and should
    /// be called when a script call completes normally or due to an early return.
    /// </summary>
    /// <remarks>
    /// This method does not perform limit checking as it represents resource release.
    /// It maintains the accuracy of the script chain depth counter for subsequent limit checks.
    /// </remarks>
    public void ExitScriptCall() => _currentScriptCallDepth--;

    /// <summary>
    /// Checks the current execution time against the configured maximum execution time limit.
    /// This method can be called independently or is automatically invoked by other limit
    /// checking methods to ensure consistent time-based protection.
    /// </summary>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the elapsed execution time exceeds the configured maximum execution time.
    /// </exception>
    public void CheckExecutionTime()
    {
        if (_executionTimer.Elapsed > _maxExecutionTime)
        {
            throw new JyroRuntimeException($"Script execution exceeded maximum time limit of {_maxExecutionTime.TotalMilliseconds:F0}ms");
        }
    }

    /// <summary>
    /// Gets the current statement execution count for monitoring and diagnostic purposes.
    /// </summary>
    public int CurrentStatementCount => _currentStatementCount;

    /// <summary>
    /// Gets the current loop iteration count for monitoring and diagnostic purposes.
    /// </summary>
    public int CurrentLoopIterations => _currentLoopIterations;

    /// <summary>
    /// Gets the current call stack depth for monitoring and diagnostic purposes.
    /// </summary>
    public int CurrentCallStackDepth => _currentCallStackDepth;

    /// <summary>
    /// Gets the elapsed execution time for monitoring and diagnostic purposes.
    /// </summary>
    public TimeSpan ElapsedExecutionTime => _executionTimer.Elapsed;
}