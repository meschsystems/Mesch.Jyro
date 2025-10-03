namespace Mesch.Jyro;

/// <summary>
/// Defines the supported types for function parameters and return values
/// within the Jyro type system. Used for compile-time type checking
/// and runtime value validation.
/// </summary>
public enum ParameterType
{
    /// <summary>
    /// Indicates that any Jyro value type is acceptable.
    /// Used for parameters or return values that do not require type constraints.
    /// </summary>
    Any,

    /// <summary>
    /// Indicates that the value must be a numeric type (see <see cref="JyroNumber"/>).
    /// </summary>
    Number,

    /// <summary>
    /// Indicates that the value must be a string type (see <see cref="JyroString"/>).
    /// </summary>
    String,

    /// <summary>
    /// Indicates that the value must be a boolean type (see <see cref="JyroBoolean"/>).
    /// </summary>
    Boolean,

    /// <summary>
    /// Indicates that the value must be an object type (see <see cref="JyroObject"/>).
    /// </summary>
    Object,

    /// <summary>
    /// Indicates that the value must be an array type (see <see cref="JyroArray"/>).
    /// </summary>
    Array,

    /// <summary>
    /// Indicates that the value must be a null type (see <see cref="JyroNull"/>).
    /// </summary>
    Null
}