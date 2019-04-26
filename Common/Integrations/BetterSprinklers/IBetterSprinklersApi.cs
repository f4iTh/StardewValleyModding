using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Common.Integrations.BetterSprinklers
{
	public interface IBetterSprinklersApi
	{
		int GetMaxGridSize();

		IDictionary<int, Vector2[]> GetSprinklerCoverage();
	}
}