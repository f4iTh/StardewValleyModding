using System;
using System.Collections.Generic;
using System.Linq;
using BreedLikeRabbits2.Common.Configs;
using BreedLikeRabbits2.Common.Menus;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;

namespace BreedLikeRabbits2 {
  /// <summary>
  /// The mod entry point.
  /// </summary>
  public class ModEntry : Mod {
    /// <summary>
    /// The mod configuration.
    /// </summary>
    private ModConfig _config;
    /// <summary>
    /// A list containing messages about new rabbits.
    /// </summary>
    private List<string> _dialogueList;
    /// <summary>
    /// A list containing new rabbits to be added.
    /// </summary>
    private List<FarmAnimal> _newRabbits;

    /// <summary>
    /// The mod entry point method.
    /// </summary>
    /// <param name="helper">The mod helper.</param>
    public override void Entry(IModHelper helper) {
      I18n.Init(helper.Translation);

      this._config = helper.ReadConfig<ModConfig>();

      helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
      helper.Events.GameLoop.Saved += this.HandleBreedRabbits;

      // helper.ConsoleCommands.Add("listrabbitinfo", "Prints details of all rabbits on the farm.", this.ListRabbitInfo);
    }

    // private void ListRabbitInfo(string command, string[] args) {
    //   if (args.Length == 0) {
    //     foreach (FarmAnimal rabbit in Game1.getFarm().getAllFarmAnimals().Where(farmAnimal => farmAnimal.type.Value == "Rabbit"))
    //       this.Monitor.Log($"\n\tname: {rabbit.Name}\n\tgender: {(rabbit.isMale() ? "Male" : "Female")}\n\thome id: {(rabbit.home.indoors.Value as AnimalHouse)?.uniqueName.Value}");
    //     return;
    //   }
    //
    //   IEnumerable<FarmAnimal> rabbits;
    //   switch (args[0]) {
    //     case "adult":
    //       rabbits = Game1.getFarm().getAllFarmAnimals().Where(farmAnimal => farmAnimal.type.Value == "Rabbit" && farmAnimal.age.Value >= farmAnimal.ageWhenMature.Value);
    //       break;
    //     case "breeder":
    //       rabbits = Game1.getFarm().getAllFarmAnimals().Where(farmAnimal => farmAnimal.type.Value == "Rabbit" && farmAnimal.age.Value >= farmAnimal.ageWhenMature.Value + 14);
    //       break;
    //     default:
    //       this.Monitor.Log($"unknown argument \"{args[0]}\".", LogLevel.Error);
    //       return;
    //   }
    //
    //   foreach (FarmAnimal rabbit in rabbits)
    //     this.Monitor.Log($"\n\tname: {rabbit.Name}\n\tgender: {(rabbit.isMale() ? "Male" : "Female")}\n\thome id: {(rabbit.home.indoors.Value as AnimalHouse)?.uniqueName.Value}");
    // }

    /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
      new GenericModConfig(
        this.Helper.ModRegistry,
        this.ModManifest,
        () => this._config,
        () => {
          this._config = new ModConfig();
          this.Helper.WriteConfig(this._config);
        },
        () => this.Helper.WriteConfig(this._config)
      ).Register();
    }

    
    /// <inheritdoc cref="IGameLoopEvents.Saved"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void HandleBreedRabbits(object sender, SavedEventArgs e) {
      this._dialogueList = new List<string>();
      this._newRabbits = new List<FarmAnimal>();

      Farm farm = Game1.getFarm();
      FarmAnimal[] buckArray = this._config.IgnoreGender ? ModEntry.GetBreeders(farm).ToArray() : ModEntry.GetMatureMales(farm).ToArray();

