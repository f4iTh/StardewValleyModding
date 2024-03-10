using CustomWarps.Common.Enums;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CustomWarps.Common.Configs {
  /// <summary>
  /// The mod configuration.
  /// </summary>
  public class ModConfig {
    /// <summary>
    /// The keybind to use to open the menu.
    /// </summary>
    public KeybindList ToggleKey { get; set; } = new(SButton.F7);
    /// <summary>
    /// The menu style.
    /// </summary>
    public MenuStyle MenuStyle { get; set; } = MenuStyle.VerticalList;
    /// <summary>
    /// How many columns should be visible when using <see cref="MenuStyle.LegacyGrid"/>.
    /// </summary>
    public int MaxGridColumns { get; set; } = 3;
    // public int MaxItemsPerGridRow { get; set; } = 8;
  }
}