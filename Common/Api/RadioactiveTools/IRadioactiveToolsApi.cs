using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ModCommon.Api.RadioactiveTools {
  public interface IRadioactiveToolsApi {
    int SprinklerRange { get; }
    int SprinklerIndex { get; }
    bool AreRadioactiveSprinklersScarecrows { get; }
    IEnumerable<Vector2> GetSprinklerCoverage(Vector2 origin);
  }
}