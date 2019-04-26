using StardewValley;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace AdjustBabyChance
{
	public class EventPatch
	{
		public static bool Prefix(ref FarmEvent __result)
		{
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 ^ 470124797 + (int)Game1.player.UniqueMultiplayerID);
			if (Game1.weddingToday)
			{
				__result = (FarmEvent)null;
				return false;
			}
			if (Game1.player.isMarried() && Game1.player.GetSpouseFriendship().DaysUntilBirthing == 0)
			{
				if (Game1.player.spouse != null)
				{
					__result = (FarmEvent)new BirthingEvent();
					return false;
				}
				long key = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				if (Game1.otherFarmers.ContainsKey(key))
				{
					__result = (FarmEvent)new PlayerCoupleBirthingEvent();
					return false;
				}
			}
			else
			{
				if (Game1.player.isMarried() && Game1.player.spouse != null && (Game1.getCharacterFromName(Game1.player.spouse, false).canGetPregnant() && Game1.player.currentLocation == Game1.getLocationFromName((string)(Game1.player.homeLocation.Value))) && random.NextDouble() < ModEntry.Config.QuestionChance)
				{
					__result = (FarmEvent)new QuestionEvent(1);
					return false;
				}
				if (Game1.player.isMarried())
				{
					long? spouse = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID);
					if (spouse.HasValue && Game1.player.GetSpouseFriendship().NextBirthingDate == (WorldDate)null && random.NextDouble() < ModEntry.Config.QuestionChance)
					{
						spouse = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID);
						long key = spouse.Value;
						if (Game1.otherFarmers.ContainsKey(key))
						{
							Farmer otherFarmer = Game1.otherFarmers[key];
							if (otherFarmer.currentLocation == Game1.player.currentLocation && (otherFarmer.currentLocation == Game1.getLocationFromName((string)(otherFarmer.homeLocation.Value)) || otherFarmer.currentLocation == Game1.getLocationFromName((string)(Game1.player.homeLocation.Value))) && playersCanGetPregnantHere(otherFarmer.currentLocation as FarmHouse))
							{
								__result = (FarmEvent)new QuestionEvent(3);
								return false;
							}
						}
					}
				}
			}
			if (Game1.IsMasterGame && random.NextDouble() < 0.5)
			{
				__result = (FarmEvent)new QuestionEvent(2);
				return false;
			}
			__result = (FarmEvent)new SoundInTheNightEvent(2);
			return false;
		}

		private static bool playersCanGetPregnantHere(FarmHouse farmHouse)
		{
			List<Child> children = farmHouse.getChildren();
			if (farmHouse.getChildrenCount() >= 2 || farmHouse.upgradeLevel < 2 || children.Count >= 2)
				return false;
			if (children.Count != 0)
				return children[0].Age > 2;
			return true;
		}
	}
}
