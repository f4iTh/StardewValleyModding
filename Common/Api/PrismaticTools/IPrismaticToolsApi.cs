using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ModCommon.Api.PrismaticTools {
  public interface IPrismaticToolsApi {
    bool ArePrismaticSprinklersScarecrows { get; }
    int SprinklerIndex { get; }
    IEnumerable<Vector2> GetSprinklerCoverage(Vector2 origin);
  }
}