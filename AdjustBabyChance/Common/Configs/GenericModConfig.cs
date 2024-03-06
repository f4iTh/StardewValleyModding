using System;
using ModCommon.Api;
using ModCommon.Api.GenericModConfigMenu;
using StardewModdingAPI;

namespace AdjustBabyChance.Common.Configs {
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

      genericModConfig.AddNumberOption(
        this._modManifest,
        () => this._getConfig().QuestionChance,
        value => this._getConfig().QuestionChance = value,
        I18n.Config_Questionchance_Name,
        () => I18n.Config_Questionchance_Tooltip("value"),
        0f,
        1f
      );
    }
  }
}