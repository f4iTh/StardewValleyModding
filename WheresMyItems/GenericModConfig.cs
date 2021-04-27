using ModCommon.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using System;

namespace WheresMyItems {

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
				.AddKeyBinding(
					"Toggle Search Menu",
					"Which button to press to open the search menu.",
					config => config.ToggleButton,
					(config, val) => config.ToggleButton = val
				);
		}
	}
}