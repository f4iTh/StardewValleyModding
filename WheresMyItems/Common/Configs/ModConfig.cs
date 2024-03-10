using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using WheresMyItems.Common.Enums;

namespace WheresMyItems.Common.Configs {
  /// <summary>
  /// The mod configuration.
  /// </summary>
  public class ModConfig {
    /// <summary>
    /// The keybind to use to open the menu.
    /// </summary>
    public KeybindList ToggleButton { get; set; } = new(SButton.F5);
    /// <summary>
    /// Which method to use to highlight chests.
    /// </summary>
    public ChestHighlightMethod ChestHighlightMethod { get; set; } = ChestHighlightMethod.TypingRipple;
    /// <summary>
    /// Which style to use to highlight items over chests.
    /// </summary>
    public ItemDisplayStyle ItemDisplayStyle { get; set; } = ItemDisplayStyle.Vertical;
    /// <summary>
    /// How many items can be drawn at once (per chest).
    /// </summary>
    public int MaxItemsDrawnOverChests { get; set; } = 3;
    /// <summary>
    /// Which guide arrow option to use.
    /// </summary>
    public GuideArrowOption GuideArrowOption { get; set; } = GuideArrowOption.WhileMenuOpen;
    // public bool DrawItemsOverChests { get; set; } = true;
    // public bool LimitMaxItemsDrawnOverChests { get; set; } = true;
  }
}