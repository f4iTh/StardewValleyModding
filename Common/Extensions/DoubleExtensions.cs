using System;

namespace ModCommon.Extensions {
  public static class DoubleExtensions {
    // public static double DegreesToStardewRotation(this double degrees) {
    //   double newAngle = degrees - 90;
    //   if (newAngle < 0)
    //     newAngle += 360;
    //
    //   return 360 - newAngle;
    // }
    //
    // public static double DegreesToStardewRotation(this float degrees) {
    //   double newAngle = degrees - 90;
    //   if (newAngle < 0)
    //     newAngle += 360;
    //
    //   return 360 - newAngle;
    // }
    //
    // public static double RadiansToStardewRotation(this double radians) {
    //   double newRadians = radians - 90 * (Math.PI / 180);
    //   if (newRadians < 0)
    //     newRadians += 2 * Math.PI;
    //
    //   return 2 * Math.PI - newRadians;
    // }

    // starts at (1, 1), goes clockwise (compared to the standard (0, 1) going counter-clockwise)
    // TODO: Math.Sin requires this as negative - figure out
    public static double RadiansToStardewRotation(this double radians) {
      double angleAdjustment = radians - 90 * (Math.PI / 180);
      double negativeAdjustment = angleAdjustment < 0 ? angleAdjustment + 2 * Math.PI : angleAdjustment;

      return 2 * Math.PI - negativeAdjustment;
    }
  }
}