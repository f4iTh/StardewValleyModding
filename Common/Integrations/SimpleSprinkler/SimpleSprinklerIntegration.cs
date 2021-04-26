using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace Common.Integrations.SimpleSprinkler {
	internal class SimpleSprinklerIntegration : BaseIntegration {
		private readonly ISimpleSprinklerApi ModApi;

		public SimpleSprinklerIntegration(IModRegistry modRegistry, IMonitor monitor)
			: base("Simple Sprinklers", "tZed.SimpleSprinkler", "1.6.0", modRegistry, monitor) {
			if (!this.IsLoaded) return;

			this.ModApi = this.GetValidatedApi<ISimpleSprinklerApi>();
			this.IsLoaded = this.ModApi != null;
		}

		public IDictionary<int, Vector2[]> GetSprinklerTiles() {
			this.AssertLoaded();
			return this.ModApi.GetNewSprinklerCoverage();
		}
	}
}