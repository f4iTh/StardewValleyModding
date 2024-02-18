﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace ModCommon.Integrations.BetterSprinklers {
  internal class BetterSprinklersIntegration : BaseIntegration {
    private readonly IBetterSprinklersApi ModApi;

    public BetterSprinklersIntegration(IModRegistry modRegistry, IMonitor monitor)
      : base("Better Sprinklers", "Speeder.BetterSprinklers", "2.3.1-unofficial.6-pathoschild", modRegistry, monitor) {
      if (!this.IsLoaded) return;

      this.ModApi = this.GetValidatedApi<IBetterSprinklersApi>();
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