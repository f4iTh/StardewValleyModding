using ActivateSprinklers.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace ActivateSprinklers
{
	internal class ActivateSprinklers : Mod
    {
        #region fields
        private ModConfig Config;
		private ModIntegrations Integrations;
		private IDictionary<int, Vector2[]> StaticSprinklerCoverage;
		#endregion

		public override void Entry(IModHelper helper)
        {
			helper.Events.GameLoop.GameLaunched += this.Initialize;
            helper.Events.Input.ButtonPressed += this.ButtonPressed;

			this.Config = this.Helper.ReadConfig<ModConfig>();
		}

		private void Initialize(object sender, EventArgs e)
		{
			this.Integrations = new ModIntegrations(this.Monitor, this.Helper.ModRegistry, this.Helper.Reflection);
			this.StaticSprinklerCoverage = this.GetStaticSprinklerTiles(this.Integrations);
		}

		private void OnTick(object sender, UpdateTickedEventArgs e)
		{
			if (!Context.IsWorldReady || Game1.currentLocation == null)
				return;

			if (e.IsMultipleOf(30))
			{
				MouseState mouse = Mouse.GetState();
				GamePadState gamepad = GamePad.GetState(PlayerIndex.One);

				int oldPower = Game1.player.toolPower;
				float stamina = Game1.player.Stamina;
				SObject sprinkler;

				if ((mouse.RightButton == ButtonState.Pressed || gamepad.Buttons.A == ButtonState.Pressed) && Game1.player.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out SObject sobj) /*&& this.IsSprinkler(sobj)*/)
				{
					sprinkler = sobj;
					if (this.Config.EXPERIMENTAL_NOT_RECOMMENDED_AllowTheSprinklersToMakeTheGameLagLikeTheresNoTomorrow && !Game1.options.gamepadControls)
					{
						Game1.player.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out SObject Sobj);
						sprinkler = Sobj;
					}

					WateringCan can = new WateringCan { WaterLeft = 100 };
					Game1.player.toolPower = 0;

					foreach (var tile in this.GetCoverageTiles(sprinkler, sprinkler.TileLocation, this.GetCurrentSprinklerTiles(this.StaticSprinklerCoverage)))
					{
						can.DoFunction(Game1.player.currentLocation, (int)tile.X * 64, (int)tile.Y * 64, 0, Game1.player);
						can.WaterLeft = can.waterCanMax;
						Game1.player.Stamina = stamina;
					}
					Game1.player.toolPower = oldPower;
				}
			}
		}

		private void ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Context.IsWorldReady || Game1.currentLocation == null)
				return;
			
			int oldPower = Game1.player.toolPower;
			float stamina = Game1.player.Stamina;
			SObject sprinkler;

			if (e.Button.IsActionButton() && Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out SObject sobj) /*&& this.IsSprinkler(sobj)*/)
			{
				sprinkler = sobj;

				if (this.Config.AbleToActivateSprinklersWithoutBeingDirectlyInFrontOfOne && !Game1.options.gamepadControls)
				{
					Game1.player.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out SObject Sobj);
					sprinkler = Sobj;
				}

				//if (!this.Config.LimitSprinklerActivationToClicksOnly)
				//	this.Helper.Events.GameLoop.UpdateTicked += this.OnTick;

				WateringCan can = new WateringCan { WaterLeft = 100 };
				Game1.player.toolPower = 0;

				if (this.Integrations.PrismaticTools.IsLoaded && this.Integrations.PrismaticTools.IsScarecrow() && sprinkler.ParentSheetIndex.Equals(this.Integrations.PrismaticTools.GetSprinklerID()))
					this.Helper.Input.Suppress(e.Button);

				foreach (var tile in this.GetCoverageTiles(sprinkler, sprinkler.TileLocation, this.GetCurrentSprinklerTiles(this.StaticSprinklerCoverage)))
				{
					can.DoFunction(Game1.player.currentLocation, (int)tile.X * 64, (int)tile.Y * 64, 0, Game1.player);
					can.WaterLeft = can.waterCanMax;
					Game1.player.Stamina = stamina;
				}
				Game1.player.toolPower = oldPower;
			}
		}

		/******************
		* From DataLayers *
		******************/

		// https://github.com/Pathoschild/StardewMods/blob/develop/DataLayers/Layers/Coverage/SprinklerLayer.cs#L132
		private IDictionary<int, Vector2[]> GetStaticSprinklerTiles(ModIntegrations integrations)
		{
			IDictionary<int, Vector2[]> tiles = new Dictionary<int, Vector2[]>();
			{
				Vector2 center = Vector2.Zero;

				tiles[599] = Utility.getAdjacentTileLocations(center).Concat(new[] { center }).ToArray();

				tiles[621] = Utility.getSurroundingTileLocationsArray(center).Concat(new[] { center }).ToArray();

				List<Vector2> iridiumTiles = new List<Vector2>();
				for (int x = -2; x <= 2; x++)
				{
					for (int y = -2; y <= 2; y++)
						iridiumTiles.Add(new Vector2(x, y));
				}
				tiles[645] = iridiumTiles.ToArray();
			}

			if (integrations.PrismaticTools.IsLoaded)
				tiles[integrations.PrismaticTools.GetSprinklerID()] = integrations.PrismaticTools.GetSprinklerCoverage().ToArray();

			if (integrations.Cobalt.IsLoaded)
				tiles[integrations.Cobalt.GetSprinklerID()] = integrations.Cobalt.GetSprinklerTiles().ToArray();

			if (integrations.SimpleSprinkler.IsLoaded)
			{
				foreach (var pair in integrations.SimpleSprinkler.GetSprinklerTiles())
				{
					int id = pair.Key;
					if (tiles.TryGetValue(id, out Vector2[] currentTiles))
						tiles[id] = currentTiles.Union(pair.Value).ToArray();
					else
						tiles[id] = pair.Value;
				}
			}
			return tiles;
		}

		private IDictionary<int, Vector2[]> GetCurrentSprinklerTiles(IDictionary<int, Vector2[]> staticTiles)
		{
			if (!this.Integrations.BetterSprinklers.IsLoaded && !this.Integrations.LineSprinklers.IsLoaded) return staticTiles;

			IDictionary<int, Vector2[]> tilesBySprinklerID = new Dictionary<int, Vector2[]>(staticTiles);
			if (this.Integrations.BetterSprinklers.IsLoaded)
			{
				foreach (var pair in this.Integrations.BetterSprinklers.GetSprinklerTiles())
					tilesBySprinklerID[pair.Key] = pair.Value;
			}
			if (this.Integrations.LineSprinklers.IsLoaded)
			{
				foreach (var pair in this.Integrations.LineSprinklers.GetSprinklerTiles())
					tilesBySprinklerID[pair.Key] = pair.Value;
			}
			return tilesBySprinklerID;
		}


		// https://github.com/Pathoschild/StardewMods/blob/develop/DataLayers/Layers/Coverage/SprinklerLayer.cs#L182
		private IEnumerable<Vector2> GetCoverageTiles(SObject sprinkler, Vector2 origin, IDictionary<int, Vector2[]> radius)
		{
			if (!radius.TryGetValue(sprinkler.ParentSheetIndex, out Vector2[] tiles))
				throw new NotSupportedException($"Unknown sprinkler ID: {sprinkler.ParentSheetIndex}");

			foreach (Vector2 tile in tiles)
				yield return origin + tile;
		}

		// https://github.com/Pathoschild/StardewMods/blob/develop/DataLayers/Layers/Coverage/SprinklerLayer.cs#L118
		//private bool IsSprinkler(SObject obj)
		//{
		//	return obj != null && this.StaticSprinklerCoverage.ContainsKey(obj.ParentSheetIndex);
		//}
    }
}