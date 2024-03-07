using System;
using ModCommon.Api;
using ModCommon.Api.GenericModConfigMenu;
using StardewModdingAPI;

namespace BreedLikeRabbits2.Common.Configs {
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
      
      genericModConfig.AddBoolOption(
        this._modManifest,
        () => this._getConfig().NameNewRabbits,
        value => this._getConfig().NameNewRabbits = value,
        I18n.Config_Namenewrabbits_Name,
        I18n.Config_Namenewrabbits_Tooltip
      );
      
      genericModConfig.AddSectionTitle(
        this._modManifest,
        I18n.Config_Section_Customization
      );

      genericModConfig.AddBoolOption(
        this._modManifest,
        () => this._getConfig().IgnoreGender,
        value => this._getConfig().IgnoreGender = value,
        I18n.Config_Ignoregender_Name,
        I18n.Config_Ignoregender_Tooltip
      );

      // genericModConfig.AddBoolOption(
      //   this._modManifest,
      //   () => this._getConfig().CheckGendersPerCoop,
      //   value => this._getConfig().CheckGendersPerCoop = value,
      //   I18n.Config_Checkgenderspercoop_Name,
      //   I18n.Config_Checkgenderspercoop_Name
      // );

      // genericModConfig.AddBoolOption(
      //   this._modManifest,
      //   () => this._getConfig().CheckRabbitsPerCoop,
      //   value => this._getConfig().CheckRabbitsPerCoop = value,
      //   I18n.Config_Buckcountinsamecoop_Name,
      //   I18n.Config_Buckcountinsamecoop_Tooltip
      // );

      genericModConfig.AddBoolOption(
        this._modManifest,
        () => this._getConfig().AccountForHappiness,
        value => this._getConfig().AccountForHappiness = value,
        I18n.Config_Accountforhappiness_Name,
        I18n.Config_Accountforhappiness_Tooltip
      );

      genericModConfig.AddBoolOption(
        this._modManifest,
        () => this._getConfig().AccountForFriendship,
        value => this._getConfig().AccountForFriendship = value,
        I18n.Config_Accountforfriendship_Name,
        I18n.Config_Accountforfriendship_Tooltip
      );

      genericModConfig.AddBoolOption(
        this._modManifest,
        () => this._getConfig().AccountForSeason,
        value => this._getConfig().AccountForSeason = value,
        I18n.Config_Accountforseason_Name,
        I18n.Config_Accountforseason_Tooltip
      );

      genericModConfig.AddBoolOption(
        this._modManifest,
        () => this._getConfig().AccountForAge,
        value => this._getConfig().AccountForAge = value,
        I18n.Config_Accountforage_Name,
        I18n.Config_Accountforage_Tooltip
      );

      genericModConfig.AddBoolOption(
        this._modManifest,
        () => this._getConfig().AllowMultiples,
        value => this._getConfig().AllowMultiples = value,
        I18n.Config_Allowmultiples_Name,
        I18n.Config_Allowmultiples_Tooltip
      );

      genericModConfig.AddNumberOption(
        this._modManifest,
        () => this._getConfig().BaseRate,
        value => this._getConfig().BaseRate = value,
        I18n.Config_Baserate_Name,
        I18n.Config_Baserate_Tooltip,
        0f,
        100f
      );
    }
  }
}