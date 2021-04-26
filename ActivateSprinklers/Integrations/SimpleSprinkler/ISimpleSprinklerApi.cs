using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ActivateSprinklers.Integrations.SimpleSprinkler {

	public interface ISimpleSprinklerApi {

		IDictionary<int, Vector2[]> GetNewSprinklerCoverage();
	}
}