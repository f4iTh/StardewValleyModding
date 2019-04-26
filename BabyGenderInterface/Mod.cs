using StardewModdingAPI;
using StardewValley;
using StardewValley.Events;
using System;

namespace BabyGenderInterface
{
	/// <summary>The mod entry point.</summary>
	public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        public override void Entry(IModHelper helper)
        {
            Mod instance = this;
            helper.Events.GameLoop.UpdateTicked += this.GameEvents_UpdateTick;

			//helper.ConsoleCommands.Add("bgender", "Debug command.", this.bgender);
			//helper.ConsoleCommands.Add("bname", "Debug command.", this.bname);
		}

		private void GameEvents_UpdateTick(object sender, EventArgs e)
		{
			if (Game1.farmEvent == null || !(Game1.farmEvent is BirthingEvent))
				return;

			if (Game1.farmEvent is BirthingEvent)
			{
				Game1.farmEvent = (FarmEvent)new CustomBirthingEvent();
				Game1.farmEvent.setUp();
			}
		}

		//private void bname(string command, string[] args)
		//{
		//	try
		//	{
		//		bool argint = Int32.TryParse(args[0], out int res);
		//		if (!argint)
		//			return;
		//		else
		//		{
		//			if (Game1.player.getChildren()[0] != null && res == 1)
		//			{
		//				bool baby1 = Int32.TryParse(args[0], out int result);
		//				if (!baby1)
		//					return;
		//				else
		//					this.Monitor.Log($"{Game1.player.getChildren()[0].Name}");
		//			}
		//			else if (Game1.player.getChildren()[1] != null && res == 2)
		//			{
		//				bool baby2 = Int32.TryParse(args[0], out int result);
		//				if (!baby2)
		//					return;
		//				else
		//					this.Monitor.Log($"{Game1.player.getChildren()[0].Name}");
		//			}
		//			else
		//				this.Monitor.Log($"No baby found with the index {res}");
		//		}
		//	}
		//	catch (Exception)
		//	{
		//	}
		//}

		//private void bgender(string command, string[] args)
		//{
		//	try
		//	{
		//		bool argint = Int32.TryParse(args[0], out int res);
		//		if (!argint)
		//			return;
		//		else
		//		{
		//			if (Game1.player.getChildren()[0] != null && res == 1)
		//			{
		//				bool baby1 = Int32.TryParse(args[0], out int result);
		//				if (!baby1)
		//					return;
		//				else
		//					this.Monitor.Log($"{Game1.player.getChildren()[0].Gender}");
		//			}
		//			else if (Game1.player.getChildren()[1] != null && res == 2)
		//			{
		//				bool baby2 = Int32.TryParse(args[0], out int result);
		//				if (!baby2)
		//					return;
		//				else
		//					this.Monitor.Log($"{Game1.player.getChildren()[0].Gender}");
		//			}
		//			else
		//				this.Monitor.Log($"No baby found with the index {res}");
		//		}
		//	}
		//	catch (Exception)
		//	{
		//	}
		//}
	}
}