using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace ShowCatchQuality {
  public class ModEntry : Mod {
    public override void Entry(IModHelper helper) {
      helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
    }

    private void OnRenderedWorld(object sender, RenderedWorldEventArgs e) {
      if (Game1.activeClickableMenu != null || Game1.player.CurrentTool == null || Game1.player.CurrentTool is not FishingRod fishingRod || !((FishingRod)Game1.player.CurrentTool).fishCaught)
        return;

      int fishQuality = this.Helper.Reflection.GetField<int>(fishingRod, "fishQuality").GetValue();
      if (fishQuality <= 0)
        return;

      int fishQualityRectX = fishQuality >= 2 ? 346 : 338;
      int fishQualityRectY = fishQuality <= 2 ? 400 : 392;
      Rectangle? fishQualityRect = new Rectangle(fishQualityRectX, fishQualityRectY, 8, 8);
      
      float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2));
      Vector2 position = Game1.GlobalToLocal(Game1.viewport, Game1.player.Position + new Vector2(-124f, num - 284f) + new Vector2(44f, 108f));
      
      e.SpriteBatch.Draw(Game1.mouseCursors, position, fishQualityRect, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
    }
  }
}