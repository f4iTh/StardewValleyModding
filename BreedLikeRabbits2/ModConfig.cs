namespace BreedLikeRabbits2 {
  internal class ModConfig {
    public bool IgnoreGender { get; set; } = false;

    public bool AccountForHappiness { get; set; } = true;

    public bool AccountForFriendship { get; set; } = true;

    public bool AccountForSeason { get; set; } = true;

    public bool AccountForAge { get; set; } = true;

    public bool AllowMultiples { get; set; } = true;

    public bool EnableDebugLogging { get; set; } = false;

    public double BaseRate { get; set; } = 42.0;
  }
}