using CustomWarps.Common;
using CustomWarps.Common.Configs;
using CustomWarps.Common.Enums;
using CustomWarps.Common.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CustomWarps {
  /// <summary>
  /// The mod entry point.
  /// </summary>
  public class ModEntry : Mod {
    /// <inheritdoc cref="IMonitor"/>
    internal static IMonitor StaticLogger;
    /// <summary>
    /// The mod configuration.
    /// </summary>
    private ModConfig _config;
    // private WarpHelper _warpHelper;
    
    /// <summary>
    /// The mod entry point method.
    /// </summary>
    /// <param name="helper">The mod helper.</param>
    public override void Entry(IModHelper helper) {
      I18n.Init(helper.Translation);

      this._config = helper.ReadConfig<ModConfig>();
      // this._warpHelper = new WarpHelper(helper.Data);
      StaticLogger = this.Monitor;

      helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
      helper.Events.Input.ButtonPressed += this.HandleOpenWarpMenu;

      //helper.ConsoleCommands.Add("addwarp", "Adds a per-save warp for the current save.", this.AddWarpCommand);
      //helper.ConsoleCommands.Add("aw", "Adds a per-save warp for the current save. [Alias of addwarp].", this.AddWarpCommand);
      //helper.ConsoleCommands.Add("cwarp", "Warps the player to a custom warp point.", this.CustomWarpCommand);
      //helper.ConsoleCommands.Add("cw", "Warps the player to a custom warp point. [Alias of cwarp].", this.CustomWarpCommand);
      //helper.ConsoleCommands.Add("listwarps", "Lists the warps for the current save file.", this.ListCustomWarpsCommand);
      //helper.ConsoleCommands.Add("lw", "Lists the warps for the current save file. [Alias of listwarps].", this.ListCustomWarpsCommand);
      //helper.ConsoleCommands.Add("currentposition", "Shows the current tile position of the player.", this.ShowTilePositionCommand);
      //helper.ConsoleCommands.Add("cpos", "Shows the current tile position of the player. [Alias of currentposition].", this.ShowTilePositionCommand);
      //helper.ConsoleCommands.Add("sortby", "Switch the sorting style for warps.", this.SwitchSortStyleCommand);
    }

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

    private void HandleOpenWarpMenu(object sender, ButtonPressedEventArgs e) {
      if (!Context.IsWorldReady || Game1.player.currentLocation == null || Game1.activeClickableMenu != null || !Context.IsPlayerFree || !this._config.ToggleKey.JustPressed())
        return;

      WarpHelper warpHelper = new(this.Helper.Data);
      Game1.activeClickableMenu = this._config.MenuStyle switch {
        MenuStyle.LegacyGrid => new GridWarpMenu(this.Helper, warpHelper, maxItemsPerColumn: this._config.MaxGridColumns),
        MenuStyle.VerticalList => new VerticalListWarpMenu(this.Helper, warpHelper),
        _ => Game1.activeClickableMenu
      };
    }

    //private void SwitchSortStyleCommand(string command, string[] args)
    //{
    //	try
    //	{
    //		if (Enum.TryParse(args[0], out WarpHelper.SortStyle which))
    //		{
    //			WarpHelper.SwitchSort(which);
    //		}
    //	}
    //	catch (Exception e)
    //	{
    //		this.Monitor.Log($"Something went wrong!\n{e}");
    //	}
    //}

    // private void ShowTilePositionCommand(string command, string[] args) {
    // 	if (!Context.IsWorldReady || Game1.player == null)
    // 		return;
    // 	this.Monitor.Log($"X-tile: {(int) Game1.player.Position.X / 64}, Y-tile: {(int) Game1.player.Position.Y / 64}", LogLevel.Info);
    // }
    //
    // private void ListCustomWarpsCommand(string command, string[] args) {
    // 	if (!Context.IsWorldReady || Game1.player == null)
    // 		return;
    // 	this.Monitor.Log($"Custom warps for {Game1.player.Name} at {Game1.player.farmName.Value}:", LogLevel.Info);
    // 	foreach (KeyValuePair<string, CustomWarp> pair in WarpHelper.CustomWarps)
    // 		this.Monitor.Log($"  - {pair.Key} > {pair.Value.MapName} at [{pair.Value.XCoordinate}, {pair.Value.YCoordinate}]", LogLevel.Info);
    // }
    //
    // private void CustomWarpCommand(string command, string[] args) {
    // 	if (!Context.IsWorldReady || Game1.player == null)
    // 		return;
    // 	if (!WarpHelper.HasKey(args[0])) return;
    // 	CustomWarp warp = WarpHelper.GetWarp(args[0]);
    // 	Game1.warpFarmer(warp.MapName, warp.XCoordinate, warp.YCoordinate, false);
    // 	this.Monitor.Log($"Warped {Game1.player.Name} to {warp.MapName} at [{warp.XCoordinate}, {warp.YCoordinate}]", LogLevel.Info);
    // }
    //
    // private void AddWarpCommand(string command, string[] args) {
    // 	if (!Context.IsWorldReady || Game1.player == null)
    // 		return;
    // 	CustomWarp warp = new CustomWarp();
    // 	string warpName = "";
    // 	string mapName = Game1.player.currentLocation.Name;
    // 	bool isGlobal = false;
    // 	bool isBuilding = false;
    // 	try {
    // 		if (args.Count() != 2) {
    // 			this.Monitor.Log("Invalid amount of arguments!", LogLevel.Error);
    // 			return;
    // 		}
    //
    // 		warpName = args[0];
    // 		isGlobal = bool.Parse(args[1]);
    // 		foreach (Farm farm in this.Helper.Multiplayer.GetActiveLocations().OfType<Farm>())
    // 		foreach (Building building in farm.buildings.Where(building => Game1.player.currentLocation == building.indoors.Value)) {
    // 			mapName = building.indoors.Value.uniqueName.Value;
    // 			isBuilding = true;
    // 			break;
    // 		}
    //
    // 		warp = new CustomWarp {
    // 			WarpName = warpName,
    // 			MapName = mapName,
    // 			XCoordinate = (int) Game1.player.Position.X / 64,
    // 			YCoordinate = (int) Game1.player.Position.Y / 64,
    // 			IsGlobal = isGlobal,
    // 			IsBuilding = isBuilding
    // 		};
    // 	}
    // 	catch (Exception e) {
    // 		this.Monitor.Log($"Something went wrong!\n{e}", LogLevel.Error);
    // 	}
    //
    // 	if (string.IsNullOrEmpty(warpName)) return;
    // 	WarpHelper.Add(warpName, warp, isGlobal);
    // 	this.Monitor.Log($"Successfully added warp to '{warpName}' at [{warp.XCoordinate}, {warp.YCoordinate}]!", LogLevel.Info);
    // }
  }
}