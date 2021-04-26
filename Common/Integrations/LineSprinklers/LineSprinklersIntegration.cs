using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace ModCommon.Integrations.LineSprinklers {
	internal class LineSprinklersIntegration : BaseIntegration {
		private readonly ILineSprinklersApi ModApi;

		public int MaxRadius { get; }

		public LineSprinklersIntegration(IModRegistry modRegistry, IMonitor monitor)
			: base("Line Sprinklers", "hootless.LineSprinklers", "1.1.0", modRegistry, monitor) {
			if (!IsLoaded) return;

			ModApi = GetValidatedApi<ILineSprinklersApi>();
			IsLoaded = ModApi != null;
			MaxRadius = ModApi?.GetMaxGridSize() ?? 0;
		}

		public IDictionary<int, Vector2[]> GetSprinklerTiles() {
			AssertLoaded();
			return ModApi.GetSprinklerCoverage();
		}
	}
}