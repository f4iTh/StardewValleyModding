using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace ModCommon.Integrations.BetterSprinklers {
	internal class BetterSprinklersIntegration : BaseIntegration {
		private readonly IBetterSprinklersApi ModApi;

		public int MaxRadius { get; }

		public BetterSprinklersIntegration(IModRegistry modRegistry, IMonitor monitor)
			: base("Better Sprinklers", "Speeder.BetterSprinklers", "2.3.1-unofficial.6-pathoschild", modRegistry, monitor) {
			if (!IsLoaded) return;

			ModApi = GetValidatedApi<IBetterSprinklersApi>();
			IsLoaded = ModApi != null;
			MaxRadius = ModApi?.GetMaxGridSize() ?? 0;
		}

		public IDictionary<int, Vector2[]> GetSprinklerTiles() {
			AssertLoaded();
			return ModApi.GetSprinklerCoverage();
		}
	}
}