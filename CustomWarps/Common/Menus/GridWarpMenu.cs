using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CustomWarps.Common.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace CustomWarps.Common.Menus {
  // TODO: custom sorting for warp items?
  // TODO: custom warp icons, categories, etc.?
  // TODO: visualize/show all currently created custom warps in current location?
  public class GridWarpMenu : IClickableMenu {
    private readonly ClickableTextureComponent _addWarpButton;
    // private readonly TextBox _searchTextBox;

    private readonly List<Dictionary<ClickableTextureComponent, CustomWarp>> _customWarpPages = new();

    private readonly IModHelper _helper;

    // private readonly ClickableTextureComponent _searchButton;
    private readonly ClickableTextureComponent _makeWarpGlobalButton;
    private readonly int _maxItemsPerColumn;
    private readonly int _maxItemsPerRow;
    private readonly ClickableTextureComponent _nextPageButton;
    private readonly ClickableTextureComponent _previousPageButton;

    private readonly ClickableTextureComponent _removeWarpButton;

    // private readonly ClickableComponent _searchTextBoxComponent;
    private readonly TextBox _textBox;
    private readonly ClickableComponent _textBoxComponent;
    private readonly ClickableTextureComponent _upperRightCloseButton;
    private readonly WarpHelper _warpHelper;

    private int _currentPage;
    private string _hoverText;
    private bool _isAddingNewWarp;
    private bool _isNamingWarp;
    private bool _isRemovingWarp;
    private bool _makeGlobal;

    private Vector2 _newWarpTile;
    // private bool _isSearching;

    // TODO: handle vertical list or create a separate menu class for it?
    // TODO: menu handling for controller
    public GridWarpMenu(IModHelper helper, WarpHelper warpHelper, int maxItemsPerRow = 8, int maxItemsPerColumn = 3) {
      /* : base(Game1.viewport.Width / 2 - (632 + borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + borderWidth * 2) / 2 - 64 + 296, 632 + borderWidth * 2, 600 + borderWidth * 2 + 16) */

      this._helper = helper;
      this._warpHelper = warpHelper;
      this._maxItemsPerRow = maxItemsPerRow;
      this._maxItemsPerColumn = maxItemsPerColumn + 1; // exclusive upper bound

      this.width = 632 + borderWidth * 2 + 16;
      // TODO: don't draw extra height if there aren't enough custom warps
      this.height = 2 * borderWidth + maxItemsPerColumn * 80 + 64;
      this.xPositionOnScreen = Game1.uiViewport.Width / 2 - (600 + borderWidth * 2) / 2;
      this.yPositionOnScreen = Game1.uiViewport.Height / 2 - borderWidth * 2 / 2 - this.height / 2;
      // this.height = 2 * borderWidth + maxItemsPerColumn * 80 - 16;

      this._upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen - 48, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
      // this._previousPageButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height + 80, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
      this._previousPageButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
      // this._nextPageButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 64, this.yPositionOnScreen + this.height + 80, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f);
      this._nextPageButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 64, this.yPositionOnScreen + this.height, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f);
      this._addWarpButton = new ClickableTextureComponent("Add", new Rectangle(this.xPositionOnScreen + this.width - 16 - 64 - 8 - 64, this.yPositionOnScreen + 8, 64, 64), "", "Add warp", Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f);
      this._removeWarpButton = new ClickableTextureComponent("Remove", new Rectangle(this.xPositionOnScreen + this.width - 16 - 64, this.yPositionOnScreen + 8, 64, 64), "", "Remove warp", Game1.mouseCursors, new Rectangle(192, 256, 64, 64), 1f);
      // this._searchButton = new ClickableTextureComponent("Search", new Rectangle(this.xPositionOnScreen + 16 + 8, this.yPositionOnScreen + 8, 64, 64), "", "Search warps", Game1.mouseCursors, new Rectangle(208, 321, 14, 15), 4f);
      this._textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor) {
        // X = Game1.viewport.Width / 2 - 192 - 128,
        // Y = Game1.viewport.Height / 2,  
        // Width = 512,
        // Height = 192
        X = Game1.uiViewport.Width / 2 - (600 + borderWidth * 2) / 2,
        Y = Game1.viewport.Height / 2,
        Width = 632 + borderWidth * 2 + 16,
        Height = 192
      };
      // this._searchTextBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor) {
      //   X = this.xPositionOnScreen + 32,
      //   Y = this.yPositionOnScreen + this.height + 16,
      //   Width = this.width - 16 - 32 - 32,
      //   Height = 192,
      // };
      this._textBoxComponent = new ClickableComponent(new Rectangle(this._textBox.X, this._textBox.Y, 192, 48), "");
      // this._searchTextBoxComponent = new ClickableComponent(new Rectangle(this._searchTextBox.X, this._searchTextBox.Y, 192, 48), "");
      this._textBox.OnEnterPressed += this.TextBoxEnter;
      // this._searchTextBox.OnEnterPressed += this.SearchTextBoxEnter;
      this._makeWarpGlobalButton = new ClickableTextureComponent("Global", new Rectangle(this._textBox.X + this._textBox.Width + 48, this._textBox.Y + 6, 36, 36), "", "Make warp global", Game1.mouseCursors, new Rectangle(!this._makeGlobal ? 227 : 236, 425, 9, 9), 4f);

      this.SetUpWarpPages();
    }

    public override void performHoverAction(int x, int y) {
      this._hoverText = "";

      if (!this._isAddingNewWarp && !this._isNamingWarp) {
        this._upperRightCloseButton.tryHover(x, y, 0.25f);
        this._removeWarpButton.tryHover(x, y, 0.25f / 4f);
        this._addWarpButton.tryHover(x, y, 0.25f / 4f);
        // this._searchButton.tryHover(x, y, 0.25f);
        this._previousPageButton.tryHover(x, y, 0.25f / 4f);
        this._nextPageButton.tryHover(x, y, 0.25f / 4f);

        if (this._removeWarpButton.containsPoint(x, y)) this._hoverText = this._removeWarpButton.hoverText;

        if (this._addWarpButton.containsPoint(x, y)) this._hoverText = this._addWarpButton.hoverText;

        // if (this._searchButton.containsPoint(x, y)) {
        //   this._hoverText = this._searchButton.hoverText;
        // }

        foreach (KeyValuePair<ClickableTextureComponent, CustomWarp> pair in this._customWarpPages[this._currentPage]) {
          pair.Key.tryHover(x, y, 0.25f);
          if (pair.Key.containsPoint(x, y)) this._hoverText = GridWarpMenu.GetWarpHoverText(pair.Value);
        }

        // if (this._searchTextBoxComponent.containsPoint(x, y)) {
        //   this._searchTextBox.SelectMe();
        //   this._searchTextBox.Selected = true;
        // }
        return;
      }

      if (this._isAddingNewWarp && !this._isNamingWarp) {
        // ReSharper disable twice PossibleLossOfFraction
        Vector2 tileLocation = new((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
        this._hoverText = $"({tileLocation.X}, {tileLocation.Y})";
        return;
      }

      if (this._textBoxComponent.containsPoint(x, y)) {
        this._textBox.SelectMe();
        this._textBox.Selected = true;
      }

      if (this._makeWarpGlobalButton.containsPoint(x, y))
        this._hoverText = this._makeWarpGlobalButton.hoverText;
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true) {
      if (!this._isAddingNewWarp && !this._isNamingWarp) {
        if (this._upperRightCloseButton.containsPoint(x, y)) {
          this.exitThisMenu();
          Game1.playSound("drumkit6");
        }

        if (this._previousPageButton.containsPoint(x, y) && this._currentPage > 0) {
          --this._currentPage;
          // if (this._previousPageButton.scale != 0.0) {
          //   this._previousPageButton.scale -= 0.25f;
          //   this._previousPageButton.scale = Math.Max(0.75f, this._previousPageButton.scale);
          // }
          Game1.playSound("drumkit6");
        }

        if (this._nextPageButton.containsPoint(x, y) && this._currentPage < this._customWarpPages.Count - 1) {
          ++this._currentPage;
          // if (this._nextPageButton.scale != 0.0) {
          //   this._nextPageButton.scale -= 0.25f;
          //   this._nextPageButton.scale = Math.Max(0.75f, this._nextPageButton.scale);
          // }
          Game1.playSound("drumkit6");
        }

        if (this._removeWarpButton.containsPoint(x, y)) {
          this._isRemovingWarp = !this._isRemovingWarp;
          // if (this._removeWarpButton.scale != 0.0) {
          //   this._removeWarpButton.scale -= 0.25f;
          //   this._removeWarpButton.scale = Math.Max(0.75f, this._removeWarpButton.scale);
          // }
          Game1.playSound("drumkit6");
        }

        if (this._addWarpButton.containsPoint(x, y)) {
          this._isAddingNewWarp = !this._isAddingNewWarp;
          // if (this._addWarpButton.scale != 0.0) {
          //   this._addWarpButton.scale -= 0.25f;
          //   this._addWarpButton.scale = Math.Max(0.75f, this._addWarpButton.scale);
          // }
          Game1.playSound("drumkit6");
        }

        // if (this._searchButton.containsPoint(x, y)) {
        //   // if (this._searchButton.scale != 0.0) {
        //   //   this._searchButton.scale -= 0.25f;
        //   //   this._searchButton.scale = Math.Max(0.75f, this._searchButton.scale);
        //   // }
        //   this._isSearching = !this._isSearching;
        //   Game1.playSound("drumkit6");
        // }

        foreach (KeyValuePair<ClickableTextureComponent, CustomWarp> pair in this._customWarpPages[this._currentPage]) {
          if (!pair.Key.containsPoint(x, y))
            continue;

          if (this._isRemovingWarp) {
            Game1.player.currentLocation.createQuestionDialogue(
              $"Are you sure you want to remove {pair.Value.WarpName}?",
              Game1.player.currentLocation.createYesNoResponses(), (_, answer) => {
                if (answer == "Yes")
                  this._warpHelper.TryRemove(pair.Value.WarpUniqueId, pair.Value.IsGlobal);
              });
            Game1.playSound("drumkit6");
          }
          else {
            // Game1.warpFarmer(pair.Value.MapName, pair.Value.TileX, pair.Value.TileY, false);
            LocationRequest location = Game1.getLocationRequest(pair.Value.MapName);
            location.OnWarp += () => Game1.player.position.Set(new Vector2(pair.Value.TileX, pair.Value.TileY) * 64f);
            Game1.warpFarmer(location, pair.Value.TileX, pair.Value.TileY, Game1.player.FacingDirection);
            Game1.playSound("drumkit6");
          }

          // if (this._isSearching) {
          //   this._searchTextBox.Selected = true;
          //   this._searchTextBox.SelectMe();
          //   this._searchTextBox.Update();
          // }

          // if (pair.Key.scale == 0.0) 
          //   continue;
          //
          // pair.Key.scale -= 0.25f;
          // pair.Key.scale = Math.Max(0.75f, pair.Key.scale);
        }

        return;
      }

      if (this._isAddingNewWarp && !this._isNamingWarp) {
        // ReSharper disable twice PossibleLossOfFraction
        this._newWarpTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
        this._isNamingWarp = !this._isNamingWarp;
        return;
      }

      this._textBox.Selected = true;
      this._textBox.SelectMe();
      this._textBox.Update();

      if (this._makeWarpGlobalButton.containsPoint(x, y)) {
        this._makeGlobal = !this._makeGlobal;
        Game1.playSound("drumkit6");
      }
    }

    public override void receiveKeyPress(Keys key) {
      if (key == Keys.Escape) {
        this._isNamingWarp = false;
        this._isAddingNewWarp = false;
        this._textBox.Text = "";
        Game1.playSound("bigDeSelect");
        Game1.exitActiveMenu();
      }
    }

    public override void receiveScrollWheelAction(int direction) {
      base.receiveScrollWheelAction(direction);

      if (direction > 0 && this._currentPage > 0) {
        this._currentPage--;
        Game1.playSound("shwip");
      }
      else if (direction < 0 && this._currentPage < this._customWarpPages.Count - 1) {
        this._currentPage++;
        Game1.playSound("shwip");
      }
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) {
      base.gameWindowSizeChanged(oldBounds, newBounds);
      this.xPositionOnScreen = Game1.viewport.Width / 2 - (632 + borderWidth * 2) / 2 - 8;
      this.yPositionOnScreen = Game1.uiViewport.Height / 2 - borderWidth * 2 + 16 + 256;
      this.RepositionElements();
    }

    public override void update(GameTime time) {
      // if (this._isSearching) {
      //   this._searchTextBox.SelectMe();
      //   this._searchTextBox.Selected = true;
      // }
      // else {
      //   this._searchTextBox.Selected = false;
      // }

      if (this._isNamingWarp) {
        this._textBox.SelectMe();
        this._textBox.Selected = true;
      }
      else {
        this._textBox.Selected = false;
      }
    }

    public override void draw(SpriteBatch b) {
      if (!this._isAddingNewWarp && !this._isNamingWarp) {
        if (!Game1.options.showMenuBackground)
          b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

        // highlight warps in current location
        // foreach(KeyValuePair<ClickableTextureComponent, CustomWarp> pair in this._customWarpPages.SelectMany(dict => dict).Where(pair => pair.Value.MapName.Equals(Game1.player.currentLocation.NameOrUniqueName)).DistinctBy(pair => new { pair.Value.TileX, pair.Value.TileY }))
        //   b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(pair.Value.TileX, pair.Value.TileY) * Game1.tileSize), new Rectangle(194, 388, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);

        Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);

        if (this._isRemovingWarp)
          GridWarpMenu.DrawTopScrollBackgroundString(b, "Remove warp");

        this._upperRightCloseButton.draw(b);

        // IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(293, 360, 24, 24), this.xPositionOnScreen + 16, this.yPositionOnScreen + 8, 256, 64, Color.White, 4f, false);
        // b.Draw(Game1.mouseCursors, this.xPositionOnScreen + 16, new Rectangle(293, 360, 24, 24), );

        if (this._customWarpPages.Count > 1) {
          if (this._currentPage > 0)
            this._previousPageButton.draw(b);
          if (this._currentPage < this._customWarpPages.Count - 1)
            this._nextPageButton.draw(b);
        }

        foreach (ClickableTextureComponent warp in this._customWarpPages[this._currentPage].Keys) warp.draw(b);
        // Color hoverColor = this._isRemovingWarp ? Color.Red : Color.White;
        // b.Draw(warp.texture, new Vector2(warp.bounds.X, warp.bounds.Y), warp.sourceRect, warp.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? hoverColor : Color.Gray, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        // b.Draw(this._addWarpButton.texture, new Vector2(this._addWarpButton.bounds.X, this._addWarpButton.bounds.Y), this._addWarpButton.sourceRect, this._addWarpButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.White : Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        // b.Draw(this._removeWarpButton.texture, new Vector2(this._removeWarpButton.bounds.X, this._removeWarpButton.bounds.Y), this._removeWarpButton.sourceRect, this._removeWarpButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.White : Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        // b.Draw(this._searchButton.texture, new Vector2(this._searchButton.bounds.X, this._searchButton.bounds.Y), this._searchButton.sourceRect, this._searchButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.White : Color.Gray, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

        this._addWarpButton.draw(b);
        this._removeWarpButton.draw(b);
        // this._searchButton.draw(b);

        // if (this._isSearching) 
        //   this._searchTextBox.Draw(b);
      }
      else if (this._isAddingNewWarp && !this._isNamingWarp) {
        GridWarpMenu.DrawTopScrollBackgroundString(b, "Add new warp");
        // ReSharper disable twice PossibleLossOfFraction
        Vector2 tileLocation = new((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f), new Rectangle(194, 388, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
      }
      else {
        GridWarpMenu.DrawTopScrollBackgroundString(b, "Name warp");
        this._textBox.Draw(b);
        b.Draw(this._makeWarpGlobalButton.texture, new Vector2(this._makeWarpGlobalButton.bounds.X, this._makeWarpGlobalButton.bounds.Y), new Rectangle(this._makeGlobal ? 236 : 227, 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
      }

      if (!string.IsNullOrWhiteSpace(this._hoverText))
        IClickableMenu.drawHoverText(b, Game1.parseText(this._hoverText, Game1.smallFont, 784), Game1.smallFont);

      this.drawMouse(b);
    }

    private static void DrawTopScrollBackgroundString(SpriteBatch b, string text) {
      if (string.IsNullOrWhiteSpace(text))
        return;

      SpriteText.drawStringWithScrollBackground(b, text, Game1.viewport.Width / 2 - SpriteText.getWidthOfString(text) / 2, 16);
    }

    private void TextBoxEnter(TextBox sender) {
      if (!this._isNamingWarp || sender.Text.Length < 1)
        return;

      string mapName = Game1.player.currentLocation.NameOrUniqueName;
      bool isBuilding = false;

      foreach (Farm farm in this._helper.Multiplayer.GetActiveLocations().OfType<Farm>()) {
        Building building = farm.buildings.FirstOrDefault(building => building.indoors.Value != null && Game1.player.currentLocation.Equals(building.indoors.Value));
        if (building == default)
          continue;

        mapName = building.indoors.Value.NameOrUniqueName;
        isBuilding = true;
        break;
      }

      this.HandleAddNewWarp(sender.Text, mapName, (int)this._newWarpTile.X, (int)this._newWarpTile.Y, this._makeGlobal, isBuilding);
      this._isAddingNewWarp = false;
      this._isNamingWarp = false;

      Game1.exitActiveMenu();
    }

    // private void SearchTextBoxEnter(TextBox sender) {
    //   if (!this._isSearching || sender.Text.Length < 1)
    //     return;
    //   
    //   this._customWarpPages.Clear();
    //   this.SetUpWarpPages(sender.Text);
    //   // this._isSearching = false;
    // }

    private void HandleAddNewWarp(string warpName, string mapName, int x, int y, bool isGlobal, bool isBuilding) {
      Guid uniqueId = Guid.NewGuid();

      if (!this._warpHelper.TryAdd(uniqueId, new CustomWarp(warpName, mapName, x, y, isGlobal, isBuilding, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), uniqueId), this._makeGlobal)) Game1.addHUDMessage(new HUDMessage("Guid already exists in the dictionary? Wow.", 3));
      // Game1.addHUDMessage(new HUDMessage($"A warp with the name \"{warpName}\" already exists!", 3));
    }

    private void RepositionElements() {
      this._previousPageButton.bounds = new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height + 80, 64, 64);
      this._nextPageButton.bounds = new Rectangle(this.xPositionOnScreen + this.width - 64, this.yPositionOnScreen + this.height + 80, 64, 64);
      this._addWarpButton.bounds = new Rectangle(this.xPositionOnScreen + this.width - 160, this.yPositionOnScreen, 64, 64);
      this._removeWarpButton.bounds = new Rectangle(this.xPositionOnScreen + this.width - 80, this.yPositionOnScreen, 64, 64);
      this._textBox.X = Game1.viewport.Width / 2 - 192 - 128;
      this._textBox.Y = Game1.viewport.Height / 2;
      this._makeWarpGlobalButton.bounds = new Rectangle(this._textBox.X + this._textBox.Width + 48, this._textBox.Y + 6, 36, 36);

      this._customWarpPages.Clear();
      this.SetUpWarpPages();
    }

    // TODO: filtering system with tokens?, e.g.: # for map names, ! for global (or not), etc.
    private void SetUpWarpPages() {
      Dictionary<ClickableTextureComponent, CustomWarp> newPage = this.CreateNewPage();
      ClickableTextureComponent[,] pageLayout = this.CreateNewPageLayout();
      int x = 0;
      int y = 0;
      foreach (KeyValuePair<Guid, CustomWarp> pair in this._warpHelper.CustomWarps) {
        // if (!string.IsNullOrWhiteSpace(filter) && pair.Value.WarpName.ToLower().Contains(filter.ToLower()))
        //   continue;
        //
        // ModEntry.StaticLogger.Log($"\n\t{filter}\n\t{pair.Value.WarpName}");

        while (this.SpaceOccupied(pageLayout, x, y)) {
          ++x;
          if (x < this._maxItemsPerRow)
            continue;

          x = 0;
          ++y;
          if (y < this._maxItemsPerColumn)
            continue;

          newPage = this.CreateNewPage();
          pageLayout = this.CreateNewPageLayout();
          x = 0;
          y = 0;
        }

        ClickableTextureComponent warp = new($"{pair.Value.WarpName}", new Rectangle(this.xPositionOnScreen + 48 + x * 64 + x * 16, this.yPositionOnScreen + 112 + y * 64 + y * 16, 64, 64), "", $"{GridWarpMenu.GetWarpHoverText(pair.Value)}", Game1.mouseCursors, new Rectangle(257, 284, 16, 16), 4f);
        newPage.Add(warp, pair.Value);
        pageLayout[x, y] = warp;
      }
    }

    private Dictionary<ClickableTextureComponent, CustomWarp> CreateNewPage() {
      Dictionary<ClickableTextureComponent, CustomWarp> dictionary = new();
      this._customWarpPages.Add(dictionary);

      return dictionary;
    }

    private ClickableTextureComponent[,] CreateNewPageLayout() {
      return new ClickableTextureComponent[this._maxItemsPerRow, this._maxItemsPerColumn];
    }

    private bool SpaceOccupied(ClickableTextureComponent[,] pageLayout, int x, int y) {
      if (pageLayout[x, y] != null)
        return true;

      if (y + 1 < this._maxItemsPerColumn)
        return pageLayout[x, y + 1] != null;

      return true;
    }

    private static string GetWarpHoverText(CustomWarp warp) {
      return $"Warp name: {warp.WarpName}\nLocation: {warp.MapName}\nCoordinates: ({warp.TileX}, {warp.TileY})\n{(warp.IsGlobal ? "Global warp" : "Local warp")}";
      // return $"Warp name: {warp.WarpName}\nLocation: {warp.MapName}\nCoordinates: ({warp.TileX}, {warp.TileY})\nGUID: {warp.WarpUniqueId}\n{(warp.IsGlobal ? "Global warp" : "Local warp")}";
      // return $"Warp name: {warp.WarpName}\nCoordinates: ({warp.TileX}, {warp.TileY})\n{(warp.IsGlobal ? "Global warp" : "Local warp")}";
    }
  }
}