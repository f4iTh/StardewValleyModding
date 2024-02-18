using System;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Events;
using StardewValley.Locations;

namespace AdjustBabyChance {
  public class EventPatch {
    public static bool Prefix(ref FarmEvent __result) {
      IReflectedMethod playersCanGetPregnantHere = ModEntry.helper.Reflection.GetMethod(typeof(Utility), "playersCanGetPregnantHere");

      Random random = new(((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2) ^ (470124797 + (int)Game1.player.UniqueMultiplayerID));
      if (Game1.weddingToday) {
        __result = null;
        return false;
      }

      if (Game1.player.isMarried() && Game1.player.GetSpouseFriendship().DaysUntilBirthing == 0) {
        if (Game1.player.spouse != null) {
          __result = new BirthingEvent();
          return false;
        }

        long key = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
        if (Game1.otherFarmers.ContainsKey(key)) {
          __result = new PlayerCoupleBirthingEvent();
          return false;
        }
      }
      else {
        if (Game1.player.isMarried() && Game1.player.spouse != null && Game1.getCharacterFromName(Game1.player.spouse, false).canGetPregnant() && Game1.player.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation.Value) && random.NextDouble() < ModEntry.config.QuestionChance) {
          __result = new QuestionEvent(1);
          return false;
        }

        if (Game1.player.isMarried()) {
          long? spouse = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID);
          if (spouse.HasValue && Game1.player.GetSpouseFriendship().NextBirthingDate == null && random.NextDouble() < ModEntry.config.QuestionChance) {
            spouse = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID);
            long key = spouse.Value;
            if (Game1.otherFarmers.ContainsKey(key)) {
              Farmer otherFarmer = Game1.otherFarmers[key];
              if (otherFarmer.currentLocation == Game1.player.currentLocation && (otherFarmer.currentLocation == Game1.getLocationFromName(otherFarmer.homeLocation.Value) || otherFarmer.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation.Value)) && playersCanGetPregnantHere.Invoke<bool>(otherFarmer.currentLocation as FarmHouse)) {
                __result = new QuestionEvent(3);
                return false;
              }
            }
          }
        }
      }

      if (Game1.IsMasterGame && random.NextDouble() < 0.5) {
        __result = new QuestionEvent(2);
        return false;
      }

      __result = new SoundInTheNightEvent(2);
      return false;
    }
  }
}