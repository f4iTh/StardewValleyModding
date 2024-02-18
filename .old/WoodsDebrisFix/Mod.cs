using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using WoodsDebrisFix.Framework;

namespace WoodsDebrisFix
{
    public class Mod : StardewModdingAPI.Mod
    {
        private int dailyVisits = 0;
        private double luck;
        private int rarechance;
        private bool enabled;

        public override void Entry(IModHelper helper)
        {
            helper.Events.Player.Warped += this.currentLocationChanged;
            helper.Events.GameLoop.DayStarted += this.afterDayStarted;
            try
            {
                ModConfig config = helper.ReadConfig<ModConfig>();
                luck = config.Luck;
                if (luck > 1 || luck < -1)
                {
                    Monitor.Log("Luck value out of range, using default value (0.035)\nPlease adjust 'Luck' to a value between -1 and 1 in the config file.", LogLevel.Error);
                    luck = 0.035;
                }
                rarechance = config.Chance;
                if (rarechance < 0)
                {
                    Monitor.Log($"Chance value out of range, using default value (100)\nPlease adjust 'Chance' to a value between 0 and {Int32.MaxValue} in the config file.", LogLevel.Error);
                    rarechance = 100;
                }
                enabled = config.Enabled;
            }
            catch (Exception ex)
            {
                Monitor.Log($"An error has occurred. Details:\n{ex}", LogLevel.Error);
                Monitor.Log($"Attempting to load with default values.", LogLevel.Info);
                rarechance = 100;
                luck = 0.035;
                enabled = true;
            }
        }

		private void afterDayStarted(object sender, EventArgs e)
        {
            dailyVisits = 0;
        }

        private void currentLocationChanged(object sender, WarpedEventArgs e)
		{
			Random random = new Random();
			Random random2 = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			if (e.NewLocation.Name == "Woods")
            {
                GameLocation woods = e.NewLocation;
				dailyVisits++;
                try
                {
                    IList<WeatherDebris> debris = this.Helper.Reflection.GetField<List<WeatherDebris>>(woods, "weatherDebris").GetValue();
                    if (debris != null && debris.Count > 0)
                        debris.Clear();

					int num2 = 25 + random2.Next(0, 75);
					if (!Game1.isRaining && Game1.currentLocation.Equals(woods))
					{
						if (!Game1.currentSeason.Equals("winter"))
						{
							int num3 = Game1.tileSize * 3;
							for (int j = 0; j < num2; j++)
							{
								int num4 = j * num3;
								Viewport viewport = Game1.graphics.GraphicsDevice.Viewport;
								float x = (float)(num4 % viewport.Width + Game1.random.Next(num3));
								int num5 = j * num3;
								viewport = Game1.graphics.GraphicsDevice.Viewport;

								if (random.Next(rarechance) == 0 && dailyVisits < 2 && enabled)
								{
									Game1.addHUDMessage(new HUDMessage("It's your lucky day!", 3) { noIcon = true, timeLeft = 3000});
									Game1.dailyLuck += luck;
									for (int i = 0; i < 3; i++)
									{
										debris.Add(new WeatherDebris(new Vector2(x, (float)(num5 / viewport.Width * num3 + Game1.random.Next(num3))), i, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
									}
									foreach (WeatherDebris wd in debris)
										wd.update();
									return;
								}
								int seasonID = Array.IndexOf(new[] { "spring", "summer", "fall" }, Game1.currentSeason);
								if (seasonID != -1)
								{
									debris.Add(new WeatherDebris(new Vector2(x, (float)(num5 / viewport.Width * num3 + Game1.random.Next(num3))), seasonID, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
									foreach (WeatherDebris wd in debris)
										wd.update();
								}
							}
						}
					}
                }
                catch (Exception ex)
                {
                    Monitor.Log($"An error has occurred. Details:\n{ex}", LogLevel.Info);
                }
            }
        }
    }
}