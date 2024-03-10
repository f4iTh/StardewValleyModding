using System;
using CustomWarps.Common.Enums;
using ModCommon.Api;
using ModCommon.Api.GenericModConfigMenu;
using StardewModdingAPI;

namespace CustomWarps.Common.Configs {
  /// <summary>
  /// A class for handling Generic Mod Config Menu integration.
  /// </summary>
  public class GenericModConfig {
    /// <summary>
    /// Generic Mod Config Menu integration.
    /// </summary>
    private readonly IGenericModConfigMenuApi _configMenu;
    /// <summary>
    /// The function to get the current config.
    /// </summary>
    private readonly Func<ModConfig> _getConfig;
    /// <summary>
    /// The mod manifest.
    /// </summary>
    private readonly IManifest _modManifest;
    /// <summary>
    /// Reset to default values.
    /// </summary>
    private readonly Action _reset;
    /// <summary>
    /// Save and apply changes.
    /// </summary>
    private readonly Action _save;

    /// <summary>
    /// The constructor for creating <see cref="IGenericModConfigMenuApi"/> integration.
    /// </summary>
    /// <param name="modRegistry">The mod registry.</param>
    /// <param name="modManifest">The mod manifest.</param>
    /// <param name="getConfig">Get the current config.</param>
    /// <param name="reset">Reset to default values.</param>
    /// <param name="save">Save and apply changes.</param>
    public GenericModConfig(IModRegistry modRegistry, IManifest modManifest, Func<ModConfig> getConfig, Action reset, Action save) {
      this._configMenu = modRegistry.GetApi<IGenericModConfigMenuApi>(UniqueModIds.GENERIC_MOD_CONFIG_MENU);

      this._modManifest = modManifest;
      this._getConfig = getConfig;
      this._reset = reset;
      this._save = save;
    }

    /// <summary>
    /// Creates the mod config menu if available.
    /// </summary>
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
        () => this._getConfig().ToggleKey,
        value => this._getConfig().ToggleKey = value,
        I18n.Config_Menutoggle_Name,
        I18n.Config_Menutoggle_Tooltip
      );
      
      genericModConfig.AddSectionTitle(
        this._modManifest,
        I18n.Config_Section_Customization
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

    /// <summary>
    /// Translates the <see cref="MenuStyle"/> text.
    /// </summary>
    /// <param name="menuStyleString">A string representation of <see cref="MenuStyle"/></param>
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