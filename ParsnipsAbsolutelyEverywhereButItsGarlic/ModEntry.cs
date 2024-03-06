using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ParsnipsAbsolutelyEverywhereButItsGarlic {
  public class ModEntry : Mod {
    public override void Entry(IModHelper helper) {
      helper.Events.GameLoop.DayStarted += this.OnDayStarted;
    }

    private void OnDayStarted(object sender, DayStartedEventArgs e) {
      foreach (Farm farm in this.Helper.Multiplayer.GetActiveLocations().OfType<Farm>()) {
        
        for (int x = 0; x < farm.map.Layers[0].LayerSize.Width; x++)
        for (int y = 0; y < farm.map.Layers[0].LayerSize.Height; y++)
          farm.makeHoeDirt(new Vector2(x, y));
        
        foreach ((Vector2 tile, TerrainFeature terrainFeature) in farm.terrainFeatures.Pairs) {
          HoeDirt dirt = terrainFeature as HoeDirt;
          dirt?.plant(476, (int)tile.X, (int)tile.Y, Game1.player, false, farm);
          dirt?.crop?.growCompletely();
        }
      }
      
    }
  }
}