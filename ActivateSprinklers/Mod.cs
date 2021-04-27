using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace ActivateSprinklers {

	public class ActivateSprinklers : Mod {
		private ModConfig Config;
		private ModIntegrations Integrations;
		private IDictionary<int, Vector2[]> CustomSprinklerCoverage;

		private bool IsSprinkler(SObject obj) => obj.IsSprinkler() || obj.bigCraftable.Value && this.CustomSprinklerCoverage.ContainsKey(obj.ParentSheetIndex);

		private bool IsReady() => Context.IsWorldReady && Game1.currentLocation != null && Game1.player.CanMove && !Game1.player.hasMenuOpen.Value;

		public override void Entry(IModHelper helper) {
			this.Config = helper.ReadConfig<ModConfig>();

			IModEvents events = helper.Events;
			events.GameLoop.GameLaunched += this.Initialize;
			events.Input.ButtonsChanged += this.OnButtonsChanged;
			events.GameLoop.UpdateTicking += this.OnGameTick;

			//helper.ConsoleCommands.Add("as_infinitereach", "Toggles infinite reach.", delegate(string function, string[] args) { this.Config.InfiniteReach = !this.Config.InfiniteReach; });
		}

		private void OnGameTick(object sender, UpdateTickingEventArgs e) {
			if (!IsReady())
				return;
			this.CustomSprinklerCoverage = this.GetCustomSprinklerCoverage();
		}

		private void Initialize(object sender, EventArgs e) {
			this.Integrations = new ModIntegrations(this.Monitor, this.Helper.ModRegistry);
			this.CustomSprinklerCoverage = this.GetCustomSprinklerCoverage();

			new GenericModConfig(
				modRegistry: this.Helper.ModRegistry,
				monitor: this.Monitor,
				manifest: this.ModManifest,
				getConfig: () => this.Config,
				reset: () => {
					this.Config = new ModConfig();
					this.Helper.WriteConfig(this.Config);
					this.Helper.ReadConfig<ModConfig>();
				},
				saveAndApply: () => {
					this.Helper.WriteConfig(this.Config);
					this.Helper.ReadConfig<ModConfig>();
				}
			);
		}

		private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e) {
			SButton actionButton;
			if (!IsReady() || (actionButton = e.Pressed.FirstOrDefault(n => n.IsActionButton())) == default)
				return;
			Vector2 tile = Game1.options.gamepadControls ? Game1.player.GetGrabTile() : this.Config.InfiniteReach ? Game1.currentCursorTile : e.Cursor.GrabTile;
			if (!Game1.player.currentLocation.Objects.TryGetValue(tile, out SObject sprinkler) || !this.IsSprinkler(sprinkler)) {
				//if (IsBuffered) {
				//	if (this.Integrations.PrismaticTools.IsLoaded && this.Integrations.PrismaticTools.IsScarecrow() && sprinkler.ParentSheetIndex.Equals(this.Integrations.PrismaticTools.GetSprinklerID()))
				//		this.Helper.Input.Suppress(actionButton);

				//	Monitor.Log("buffered input", LogLevel.Debug);

				//	if (ActivateSprinkler(sprinkler))
				//		IsBuffered = false;
				//}
				return;
			}
			if (this.Integrations.PrismaticTools.IsLoaded && this.Integrations.PrismaticTools.IsScarecrow() && sprinkler.ParentSheetIndex.Equals(this.Integrations.PrismaticTools.GetSprinklerID()))
				this.Helper.Input.Suppress(actionButton);

			ActivateSprinkler(sprinkler);
		}

		private void ActivateSprinkler(SObject sprinkler) {
			int oldPower = Game1.player.toolPower;
			float currentStamina = Game1.player.Stamina;
			WateringCan can = new WateringCan { WaterLeft = 100 };
			Game1.player.toolPower = 0;

			foreach (Vector2 tile in this.GetCoverage(sprinkler, sprinkler.TileLocation, this.CustomSprinklerCoverage)) {
				can.DoFunction(Game1.player.currentLocation, (int)tile.X * 64, (int)tile.Y * 64, 0, Game1.player);
				can.WaterLeft = can.waterCanMax;
				Game1.player.Stamina = currentStamina;
			}
			Game1.player.toolPower = oldPower;
		}

		private IDictionary<int, Vector2[]> GetCustomSprinklerCoverage() {
			IDictionary<int, Vector2[]> sprinklerCoverage = new Dictionary<int, Vector2[]>();

			if (this.Integrations.BetterSprinklers.IsLoaded)
				foreach (KeyValuePair<int, Vector2[]> kvp in this.Integrations.BetterSprinklers.GetSprinklerTiles())
					sprinklerCoverage[kvp.Key] = kvp.Value;
			if (this.Integrations.LineSprinklers.IsLoaded)
				foreach (KeyValuePair<int, Vector2[]> kvp in this.Integrations.LineSprinklers.GetSprinklerTiles())
					sprinklerCoverage[kvp.Key] = kvp.Value;
			if (this.Integrations.SimpleSprinkler.IsLoaded)
				foreach (KeyValuePair<int, Vector2[]> kvp in this.Integrations.SimpleSprinkler.GetSprinklerTiles())
					sprinklerCoverage[kvp.Key] = kvp.Value;

			return sprinklerCoverage;
		}

		private IEnumerable<Vector2> GetCoverage(SObject sprinkler, Vector2 origin, IDictionary<int, Vector2[]> customSprinklerCoverage) {
			IEnumerable<Vector2> sprinklerCoverage = sprinkler.GetSprinklerTiles();

			if (customSprinklerCoverage.TryGetValue(sprinkler.ParentSheetIndex, out Vector2[] sprinklerCoverageTiles))
				sprinklerCoverage = new HashSet<Vector2>(sprinklerCoverage.Concat(sprinklerCoverageTiles.Select(tile => tile + origin)));

			return sprinklerCoverage;
		}
	}

	//public class BufferedInput {
	//	public SButton Button;
	//	public GameTime TimeStamp;

	//	public BufferedInput(SButton sButton, GameTime timeStamp) {
	//		this.Button = sButton;
	//		this.TimeStamp = timeStamp;
	//	}

	//	public bool IsValid() => this.TimeStamp.TotalGameTime.TotalSeconds + 3f >= Game1.currentGameTime.TotalGameTime.TotalSeconds;

	//	public override string ToString() => $"button:{Button}; timestamp:{TimeStamp.TotalGameTime.TotalSeconds}s";
	//}
}