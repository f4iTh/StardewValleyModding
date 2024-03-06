using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using WheresMyItems.Common.Enums;

namespace WheresMyItems.Common.Configs {
  public class ModConfig {
    public KeybindList ToggleButton { get; set; } = new(SButton.F5);
    public HighlightMethod ChestHighlightMethod { get; set; } = HighlightMethod.TypingRipple;
    public bool DrawItemsOverChests { get; set; } = true;
    public bool LimitMaxItemsDrawnOverChests { get; set; } = true;
    public int MaxItemsDrawnOverChests { get; set; } = 3;
    public ItemDrawDirection ItemDrawDirection { get; set; } = ItemDrawDirection.Vertical;
    public GuideArrowOptions GuideArrowOption { get; set; } = GuideArrowOptions.WhileMenuOpen;
  }
}