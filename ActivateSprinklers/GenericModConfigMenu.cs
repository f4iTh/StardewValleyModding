using Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using System;

namespace ActivateSprinklers {

	public class GenericModConfigMenu {
		private readonly GenericModConfigMenuIntegration<ModConfig> ConfigMenu;

		private readonly IModRegistry ModRegistry;

		public GenericModConfigMenu(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply) {
			this.ModRegistry = modRegistry;
			this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
		}

		public void Register() {
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