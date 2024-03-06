using CustomWarps.Common.Enums;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CustomWarps.Common.Configs {
  public class ModConfig {
    public KeybindList ToggleKey { get; set; } = new(SButton.F7);

    public MenuStyle MenuStyle { get; set; } = MenuStyle.LegacyGrid;

    // public int MaxItemsPerGridRow { get; set; } = 8;
    public int MaxItemsPerGridColumn { get; set; } = 3;
  }
}