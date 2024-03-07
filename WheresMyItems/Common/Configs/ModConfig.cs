using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using WheresMyItems.Common.Enums;

namespace WheresMyItems.Common.Configs {
  public class ModConfig {
    public KeybindList ToggleButton { get; set; } = new(SButton.F5);
    public ChestHighlightMethod ChestHighlightMethod { get; set; } = ChestHighlightMethod.TypingRipple;
    // public bool DrawItemsOverChests { get; set; } = true;
    public ItemDisplayStyle ItemDisplayStyle { get; set; } = ItemDisplayStyle.Vertical;
    // public bool LimitMaxItemsDrawnOverChests { get; set; } = true;
    public int MaxItemsDrawnOverChests { get; set; } = 3;
    public GuideArrowOption GuideArrowOption { get; set; } = GuideArrowOption.WhileMenuOpen;
  }
}