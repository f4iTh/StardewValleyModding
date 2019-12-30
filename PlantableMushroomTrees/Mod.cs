using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Linq;
using xTile.Dimensions;

namespace PlantableMushroomTrees
{
	public class Mod : StardewModdingAPI.Mod
    {
        private ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            helper.Events.Input.ButtonPressed += this.ButtonPressed;
			helper.Events.Display.RenderingHud += this.RenderingHud;
			helper.ConsoleCommands.Add("pmt", "The main command for Plantable Mushroom Trees\narguments:\n" +
									   "  - altkey/requirealtkey: Toggles setting for requiring alt key to plant.\n" +
									   "  - showgrid/showplantinggrid: Toggles setting for showing planting grid.\n" +
									   "  - instant/instantmushroomtree: Toggles setting for instant mushroom trees.\n" +
									   "  - status: Shows the current config settings.", this.PlantableMushroomTreeCommand);
		}

		private void PlantableMushroomTreeCommand(string command, string[] args)
		{
			try
			{
				if (args.Count() == 1)
				{
					switch (args[0].ToLower())
					{
						case "altkey":
						case "requirealtkey":
							this.Config.RequireAltKey = !this.Config.RequireAltKey;
							string message1 = this.Config.RequireAltKey == true ? "Planting requires holding left-alt." : "Planting no longer requires holding left-alt.";
							Helper.Data.WriteJsonFile<ModConfig>("config.json", Config);
							Monitor.Log(message1, LogLevel.Info);
							return;
						case "showgrid":
						case "showplantinggrid":
							this.Config.ShowPlantingGrid = !this.Config.ShowPlantingGrid;
							string message2 = this.Config.ShowPlantingGrid == true ? "Showing planting grid." : "No longer showing planting grid.";
							Helper.Data.WriteJsonFile<ModConfig>("config.json", Config);
							Monitor.Log(message2, LogLevel.Info);
							return;
						case "instant":
						case "instantmushroomtree":
							this.Config.InstantMushroomTree = !this.Config.InstantMushroomTree;
							string message3 = this.Config.InstantMushroomTree == true ? "Planted mushroom trees will grow instantly." : "Planted mushroom trees will no longer grow instantly.";
							Helper.Data.WriteJsonFile<ModConfig>("config.json", Config);
							Monitor.Log(message3, LogLevel.Info);
							return;
						case "status":
							Monitor.Log($"Current config:\n" +
										$"  - InstantMushroomTree: {this.Config.InstantMushroomTree}\n" +
										$"  - ShowPlantingGrid: {this.Config.ShowPlantingGrid}\n" +
										$"  - RequireAltKey: {this.Config.RequireAltKey}", LogLevel.Info);
							return;
						default:
							Monitor.Log("No command found!", LogLevel.Info);
							return;
					}
				}
				else
				{
					Monitor.Log("No command found!\n- Use 'help pmt' for help.", LogLevel.Info);
					return;
				}
			}
			catch (Exception ex)
			{
				Monitor.Log(ex.Message, LogLevel.Error);
				return;
			}
		}

		private void RenderingHud(object sender, RenderingHudEventArgs e)
		{
			if (!Context.IsWorldReady || Game1.player.currentLocation == null || Game1.activeClickableMenu != null)
				return;
			Item item = Game1.player.CurrentItem;
			if (item == null)
				return;
			if (!(item.ParentSheetIndex == 420 || item.ParentSheetIndex == 422))
				return;
			if (!this.Config.ShowPlantingGrid)
				return;
			drawPlacementBounds((StardewValley.Object)item, e.SpriteBatch, Game1.player.currentLocation);
		}

