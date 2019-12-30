using StardewValley;
using StardewValley.Characters;
using System.Collections.Generic;

namespace BabiesGalore.Patches.NPC
{
	public class canGetPregnantPatch
	{
		public static bool Prefix(StardewValley.NPC __instance, ref bool __result)
		{
			if (ModEntry.config.AllowMoreThanTwoChildren)
			{
				if (__instance is Horse || __instance.Name.Equals("Krobus") || __instance.isRoommate())
				{
					__result = false;
					return false;
				}
				Farmer spouse = __instance.getSpouse();
				if (spouse == null || spouse.divorceTonight.Value)
				{
					__result = false;
					return false;
				}
				int heartLevelForNpc = spouse.getFriendshipHeartLevelForNPC(__instance.Name);
				Friendship spouseFriendship = spouse.GetSpouseFriendship();
				List<Child> children = spouse.getChildren();
				__instance.DefaultMap = spouse.homeLocation.Value;
				if (StardewValley.Utility.getHomeOfFarmer(spouse).upgradeLevel < 2 || spouseFriendship.DaysUntilBirthing >= 0 || (heartLevelForNpc < 10 || spouse.GetDaysMarried() < 7))
				{
					__result = false;
					return false;
				}
				if (children.Count == 0)
				{
					__result = true;
					return false;
				}
				if (children[children.Count - 1].Age > 2)
				{
					__result = true;
					return false;
				}
				__result = false;
				return false;
			}
			return true;
		}
	}
}