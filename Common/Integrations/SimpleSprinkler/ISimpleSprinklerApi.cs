using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ModCommon.Integrations.SimpleSprinkler {
  public interface ISimpleSprinklerApi {
    IDictionary<int, Vector2[]> GetNewSprinklerCoverage();
  }
}