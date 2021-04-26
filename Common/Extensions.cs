using Microsoft.Xna.Framework.Input;

namespace ModCommon {
	public static class Extensions {
		public static bool PressingShift(this KeyboardState kb) {
			return kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift);
		}
	}
}