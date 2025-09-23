using Mesch.Jyro.CliInterpreter;

/// <summary>
/// Entry point for the Jyro console application.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code: 0 for success, non-zero for failure.</returns>
    private static async Task<int> Main(string[] args)
    {
        var commandFactory = new JyroCommandFactory();
        var rootCommand = commandFactory.CreateRootCommand();
        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }
}