using ActivateSprinklers.Common.Enums;

namespace ActivateSprinklers.Common.Configs {
  public class ModConfig {
    public bool InfiniteReach { get; set; }
    public SprinklerAnimation SprinklerAnimation { get; set; } = SprinklerAnimation.WateringCanAnimation;
    public AdjacentTileDirection AdjacentTileDirection { get; set; } = AdjacentTileDirection.LeftRight;
  }
}