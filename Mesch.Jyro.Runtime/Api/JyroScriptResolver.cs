namespace Mesch.Jyro;

/// <summary>
/// Represents a delegate that resolves Jyro script source text for a given script name.
/// This enables dynamic script loading and modular script organization by allowing
/// the host application to provide script content on demand during execution.
/// </summary>
/// <param name="name">
/// The name or identifier of the script to resolve. This could be a filename,
/// module name, or any other identifier meaningful to the host application.
/// </param>
/// <returns>
/// The source text of the requested script, or null if the script cannot be found
/// or resolved. Returning null will typically result in a runtime error during execution.
/// </returns>
/// <remarks>
/// This delegate is typically used in scenarios where scripts need to import or include
/// other scripts dynamically. The host application can implement this delegate to load
/// scripts from files, databases, web services, or any other source as needed.
/// 
/// <para>
/// Example implementations might resolve scripts from:
/// - Local file system based on filename
/// - Embedded resources in assemblies
/// - Remote repositories or web services
/// - Database storage systems
/// - In-memory caches or registries
/// </para>
/// </remarks>
public delegate string? JyroScriptResolver(string name);