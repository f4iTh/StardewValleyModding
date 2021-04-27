using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;

namespace ParsnipsAbsolutelyEverywhereButItsGarlic {

	public class ModEntry : Mod {

		public override void Entry(IModHelper helper) => helper.Events.GameLoop.DayStarted += this.TheTimeOfGarlicHasCome;

		private void TheTimeOfGarlicHasCome(object sender, DayStartedEventArgs e) {
			foreach (Farm farm in this.Helper.Multiplayer.GetActiveLocations().OfType<Farm>()) {
				for (int x = 0; x < farm.map.Layers[0].LayerSize.Width; x++) {
					for (int y = 0; y < farm.map.Layers[0].LayerSize.Height; y++) {
						farm.makeHoeDirt(new Vector2((float)x, (float)y));
					}
				}
				foreach (KeyValuePair<Vector2, TerrainFeature> pair in farm.terrainFeatures.Pairs) {
					Vector2 tile = pair.Key;
					HoeDirt dirt = pair.Value as HoeDirt;
					bool flag = dirt == null;
					if (!flag) {
						bool flag2 = dirt.crop != null;
						if (!flag2) {
							dirt.plant(476, (int)tile.X, (int)tile.Y, Game1.player, false, farm);
							dirt.crop.growCompletely();
						}
					}
				}
			}
		}
	}
}