      foreach (FarmAnimal parent in farm.getAllFarmAnimals()) {
        if (parent.type.Value != "Rabbit" || parent.age.Value < parent.ageWhenMature.Value + 14 || (parent.isMale() && !this._config.IgnoreGender))
          continue;

        // make sure parents are from the same coop when not ignoring gender
        if (!this._config.IgnoreGender && /* this._config.CheckRabbitsPerCoop && */ !ModEntry.HasMaleAndFemaleInCoop(parent.home.indoors.Value as AnimalHouse))
          continue;

        Building home = parent.home;
        int currentAnimalsCount = ModEntry.GetMaxOccupants(home) - home.currentOccupants.Value;
        // int buckCount = /* !this._config.IgnoreGender && */ this._config.CheckRabbitsPerCoop ? GetBuckCountInTheSameCoopAsParent(parent, buckArray) : buckArray.Length;
        int buckCount = ModEntry.GetBuckCountInTheSameCoopAsParent(parent, buckArray);
        double kitChance = this.GetKitChance(buckCount, parent);
        if (!(Game1.random.NextDouble() <= kitChance) || currentAnimalsCount <= 0)
          continue;

        int kitCount = this.GetKitCount(currentAnimalsCount, parent);
        if (kitCount <= 0)
          continue;

        // increment early to avoid potential overcrowding
        home.currentOccupants.Value += kitCount;

        for (int i = 0; i < kitCount; i++) {
          FarmAnimal newRabbit = this.GenerateNewRabbit(home, parent);
          if (!this._config.NameNewRabbits)
            ModEntry.HandleAddAndNameRabbit(Dialogue.randomName(), newRabbit);
          else
            this._newRabbits.Add(newRabbit);
        }

        this._dialogueList.Add(kitCount > 1 ? I18n.Strings_Birthingmessage_Multiple(parent.Name, kitCount) : I18n.Strings_Birthingmessage_Single(parent.Name));
        // this.Monitor.Log($"\n name: {farmAnimal.Name};\n monthsOld: {farmAnimal.age.Value / 28};\n happiness: {farmAnimal.happiness.Value};\n friendship: {farmAnimal.friendshipTowardFarmer.Value};\n spaceAvailable: {currentAnimalsCount};\n chanceOfLitter: {kitChance}");
      }

      if (this._dialogueList.Count <= 0)
        return;

