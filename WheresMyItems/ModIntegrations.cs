using ModCommon.Integrations.JsonAssets;
using StardewModdingAPI;

namespace WheresMyItems {

	internal class ModIntegrations {
		public JsonAssetsIntegration JsonAssets { get; }

		public ModIntegrations(IModRegistry modRegistry, IMonitor monitor) {
			this.JsonAssets = new JsonAssetsIntegration(modRegistry, monitor);
		}
	}
}