using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ParsnipsAbsolutelyEverywhereButItsGarlic {
  /// <summary>The mod entry point.</summary>
  public class ModEntry : Mod {
    /// <summary>The mod entry point method.</summary>
    /// <param name="helper">The mod helper.</param>
    public override void Entry(IModHelper helper) {
      helper.Events.GameLoop.DayStarted += this.OnDayStarted;
    }

    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void OnDayStarted(object sender, DayStartedEventArgs e) {
      foreach (Farm farm in this.Helper.Multiplayer.GetActiveLocations().OfType<Farm>()) {
        for (int x = 0; x < farm.map.Layers[0].LayerSize.Width; x++)
        for (int y = 0; y < farm.map.Layers[0].LayerSize.Height; y++)
          farm.makeHoeDirt(new Vector2(x, y));

        foreach (HoeDirt dirt in from TerrainFeature terrainFeature in farm.terrainFeatures.Values select terrainFeature as HoeDirt) {
          dirt?.plant("476", Game1.player, false);
          dirt?.crop?.growCompletely();
        }
      }
    }
  }
}