      Game1.multipleDialogues(this._dialogueList.ToArray());
      if (this._config.NameNewRabbits)
        Game1.afterDialogues = () => Game1.activeClickableMenu ??= new NamingMenuMultiple(ModEntry.HandleDoneNaming, this._newRabbits.ToArray(), this.Helper.Reflection);
    }

    /// <summary>
    /// Gets the chance of a new rabbit.
    /// </summary>
    /// <param name="buckCount">The amount of available breeders..</param>
    /// <param name="farmAnimal">The parent rabbit.</param>
    // TODO: customizable config values?
    private double GetKitChance(int buckCount, FarmAnimal farmAnimal) {
      // this.Monitor.Log($"buckCount: {buckCount}");
      if (buckCount == 0)
        return 0;

      double ageBonus = 0.0;
      double happinessBonus = 0.0;
      double friendshipBonus = 0.0;
      double seasonBonus = 0.0;
      double baseRate = 1.0 / this._config.BaseRate - farmAnimal.daysSinceLastFed.Value / 20.0;
      double rabbitCountChance = buckCount / 100.0;

      if (rabbitCountChance > 0.03)
        rabbitCountChance = 0.03;

      if (this._config.AccountForAge)
        ageBonus = farmAnimal.age.Value / 4200.0;

      if (this._config.AccountForAge && farmAnimal.age.Value > 112)
        ageBonus = 0.04 - farmAnimal.age.Value / 4200.0;

      if (this._config.AccountForHappiness)
        happinessBonus = farmAnimal.happiness.Value / 7200.0;

      if (this._config.AccountForFriendship)
        friendshipBonus = farmAnimal.friendshipTowardFarmer.Value / 60000.0;

      if (this._config.AccountForSeason)
        seasonBonus = Game1.currentSeason switch {
          "spring" => 0.0,
          "summer" => -0.02,
          "fall" => 0.0,
          "winter" => 0.01,
          _ => seasonBonus
        };

      // this.Monitor.Log($"mat:{rabbitCountChance};age:{farmAnimal.age.Value}|{ageBonus};hap:{happinessBonus}fre:{friendshipBonus};sea:{seasonBonus}");
      return baseRate + rabbitCountChance + ageBonus + happinessBonus + friendshipBonus + seasonBonus;
    }

    /// <summary>
    /// Gets the count of rabbits that will be born.
    /// </summary>
    /// <param name="freeSpace">How much free space the coop has.</param>
    /// <param name="animal">The parent rabbit.</param>
    private int GetKitCount(int freeSpace, FarmAnimal animal) {
      if (!this._config.AllowMultiples)
        return 1;

      int minValue = -1 + animal.happiness.Value / 80;
      int maxValue = 4 + animal.happiness.Value / 16;
      int value = Math.Min(freeSpace, Game1.random.Next(minValue, maxValue));

      return Math.Max(0, value);
    }

    /// <summary>
    /// Handles adding new rabbits.
    /// </summary>
    /// <param name="names">The names of new rabbits.</param>
    /// <param name="newRabbits">The new rabbits.</param>
    private static void HandleDoneNaming(string[] names, FarmAnimal[] newRabbits) {
      // int nameCount = names.Length;
      // this.Monitor.Log($"There are {nameCount} name{(nameCount > 1 ? "s" : "")} in total:");

      for (int i = 0; i < names.Length; i++) {
        string currentName = names[i];
        FarmAnimal currentRabbit = newRabbits[i];

        // this.Monitor.Log($"\t{currentName}");
        ModEntry.HandleAddAndNameRabbit(currentName, currentRabbit);
      }
    }

    /// <summary>
    /// Creates a rabbit.
    /// </summary>
    /// <param name="building">The home of the parent rabbit.</param>
    /// <param name="parent">The parent rabbit.</param>
    private FarmAnimal GenerateNewRabbit(Building building, FarmAnimal parent) {
      Multiplayer multiplayer = this.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
      FarmAnimal farmAnimal = new("Rabbit", multiplayer.getNewID(), Game1.player.UniqueMultiplayerID) {
        home = building,
        parentId = {
          Value = parent.myID.Value
        },
        homeLocation = {
          Value = new Vector2(building.tileX.Value, building.tileY.Value)
        }
      };

      return farmAnimal;
    }

    /// <summary>
    /// Handles adding and naming the new rabbit.
    /// </summary>
    /// <param name="name">The name of the rabbit.</param>
    /// <param name="farmAnimal">The new rabbit.</param>
    private static void HandleAddAndNameRabbit(string name, FarmAnimal farmAnimal) {
      string randomName = Dialogue.randomName();

      farmAnimal.Name = !string.IsNullOrWhiteSpace(name) ? name : randomName;
      farmAnimal.displayName = !string.IsNullOrWhiteSpace(name) ? name : randomName;

      farmAnimal.setRandomPosition(farmAnimal.home.indoors.Value);
      (farmAnimal.home.indoors.Value as AnimalHouse)?.animals.Add(farmAnimal.myID.Value, farmAnimal);
      (farmAnimal.home.indoors.Value as AnimalHouse)?.animalsThatLiveHere.Add(farmAnimal.myID.Value);
      // farmAnimal.home.currentOccupants.Value++;
    }

    /// <summary>
    /// Whether a coop has both a female and a male rabbit.
    /// </summary>
    /// <param name="animalHouse">The home of the parents.</param>
    private static bool HasMaleAndFemaleInCoop(AnimalHouse animalHouse) {
      bool hasMale = false;
      bool hasFemale = false;

      foreach (FarmAnimal animal in animalHouse.animals.Values.Cast<FarmAnimal>()) {
        if (hasMale && hasFemale) return true;
        if (animal.isMale()) hasMale = true;
        if (!animal.isMale()) hasFemale = true;
      }

      return hasMale && hasFemale;
    }

    /// <summary>
    /// Gets the max animal count of a coop.
    /// </summary>
    /// <param name="building">The building.</param>
    private static int GetMaxOccupants(Building building) {
      return building.buildingType.Value switch {
        "Coop" => 4,
        "Big Coop" => 8,
        "Deluxe Coop" => 12,
        _ => 0
      };
    }

    /// <summary>
    /// Gets the count of breeders in the same coop as the other parent.
    /// </summary>
    /// <param name="parent">The parent rabbit.</param>
    /// <param name="buckEnumerable">The breeders.</param>
    private static int GetBuckCountInTheSameCoopAsParent(FarmAnimal parent, IEnumerable<FarmAnimal> buckEnumerable) {
      return buckEnumerable.Count(animal => (animal.home.indoors.Value as AnimalHouse)?.uniqueName == (parent.home.indoors.Value as AnimalHouse)?.uniqueName);
    }

    /// <summary>
    /// Gets the count of available breeders on the farm.
    /// </summary>
    /// <param name="farm">The farm location.</param>
    private static IEnumerable<FarmAnimal> GetBreeders(Farm farm) {
      return farm.getAllFarmAnimals().Where(farmAnimal => farmAnimal.type.Value == "Rabbit" && farmAnimal.age.Value >= farmAnimal.ageWhenMature.Value + 14);

      // int totalRabbits = 0;
      // int totalBreeders = 0;
      // foreach (FarmAnimal farmAnimal in farm.getAllFarmAnimals().Where(farmAnimal => farmAnimal.type.Value == "Rabbit")) {
      //   totalRabbits++;
      //   if (farmAnimal.age.Value >= farmAnimal.ageWhenMature.Value + 14)
      //     totalBreeders++;
      // }
      //
      // // this.Monitor.Log($"There are {totalRabbits} Rabbits. {totalBreeders} breeders.");
      //
      // return totalBreeders;
    }

    /// <summary>
    /// Gets the count of adult male rabbits on the farm.
    /// </summary>
    /// <param name="farm">The farm location.</param>
    private static IEnumerable<FarmAnimal> GetMatureMales(Farm farm) {
      return farm.getAllFarmAnimals().Where(farmAnimal => farmAnimal.type.Value == "Rabbit" && farmAnimal.isMale() && !farmAnimal.isBaby());
    }
  }
}