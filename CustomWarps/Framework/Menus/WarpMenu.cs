using CustomWarps.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomWarps.Framework.Menus
{
	public class WarpMenu : IClickableMenu
	{
		private ClickableTextureComponent previousPageButton;
		private ClickableTextureComponent nextPageButton;
		private ClickableTextureComponent addWarpButton;
		private bool isAddingNewWarp;
		private ClickableTextureComponent removeWarpButton;
		private bool showRemoveWarpButtons;
		private List<Dictionary<ClickableTextureComponent, CustomWarp>> pagesOfCustomWarps = new List<Dictionary<ClickableTextureComponent, CustomWarp>>();
		private List<Dictionary<ClickableTextureComponent, CustomWarp>> pagesOfRemoveButtons = new List<Dictionary<ClickableTextureComponent, CustomWarp>>();
		private TextBox textBox;
		private TextBoxEvent textBoxEvent;
		private ClickableComponent textBoxCC;
		private bool isNamingWarp;
		private Vector2 newWarpTile;
		private bool makeGlobal = false;
		private ClickableTextureComponent makeWarpGlobalButton;
		private int currentPage = 0;
		private string hoverText;

		//private ClickableTextureComponent settingsPageButton;
		//private bool showSettingsPage;
		
		private readonly IModHelper helper;
		private readonly LocationHelper locationHelper;

		public WarpMenu(IModHelper Helper)
			: base(Game1.viewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64 + 296, 632 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 16, false)
		{
			this.xPositionOnScreen -= 8;
			this.width += 16;
			this.yPositionOnScreen = IClickableMenu.borderWidth + 16 + 256;
			this.height = 2 * IClickableMenu.borderWidth + 144;
			this.helper = Helper;
			this.locationHelper = new LocationHelper(this.helper);
			//this.translationHelper = helper.Translation;
			this.previousPageButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height + 80, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
			this.nextPageButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 64, this.yPositionOnScreen + this.height + 80, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
			this.addWarpButton = new ClickableTextureComponent("Add", new Rectangle(this.xPositionOnScreen + this.width - 160, this.yPositionOnScreen, 64, 64), "", "Add warp", Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f, false);
			this.removeWarpButton = new ClickableTextureComponent("Remove", new Rectangle(this.xPositionOnScreen + this.width - 80, this.yPositionOnScreen, 64, 64), "", "Remove warp", Game1.mouseCursors, new Rectangle(192, 256, 64, 64), 1f, false);
			this.setUpWarpPages();
			this.setUpRemoveWarpPages();
			this.textBox = new TextBox((Texture2D)null, (Texture2D)null, Game1.dialogueFont, Game1.textColor)
			{
				X = Game1.viewport.Width / 2 - 192 - 128,
				Y = Game1.viewport.Height / 2,
				Width = 512,
				Height = 192
			};
			//this.settingsPageButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 16 + 12, this.yPositionOnScreen + 16, 64, 64), Game1.mouseCursors, new Rectangle(64, 368, 16, 16), 4f, false);
			this.textBoxCC = new ClickableComponent(new Rectangle(this.textBox.X, this.textBox.Y, 192, 48), "");
			this.textBoxEvent = new TextBoxEvent(this.textBoxEnter);
			this.makeWarpGlobalButton = new ClickableTextureComponent("Global", new Rectangle(this.textBox.X + this.textBox.Width + 48, this.textBox.Y + 6, 36, 36), "", "Make warp global", Game1.mouseCursors, new Rectangle(!this.makeGlobal ? 227 : 236, 425, 9, 9), 4f, false);
		}

		private void textBoxEnter(TextBox sender)
		{
			if (!this.isNamingWarp)
				return;
			if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is WarpMenu))
			{
				this.textBox.OnEnterPressed -= this.textBoxEvent;
			}
			else
			{
				if (sender.Text.Length < 1)
					return;
				else
				{
					this.textBox.OnEnterPressed -= this.textBoxEvent;
					string mapName = Game1.player.currentLocation.Name;
					bool isBuilding = false;
					foreach (Farm farm in this.helper.Multiplayer.GetActiveLocations().OfType<Farm>())
					{
						foreach (Building building in farm.buildings)
						{
							if (Game1.player.currentLocation == building.indoors.Value)
							{
								mapName = building.indoors.Value.uniqueName.Value;
								isBuilding = true;
								break;
							}
						}
					}
					if (!ModEntry.WarpHelper.BooleanAdd(sender.Text,
					new CustomWarp()
					{
						WarpName = sender.Text,
						MapName = mapName,
						xCoordinate = (int)this.newWarpTile.X,
						yCoordinate = (int)this.newWarpTile.Y,
						IsBuilding = isBuilding,
						IsGlobal = this.makeGlobal
					},
					this.makeGlobal))
					{
						Game1.addHUDMessage(new HUDMessage($"A warp with the name '{sender.Text}' already exists!", 3));
					}
					this.isAddingNewWarp = false;
					this.isNamingWarp = false;
					Game1.exitActiveMenu();
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			if (!this.isAddingNewWarp && !this.isNamingWarp)
			{
				if (this.removeWarpButton.containsPoint(x, y))
				{
					this.hoverText = this.removeWarpButton.hoverText;
					this.removeWarpButton.scale = Math.Min(this.removeWarpButton.scale + 0.02f, this.removeWarpButton.baseScale + 0.1f);
				}
				else
					this.removeWarpButton.scale = Math.Max(this.removeWarpButton.scale - 0.02f, this.removeWarpButton.baseScale);
				if (this.addWarpButton.containsPoint(x, y))
				{
					this.hoverText = this.addWarpButton.hoverText;
					this.addWarpButton.scale = Math.Min(this.addWarpButton.scale + 0.02f, this.addWarpButton.baseScale + 0.1f);
				}
				else
					this.addWarpButton.scale = Math.Max(this.addWarpButton.scale - 0.02f, this.addWarpButton.baseScale);
				if (this.previousPageButton.containsPoint(x, y))
					this.previousPageButton.scale = Math.Min(this.previousPageButton.scale + 0.02f, this.previousPageButton.baseScale + 0.1f);
				else
					this.previousPageButton.scale = Math.Max(this.previousPageButton.scale - 0.02f, this.previousPageButton.baseScale);
				if (this.nextPageButton.containsPoint(x, y))
					this.nextPageButton.scale = Math.Min(this.nextPageButton.scale + 0.02f, this.nextPageButton.baseScale + 0.1f);
				else
					this.nextPageButton.scale = Math.Max(this.nextPageButton.scale - 0.02f, this.nextPageButton.baseScale);
				if (this.showRemoveWarpButtons)
				{
					foreach (var pair in this.pagesOfRemoveButtons[this.currentPage])
					{
						if (pair.Key.containsPoint(x, y))
						{
							pair.Key.scale = Math.Min(pair.Key.scale + 0.02f, pair.Key.baseScale + 0.1f);
							this.hoverText = pair.Key.hoverText;
						}
						else
							pair.Key.scale = Math.Max(pair.Key.scale - 0.02f, pair.Key.baseScale);
					}
				}
				else
				{
					foreach (var pair in this.pagesOfCustomWarps[this.currentPage])
					{
						if (pair.Key.containsPoint(x, y))
						{
							pair.Key.scale = Math.Min(pair.Key.scale + 0.02f, pair.Key.baseScale + 0.1f);
							this.hoverText = this.getHoverText(pair.Value);
						}
						else
							pair.Key.scale = Math.Max(pair.Key.scale - 0.02f, pair.Key.baseScale);
					}
				}
			}
			else if (this.isAddingNewWarp && !this.isNamingWarp)
			{
				Vector2 tileLocation = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				this.hoverText = $"X: {tileLocation.X}, Y: {tileLocation.Y}";
			}
			else
			{
				if (this.textBoxCC.containsPoint(x, y))
				{
					this.textBox.Selected = true;
					this.textBox.SelectMe();
				}
				if (this.makeWarpGlobalButton.containsPoint(x, y))
					this.hoverText = this.makeWarpGlobalButton.hoverText;
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!this.isAddingNewWarp && !this.isNamingWarp)
			{
				if (this.previousPageButton.containsPoint(x, y) && this.currentPage > 0)
				{
					--this.currentPage;
					if (this.previousPageButton.scale != 0.0)
					{
						this.previousPageButton.scale -= 0.25f;
						this.previousPageButton.scale = Math.Max(0.75f, this.previousPageButton.scale);
					}
					Game1.playSound("drumkit6");
				}
				if (this.nextPageButton.containsPoint(x, y) && this.currentPage < this.pagesOfCustomWarps.Count - 1)
				{
					++this.currentPage;
					if (this.nextPageButton.scale != 0.0)
					{
						this.nextPageButton.scale -= 0.25f;
						this.nextPageButton.scale = Math.Max(0.75f, this.nextPageButton.scale);
					}
					Game1.playSound("drumkit6");
				}
				//if (this.settingsPageButton.containsPoint(x, y))
				//{
				//	this.showSettingsPage = !this.showSettingsPage;
				//	if (this.showSettingsPage)
				//	{
				//		this.previousPageButton.bounds.Y += 256;
				//		this.nextPageButton.bounds.Y += 256;
				//	}
				//	else
				//	{
				//		this.previousPageButton.bounds.Y -= 256;
				//		this.nextPageButton.bounds.Y -= 256;
				//	}
				//	Game1.playSound("smallSelect");
				//}
				if (this.removeWarpButton.containsPoint(x, y))
				{
					this.showRemoveWarpButtons = !this.showRemoveWarpButtons;
					if (this.removeWarpButton.scale != 0.0)
					{
						this.removeWarpButton.scale -= 0.25f;
						this.removeWarpButton.scale = Math.Max(0.75f, this.removeWarpButton.scale);
					}
					Game1.playSound("drumkit6");
				}
				if (this.addWarpButton.containsPoint(x, y))
				{
					this.isAddingNewWarp = !this.isAddingNewWarp;
					if (this.addWarpButton.scale != 0.0)
					{
						this.addWarpButton.scale -= 0.25f;
						this.addWarpButton.scale = Math.Max(0.75f, this.addWarpButton.scale);
					}
					Game1.playSound("drumkit6");
				}
				if (this.showRemoveWarpButtons)
				{
					foreach (var pair in this.pagesOfRemoveButtons[this.currentPage])
					{
						if (pair.Key.containsPoint(x, y))
						{
							Game1.player.currentLocation.createQuestionDialogue($"Are you sure you want to remove {pair.Value.WarpName}?", Game1.player.currentLocation.createYesNoResponses(), (GameLocation.afterQuestionBehavior)((f, answer) =>
							{
								if (answer == "Yes")
								{
									ModEntry.WarpHelper.Remove(pair.Value.WarpName, pair.Value.IsGlobal);
								}
							}), null);
							Game1.playSound("drumkit6");
						}
						if (pair.Key.scale != 0.0)
						{
							pair.Key.scale -= 0.25f;
							pair.Key.scale = Math.Max(0.75f, pair.Key.scale);
						}
					}
				}
				else
				{
					foreach (var pair in this.pagesOfCustomWarps[this.currentPage])
					{
						if (pair.Key.containsPoint(x, y))
						{
							Game1.playSound("drumkit6");
							Game1.warpFarmer(pair.Value.MapName, pair.Value.xCoordinate, pair.Value.yCoordinate, false);
						}
						if (pair.Key.scale != 0.0)
						{
							pair.Key.scale -= 0.25f;
							pair.Key.scale = Math.Max(0.75f, pair.Key.scale);
						}
					}
				}
			}
			else if (this.isAddingNewWarp && !this.isNamingWarp)
			{
				this.newWarpTile = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				this.isNamingWarp = !this.isNamingWarp;
			}
			else
			{
				this.textBox.Selected = true;
				this.textBox.SelectMe();
				this.textBox.Update();
				if (this.makeWarpGlobalButton.containsPoint(x, y))
					this.makeGlobal = !this.makeGlobal;
				Game1.playSound("drumkit6");
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Escape)
			{
				this.isNamingWarp = false;
				this.isAddingNewWarp = false;
				this.textBox.Text = "";
				Game1.playSound("bigDeSelect");
				Game1.exitActiveMenu();
			}
			if (this.isNamingWarp)
			{
				if (key == Keys.Enter)
					Game1.playSound("bigDeSelect");
				else
					Game1.playSound("cowboy_monsterhit");
				if (this.textBox.Text.Length > 0 && key == Keys.Enter)
					this.textBoxEnter(this.textBox);
			}
		}

		public override void update(GameTime time)
		{
			if (this.isNamingWarp)
			{
				this.textBox.SelectMe();
				this.textBox.Selected = true;
			}
			else
			{
				this.textBox.Selected = false;
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!this.isAddingNewWarp && !this.isNamingWarp)
			{
				//if (this.showSettingsPage)
				//{
				//	Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height + 256 + 80, false, true, (string)null, false, true);
				//}
				Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height + 80, false, true, (string)null, false, true);
				//b.Draw(this.settingsPageButton.texture, new Vector2(this.settingsPageButton.bounds.X, this.settingsPageButton.bounds.Y + (this.showSettingsPage ? 8 : 0)), new Rectangle?(this.settingsPageButton.sourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				//b.Draw(this.previousPageButton.texture, new Vector2(this.previousPageButton.bounds.X, this.previousPageButton.bounds.Y), this.previousPageButton.sourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				//b.Draw(this.nextPageButton.texture, new Vector2(this.nextPageButton.bounds.X, this.nextPageButton.bounds.Y), this.nextPageButton.sourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				if (this.pagesOfCustomWarps.Count > 1)
				{
					this.previousPageButton.draw(b);
					this.nextPageButton.draw(b);
				}
				if (this.showRemoveWarpButtons)
				{
					foreach (var warp in this.pagesOfCustomWarps[this.currentPage].Keys)
						b.Draw(warp.texture, new Vector2(warp.bounds.X, warp.bounds.Y), warp.sourceRect, Color.Gray, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					foreach (var removeWarp in this.pagesOfRemoveButtons[this.currentPage].Keys)
						b.Draw(removeWarp.texture, new Vector2(removeWarp.bounds.X, removeWarp.bounds.Y), removeWarp.sourceRect, removeWarp.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.White : Color.Gray, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
				}
				else
				{
					foreach (var warp in this.pagesOfCustomWarps[this.currentPage].Keys)
						b.Draw(warp.texture, new Vector2(warp.bounds.X, warp.bounds.Y), warp.sourceRect, warp.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.White : Color.Gray, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
				b.Draw(this.addWarpButton.texture, new Vector2(this.addWarpButton.bounds.X, this.addWarpButton.bounds.Y), this.addWarpButton.sourceRect, this.addWarpButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.White : Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				b.Draw(this.removeWarpButton.texture, new Vector2(this.removeWarpButton.bounds.X, this.removeWarpButton.bounds.Y), this.removeWarpButton.sourceRect, this.removeWarpButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.White : Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}
			else if (this.isAddingNewWarp && !this.isNamingWarp)
			{
				string s = "Add new warp";
				SpriteText.drawStringWithScrollBackground(b, s, Game1.viewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, 16, "", 1f, -1);
				Vector2 tileLocation = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
			}
			else
			{
				this.textBox.Draw(b, true);
				b.Draw(this.makeWarpGlobalButton.texture, new Vector2(this.makeWarpGlobalButton.bounds.X, this.makeWarpGlobalButton.bounds.Y), new Rectangle?(new Rectangle(!this.makeGlobal ? 227 : 236, 425, 9, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (this.hoverText != null && this.hoverText.Count<char>() > 0)
				IClickableMenu.drawHoverText(b, Game1.parseText(this.hoverText, Game1.smallFont, 784), Game1.smallFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
			base.drawMouse(b);
		}

		private void setUpWarpPages()
		{
			Dictionary<ClickableTextureComponent, CustomWarp> newPage = createNewPage(1);
			ClickableTextureComponent[,] pageLayout = createNewPageLayout();
			int x1 = 0;
			int y1 = 0;
			foreach (var pair in ModEntry.WarpHelper.CustomWarps)
			{
				while (spaceOccupied(pageLayout, x1, y1))
				{
					++x1;
					if (x1 >= 8)
					{
						x1 = 0;
						++y1;
						if (y1 >= 3)
						{
							newPage = createNewPage(1);
							pageLayout = createNewPageLayout();
							x1 = 0;
							y1 = 0;
						}
					}
				}
				ClickableTextureComponent warp = new ClickableTextureComponent($"{pair.Value.WarpName}", new Rectangle(this.xPositionOnScreen + 48 + x1 * 64 + x1 * 16, this.yPositionOnScreen + 112 + y1 * 64 + y1 * 16, 64, 64), "", $"{this.getHoverText(pair.Value)}", Game1.mouseCursors, new Rectangle(257, 284, 16, 16), 4f, false);
				newPage.Add(warp, pair.Value);
				pageLayout[x1, y1] = warp;
			}
		}

		private void setUpRemoveWarpPages()
		{
			Dictionary<ClickableTextureComponent, CustomWarp> newPage = createNewPage(2);
			ClickableTextureComponent[,] pageLayout = createNewPageLayout();
			int x1 = 0;
			int y1 = 0;
			foreach (var pair in ModEntry.WarpHelper.CustomWarps)
			{
				while (spaceOccupied(pageLayout, x1, y1))
				{
					++x1;
					if (x1 >= 8)
					{
						x1 = 0;
						++y1;
						if (y1 >= 3)
						{
							newPage = createNewPage(2);
							pageLayout = createNewPageLayout();
							x1 = 0;
							y1 = 0;
						}
					}
				}
				ClickableTextureComponent warpRemove = new ClickableTextureComponent($"Remove {pair.Value.WarpName}", new Rectangle(this.xPositionOnScreen + 48 + x1 * 64 + x1 * 16, this.yPositionOnScreen + 112 + y1 * 64 + y1 * 16, 24, 24), "", $"Remove {pair.Value.WarpName}", Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 2f, false);
				newPage.Add(warpRemove, pair.Value);
				pageLayout[x1, y1] = warpRemove;
			}
		}

		private string getHoverText(CustomWarp warp)
		{
			return $"Warp name: {warp.WarpName}\nLocation: {(!warp.IsBuilding ? this.locationHelper.GetLocationName(warp.MapName) : this.getTypeOfBuilding(warp.MapName))}\nX-tile: {warp.xCoordinate}\nY-tile: {warp.yCoordinate}\n{(warp.IsGlobal ? "Global warp" : "Local warp")}";
		}

		private string getTypeOfBuilding(string name)
		{
			foreach (Farm farm in this.helper.Multiplayer.GetActiveLocations().OfType<Farm>())
			{
				foreach (Building building in farm.buildings)
				{
					if (building.indoors.Value != null && building.indoors.Value.uniqueName.Value == name)
					{
						return building.indoors.Value.Name;
					}
				}
			}
			return "Unknown building";
		}

		private bool spaceOccupied(ClickableTextureComponent[,] pageLayout, int x, int y)
		{
			if (pageLayout[x, y] != null)
				return true;
			if (y + 1 < 3)
				return pageLayout[x, y + 1] != null;
			return true;
		}

		private ClickableTextureComponent[,] createNewPageLayout()
		{
			return new ClickableTextureComponent[8, 3];
		}

		private Dictionary<ClickableTextureComponent, CustomWarp> createNewPage(int which)
		{
			Dictionary<ClickableTextureComponent, CustomWarp> dictionary = new Dictionary<ClickableTextureComponent, CustomWarp>();
			switch (which)
			{
				case 1:
					this.pagesOfCustomWarps.Add(dictionary);
					break;
				case 2:
					this.pagesOfRemoveButtons.Add(dictionary);
					break;
			}
			return dictionary;
		}
	}
}