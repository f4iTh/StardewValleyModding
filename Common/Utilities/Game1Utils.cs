using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace ActivateSprinklers.ModCommon.Utilities;

public static class Game1Utils {
  // TODO: figure out a better way for this; definitely not the nicest solution
  /// <summary>Whether the game currently targets the UI screen.</summary>
  public static bool IsCurrentTargetUiScreen() { 
    RenderTargetBinding[] renderTargets = Game1.graphics.GraphicsDevice.GetRenderTargets();
    return renderTargets.Length > 0 && renderTargets[0].RenderTarget == Game1.game1.uiScreen;
  }
}