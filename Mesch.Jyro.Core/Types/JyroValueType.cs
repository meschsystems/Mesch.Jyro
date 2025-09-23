namespace Mesch.Jyro;

/// <summary>
/// Defines the types of values that can exist in the Jyro runtime data model.
/// These correspond to the primitive and composite types supported by the language.
/// </summary>
public enum JyroValueType
{
    /// <summary>
    /// Represents the absence of a value or a null reference.
    /// </summary>
    Null,

    /// <summary>
    /// Represents a boolean value that can be either true or false.
    /// </summary>
    Boolean,

    /// <summary>
    /// Represents a numeric value stored as a double-precision floating-point number.
    /// This encompasses both integer and decimal representations.
    /// </summary>
    Number,

    /// <summary>
    /// Represents a sequence of Unicode characters.
    /// </summary>
    String,

    /// <summary>
    /// Represents an ordered collection of values that can be accessed by numeric index.
    /// </summary>
    Array,

    /// <summary>
    /// Represents a collection of key-value pairs where keys are strings and values are any JyroValue type.
    /// </summary>
    Object
}