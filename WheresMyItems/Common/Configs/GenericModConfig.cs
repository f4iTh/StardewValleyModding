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

      genericModConfig.AddKeybindList(
        this._modManifest,
        () => this._getConfig().ToggleButton,
        value => this._getConfig().ToggleButton = value,
        I18n.Config_Searchmenu_Title,
        I18n.Config_Searchmenu_Tooltip
      );

      genericModConfig.AddTextOption(
        this._modManifest,
        () => this._getConfig().ChestHighlightMethod.ToString(),
        value => this._getConfig().ChestHighlightMethod = (HighlightMethod)Enum.Parse(typeof(HighlightMethod), value),
        I18n.Config_Highlightchests_Name,
        I18n.Config_Highlightchests_Tooltip,
        Enum.GetNames(typeof(HighlightMethod)),
        GenericModConfig.TranslateHighlightMethod
      );

      genericModConfig.AddBoolOption(
        this._modManifest,
        () => this._getConfig().DrawItemsOverChests,
        value => this._getConfig().DrawItemsOverChests = value,
        I18n.Config_Drawitemsoverchests_Name,
        I18n.Config_Drawitemsoverthests_Tooltip
      );

      genericModConfig.AddBoolOption(
        this._modManifest,
        () => this._getConfig().LimitMaxItemsDrawnOverChests,
        value => this._getConfig().LimitMaxItemsDrawnOverChests = value,
        I18n.Config_Limitdrawnitemsoverchests_Name,
        I18n.Config_Limitdrawnitemsoverthests_Tooltip
      );

      genericModConfig.AddNumberOption(
        this._modManifest,
        () => this._getConfig().MaxItemsDrawnOverChests,
        value => this._getConfig().MaxItemsDrawnOverChests = value,
        I18n.Config_Maxitemsoverchests_Name,
        I18n.Config_Maxitemsoverthests_Tooltip,
        1,
        36,
        1
      );

      genericModConfig.AddTextOption(
        this._modManifest,
        () => this._getConfig().ItemDrawDirection.ToString(),
        value => this._getConfig().ItemDrawDirection = (ItemDrawDirection)Enum.Parse(typeof(ItemDrawDirection), value),
        I18n.Config_Drawdirection_Name,
        I18n.Config_Drawdirection_Tooltip,
        Enum.GetNames(typeof(ItemDrawDirection)),
        GenericModConfig.TranslateDrawDirection
      );

      genericModConfig.AddTextOption(
        this._modManifest,
        () => this._getConfig().GuideArrowOption.ToString(),
        value => this._getConfig().GuideArrowOption = (GuideArrowOptions)Enum.Parse(typeof(GuideArrowOptions), value),
        I18n.Config_Displayguidearrows_Name,
        I18n.Config_Displayguidearrows_Tooltip,
        Enum.GetNames(typeof(GuideArrowOptions)),
        GenericModConfig.TranslateGuideArrowOption
      );
    }

    private static string TranslateHighlightMethod(string highlightMethodString) {
      if (!Enum.TryParse(highlightMethodString, out HighlightMethod highlightMethod))
        return highlightMethodString;

      return highlightMethod switch {
        HighlightMethod.None => I18n.Config_Highlightchests_Values_None(),
        HighlightMethod.TypingRipple => I18n.Config_Highlightchests_Values_Ripple(),
        HighlightMethod.PulsatingChest => I18n.Config_Highlightchests_Values_Pulsating(),
        _ => highlightMethod.ToString()
      };
    }

    private static string TranslateDrawDirection(string drawDirectionString) {
      if (!Enum.TryParse(drawDirectionString, out ItemDrawDirection drawDirection))
        return drawDirectionString;

      return drawDirection switch {
        ItemDrawDirection.Horizontal => I18n.Config_Drawdirection_Values_Horizontal(),
        ItemDrawDirection.Vertical => I18n.Config_Drawdirection_Values_Vertical(),
        // ItemDrawDirection.GridHorizontal => I18n.Config_Drawdirection_Values_Gridhorizontal(),
        _ => drawDirection.ToString()
      };
    }

    private static string TranslateGuideArrowOption(string guideArrowOptionString) {
      if (!Enum.TryParse(guideArrowOptionString, out GuideArrowOptions guideArrowOption))
        return guideArrowOptionString;

      return guideArrowOption switch {
        GuideArrowOptions.None => I18n.Config_Displayguidearrows_Values_None(),
        GuideArrowOptions.WhileMenuOpen => I18n.Config_Displayguidearrows_Values_Whilemenuopen(),
        GuideArrowOptions.UntilNextMenu => I18n.Config_Displayguidearrows_Values_Untilnextmenu(),
        _ => guideArrowOption.ToString()
      };
    }
  }
}