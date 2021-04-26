using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ModCommon.Integrations.LineSprinklers {
	public interface ILineSprinklersApi {
		int GetMaxGridSize();
		IDictionary<int, Vector2[]> GetSprinklerCoverage();
	}
}