using ActivateSprinklers.Common.Enums;

namespace ActivateSprinklers.Common.Configs {
  /// <summary>The mod config model.</summary>
  public class ModConfig {
    /// <summary>Whether sprinklers can be activated from any range.</summary>
    public bool InfiniteReach { get; set; }

    /// <summary>Which sprinkler animation to use.</summary>
    public SprinklerAnimation SprinklerAnimation { get; set; } = SprinklerAnimation.WateringCanAnimation;

    /// <summary>Which adjacent tiles to search for sprinklers.</summary>
    public AdjacentTileDirection AdjacentTileDirection { get; set; } = AdjacentTileDirection.LeftRight;
  }
}