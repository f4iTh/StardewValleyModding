using System;
using ModCommon.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace WheresMyItems {
  public class GenericModConfig {
    private readonly GenericModConfigMenuIntegration<ModConfig> configMenu;

    public GenericModConfig(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply) {
      this.configMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
    }

    public void Register() {
      if (!this.configMenu.IsLoaded)
        return;

      this.configMenu.RegisterConfig(true)
        .AddKeyBinding(
          "Toggle Search Menu",
          "Which button to press to open the search menu.",
          config => config.ToggleButton,
          (config, val) => config.ToggleButton = val
        );
    }
  }
}