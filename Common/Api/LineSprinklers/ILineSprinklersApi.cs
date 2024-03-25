using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ModCommon.Api.LineSprinklers;

public interface ILineSprinklersApi {
  int GetMaxGridSize();
  IDictionary<int, Vector2[]> GetSprinklerCoverage();
}