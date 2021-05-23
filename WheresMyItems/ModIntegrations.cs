using ModCommon.Integrations.JsonAssets;
using StardewModdingAPI;

namespace WheresMyItems {
	public class ModIntegrations {
		private JsonAssetsIntegration JsonAssets { get; }
		
		public ModIntegrations(IModRegistry modRegistry, IMonitor monitor) {
			this.JsonAssets = new JsonAssetsIntegration(modRegistry, monitor);
		}
	}
}