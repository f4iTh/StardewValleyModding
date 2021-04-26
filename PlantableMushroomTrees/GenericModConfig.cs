using Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using System;

namespace PlantableMushroomTrees {
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
				.AddLabel(
					"General Options"
				)
				.AddCheckbox(
					"Instant Mushroom Tree",
					"Whether to grow mushroom trees to max growth stage upon planting.",
					config => config.InstantMushroomTree,
					(config, value) => config.InstantMushroomTree = value
				)
				.AddCheckbox(
					"Require Alt key",
					"Whether to require pressing alt key for planting mushroom trees.",
					config => config.RequireAltKey,
					(config, value) => config.RequireAltKey = value
				)
				.AddCheckbox(
					"Show Grid",
					"Whether to show grid when holding a mushroom.",
					config => config.ShowPlantingGrid,
					(config, value) => config.ShowPlantingGrid = value
				);
				
		}
	}
}
