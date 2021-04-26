using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ModCommon.Integrations.PrismaticTools {
	public interface IPrismaticToolsApi {
		bool ArePrismaticSprinklersScarecrows { get; }
		int SprinklerIndex { get; }
		IEnumerable<Vector2> GetSprinklerCoverage(Vector2 origin);
	}
}