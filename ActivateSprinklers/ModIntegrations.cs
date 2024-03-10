using ModCommon.Api;
using ModCommon.Api.BetterSprinklers;
using ModCommon.Api.SimpleSprinkler;
using StardewModdingAPI;
// using ModCommon.Api.LineSprinklers;
// using ModCommon.Api.PrismaticTools;
// using ModCommon.Api.RadioactiveTools;

namespace ActivateSprinklers {
  /// <summary>The class containing mod integrations.</summary>
  internal class ModIntegrations {
    /// <summary>The class constructor.</summary>
    /// <param name="modRegistry"></param>
    public ModIntegrations(IModRegistry modRegistry) {
      this.BetterSprinklersApi = modRegistry.GetApi<IBetterSprinklersApi>(UniqueModIds.BETTER_SPRINKLERS);
      // this.LineSprinklersApi = modRegistry.GetApi<ILineSprinklersApi>(UniqueModIds.LINE_SPRINKLERS);
      // this.PrismaticToolsApi = modRegistry.GetApi<IPrismaticToolsApi>(UniqueModIds.PRISMATIC_TOOLS);
      // this.RadioactiveToolsApi = modRegistry.GetApi<IRadioactiveToolsApi>(UniqueModIds.RADIOACTIVE_TOOLS);
      this.SimpleSprinklerApi = modRegistry.GetApi<ISimpleSprinklerApi>(UniqueModIds.SIMPLE_SPRINKLERS);
    }

    /// <summary><see cref="IBetterSprinklersApi" /> integration.</summary>
    public IBetterSprinklersApi BetterSprinklersApi { get; }

    // public ILineSprinklersApi LineSprinklersApi { get; }
    // public IPrismaticToolsApi PrismaticToolsApi { get; }
    // public IRadioactiveToolsApi RadioactiveToolsApi { get; }
    /// <summary><see cref="ISimpleSprinklerApi" /> integration.</summary>
    public ISimpleSprinklerApi SimpleSprinklerApi { get; }
  }
}