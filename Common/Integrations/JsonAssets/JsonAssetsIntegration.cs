using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ModCommon.Integrations.JsonAssets {
  internal class JsonAssetsIntegration : BaseIntegration {
    private readonly IJsonAssetsApi ModApi;

    public JsonAssetsIntegration(IModRegistry modRegistry, IMonitor monitor)
      : base("Json Assets", "spacechase0.JsonAssets", "1.5.1", modRegistry, monitor) {
      if (!this.IsLoaded)
        return;
      this.ModApi = this.GetValidatedApi<IJsonAssetsApi>();
      this.IsLoaded = this.ModApi != null;
    }

    public bool TryGetCustomSpriteSheet(object entity, out Texture2D texture, out Rectangle sourceRect, bool currentSpriteOnly = false) {
      return currentSpriteOnly
        ? this.ModApi.TryGetCustomSprite(entity, out texture, out sourceRect)
        : this.ModApi.TryGetCustomSpriteSheet(entity, out texture, out sourceRect);
    }
  }
}