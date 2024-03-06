using System;
using ActivateSprinklers.Common.Enums;
using ModCommon.Api;
using ModCommon.Api.GenericModConfigMenu;
using StardewModdingAPI;

namespace ActivateSprinklers.Common.Configs {
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

      genericModConfig.AddBoolOption(
        this._modManifest,
        () => this._getConfig().InfiniteReach,
        value => this._getConfig().InfiniteReach = value,
        I18n.Config_Reach_Name,
        I18n.Config_Reach_Tooltip
      );

      genericModConfig.AddTextOption(
        this._modManifest,
        () => this._getConfig().SprinklerAnimation.ToString(),
        value => this._getConfig().SprinklerAnimation = (SprinklerAnimation)Enum.Parse(typeof(SprinklerAnimation), value),
        I18n.Config_Animation_Name,
        I18n.Config_Animation_Tooltip,
        Enum.GetNames(typeof(SprinklerAnimation)),
        GenericModConfig.TranslateSprinklerAnimation
      );

      genericModConfig.AddTextOption(
        this._modManifest,
        () => this._getConfig().AdjacentTileDirection.ToString(),
        value => this._getConfig().AdjacentTileDirection = (AdjacentTileDirection)Enum.Parse(typeof(AdjacentTileDirection), value),
        I18n.Config_Adjacent_Name,
        I18n.Config_Adjacent_Tooltip,
        Enum.GetNames(typeof(AdjacentTileDirection)),
        GenericModConfig.TranslateAdjacentTileDirection
      );
    }

    private static string TranslateSprinklerAnimation(string animationString) {
      if (!Enum.TryParse(animationString, out SprinklerAnimation animation))
        return animationString;

      return animation switch {
        SprinklerAnimation.None => I18n.Config_Animation_Values_None(),
        SprinklerAnimation.NewDayAnimation => I18n.Config_Animation_Values_Newday(),
        SprinklerAnimation.WateringCanAnimation => I18n.Config_Animation_Values_Wateringcan(),
        _ => animation.ToString()
      };
    }

    private static string TranslateAdjacentTileDirection(string adjacentTileDirectionString) {
      if (!Enum.TryParse(adjacentTileDirectionString, out AdjacentTileDirection direction))
        return adjacentTileDirectionString;

      return direction switch {
        AdjacentTileDirection.None => I18n.Config_Adjacent_Values_None(),
        AdjacentTileDirection.Left => I18n.Config_Adjacent_Values_Left(),
        AdjacentTileDirection.Right => I18n.Config_Adjacent_Values_Right(),
        AdjacentTileDirection.LeftRight => I18n.Config_Adjacent_Values_Leftright(),
        _ => direction.ToString()
      };
    }
  }
}