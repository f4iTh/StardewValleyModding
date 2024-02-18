using System.Collections.Generic;
using StardewValley.Characters;
using StardewValley.Locations;

namespace BabiesGalore.Patches.Utility {
  public class playersCanGetPregnantHerePatch {
    public static bool Prefix(ref FarmHouse farmHouse, ref bool __result) {
      if (ModEntry.config.AllowMoreThanTwoChildren) {
        List<Child> children = farmHouse.getChildren();
        if (farmHouse.upgradeLevel < 2) {
          __result = false;
          return false;
        }

        if (children[children.Count - 1].Age > 2) {
          __result = true;
          return false;
        }
      }

      return true;
    }
  }
}