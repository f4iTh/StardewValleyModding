using System;
using CustomWarps.Common.Enums;
using ModCommon.Api;
using ModCommon.Api.GenericModConfigMenu;
using StardewModdingAPI;

namespace CustomWarps.Common.Configs {
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
        () => this._getConfig().ToggleKey,
        value => this._getConfig().ToggleKey = value,
        I18n.Config_Menutoggle_Name,
        I18n.Config_Menutoggle_Tooltip
      );

      genericModConfig.AddTextOption(
        this._modManifest,
        () => this._getConfig().MenuStyle.ToString(),
        value => this._getConfig().MenuStyle = (MenuStyle)Enum.Parse(typeof(MenuStyle), value),
        I18n.Config_Menustyle_Name,
        I18n.Config_Menustyle_Tooltip,
        Enum.GetNames(typeof(MenuStyle)),
        GenericModConfig.TranslateMenuStyle
      );

      // genericModConfig.AddNumberOption(
      //   this._modManifest,
      //   () => this._getConfig().MaxItemsPerGridRow,
      //   value => this._getConfig().MaxItemsPerGridRow = value,
      //   I18n.Config_Maxrows_Name,
      //   I18n.Config_Maxrows_Tooltip,
      //   1,
      //   10,
      //   1
      // );

      genericModConfig.AddNumberOption(
        this._modManifest,
        () => this._getConfig().MaxGridColumns,
        value => this._getConfig().MaxGridColumns = value,
        I18n.Config_Maxrows_Name,
        I18n.Config_Maxrows_Tooltip,
        1,
        8,
        1
      );
    }

    private static string TranslateMenuStyle(string menuStyleString) {
      if (!Enum.TryParse(menuStyleString, out MenuStyle menuStyle))
        return menuStyleString;

      return menuStyle switch {
        MenuStyle.LegacyGrid => I18n.Config_Menustyle_Values_Grid(),
        MenuStyle.VerticalList => I18n.Config_Menustyle_Values_Verticallist(),
        _ => menuStyle.ToString()
      };
    }
  }
}