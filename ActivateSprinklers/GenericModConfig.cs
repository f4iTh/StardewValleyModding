using ModCommon.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using System;

namespace ActivateSprinklers {

	public class GenericModConfig {
		private readonly GenericModConfigMenuIntegration<ModConfig> ConfigMenu;

		public GenericModConfig(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply) {
			this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
			this.Register();
		}

		private void Register() {
			GenericModConfigMenuIntegration<ModConfig> menu = this.ConfigMenu;
			if (!menu.IsLoaded)
				return;

			menu.RegisterConfig(true)
				.AddCheckbox(
					"Infinite Reach",
					"Toggles infinite reach.",
					config => config.InfiniteReach,
					(config, value) => config.InfiniteReach = value
				);
		}
	}
}