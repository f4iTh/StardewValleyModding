using Common.Integrations.BetterSprinklers;
using Common.Integrations.SimpleSprinkler;
using Common.Integrations.PrismaticTools;
using Common.Integrations.Cobalt;
using StardewModdingAPI;

namespace ActivateSprinklers.Framework
{
	internal class ModIntegrations
	{
		public BetterSprinklersIntegration BetterSprinklers { get; }
		public SimpleSprinklerIntegration SimpleSprinkler { get; }
		public PrismaticToolsIntegration PrismaticTools { get; }
		public CobaltIntegration Cobalt { get; }

		public ModIntegrations(IMonitor monitor, IModRegistry modRegistry, IReflectionHelper reflection)
		{
			this.BetterSprinklers = new BetterSprinklersIntegration(modRegistry, monitor);
			this.SimpleSprinkler = new SimpleSprinklerIntegration(modRegistry, monitor);
			this.PrismaticTools = new PrismaticToolsIntegration(modRegistry, monitor);
			this.Cobalt = new CobaltIntegration(modRegistry, monitor);
		}
	}
}
