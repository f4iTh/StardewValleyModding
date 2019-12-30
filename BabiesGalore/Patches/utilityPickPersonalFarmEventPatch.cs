#if DEBUG 
using StardewValley.Events;

namespace BabiesGalore.Patches
{
	public class utilityPickPersonalFarmEventPatch
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