using CustomWarps.Common.Enums;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CustomWarps.Common.Configs {
  /// <summary>The mod configuration.</summary>
  public class ModConfig {
    /// <summary>The keybind to use to open the menu.</summary>
    public KeybindList ToggleKey { get; set; } = new(SButton.F7);

    /// <summary>Whether to close the menu after warping.</summary>
    public bool CloseMenuOnWarp { get; set; } = true;

    /// <summary>The menu style.</summary>
    public MenuStyle MenuStyle { get; set; } = MenuStyle.VerticalList;

    /// <summary>How many rows should be visible on one page when using <see cref="MenuStyle.LegacyGrid" />.</summary>
    public int MaxGridRows { get; set; } = 3;
    
    // /// <summary>How many columns should be visible on one page when using <see cref="MenuStyle.LegacyGrid" />.</summary>
    // public int MaxGridColumns { get; set; } = 8;
  }
}