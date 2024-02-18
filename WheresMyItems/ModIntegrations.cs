using ModCommon.Integrations.JsonAssets;
using StardewModdingAPI;

namespace WheresMyItems {
  public class ModIntegrations {
    public ModIntegrations(IModRegistry modRegistry, IMonitor monitor) {
      this.JsonAssets = new JsonAssetsIntegration(modRegistry, monitor);
    }

    private JsonAssetsIntegration JsonAssets { get; }
  }
}