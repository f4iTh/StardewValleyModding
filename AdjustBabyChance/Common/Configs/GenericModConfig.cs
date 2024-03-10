using System;
using ModCommon.Api;
using ModCommon.Api.GenericModConfigMenu;
using StardewModdingAPI;

namespace AdjustBabyChance.Common.Configs {
  /// <summary>A class for handling Generic Mod Config Menu integration.</summary>
  public class GenericModConfig {
    /// <summary>Generic Mod Config Menu integration.</summary>
    private readonly IGenericModConfigMenuApi _configMenu;

    /// <summary>The function to get the current config.</summary>
    private readonly Func<ModConfig> _getConfig;

    /// <summary>The mod manifest.</summary>
    private readonly IManifest _modManifest;

    /// <summary>Reset to default values.</summary>
    private readonly Action _reset;

    /// <summary>Save and apply changes.</summary>
    private readonly Action _save;

    /// <summary>The constructor for creating <see cref="IGenericModConfigMenuApi" /> integration.</summary>
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

    /// <summary>Creates the mod config menu if available.</summary>
    public void Register() {
      IGenericModConfigMenuApi genericModConfig = this._configMenu;
      if (genericModConfig == null)
        return;

      genericModConfig.Register(
        this._modManifest,
        this._reset,
        this._save
      );

      genericModConfig.AddNumberOption(
        this._modManifest,
        () => this._getConfig().QuestionChance,
        value => this._getConfig().QuestionChance = value,
        I18n.Config_Questionchance_Name,
        I18n.Config_Questionchance_Tooltip,
        0f,
        1f
      );
    }
  }
}