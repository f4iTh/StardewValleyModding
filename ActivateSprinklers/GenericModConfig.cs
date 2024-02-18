using System;
using ModCommon.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace ActivateSprinklers {
    public class GenericModConfig {
        private readonly GenericModConfigMenuIntegration<ModConfig> ConfigMenu;

        public GenericModConfig(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply) {
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
        }

        public void Register() {
            if (!this.ConfigMenu.IsLoaded)
                return;

            this.ConfigMenu.RegisterConfig(true)
                .AddCheckbox(
                    "Infinite Reach",
                    "Toggles infinite reach.",
                    config => config.InfiniteReach,
                    (config, value) => config.InfiniteReach = value
                );
        }
    }
}
