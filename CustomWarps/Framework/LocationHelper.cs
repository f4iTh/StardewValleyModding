﻿using StardewModdingAPI;
using StardewValley;

namespace CustomWarps.Framework {
  public class LocationHelper {
    private readonly IModHelper helper;

    public LocationHelper(IModHelper helper) {
      this.helper = helper;
    }

    public string GetLocationName(string name) {
      ITranslationHelper translation = this.helper.Translation;
      string locationName = translation.Get(name);
      if (locationName.Contains("player_name"))
        locationName = translation.Get(name, new { player_name = Game1.player.Name });
      if (locationName.Contains("farm_name"))
        locationName = translation.Get(name, new { farm_name = Game1.player.farmName.Value });
      return locationName;
    }
  }
}