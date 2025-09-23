namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Defines thresholds for categorizing code complexity levels.
/// </summary>
public sealed class ComplexityThresholds
{
    /// <summary>
    /// Initializes with industry-standard default thresholds.
    /// </summary>
    public ComplexityThresholds()
    {
        LowComplexityThreshold = 5;
        ModerateComplexityThreshold = 10;
        HighComplexityThreshold = 15;
        LowNestingThreshold = 3;
        ModerateNestingThreshold = 5;
        HighNestingThreshold = 7;
    }

    public int LowComplexityThreshold { get; set; }
    public int ModerateComplexityThreshold { get; set; }
    public int HighComplexityThreshold { get; set; }
    public int LowNestingThreshold { get; set; }
    public int ModerateNestingThreshold { get; set; }
    public int HighNestingThreshold { get; set; }
}