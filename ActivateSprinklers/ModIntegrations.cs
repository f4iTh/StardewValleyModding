using Common.Integrations.BetterSprinklers;
using Common.Integrations.LineSprinklers;
using Common.Integrations.PrismaticTools;
using Common.Integrations.SimpleSprinkler;
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