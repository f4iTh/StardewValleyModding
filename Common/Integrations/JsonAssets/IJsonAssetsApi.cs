using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ModCommon.Integrations.JsonAssets {
	public interface IJsonAssetsApi {
		bool TryGetCustomSprite(object entity, out Texture2D texture, out Rectangle sourceRect);
		bool TryGetCustomSpriteSheet(object entity, out Texture2D texture, out Rectangle sourceRect);
	}
}