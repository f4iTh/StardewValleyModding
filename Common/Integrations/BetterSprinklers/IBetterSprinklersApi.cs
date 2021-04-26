using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ModCommon.Integrations.BetterSprinklers {
	public interface IBetterSprinklersApi {
		int GetMaxGridSize();
		IDictionary<int, Vector2[]> GetSprinklerCoverage();
	}
}