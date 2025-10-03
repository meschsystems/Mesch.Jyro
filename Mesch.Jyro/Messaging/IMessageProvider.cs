namespace Mesch.Jyro;

/// <summary>
/// Defines the contract for message providers that transform diagnostic messages into human-readable strings.
/// Message providers enable localization, custom formatting, and consistent presentation of diagnostic
/// information across different user interfaces and development environments.
/// </summary>
/// <remarks>
/// Message providers serve as the bridge between the structured diagnostic information produced by
/// the Jyro compilation pipeline and the formatted text presented to users. This abstraction enables:
/// 
/// <list type="bullet">
/// <item><description>Localization: Supporting multiple languages and cultural formatting preferences</description></item>
/// <item><description>Customization: Allowing different applications to format messages according to their UI requirements</description></item>
/// <item><description>Consistency: Ensuring uniform message presentation across different tools and contexts</description></item>
/// <item><description>Template management: Centralizing message templates for maintainability and updates</description></item>
/// </list>
/// 
/// Implementations can range from simple template-based formatters to sophisticated localization
/// systems that adapt messages based on user preferences, context, and target audience.
/// </remarks>
public interface IMessageProvider
{
    /// <summary>
    /// Formats a diagnostic message into a complete human-readable string suitable for display.
    /// The formatted string should include all relevant information from the message including
    /// location, severity, code, and any contextual arguments.
    /// </summary>
    /// <param name="message">
    /// The diagnostic message to format. Contains all structured information including
    /// message code, severity, location, and arguments that should be incorporated into the output.
    /// </param>
    /// <returns>
    /// A formatted string that presents the diagnostic information in a user-friendly manner.
    /// The format should be consistent with the provider's formatting strategy and may include
    /// localized text if the provider supports internationalization.
    /// </returns>
    /// <remarks>
    /// The formatted output typically includes elements such as:
    /// <list type="bullet">
    /// <item><description>Source location (line and column numbers)</description></item>
    /// <item><description>Message code for reference and tool integration</description></item>
    /// <item><description>Severity indicator for visual emphasis</description></item>
    /// <item><description>Descriptive text with context-specific arguments substituted</description></item>
    /// </list>
    /// </remarks>
    string Format(IMessage message);

    /// <summary>
    /// Retrieves the default message template for the specified diagnostic code.
    /// Templates provide the base text format that will be populated with message-specific
    /// arguments to create the final diagnostic text.
    /// </summary>
    /// <param name="code">
    /// The diagnostic code for which to retrieve the template.
    /// Each message code should have a corresponding template that defines the standard
    /// format for messages of that type.
    /// </param>
    /// <returns>
    /// The message template string with placeholder markers for argument substitution,
    /// or null if no template is defined for the specified code.
    /// Templates typically use standard string formatting placeholders like {0}, {1}, etc.
    /// </returns>
    /// <remarks>
    /// Templates enable consistent messaging and support for localization by separating
    /// the message structure from the specific content. This method is primarily used
    /// by development tools, testing frameworks, and diagnostic systems that need access
    /// to the underlying message formats.
    /// </remarks>
    string? GetTemplate(MessageCode code);
}