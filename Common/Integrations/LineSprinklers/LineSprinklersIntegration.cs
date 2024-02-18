using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace ModCommon.Integrations.LineSprinklers {
  internal class LineSprinklersIntegration : BaseIntegration {
    private readonly ILineSprinklersApi ModApi;

    public LineSprinklersIntegration(IModRegistry modRegistry, IMonitor monitor)
      : base("Line Sprinklers", "hootless.LineSprinklers", "1.1.0", modRegistry, monitor) {
      if (!this.IsLoaded) return;

      this.ModApi = this.GetValidatedApi<ILineSprinklersApi>();
      this.IsLoaded = this.ModApi != null;
      this.MaxRadius = this.ModApi?.GetMaxGridSize() ?? 0;
    }

    public int MaxRadius { get; }

    public IDictionary<int, Vector2[]> GetSprinklerTiles() {
      this.AssertLoaded();
      return this.ModApi.GetSprinklerCoverage();
    }
  }
}