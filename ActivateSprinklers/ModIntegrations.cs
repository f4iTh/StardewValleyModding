using ModCommon.Api.BetterSprinklers;
using ModCommon.Api.LineSprinklers;
using ModCommon.Api.PrismaticTools;
using ModCommon.Api.RadioactiveTools;
using ModCommon.Api.SimpleSprinkler;
using StardewModdingAPI;

namespace ActivateSprinklers {
  internal class ModIntegrations {
    public ModIntegrations(IModRegistry modRegistry) {
      this.BetterSprinklersApi = modRegistry.GetApi<IBetterSprinklersApi>("Speeder.BetterSprinklers");
      // this.LineSprinklersApi = modRegistry.GetApi<ILineSprinklersApi>("hootless.LineSprinklers");
      this.LineSprinklersApi = modRegistry.GetApi<ILineSprinklersApi>("nanz.LineSprinklers");
      this.PrismaticToolsApi = modRegistry.GetApi<IPrismaticToolsApi>("stokastic.PrismaticTools");
      this.RadioactiveToolsApi = modRegistry.GetApi<IRadioactiveToolsApi>("kakashigr.RadioactiveTools");
      this.SimpleSprinklerApi = modRegistry.GetApi<ISimpleSprinklerApi>("tZed.SimpleSprinkler");
    }

    public IBetterSprinklersApi BetterSprinklersApi { get; }
    public ILineSprinklersApi LineSprinklersApi { get; }
    public IPrismaticToolsApi PrismaticToolsApi { get; }
    public IRadioactiveToolsApi RadioactiveToolsApi { get; }
    public ISimpleSprinklerApi SimpleSprinklerApi { get; }
  }
}