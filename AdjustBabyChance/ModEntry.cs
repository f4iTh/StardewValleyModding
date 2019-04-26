using Harmony;
using StardewModdingAPI;
using StardewValley;

namespace AdjustBabyChance
{
	public class ModEntry : Mod
	{
		public static ModConfig Config;

		public override void Entry(IModHelper helper)
		{
			Config = this.Helper.ReadConfig<ModConfig>();
			helper.ConsoleCommands.Add("setbabychance", "Adjusts the baby chance.\n  - Accepts values between 0 and 1.", this.setBabyChanceCommand);
			helper.ConsoleCommands.Add("getbabychance", "Shows the current chance of the baby question appearing.", this.getBabyChanceCommand);
			if (Config.QuestionChance < 0.00 || Config.QuestionChance > 1.00)
			{
				this.Monitor.Log($"Baby question chance must be set to between 0 and 1. Using default value of '0.05'.", LogLevel.Error);
				Config.QuestionChance = 0.05;
			}

			HarmonyInstance harmony = HarmonyInstance.Create("com.f4iTh.AdjustBabyChance");
			harmony.Patch(helper.Reflection.GetMethod(typeof(Utility), "pickPersonalFarmEvent").MethodInfo, new HarmonyMethod(typeof(EventPatch), "Prefix", null), null);
		}

		private void getBabyChanceCommand(string command, string[] args)
		{
			this.Monitor.Log($"Baby question chance is set to '{Config.QuestionChance}'.");
		}

		private void setBabyChanceCommand(string command, string[] args)
		{
			if (double.TryParse(args[0], out double newchance))
			{
				if (newchance < 0.00 || newchance > 1.00)
				{
					this.Monitor.Log($"Baby question chance must be set to between 0 and 1. Please make sure '{newchance}' is a valid number between 0 and 1.", LogLevel.Error);
					return;
				}
				Config.QuestionChance = newchance;
				this.Helper.Data.WriteJsonFile<ModConfig>("config.json", Config);
				this.Monitor.Log($"Baby question chance was successfully set to '{newchance}'.", LogLevel.Info);
				return;
			}
			this.Monitor.Log($"Something went wrong while parsing input. Please make sure '{newchance}' is a valid number between 0 and 1.");
		}
	}
}
