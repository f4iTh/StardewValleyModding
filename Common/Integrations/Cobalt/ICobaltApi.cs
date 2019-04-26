using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Common.Integrations.Cobalt
{
	public interface ICobaltApi
	{
		int GetSprinklerID();

		IEnumerable<Vector2> GetSprinklerCoverage(Vector2 origin);
	}
}