		private void ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Context.IsWorldReady || Game1.currentLocation == null || Game1.player.CurrentItem == null)
				return;
			Item item = Game1.player.CurrentItem;
			GameLocation location = Game1.player.currentLocation;
			if (!(item.ParentSheetIndex == 420 || item.ParentSheetIndex == 422))
				return;
			if (e.Button.IsActionButton() && Game1.player.currentLocation.terrainFeatures.TryGetValue(e.Cursor.GrabTile, out TerrainFeature tf) && tf is HoeDirt hd && hd.state.Value != 2 && hd.crop == null)
			{
				Vector2 tree = e.Cursor.GrabTile;
				if (this.Config.RequireAltKey && !Helper.Input.IsDown(SButton.LeftAlt))
					return;
				if (item != null && Game1.player.CurrentItem.Stack > 0 && playerCanPlaceItemHere(Game1.player.currentLocation, item, (int)e.Cursor.GrabTile.X*64, (int)e.Cursor.GrabTile.Y*64, Game1.player) && canPlantThisSeedHere(item.ParentSheetIndex, (int)e.Cursor.GrabTile.X, (int)e.Cursor.GrabTile.Y, false) && canBePlacedHere((StardewValley.Object)item, Game1.player.currentLocation, e.Cursor.GrabTile))
				{
					this.Helper.Input.Suppress(e.Button);
					tryToPlaceItem(location, item, (int)e.Cursor.GrabTile.X*64, (int)e.Cursor.GrabTile.Y*64);
				}
			}
		}

		public bool canPlantThisSeedHere(int objectIndex, int tileX, int tileY, bool isFertilizer = false)
		{
			if (!(objectIndex == 420 || objectIndex == 422))
				return false;
			if (Game1.player.currentLocation.terrainFeatures.TryGetValue(new Vector2(tileX, tileY), out TerrainFeature tf) && tf is HoeDirt hd && hd.crop == null)
			{	
				Crop crop = new Crop(objectIndex, tileX, tileY);
				if (!Game1.currentLocation.IsOutdoors || Game1.currentLocation.IsGreenhouse)
					return !crop.raisedSeeds.Value || !Utility.doesRectangleIntersectTile(Game1.player.GetBoundingBox(), tileX, tileY);
				if (objectIndex == 309 || objectIndex == 310 || objectIndex == 311 || objectIndex == 420 || objectIndex == 422)
					return true;
				if (Game1.didPlayerJustClickAtAll() && !Game1.doesHUDMessageExist(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924")))
				{
					Game1.playSound("cancel");
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924"));
				}
			}
			return false;
		}

		public bool canBePlacedHere(StardewValley.Object o, GameLocation l, Vector2 tile)
		{
			if (!(o.ParentSheetIndex == 420 || o.ParentSheetIndex == 422))
				return false;
			if (o.ParentSheetIndex == 710 && l.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Water", "Back") != null && (!l.objects.ContainsKey(tile) && l.doesTileHaveProperty((int)tile.X + 1, (int)tile.Y, "Water", "Back") != null) && l.doesTileHaveProperty((int)tile.X - 1, (int)tile.Y, "Water", "Back") != null || l.doesTileHaveProperty((int)tile.X, (int)tile.Y + 1, "Water", "Back") != null && l.doesTileHaveProperty((int)tile.X, (int)tile.Y - 1, "Water", "Back") != null || (o.ParentSheetIndex == 105 && (o.bigCraftable.Value && (l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is Tree) && !l.objects.ContainsKey(tile) || o != null && o.name.Contains("Bomb") && (!l.isTileOccupiedForPlacement(tile, o) || l.isTileOccupiedByFarmer(tile) != null))))
				return true;
			if (((o.Category == -74 || o.Category == -19) || (o.ParentSheetIndex == 420 || o.ParentSheetIndex == 422)) && !l.isTileHoeDirt(tile))
			{
				switch (o.ParentSheetIndex)
				{
					case 309:
					case 310:
					case 311:
					case 628:
					case 629:
					case 630:
					case 631:
					case 632:
					case 633:
					case 420:
					case 422:
						return !isTileOccupiedForPlacement(l, tile, o);
					default:
						return false;
				}
			}
			else
			{
				if (o.Category == -19 && l.isTileHoeDirt(tile) && (l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is HoeDirt && (l.terrainFeatures[tile] as HoeDirt).fertilizer.Value != 0 || l.objects.ContainsKey(tile) && l.objects[tile] is IndoorPot && (l.objects[tile] as IndoorPot).hoeDirt.Value.fertilizer.Value != 0))
					return false;
				return !isTileOccupiedForPlacement(l, tile, o);
			}
		}

		public void drawPlacementBounds(StardewValley.Object obj, SpriteBatch spriteBatch, GameLocation location)
		{
			if (!(obj.ParentSheetIndex == 420 || obj.ParentSheetIndex == 422))
				return;
			int x = Game1.getOldMouseX() + Game1.viewport.X;
			int y = Game1.getOldMouseY() + Game1.viewport.Y;
			if ((double)Game1.mouseCursorTransparency == 0.0)
			{
				x = (int)Game1.player.GetGrabTile().X * 64;
				y = (int)Game1.player.GetGrabTile().Y * 64;
			}
			if (Game1.player.GetGrabTile().Equals(Game1.player.getTileLocation()) && (double)Game1.mouseCursorTransparency == 0.0)
			{
				Vector2 translatedVector2 = Utility.getTranslatedVector2(Game1.player.GetGrabTile(), Game1.player.FacingDirection, 1f);
				x = (int)translatedVector2.X * 64;
				y = (int)translatedVector2.Y * 64;
			}
			KeyboardState kb = Game1.GetKeyboardState();
			bool flag;
			if (this.Config.RequireAltKey) flag = playerCanPlaceItemHere(location, (Item)obj, x, y, Game1.player) && (this.Config.RequireAltKey && kb.IsKeyDown(Keys.LeftAlt));
			else flag = playerCanPlaceItemHere(location, (Item)obj, x, y, Game1.player);
			spriteBatch.Draw(Game1.mouseCursors, new Vector2((float)(x / 64 * 64 - Game1.viewport.X), (float)(y / 64 * 64 - Game1.viewport.Y)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(flag ? 194 : 210, 388, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
			//obj.draw(spriteBatch, x / 64, y / 64, 0.5f);
		}

		public bool playerCanPlaceItemHere(GameLocation location, Item item, int x, int y, Farmer f)
		{
			if (item == null || item is Tool || (Game1.eventUp || (f.bathingClothes.Value)) || !Utility.withinRadiusOfPlayer(x, y, 1, f) && (!Utility.withinRadiusOfPlayer(x, y, 2, f) || !Game1.isAnyGamePadButtonBeingPressed() || (double)Game1.mouseCursorTransparency != 0.0) && (!(item is Furniture) && !(item is Wallpaper) || !(location is DecoratableLocation)))
				return false;
			if (!(item.ParentSheetIndex == 420 || item.ParentSheetIndex == 422))
				return false;
			Vector2 vector2 = new Vector2((float)(x / 64), (float)(y / 64));
			if (canBePlacedHere((StardewValley.Object)item, location, vector2))
			{
				if (!((StardewValley.Object)item).isPassable())
				{
					foreach (Character farmer in location.farmers)
					{
						if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)vector2.X * 64, (int)vector2.Y * 64, 64, 64)))
							return false;
					}
				}
				if (isViableSeedSpot(location, vector2, item))
					return true;
			}
			return false;
		}

		public bool isTileOccupiedForPlacement(GameLocation l, Vector2 tileLocation, StardewValley.Object toPlace = null)
		{
			StardewValley.Object @object;
			l.objects.TryGetValue(tileLocation, out @object);
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			Microsoft.Xna.Framework.Rectangle boundingBox;
			for (int index = 0; index < l.characters.Count; ++index)
			{
				if (l.characters[index] != null)
				{
					boundingBox = l.characters[index].GetBoundingBox();
					if (boundingBox.Intersects(rectangle))
						return true;
				}
			}
			if (l.isTileOccupiedByFarmer(tileLocation) != null && (toPlace == null || !toPlace.isPassable()))
				return true;
			if ((NetCollection<LargeTerrainFeature>)l.largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in l.largeTerrainFeatures)
				{
					boundingBox = largeTerrainFeature.getBoundingBox();
					if (boundingBox.Intersects(rectangle))
						return true;
				}
			}
			if (l.terrainFeatures.ContainsKey(tileLocation) && rectangle.Intersects(l.terrainFeatures[tileLocation].getBoundingBox(tileLocation)) && (!l.terrainFeatures[tileLocation].isPassable((Character)null) || l.terrainFeatures[tileLocation] is HoeDirt && ((HoeDirt)l.terrainFeatures[tileLocation]).crop != null || toPlace != null && toPlace.name.Contains("Sapling")) || !l.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport) && (toPlace == null || !(toPlace is Wallpaper)))
				return true;
			if (toPlace != null && (toPlace.Category == -74 || toPlace.Category == -19) && (@object != null && @object is IndoorPot && (@object as IndoorPot).hoeDirt.Value.canPlantThisSeedHere(toPlace.ParentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, toPlace.Category == -19)))
				return false;
			return @object != null;
		}

		public bool isViableSeedSpot(GameLocation location, Vector2 tileLocation, Item item)
		{
			if (!(item.ParentSheetIndex == 420 || item.ParentSheetIndex == 422))
				return false;
			if (location.terrainFeatures.ContainsKey(tileLocation) && location.terrainFeatures[tileLocation] is HoeDirt && canPlantThisSeedHere((item as StardewValley.Object).ParentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, false))
				return true;
			if (location.isTileHoeDirt(tileLocation) || !location.terrainFeatures.ContainsKey(tileLocation))
				return ((StardewValley.Object)item).isWildTreeSeed();
			return false;
		}

		public void tryToPlaceItem(GameLocation location, Item item, int x, int y)
		{
			if (item is Tool)
				return;
			Vector2 vector2 = new Vector2((float)(x / 64), (float)(y / 64));
			if (playerCanPlaceItemHere(location, item, x, y, Game1.player))
			{
				if (item is Furniture)
					Game1.player.ActiveObject = (StardewValley.Object)null;
				if (this.Config.RequireAltKey && !Game1.GetKeyboardState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt))
					return;
				if (playerCanPlaceItemHere(location, item, x, y, Game1.player) && placementAction((StardewValley.Object)item, location, x, y, Game1.player))
					Game1.player.reduceActiveItemByOne();
				else
				{
					if (!(item is Furniture))
						return;
					Game1.player.ActiveObject = (StardewValley.Object)(item as Furniture);
				}
			}
			else
				Utility.withinRadiusOfPlayer(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y, 3, Game1.player);
		}

		public bool placementAction(StardewValley.Object obj, GameLocation location, int x, int y, Farmer who = null)
		{
			if (!(obj.ParentSheetIndex == 420 || obj.ParentSheetIndex == 422))
				return false;
			Vector2 index1 = new Vector2((float)(x / 64), (float)(y / 64));
			switch (obj.ParentSheetIndex)
			{
				case 420:
				case 422:
					bool flag = location.terrainFeatures.ContainsKey(index1) && location.terrainFeatures[index1] is HoeDirt && (location.terrainFeatures[index1] as HoeDirt).crop == null;
					string str = location.doesTileHaveProperty((int)index1.X, (int)index1.Y, "NoSpawn", "Back");
					if (!flag && (location.objects.ContainsKey(index1) || location.terrainFeatures.ContainsKey(index1) || !(location is Farm) && !location.IsGreenhouse || str != null && (str.Equals("Tree") || str.Equals("All") || str.Equals("True"))))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13021"));
						return false;
					}
					if (str != null && (str.Equals("Tree") || str.Equals("All") || str.Equals("True")))
						return false;
					if (flag || location.isTileLocationOpen(new Location(x * 64, y * 64)) && !location.isTileOccupied(new Vector2((float)x, (float)y), "") && location.doesTileHaveProperty(x, y, "Water", "Back") == null)
					{
						location.terrainFeatures.Remove(index1);
						if (this.Config.InstantMushroomTree)
							location.terrainFeatures.Add(index1, (TerrainFeature)new Tree(7, 5));
						else
							location.terrainFeatures.Add(index1, (TerrainFeature)new Tree(7, 0));
						location.playSound("dirtyHit");
						return true;
					}
					break;
				default:
					return false;
			}
			return false;
		}
	}
}