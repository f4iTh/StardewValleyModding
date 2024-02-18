using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ModCommon.Integrations.BetterSprinklers {
  public interface IBetterSprinklersApi {
    int GetMaxGridSize();
    IDictionary<int, Vector2[]> GetSprinklerCoverage();
  }
}