using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BreedLikeRabbits2
{
	public class ModEntry : Mod
	{
		private readonly Random _random = new Random();

		private ModConfig _config;

		public override void Entry(IModHelper helper)
		{
			this._config = helper.ReadConfig<ModConfig>();
			helper.Events.GameLoop.Saved += this.BreedRabbit;
		}

		private void BreedRabbit(object sender, SavedEventArgs e)
		{
			Farm farm = Game1.getFarm();
			int breedersCount = this.GetBreeders(farm);
			int buckCount = this._config.IgnoreGender ? breedersCount : this.GetMatureMales(farm);
			List<string> list = new List<string>();
			foreach (FarmAnimal farmAnimal in farm.getAllFarmAnimals()) {
				if (farmAnimal.type.Value != "Rabbit" || farmAnimal.age.Value < farmAnimal.ageWhenMature.Value + 14 || (farmAnimal.isMale() && !this._config.IgnoreGender)) continue;
				Building home = farmAnimal.home;
				int maxAnimalsCount = (home.buildingType.Value == "Coop") ? 4 : ((home.buildingType.Value == "Big Coop") ? 8 : 12);
				int currentAnimalsCount = maxAnimalsCount - home.currentOccupants.Value;
				double kitChance = this.GetChanceOfKits(buckCount, farmAnimal);
				if (this._random.NextDouble() <= kitChance && currentAnimalsCount > 0)
				{
					int kitCount;
					if ((kitCount = this.GetNumberOfKits(currentAnimalsCount, farmAnimal)) > 0)
					{
						for (int i = 0; i < kitCount; i++)
							this.AddKit(home);
						string item = $"During the night, {farmAnimal.Name} gave birth to a litter of {kitCount} baby rabbits.";
						list.Add(item);
					}
				}
				this.LogMessage($"\n name: {farmAnimal.Name};\n monthsOld: {farmAnimal.age.Value / 28};\n happiness: {farmAnimal.happiness.Value};\n friendship: {farmAnimal.friendshipTowardFarmer.Value};\n spaceAvailable: {currentAnimalsCount};\n chanceOfLitter: {kitChance}");
			}
			if (list.Count > 0)
				Game1.multipleDialogues(list.ToArray());
		}
		
		private int GetMatureMales(Farm farm)
		{
			int num = 0;
			foreach (FarmAnimal farmAnimal in farm.getAllFarmAnimals())
			{
				if (farmAnimal.type.Value == "Rabbit" && farmAnimal.isMale() && !farmAnimal.isBaby())
					num++;
			}
			return num;
		}
		
		private int GetBreeders(Farm farm)
		{
			int rabbitsTotal = 0;
			int rabbitBreeders = 0;
			foreach (FarmAnimal farmAnimal in farm.getAllFarmAnimals().Where(farmAnimal => farmAnimal.type.Value == "Rabbit")) {
				rabbitsTotal++;
				if (farmAnimal.age.Value >= farmAnimal.ageWhenMature.Value + 14)
					rabbitBreeders++;
			}
			this.LogMessage($"There are {rabbitsTotal} Rabbits. {rabbitBreeders} breeders.");
			return rabbitBreeders;
		}
		
		private double GetChanceOfKits(int buckCount, FarmAnimal animal)
		{
			double result;
			if (buckCount == 0)
				result = 0.0;
			else
			{
				double ageBonus = 0.0;
				double happinessBonus = 0.0;
				double friendshipBonus = 0.0;
				double seasonBonus = 0.0;
				double baseRate = 1.0 / this._config.BaseRate;
				baseRate -= animal.daysSinceLastFed.Value / 20.0;
				double buckCountChance = buckCount / 100.0;
				if (buckCountChance > 0.03)
					buckCountChance = 0.03;
				if (this._config.AccountForAge)
					ageBonus = animal.age.Value / 4200.0;
				if (this._config.AccountForAge && animal.age.Value > 112)
					ageBonus = 0.04 - animal.age.Value / 4200.0;
				if (this._config.AccountForHappiness)
					happinessBonus = animal.happiness.Value / 7200.0;
				if (this._config.AccountForFriendship)
					friendshipBonus = animal.friendshipTowardFarmer.Value / 60000.0;
				if (this._config.AccountForSeason)
				{
					string currentSeason = Game1.currentSeason;
					if (currentSeason != "winter")
					{
						if (currentSeason != "summer")
							seasonBonus = 0.0;
						else
							seasonBonus = -0.02;
					}
					else
						seasonBonus = 0.01;
				}
				double kitChance = baseRate + buckCountChance + ageBonus + happinessBonus + friendshipBonus + seasonBonus;
				this.LogMessage($"mat:{buckCountChance};age:{animal.age.Value}|{ageBonus};hap:{happinessBonus}fre:{friendshipBonus};sea:{seasonBonus}");
				result = kitChance;
			}
			return result;
		}
		
		private int GetNumberOfKits(int freeSpace, FarmAnimal animal)
		{
			int result;
			if (!this._config.AllowMultiples)
				result = 1;
			else
			{
				int minValue = -1 + animal.happiness.Value / 80;
				int maxValue = 4 + animal.happiness.Value / 16;
				int value = this._random.Next(minValue, maxValue);
				value = Math.Min(freeSpace, value);
				result = Math.Max(0, value);
			}
			return result;
		}
		
		private void AddKit(Building building)
		{
			FarmAnimal farmAnimal = new FarmAnimal("Rabbit", this.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue().getNewID(), Game1.player.UniqueMultiplayerID) {Name = "Rabbit", home = building};
			building.currentOccupants.Value++;
			farmAnimal.homeLocation.Value = new Vector2(building.tileX.Value, building.tileY.Value);
			farmAnimal.setRandomPosition(farmAnimal.home.indoors.Value);
			(building.indoors.Value as AnimalHouse)?.animals.Add(farmAnimal.myID.Value, farmAnimal);
			(building.indoors.Value as AnimalHouse)?.animalsThatLiveHere.Add(farmAnimal.myID.Value);
		}
		
		private void LogMessage(string msg)
		{
			if (this._config.EnableDebugLogging)
				this.Monitor.Log(msg, LogLevel.Info);
		}
	}
}