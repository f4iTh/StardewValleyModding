using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace Common.Integrations.Cobalt
{
	internal class CobaltIntegration : BaseIntegration
	{
		private readonly ICobaltApi ModApi;

		public CobaltIntegration(IModRegistry modRegistry, IMonitor monitor)
			: base("Cobalt", "spacechase0.Cobalt", "1.1", modRegistry, monitor)
		{
			if (!this.IsLoaded) return;

			this.ModApi = this.GetValidatedApi<ICobaltApi>();
			this.IsLoaded = this.ModApi != null;
		}

		public int GetSprinklerID()
		{
			this.AssertLoaded();
			return this.ModApi.GetSprinklerID();
		}

		public IEnumerable<Vector2> GetSprinklerTiles()
		{
			this.AssertLoaded();
			return this.ModApi.GetSprinklerCoverage(Vector2.Zero);
		}
	}
}