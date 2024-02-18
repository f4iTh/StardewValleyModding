using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
#if DEBUG
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley.Characters;
using System;
using System.Linq;
#endif

namespace BabiesGalore
{
	public class ModEntry : Mod
	{
		public static IModHelper helper;
		public static ModConfig config;
		private Harmony harmony;

		public override void Entry(IModHelper helper)
		{
			ModEntry.helper = helper;
			ModEntry.config = helper.ReadConfig<ModConfig>() ?? new ModConfig();

			harmony = new Harmony("com.f4iTh.babiesgalore");
			harmony.Patch(helper.Reflection.GetMethod(new NPC(), "canGetPregnant").MethodInfo, prefix: new HarmonyMethod(typeof(Patches.NPC.canGetPregnantPatch), "Prefix", null));
			harmony.Patch(helper.Reflection.GetMethod(typeof(Utility), "playersCanGetPregnantHere").MethodInfo, prefix: new HarmonyMethod(typeof(Patches.Utility.playersCanGetPregnantHerePatch), "Prefix", null));

			helper.ConsoleCommands.Add("togglechildrenlimit", "Disables or allows having more than two children (default: true)", this.toggleChildrenLimitCommand);

#if DEBUG
			harmony.Patch(helper.Reflection.GetMethod(typeof(Utility), "pickPersonalFarmEvent").MethodInfo, postfix: new HarmonyMethod(typeof(Patches.Utility.pickPersonalFarmEventPatch), "Postfix", null));

			helper.ConsoleCommands.Add("givebaby", "Attempts to start birthing event at night.", this.attemptBirthCommand);
			helper.ConsoleCommands.Add("bg_debug", "Debug command.", this.babiesGaloreDebugCommand);

			helper.Events.GameLoop.DayStarted += this.onDayStarted;
#endif
		}

		private void toggleChildrenLimitCommand(string command, string[] args)
		{
			ModEntry.config.AllowMoreThanTwoChildren = !ModEntry.config.AllowMoreThanTwoChildren;
			base.Helper.WriteConfig<ModConfig>(ModEntry.config);
			base.Monitor.Log($"{(ModEntry.config.AllowMoreThanTwoChildren ? "The player can now have more than two children." : "The player can no longer have more than two children.")}");
		}

#if DEBUG
		public static bool attemptBirth = false;

		private void attemptBirthCommand(string command, string[] args)
		{
			attemptBirth = !attemptBirth;
			base.Monitor.Log($"{(attemptBirth ? "Attempting to start birth event at night." : "No longer attempting to start birth event at night.")}", LogLevel.Info);
		}

		private void babiesGaloreDebugCommand(string command, string[] args)
		{
			try
			{
				if (args.Length == 0)
				{
					base.Monitor.Log("Please enter arguments.");
					return;
				}
				if (args[0] == "getchildrencount")
				{
					base.Monitor.Log(Game1.player.getChildrenCount().ToString());
				}
				else if (args[0] == "getchildren")
				{
					var childrenList = Game1.player.getChildren();
					for (int i = 0; i < childrenList.Count; i++)
					{
						var child = childrenList[i];
						string isMale = child.Gender == 0 ? "male" : "female";
						base.Monitor.Log($"Child #{i + 1} - {child.Name} ({isMale}) - [{child.daysOld.Value} day{(child.daysOld.Value != 1 ? "s" : "")} old]");
					}
				}
				else if (args[0] == "setchildage")
				{
					if (Int32.TryParse(args[1], out int childNumber) && Int32.TryParse(args[2], out int daysOld))
					{
						Game1.player.getChildren()[childNumber - 1].daysOld.Value = daysOld;
					}
					else
					{
						string[] babyNumbers = (args[1].Split('-'));
						if (Int32.TryParse(args[2], out int _daysOld))
						{
							foreach (var babyNumber in babyNumbers)
							{
								if (Int32.TryParse(babyNumber, out int num))
									Game1.player.getChildren()[num - 1].daysOld.Value = _daysOld;
							}
						}
					}
				}
				else if (args[0] == "summon")
				{
					Random random = new Random();

					bool isMale = Game1.player.getNumberOfChildren() >= 2 ? random.NextDouble() < 0.5 : Game1.player.getNumberOfChildren() != 0 ? Game1.player.getChildren()[0].Gender == 1 : random.NextDouble() < 0.5;
					double num = (Game1.player.spouse != null && Game1.player.spouse.Equals("Maru") ? 0.5 : 0.0) + (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
					bool isDarkSkinned = random.NextDouble() < num;
					string name = "";
					if (args.Length == 2 && !string.IsNullOrEmpty(args[1]) && args[1] != " ")
						name = args[1];
					else
						name = Dialogue.randomName();

					Child baby = new Child(name, isMale, isDarkSkinned, Game1.player);
					baby.Age = 0;
					baby.Position = new Vector2(16f, 4f) * 64f + new Vector2(0.0f, -24f);
					Utility.getHomeOfFarmer(Game1.player).characters.Add((NPC)baby);
				}
				else if (args[0] == "marry")
				{
					if (args.Count() < 2)
					{
						base.Monitor.Log("Don't forget to enter name of the person you want to marry!");
						return;
					}
					if (string.IsNullOrEmpty(args[1]) || Game1.getCharacterFromName(args[1], true) == null)
					{
						base.Monitor.Log($"Could not find a person with the name '{args[1]}'.");
						return;
					}
					try
					{
						if (Game1.player.HouseUpgradeLevel < 2)
						{
							Utility.getHomeOfFarmer(Game1.player).moveObjectsForHouseUpgrade(2);
							Utility.getHomeOfFarmer(Game1.player).setMapForUpgradeLevel(2);
							Game1.player.HouseUpgradeLevel = 2;
							Game1.removeFrontLayerForFarmBuildings();
							Game1.addNewFarmBuildingMaps();
						}
						NPC n1 = Utility.fuzzyCharacterSearch(args[1], true);
						if (n1 != null && !Game1.player.friendshipData.ContainsKey(n1.Name))
							Game1.player.friendshipData.Add(n1.Name, new Friendship());
						Game1.player.changeFriendship(2500, n1);
						Game1.player.spouse = n1.Name;
						Game1.player.friendshipData[n1.Name].WeddingDate = new WorldDate(Game1.Date);
						Game1.player.friendshipData[n1.Name].Status = FriendshipStatus.Married;
					}
					catch (Exception ex)
					{
						base.Monitor.Log(ex.Message);
					}
					finally
					{
						base.Monitor.Log($"Successfully married to {args[1]}!");
					}
					return;
				}
				else if (args[0] == "daysmarried")
				{
					if (Game1.player.spouse == null || !Game1.player.isMarried())
					{
						base.Monitor.Log("Player isn't married, no data found.");
						return;
					}
					int daysmarried = Game1.player.friendshipData[Game1.player.spouse].DaysMarried;
					base.Monitor.Log($"{daysmarried} day{(daysmarried != 1 ? "s" : "")} married.");
				}
				else if (args[0] == "spousedata")
				{
					if (Game1.player.spouse == null || !Game1.player.isMarried())
					{
						base.Monitor.Log("Player isn't married, no data found.");
						return;
					}
					var spouseData = Game1.player.friendshipData[Game1.player.spouse];
					base.Monitor.Log($"Days until birthing: {spouseData.DaysUntilBirthing}");
				}
			}
			catch (Exception ex)
			{
				base.Monitor.Log(ex.Message);
			}
		}
		private void onDayStarted(object sender, DayStartedEventArgs e) => attemptBirth = false;
#endif
	}
}