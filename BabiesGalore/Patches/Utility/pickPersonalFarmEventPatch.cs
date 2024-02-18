#if DEBUG 
using StardewValley.Events;

namespace BabiesGalore.Patches.Utility
{
	public class pickPersonalFarmEventPatch
	{
		public static void Postfix(ref FarmEvent __result)
		{
			if (ModEntry.attemptBirth)
			{
				__result = new CustomBirthingEvent(ModEntry.helper);
			}
		}
	}
}
#endif