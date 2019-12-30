using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Common.Integrations.PrismaticTools
{
	public interface IPrismaticToolsApi
	{
		bool ArePrismaticSprinklersScarecrows { get; }

		int SprinklerIndex { get; }

		IEnumerable<Vector2> GetSprinklerCoverage(Vector2 origin);
	}
}