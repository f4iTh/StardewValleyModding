using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using System;

namespace ShowCatchQuality {

	public class ModEntry : Mod {

		public override void Entry(IModHelper helper) {
			helper.Events.Display.RenderedWorld += this.RenderedWorld;
		}

		private void RenderedWorld(object sender, RenderedWorldEventArgs e) {
			if (Game1.activeClickableMenu != null || Game1.player.CurrentTool == null || !(Game1.player.CurrentTool is FishingRod fishingRod) || !(Game1.player.CurrentTool as FishingRod).fishCaught)
				return;
			int fishQuality;
			if ((fishQuality = this.Helper.Reflection.GetField<int>(fishingRod, "fishQuality").GetValue()) <= 0)
				return;
			float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2));
			Rectangle? fishQualityRectangle = fishQuality == 4 ? new Rectangle?(new Rectangle(346, 392, 8, 8)) : fishQuality == 2 ? new Rectangle?(new Rectangle(346, 400, 8, 8)) : new Rectangle?(new Rectangle(338, 400, 8, 8));
			e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, Game1.player.Position + new Vector2(-124f, num - 284f) + new Vector2(44f, 108f)), fishQualityRectangle, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
		}
	}
}