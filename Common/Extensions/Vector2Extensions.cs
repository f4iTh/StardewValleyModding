using System;
using Microsoft.Xna.Framework;

namespace ActivateSprinklers.ModCommon.Extensions {
  public static class Vector2Extensions {
    public static double CalculateAngleToTarget(this Vector2 origin, Vector2 target, bool toDegrees = false) {
      float xDiff = target.X - origin.X;
      float yDiff = target.Y - origin.Y;
      double angle = -Math.Atan2(yDiff, xDiff);
      double adjustedForNegative = angle < 0 ? 2 * Math.PI + angle : angle;

      return toDegrees ? adjustedForNegative * 180 / Math.PI : adjustedForNegative;
    }
  }
}