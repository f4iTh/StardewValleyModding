using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ParsnipsAbsolutelyEverywhereButItsGarlic {
	public class ModEntry : Mod {
		public override void Entry(IModHelper helper) {
			helper.Events.GameLoop.DayStarted += delegate {
				foreach (Farm farm in helper.Multiplayer.GetActiveLocations().OfType<Farm>()) {
					for (int x = 0; x < farm.map.Layers[0].LayerSize.Width; x++)
					for (int y = 0; y < farm.map.Layers[0].LayerSize.Height; y++)
						farm.makeHoeDirt(new Vector2(x, y));
					foreach (KeyValuePair<Vector2, TerrainFeature> pair in farm.terrainFeatures.Pairs) {
						Vector2 tile = pair.Key;
						HoeDirt dirt = pair.Value as HoeDirt;
						bool flag = dirt == null;
						if (flag) continue;
						bool flag2 = dirt.crop != null;
						if (flag2) continue;
						dirt.plant(476, (int) tile.X, (int) tile.Y, Game1.player, false, farm);
						dirt.crop.growCompletely();
					}
				}
			};
		}
	}
}