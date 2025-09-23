namespace Mesch.Jyro;

/// <summary>
/// Manages a stack of variable scopes for Jyro program execution, supporting
/// both block-scoped and file-scoped variable declarations with proper shadowing
/// semantics. The innermost scope takes precedence for variable lookups and assignments.
/// </summary>
public sealed class VariableScopes
{
    private readonly Stack<Dictionary<string, JyroValue>> _scopeStack = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableScopes"/> class
    /// with a single global scope dictionary for file-level variable storage.
    /// </summary>
    public VariableScopes()
    {
        _scopeStack.Push(new Dictionary<string, JyroValue>(StringComparer.Ordinal));
    }

    /// <summary>
    /// Pushes a new variable scope onto the stack, creating a new namespace
    /// for block-scoped variable declarations.
    /// </summary>
    public void PushScope()
    {
        _scopeStack.Push(new Dictionary<string, JyroValue>(StringComparer.Ordinal));
    }

    /// <summary>
    /// Pops the innermost variable scope from the stack, returning to the
    /// previous scope level. The global scope cannot be popped.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to pop the global scope.
    /// </exception>
    public void PopScope()
    {
        if (_scopeStack.Count > 1)
        {
            _scopeStack.Pop();
        }
        else
        {
            throw new InvalidOperationException("Cannot pop the global scope");
        }
    }

    /// <summary>
    /// Declares a new variable in the current scope with the specified initial value.
    /// If a variable with the same name already exists in the current scope,
    /// its value will be updated.
    /// </summary>
    /// <param name="variableName">
    /// The name of the variable to declare.
    /// </param>
    /// <param name="initialValue">
    /// The initial value to assign to the variable.
    /// </param>
    public void Declare(string variableName, JyroValue initialValue)
    {
        _scopeStack.Peek()[variableName] = initialValue;
    }

    /// <summary>
    /// Attempts to resolve the value of a variable by searching from the current
    /// scope outward through all containing scopes to the global scope.
    /// </summary>
    /// <param name="variableName">
    /// The name of the variable to resolve.
    /// </param>
    /// <param name="resolvedValue">
    /// When this method returns, contains the variable's value if found,
    /// or <see cref="JyroNull.Instance"/> if not found.
    /// </param>
    /// <returns>
    /// <c>true</c> if the variable exists in any scope; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGet(string variableName, out JyroValue resolvedValue)
    {
        foreach (var scopeDictionary in _scopeStack)
        {
            if (scopeDictionary.TryGetValue(variableName, out var candidateValue) && candidateValue is not null)
            {
                resolvedValue = candidateValue;
                return true;
            }
        }

        resolvedValue = JyroNull.Instance;
        return false;
    }

    /// <summary>
    /// Attempts to update the value of an existing variable by searching from
    /// the current scope outward and updating the variable in the closest scope
    /// where it is defined.
    /// </summary>
    /// <param name="variableName">
    /// The name of the variable to update.
    /// </param>
    /// <param name="newValue">
    /// The new value to assign to the variable.
    /// </param>
    /// <returns>
    /// <c>true</c> if the variable was found and updated; otherwise, <c>false</c>
    /// if the variable does not exist in any scope.
    /// </returns>
    public bool TrySet(string variableName, JyroValue newValue)
    {
        foreach (var scopeDictionary in _scopeStack)
        {
            if (scopeDictionary.ContainsKey(variableName))
            {
                scopeDictionary[variableName] = newValue;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Clears all variable scopes and resets the scope stack to contain
    /// only a single empty global scope.
    /// </summary>
    public void Reset()
    {
        _scopeStack.Clear();
        _scopeStack.Push(new Dictionary<string, JyroValue>(StringComparer.Ordinal));
    }
}