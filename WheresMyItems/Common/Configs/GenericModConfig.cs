using System;
using ModCommon.Api;
using ModCommon.Api.GenericModConfigMenu;
using StardewModdingAPI;
using WheresMyItems.Common.Enums;

namespace WheresMyItems.Common.Configs {
  public class GenericModConfig {
    private readonly IGenericModConfigMenuApi _configMenu;
    private readonly Func<ModConfig> _getConfig;

    private readonly IManifest _modManifest;
    private readonly Action _reset;
    private readonly Action _save;

    public GenericModConfig(IModRegistry modRegistry, IManifest modManifest, Func<ModConfig> getConfig, Action reset, Action save) {
      this._configMenu = modRegistry.GetApi<IGenericModConfigMenuApi>(UniqueModIds.GENERIC_MOD_CONFIG_MENU);

      this._modManifest = modManifest;
      this._getConfig = getConfig;
      this._reset = reset;
      this._save = save;
    }

    public void Register() {
      IGenericModConfigMenuApi genericModConfig = this._configMenu;
      if (genericModConfig == null)
        return;

      genericModConfig.Register(
        this._modManifest,
        this._reset,
        this._save
      );
      
      genericModConfig.AddSectionTitle(
        this._modManifest,
        I18n.Config_Section_General
      );

      genericModConfig.AddKeybindList(
        this._modManifest,
        () => this._getConfig().ToggleButton,
        value => this._getConfig().ToggleButton = value,
        I18n.Config_Searchmenu_Name,
        I18n.Config_Searchmenu_Tooltip
      );
      
      genericModConfig.AddSectionTitle(
        this._modManifest,
        I18n.Config_Section_Highlighting
      );

      genericModConfig.AddTextOption(
        this._modManifest,
        () => this._getConfig().ChestHighlightMethod.ToString(),
        value => this._getConfig().ChestHighlightMethod = (ChestHighlightMethod)Enum.Parse(typeof(ChestHighlightMethod), value),
        I18n.Config_Highlightchests_Name,
        I18n.Config_Highlightchests_Tooltip,
        Enum.GetNames(typeof(ChestHighlightMethod)),
        GenericModConfig.TranslateHighlightMethod
      );

      genericModConfig.AddTextOption(
        this._modManifest,
        () => this._getConfig().ItemDisplayStyle.ToString(),
        value => this._getConfig().ItemDisplayStyle = (ItemDisplayStyle)Enum.Parse(typeof(ItemDisplayStyle), value),
        I18n.Config_Drawdirection_Name,
        I18n.Config_Drawdirection_Tooltip,
        Enum.GetNames(typeof(ItemDisplayStyle)),
        GenericModConfig.TranslateDrawDirection
      );

      genericModConfig.AddNumberOption(
        this._modManifest,
        () => this._getConfig().MaxItemsDrawnOverChests,
        value => this._getConfig().MaxItemsDrawnOverChests = value,
        I18n.Config_Maxitemsoverchests_Name,
        I18n.Config_Maxitemsoverthests_Tooltip,
        -1,
        36,
        1
      );
      
      genericModConfig.AddTextOption(
        this._modManifest,
        () => this._getConfig().GuideArrowOption.ToString(),
        value => this._getConfig().GuideArrowOption = (GuideArrowOption)Enum.Parse(typeof(GuideArrowOption), value),
        I18n.Config_Displayguidearrows_Name,
        I18n.Config_Displayguidearrows_Tooltip,
        Enum.GetNames(typeof(GuideArrowOption)),
        GenericModConfig.TranslateGuideArrowOption
      );
    }

    private static string TranslateHighlightMethod(string highlightMethodString) {
      if (!Enum.TryParse(highlightMethodString, out ChestHighlightMethod highlightMethod))
        return highlightMethodString;

      return highlightMethod switch {
        ChestHighlightMethod.None => I18n.Strings_Common_None(),
        ChestHighlightMethod.TypingRipple => I18n.Config_Highlightchests_Values_Ripple(),
        ChestHighlightMethod.PulsatingChest => I18n.Config_Highlightchests_Values_Pulsating(),
        _ => highlightMethod.ToString()
      };
    }

    private static string TranslateDrawDirection(string drawDirectionString) {
      if (!Enum.TryParse(drawDirectionString, out ItemDisplayStyle drawDirection))
        return drawDirectionString;

      return drawDirection switch {
        ItemDisplayStyle.None => I18n.Strings_Common_None(),
        ItemDisplayStyle.Horizontal => I18n.Config_Drawdirection_Values_Horizontal(),
        ItemDisplayStyle.Vertical => I18n.Config_Drawdirection_Values_Vertical(),
        // ItemDrawDirection.GridHorizontal => I18n.Config_Drawdirection_Values_Gridhorizontal(),
        _ => drawDirection.ToString()
      };
    }

    private static string TranslateGuideArrowOption(string guideArrowOptionString) {
      if (!Enum.TryParse(guideArrowOptionString, out GuideArrowOption guideArrowOption))
        return guideArrowOptionString;

      return guideArrowOption switch {
        GuideArrowOption.None => I18n.Strings_Common_None(),
        GuideArrowOption.WhileMenuOpen => I18n.Config_Displayguidearrows_Values_Whilemenuopen(),
        GuideArrowOption.UntilNextMenu => I18n.Config_Displayguidearrows_Values_Untilnextmenu(),
        _ => guideArrowOption.ToString()
      };
    }
  }
}