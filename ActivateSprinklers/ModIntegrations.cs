using ModCommon.Api;
using ModCommon.Api.BetterSprinklers;
// using ModCommon.Api.LineSprinklers;
// using ModCommon.Api.PrismaticTools;
// using ModCommon.Api.RadioactiveTools;
using ModCommon.Api.SimpleSprinkler;
using StardewModdingAPI;

namespace ActivateSprinklers {
  internal class ModIntegrations {
    public ModIntegrations(IModRegistry modRegistry) {
      this.BetterSprinklersApi = modRegistry.GetApi<IBetterSprinklersApi>(UniqueModIds.BETTER_SPRINKLERS);
      // this.LineSprinklersApi = modRegistry.GetApi<ILineSprinklersApi>(UniqueModIds.LINE_SPRINKLERS);
      // this.PrismaticToolsApi = modRegistry.GetApi<IPrismaticToolsApi>(UniqueModIds.PRISMATIC_TOOLS);
      // this.RadioactiveToolsApi = modRegistry.GetApi<IRadioactiveToolsApi>(UniqueModIds.RADIOACTIVE_TOOLS);
      this.SimpleSprinklerApi = modRegistry.GetApi<ISimpleSprinklerApi>(UniqueModIds.SIMPLE_SPRINKLERS);
    }

    public IBetterSprinklersApi BetterSprinklersApi { get; }
    // public ILineSprinklersApi LineSprinklersApi { get; }
    // public IPrismaticToolsApi PrismaticToolsApi { get; }
    // public IRadioactiveToolsApi RadioactiveToolsApi { get; }
    public ISimpleSprinklerApi SimpleSprinklerApi { get; }
  }
}