namespace BreedLikeRabbits2.Common.Configs {
  /// <summary>
  /// The mod configuration.
  /// </summary>
  public class ModConfig {
    /// <summary>
    /// Whether to name new rabbits.
    /// </summary>
    public bool NameNewRabbits { get; set; } = true;
    /// <summary>
    /// Whether to ignore genders (male) in calculations.
    /// </summary>
    public bool IgnoreGender { get; set; }
    // public bool CheckGendersPerCoop { get; set; } = true;
    // public bool CheckBucksPerCoop { get; set; } = true;
    /// <summary>
    /// Whether to account for happiness.
    /// </summary>
    public bool AccountForHappiness { get; set; } = true;
    /// <summary>
    /// Whether to account for friendship.
    /// </summary>
    public bool AccountForFriendship { get; set; } = true;
    /// <summary>
    /// Whether to account for season.
    /// </summary>
    public bool AccountForSeason { get; set; } = true;
    /// <summary>
    /// Whether to account for age.
    /// </summary>
    public bool AccountForAge { get; set; } = true;
    /// <summary>
    /// Whether to allow multiple new rabbits at once.
    /// </summary>
    public bool AllowMultiples { get; set; } = true;
    /// <summary>
    /// The base fertility rate.
    /// </summary>
    public float BaseRate { get; set; } = 42.0f;
  }
}