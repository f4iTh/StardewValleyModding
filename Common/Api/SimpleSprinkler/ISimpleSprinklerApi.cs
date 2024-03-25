using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ModCommon.Api.SimpleSprinkler;

public interface ISimpleSprinklerApi {
  IDictionary<int, Vector2[]> GetNewSprinklerCoverage();
}