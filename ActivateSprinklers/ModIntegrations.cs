using ModCommon.Integrations.BetterSprinklers;
using ModCommon.Integrations.LineSprinklers;
using ModCommon.Integrations.PrismaticTools;
using ModCommon.Integrations.SimpleSprinkler;
using StardewModdingAPI;

namespace ActivateSprinklers {

	internal class ModIntegrations {
		public BetterSprinklersIntegration BetterSprinklers { get; }
		public LineSprinklersIntegration LineSprinklers { get; }
		public PrismaticToolsIntegration PrismaticTools { get; }
		public SimpleSprinklerIntegration SimpleSprinkler { get; }

		public ModIntegrations(IMonitor monitor, IModRegistry modRegistry, IModHelper helper) {
			this.BetterSprinklers = new BetterSprinklersIntegration(modRegistry, monitor);
			this.LineSprinklers = new LineSprinklersIntegration(modRegistry, monitor);
			this.PrismaticTools = new PrismaticToolsIntegration(modRegistry, monitor);
			this.SimpleSprinkler = new SimpleSprinklerIntegration(modRegistry, monitor);
		}
	}
}