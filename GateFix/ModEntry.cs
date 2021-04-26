using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GateFix {

	public class ModEntry : Mod {
		private HarmonyInstance Harmony;

		public override void Entry(IModHelper helper) {
			this.Harmony = HarmonyInstance.Create("com.f4iTh.gatefix");
			this.Harmony.Patch(helper.Reflection.GetMethod(new Fence(), "loadFenceTexture", true).MethodInfo, new HarmonyMethod(typeof(loadFenceTexturePatch), "Prefix", null), null);
		}
	}

	public class loadFenceTexturePatch {

		public static bool Prefix(Fence __instance, ref Texture2D __result) {
#if DEBUG
			//foreach (var tile in Utility.getAdjacentTileLocations(__instance.TileLocation)) {
			//	if (Game1.player.currentLocation.objects.ContainsKey(tile) && Game1.player.currentLocation.objects[tile] is Fence fence) {
			//		if (fence.whichType.Value == 4) {
			//			var gate = fence;
			//			foreach (var possibleFence in Utility.getAdjacentTileLocations(gate.TileLocation)) {
			//				if (Game1.player.currentLocation.objects.ContainsKey(possibleFence) && Game1.player.currentLocation.objects[tile] is Fence fence_ && !fence_.isGate.Value) {
			//					//__result = Game1.content.Load<Texture2D>("LooseSprites\\Fence" + fence_.whichType.Value);
			//					break;
			//				}
			//			}
			//		} else {
			//			//__result = Game1.content.Load<Texture2D>("LooseSprites\\Fence" + fence.whichType.Value);
			//		}
			//		if (fence.isGate.Value) this.Monitor.Log($"Gate type {fence.whichType} at {tile}");
			//		else this.Monitor.Log($"Fence type {fence.whichType} at {tile}");
			//	}
			//}
#endif
			if (__instance.whichType.Value == 4) {
				Dictionary<Vector2, int> fenceDict = new Dictionary<Vector2, int>();
				for (int x = (int)__instance.TileLocation.X - 1; x <= (int)__instance.TileLocation.X + 1; x++) {
					for (int y = (int)__instance.TileLocation.Y - 1; y <= (int)__instance.TileLocation.Y + 1; y++) {
						if (x == __instance.TileLocation.X | y == __instance.TileLocation.Y) {
							Vector2 tile = new Vector2(x, y);
							if (Game1.player.currentLocation.objects.ContainsKey(tile) && Game1.player.currentLocation.objects[tile] is Fence fence && fence != null && fence.whichType.Value != 4) {
								if (fence.isGate.Value) {
									__result = Game1.content.Load<Texture2D>("LooseSprites\\Fence" + fence.whichType.Value);
									return false;
								}
								if (!fenceDict.ContainsKey(tile))
									fenceDict.Add(tile, fence.whichType.Value);
							}
						}
					}
				}
				for (int x = (int)__instance.TileLocation.X - 2; x <= (int)__instance.TileLocation.X + 2; x++) {
					for (int y = (int)__instance.TileLocation.Y - 2; y <= (int)__instance.TileLocation.Y + 2; y++) {
						if (x == __instance.TileLocation.X | y == __instance.TileLocation.Y) {
							Vector2 tile = new Vector2(x, y);
							if (Game1.player.currentLocation.objects.ContainsKey(tile) && Game1.player.currentLocation.objects[tile] is Fence fence && fence != null && fence.whichType.Value != 4) {
								if (!fenceDict.ContainsKey(tile))
									fenceDict.Add(tile, fence.whichType.Value);
							}
						}
					}
				}
				if (fenceDict.Count > 0) {
					int which = fenceDict.Values.GroupBy(v => v)
						.OrderByDescending(g => g.Count())
						.First()
						.Key;
					__result = Game1.content.Load<Texture2D>("LooseSprites\\Fence" + which);
					return false;
				}
			}
			__result = Game1.content.Load<Texture2D>("LooseSprites\\Fence" + Math.Max(1, __instance.isGate.Value && __instance.whichType.Value == 4 ? 1 : __instance.whichType.Value));
			return false;
		}
	}
}