namespace BreedLikeRabbits2.Common.Configs {
  public class ModConfig {
    public bool NameNewRabbits { get; set; } = true;
    public bool IgnoreGender { get; set; }
    // public bool CheckGendersPerCoop { get; set; } = true;
    // public bool CheckBucksPerCoop { get; set; } = true;
    public bool AccountForHappiness { get; set; } = true;
    public bool AccountForFriendship { get; set; } = true;
    public bool AccountForSeason { get; set; } = true;
    public bool AccountForAge { get; set; } = true;
    public bool AllowMultiples { get; set; } = true;
    public float BaseRate { get; set; } = 42.0f;
  }
}