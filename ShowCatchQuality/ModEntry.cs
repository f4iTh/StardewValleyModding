using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace ShowCatchQuality {
	public class ModEntry : Mod {
		public override void Entry(IModHelper helper) {
			helper.Events.Display.RenderedWorld += (sender, e) => {
				if (Game1.activeClickableMenu != null ||
				    Game1.player.CurrentTool == null ||
				    !(Game1.player.CurrentTool is FishingRod fishingRod) ||
				    !((FishingRod) Game1.player.CurrentTool).fishCaught)
					return;
				int fishQuality;
				if ((fishQuality = helper.Reflection.GetField<int>(fishingRod, "fishQuality").GetValue()) <= 0)
					return;
				float num = (float) (4.0 * Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2));
				Rectangle? fishQualityRectangle = new Rectangle(
					fishQuality >= 2 ? 346 : 338,
					fishQuality <= 2 ? 400 : 392,
					8,
					8
				);
				e.SpriteBatch.Draw(
					Game1.mouseCursors,
					Game1.GlobalToLocal(
						Game1.viewport,
						Game1.player.Position + new Vector2(-124f, num - 284f) + new Vector2(44f, 108f)
					),
					fishQualityRectangle,
					Color.White,
					0f,
					Vector2.Zero,
					3f,
					SpriteEffects.None,
					0f);
			};
		}
	}
}