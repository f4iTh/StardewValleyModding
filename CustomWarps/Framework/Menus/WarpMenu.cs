using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CustomWarps.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace CustomWarps.Framework.Menus {
  [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
  [SuppressMessage("ReSharper", "InvertIf")]
  [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
  public class WarpMenu : IClickableMenu {
    private readonly ClickableTextureComponent addWarpButton;
    private readonly IModHelper helper;
    private readonly LocationHelper locationHelper;
    private readonly ClickableTextureComponent makeWarpGlobalButton;
    private readonly ClickableTextureComponent nextPageButton;
    private readonly List<Dictionary<ClickableTextureComponent, CustomWarp>> pagesOfCustomWarps = new();
    private readonly List<Dictionary<ClickableTextureComponent, CustomWarp>> pagesOfRemoveButtons = new();
    private readonly ClickableTextureComponent previousPageButton;
    private readonly ClickableTextureComponent removeWarpButton;
    private readonly TextBox textBox;
    private readonly ClickableComponent textBoxComponent;
    private readonly TextBoxEvent textBoxEvent;

    private int currentPage;
    private string hoverText;
    private bool isAddingNewWarp;
    private bool isNamingWarp;
    private bool makeGlobal;
    private Vector2 newWarpTile;
    private bool showRemoveWarpButtons;

    public WarpMenu(IModHelper helper)
      : base(Game1.viewport.Width / 2 - (632 + borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + borderWidth * 2) / 2 - 64 + 296, 632 + borderWidth * 2, 600 + borderWidth * 2 + 16) {
      this.xPositionOnScreen -= 8;
      this.width += 16;
      this.yPositionOnScreen = borderWidth + 16 + 256;
      this.height = 2 * borderWidth + 144;
      this.helper = helper;
      this.locationHelper = new LocationHelper(this.helper);
      //this.translationHelper = helper.Translation;
      this.previousPageButton = new ClickableTextureComponent(
        new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height + 80, 64, 64), Game1.mouseCursors,
        Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
      this.nextPageButton = new ClickableTextureComponent(
        new Rectangle(this.xPositionOnScreen + this.width - 64, this.yPositionOnScreen + this.height + 80, 64, 64),
        Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f);
      this.addWarpButton = new ClickableTextureComponent("Add",
        new Rectangle(this.xPositionOnScreen + this.width - 160, this.yPositionOnScreen, 64, 64), "", "Add warp",
        Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f);
      this.removeWarpButton = new ClickableTextureComponent("Remove",
        new Rectangle(this.xPositionOnScreen + this.width - 80, this.yPositionOnScreen, 64, 64), "", "Remove warp",
        Game1.mouseCursors, new Rectangle(192, 256, 64, 64), 1f);
      this.SetUpWarpPages();
      this.SetUpRemoveWarpPages();
      this.textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor) {
        X = Game1.viewport.Width / 2 - 192 - 128,
        Y = Game1.viewport.Height / 2,
        Width = 512,
        Height = 192
      };
      this.textBoxComponent = new ClickableComponent(new Rectangle(this.textBox.X, this.textBox.Y, 192, 48), "");
      this.textBoxEvent = this.TextBoxEnter;
      this.makeWarpGlobalButton = new ClickableTextureComponent("Global",
        new Rectangle(this.textBox.X + this.textBox.Width + 48, this.textBox.Y + 6, 36, 36), "", "Make warp global",
        Game1.mouseCursors, new Rectangle(!this.makeGlobal ? 227 : 236, 425, 9, 9), 4f);
    }

    private void TextBoxEnter(TextBox sender) {
      if (!this.isNamingWarp)
        return;
      if (Game1.activeClickableMenu is not WarpMenu) {
        this.textBox.OnEnterPressed -= this.textBoxEvent;
      }
      else {
        if (sender.Text.Length < 1) return;

        this.textBox.OnEnterPressed -= this.textBoxEvent;
        string mapName = Game1.player.currentLocation.Name;
        bool isBuilding = false;
        foreach (Farm farm in this.helper.Multiplayer.GetActiveLocations().OfType<Farm>())
        foreach (Building building in farm.buildings.Where(building => Game1.player.currentLocation.Equals(building.indoors.Value))) {
          mapName = building.indoors.Value.uniqueName.Value;
          isBuilding = true;
          break;
        }

        if (!ModEntry.WarpHelper.TryAdd(sender.Text,
              new CustomWarp {
                WarpName = sender.Text,
                MapName = mapName,
                XCoordinate = (int)this.newWarpTile.X,
                YCoordinate = (int)this.newWarpTile.Y,
                IsBuilding = isBuilding,
                IsGlobal = this.makeGlobal
              },
              this.makeGlobal))
          Game1.addHUDMessage(new HUDMessage($"A warp with the name '{sender.Text}' already exists!", 3));
        this.isAddingNewWarp = false;
        this.isNamingWarp = false;
        Game1.exitActiveMenu();
      }
    }

    public override void performHoverAction(int x, int y) {
      this.hoverText = "";
      if (this.isAddingNewWarp == false && !this.isNamingWarp) {
        if (this.removeWarpButton.containsPoint(x, y)) {
          this.hoverText = this.removeWarpButton.hoverText;
          this.removeWarpButton.scale = Math.Min(this.removeWarpButton.scale + 0.02f, this.removeWarpButton.baseScale + 0.1f);
        }
        else {
          this.removeWarpButton.scale = Math.Max(this.removeWarpButton.scale - 0.02f, this.removeWarpButton.baseScale);
        }

        if (this.addWarpButton.containsPoint(x, y)) {
          this.hoverText = this.addWarpButton.hoverText;
          this.addWarpButton.scale = Math.Min(this.addWarpButton.scale + 0.02f, this.addWarpButton.baseScale + 0.1f);
        }
        else {
          this.addWarpButton.scale = Math.Max(this.addWarpButton.scale - 0.02f, this.addWarpButton.baseScale);
        }

        this.previousPageButton.scale = this.previousPageButton.containsPoint(x, y)
          ? Math.Min(this.previousPageButton.scale + 0.02f, this.previousPageButton.baseScale + 0.1f)
          : Math.Max(this.previousPageButton.scale - 0.02f, this.previousPageButton.baseScale);
        this.nextPageButton.scale = this.nextPageButton.containsPoint(x, y)
          ? Math.Min(this.nextPageButton.scale + 0.02f, this.nextPageButton.baseScale + 0.1f)
          : Math.Max(this.nextPageButton.scale - 0.02f, this.nextPageButton.baseScale);
        if (this.showRemoveWarpButtons)
          foreach (KeyValuePair<ClickableTextureComponent, CustomWarp> pair in this.pagesOfRemoveButtons[this.currentPage])
            if (pair.Key.containsPoint(x, y)) {
              pair.Key.scale = Math.Min(pair.Key.scale + 0.02f, pair.Key.baseScale + 0.1f);
              this.hoverText = pair.Key.hoverText;
            }
            else {
              pair.Key.scale = Math.Max(pair.Key.scale - 0.02f, pair.Key.baseScale);
            }
        else
          foreach (KeyValuePair<ClickableTextureComponent, CustomWarp> pair in this.pagesOfCustomWarps[this.currentPage])
            if (pair.Key.containsPoint(x, y)) {
              pair.Key.scale = Math.Min(pair.Key.scale + 0.02f, pair.Key.baseScale + 0.1f);
              this.hoverText = this.GetHoverText(pair.Value);
            }
            else {
              pair.Key.scale = Math.Max(pair.Key.scale - 0.02f, pair.Key.baseScale);
            }
      }
      else if (this.isAddingNewWarp && !this.isNamingWarp) {
        Vector2 tileLocation = new((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
        this.hoverText = $"X: {tileLocation.X}, Y: {tileLocation.Y}";
      }
      else {
        if (this.textBoxComponent.containsPoint(x, y)) {
          this.textBox.Selected = true;
          this.textBox.SelectMe();
        }

        if (this.makeWarpGlobalButton.containsPoint(x, y))
          this.hoverText = this.makeWarpGlobalButton.hoverText;
      }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true) {
      if (this.isAddingNewWarp == false && !this.isNamingWarp) {
        if (this.previousPageButton.containsPoint(x, y) && this.currentPage > 0) {
          --this.currentPage;
          if (this.previousPageButton.scale != 0.0) {
            this.previousPageButton.scale -= 0.25f;
            this.previousPageButton.scale = Math.Max(0.75f, this.previousPageButton.scale);
          }

          Game1.playSound("drumkit6");
        }

        if (this.nextPageButton.containsPoint(x, y) && this.currentPage < this.pagesOfCustomWarps.Count - 1) {
          ++this.currentPage;
          if (this.nextPageButton.scale != 0.0) {
            this.nextPageButton.scale -= 0.25f;
            this.nextPageButton.scale = Math.Max(0.75f, this.nextPageButton.scale);
          }

          Game1.playSound("drumkit6");
        }

        if (this.removeWarpButton.containsPoint(x, y)) {
          this.showRemoveWarpButtons = !this.showRemoveWarpButtons;
          if (this.removeWarpButton.scale != 0.0) {
            this.removeWarpButton.scale -= 0.25f;
            this.removeWarpButton.scale = Math.Max(0.75f, this.removeWarpButton.scale);
          }

          Game1.playSound("drumkit6");
        }

        if (this.addWarpButton.containsPoint(x, y)) {
          this.isAddingNewWarp = !this.isAddingNewWarp;
          if (this.addWarpButton.scale != 0.0) {
            this.addWarpButton.scale -= 0.25f;
            this.addWarpButton.scale = Math.Max(0.75f, this.addWarpButton.scale);
          }

          Game1.playSound("drumkit6");
        }

        if (this.showRemoveWarpButtons)
          foreach (KeyValuePair<ClickableTextureComponent, CustomWarp> pair in this.pagesOfRemoveButtons[this.currentPage]) {
            if (pair.Key.containsPoint(x, y)) {
              Game1.player.currentLocation.createQuestionDialogue(
                $"Are you sure you want to remove {pair.Value.WarpName}?",
                Game1.player.currentLocation.createYesNoResponses(), (_, answer) => {
                  if (answer == "Yes") ModEntry.WarpHelper.Remove(pair.Value.WarpName, pair.Value.IsGlobal);
                });
              Game1.playSound("drumkit6");
            }

            if (pair.Key.scale == 0.0) continue;
            pair.Key.scale -= 0.25f;
            pair.Key.scale = Math.Max(0.75f, pair.Key.scale);
          }
        else
          foreach (KeyValuePair<ClickableTextureComponent, CustomWarp> pair in this.pagesOfCustomWarps[this.currentPage]) {
            if (pair.Key.containsPoint(x, y)) {
              Game1.playSound("drumkit6");
              Game1.warpFarmer(pair.Value.MapName, pair.Value.XCoordinate, pair.Value.YCoordinate, false);
            }

            if (pair.Key.scale == 0.0) continue;
            pair.Key.scale -= 0.25f;
            pair.Key.scale = Math.Max(0.75f, pair.Key.scale);
          }
      }
      else if (this.isAddingNewWarp && !this.isNamingWarp) {
        this.newWarpTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
        this.isNamingWarp = !this.isNamingWarp;
      }
      else {
        this.textBox.Selected = true;
        this.textBox.SelectMe();
        this.textBox.Update();
        if (this.makeWarpGlobalButton.containsPoint(x, y))
          this.makeGlobal = !this.makeGlobal;
        Game1.playSound("drumkit6");
      }
    }

    public override void receiveKeyPress(Keys key) {
      if (key == Keys.Escape) {
        this.isNamingWarp = false;
        this.isAddingNewWarp = false;
        this.textBox.Text = "";
        Game1.playSound("bigDeSelect");
        Game1.exitActiveMenu();
      }

      if (!this.isNamingWarp) return;
      Game1.playSound(key == Keys.Enter ? "bigDeSelect" : "cowboy_monsterhit");
      if (this.textBox.Text.Length > 0 && key == Keys.Enter)
        this.TextBoxEnter(this.textBox);
    }

    public override void update(GameTime time) {
      if (this.isNamingWarp) {
        this.textBox.SelectMe();
        this.textBox.Selected = true;
      }
      else {
        this.textBox.Selected = false;
      }
    }

    public override void draw(SpriteBatch b) {
      if (this.isAddingNewWarp == false && !this.isNamingWarp) {
        Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height + 80, false, true);
        if (this.pagesOfCustomWarps.Count > 1) {
          this.previousPageButton.draw(b);
          this.nextPageButton.draw(b);
        }

        if (this.showRemoveWarpButtons) {
          foreach (ClickableTextureComponent warp in this.pagesOfCustomWarps[this.currentPage].Keys)
            b.Draw(warp.texture, new Vector2(warp.bounds.X, warp.bounds.Y), warp.sourceRect, Color.Gray, 0f,
              Vector2.Zero, 4f, SpriteEffects.None, 1f);
          foreach (ClickableTextureComponent removeWarp in this.pagesOfRemoveButtons[this.currentPage].Keys)
            b.Draw(removeWarp.texture, new Vector2(removeWarp.bounds.X, removeWarp.bounds.Y),
              removeWarp.sourceRect,
              removeWarp.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.White : Color.Gray,
              0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
        }
        else {
          foreach (ClickableTextureComponent warp in this.pagesOfCustomWarps[this.currentPage].Keys)
            b.Draw(warp.texture, new Vector2(warp.bounds.X, warp.bounds.Y), warp.sourceRect,
              warp.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.White : Color.Gray, 0f,
              Vector2.Zero, 4f, SpriteEffects.None, 1f);
        }

        b.Draw(this.addWarpButton.texture, new Vector2(this.addWarpButton.bounds.X, this.addWarpButton.bounds.Y),
          this.addWarpButton.sourceRect,
          this.addWarpButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.White : Color.Gray,
          0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        b.Draw(this.removeWarpButton.texture,
          new Vector2(this.removeWarpButton.bounds.X, this.removeWarpButton.bounds.Y),
          this.removeWarpButton.sourceRect,
          this.removeWarpButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())
            ? Color.White
            : Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
      }
      else if (this.isAddingNewWarp && !this.isNamingWarp) {
        const string s = "Add new warp";
        SpriteText.drawStringWithScrollBackground(b, s, Game1.viewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, 16);
        Vector2 tileLocation = new((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f),
          new Rectangle(194, 388, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
      }
      else {
        this.textBox.Draw(b);
        b.Draw(this.makeWarpGlobalButton.texture,
          new Vector2(this.makeWarpGlobalButton.bounds.X, this.makeWarpGlobalButton.bounds.Y),
          new Rectangle(!this.makeGlobal ? 227 : 236, 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f,
          SpriteEffects.None, 1f);
      }

      if (this.hoverText != null && this.hoverText.Any())
        drawHoverText(b, Game1.parseText(this.hoverText, Game1.smallFont, 784), Game1.smallFont);
      this.drawMouse(b);
    }

    private void SetUpWarpPages() {
      Dictionary<ClickableTextureComponent, CustomWarp> newPage = this.CreateNewPage(1);
      ClickableTextureComponent[,] pageLayout = CreateNewPageLayout();
      int x1 = 0;
      int y1 = 0;
      foreach (KeyValuePair<string, CustomWarp> pair in ModEntry.WarpHelper.CustomWarps) {
        while (SpaceOccupied(pageLayout, x1, y1)) {
          ++x1;
          if (x1 >= 8) {
            x1 = 0;
            ++y1;
            if (y1 >= 3) {
              newPage = this.CreateNewPage(1);
              pageLayout = CreateNewPageLayout();
              x1 = 0;
              y1 = 0;
            }
          }
        }

        ClickableTextureComponent warp = new($"{pair.Value.WarpName}",
          new Rectangle(this.xPositionOnScreen + 48 + x1 * 64 + x1 * 16,
            this.yPositionOnScreen + 112 + y1 * 64 + y1 * 16, 64, 64), "", $"{this.GetHoverText(pair.Value)}",
          Game1.mouseCursors, new Rectangle(257, 284, 16, 16), 4f);
        newPage.Add(warp, pair.Value);
        pageLayout[x1, y1] = warp;
      }
    }

    private void SetUpRemoveWarpPages() {
      Dictionary<ClickableTextureComponent, CustomWarp> newPage = this.CreateNewPage(2);
      ClickableTextureComponent[,] pageLayout = CreateNewPageLayout();
      int x1 = 0;
      int y1 = 0;
      foreach (KeyValuePair<string, CustomWarp> pair in ModEntry.WarpHelper.CustomWarps) {
        while (SpaceOccupied(pageLayout, x1, y1)) {
          ++x1;
          if (x1 >= 8) {
            x1 = 0;
            ++y1;
            if (y1 >= 3) {
              newPage = this.CreateNewPage(2);
              pageLayout = CreateNewPageLayout();
              x1 = 0;
              y1 = 0;
            }
          }
        }

        ClickableTextureComponent warpRemove = new($"Remove {pair.Value.WarpName}",
          new Rectangle(this.xPositionOnScreen + 48 + x1 * 64 + x1 * 16,
            this.yPositionOnScreen + 112 + y1 * 64 + y1 * 16, 24, 24), "", $"Remove {pair.Value.WarpName}",
          Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 2f);
        newPage.Add(warpRemove, pair.Value);
        pageLayout[x1, y1] = warpRemove;
      }
    }

    private string GetHoverText(CustomWarp warp) {
      return $@"Warp name: {warp.WarpName}
                    Location: {(!warp.IsBuilding ? this.locationHelper.GetLocationName(warp.MapName) : this.GetTypeOfBuilding(warp.MapName))}
                    X-tile: {warp.XCoordinate}
                    Y-tile: {warp.YCoordinate}
                    {(warp.IsGlobal ? "Global warp" : "Local warp")}";
    }

    private string GetTypeOfBuilding(string name) {
      foreach (Farm farm in this.helper.Multiplayer.GetActiveLocations().OfType<Farm>())
      foreach (Building building in farm.buildings.Where(building => building.indoors.Value != null && building.indoors.Value.uniqueName.Value == name))
        return building.indoors.Value.Name;
      return "Unknown building";
    }

    private static bool SpaceOccupied(ClickableTextureComponent[,] pageLayout, int x, int y) {
      if (pageLayout[x, y] != null)
        return true;
      if (y + 1 < 3)
        return pageLayout[x, y + 1] != null;
      return true;
    }

    private static ClickableTextureComponent[,] CreateNewPageLayout() {
      return new ClickableTextureComponent[8, 3];
    }

    private Dictionary<ClickableTextureComponent, CustomWarp> CreateNewPage(int which) {
      Dictionary<ClickableTextureComponent, CustomWarp> dictionary = new();
      switch (which) {
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