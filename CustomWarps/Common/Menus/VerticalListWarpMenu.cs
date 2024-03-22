using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CustomWarps.Common.Menus.Elements;
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
  /// <summary>The vertical list warp menu.</summary>
  public class VerticalListWarpMenu : IClickableMenu {
    /// <summary>How many warps should be visible at once.</summary>
    private const int MAX_VISIBLE_ITEMS = 5;

    /// <summary>The previously selected tab.</summary>
    private static string _lastSelectedTab = "all-warps";

    /// <summary>The previously selected sorting option.</summary>
    private static string _lastSelectedSortingOption = "date-descending";

    /// <summary>The add warp button.</summary>
    private readonly ClickableTextureComponent _addWarpButton;

    /// <summary>The tab element for all warps.</summary>
    private readonly ClickableTextureComponent _allWarpsTab;

    /// <summary>The custom warp buttons.</summary>
    private readonly List<ClickableComponent> _customWarpButtons = new();

    /// <summary>The unsorted list containing custom warps.</summary>
    private readonly List<CustomWarp> _customWarpsDefault = new();

    /// <summary>The down arrow for scrolling down the list.</summary>
    private readonly ClickableTextureComponent _downArrow;

    /// <summary>The dropdown element for sorting options.</summary>
    private readonly DropdownElement _dropDown;

    /// <summary>The tab element for global warps.</summary>
    private readonly ClickableTextureComponent _globalWarpsTab;

    /// <inheritdoc cref="IModHelper" />
    private readonly IModHelper _helper;

    /// <summary>The tab element for local warps.</summary>
    private readonly ClickableTextureComponent _localWarpsTab;

    /// <summary>The button to make the warp accessible from any save file.</summary>
    private readonly ClickableTextureComponent _makeWarpGlobalButton;

    /// <summary>The remove warp button.</summary>
    private readonly ClickableTextureComponent _removeWarpButton;

    /// <summary>The scrollbar.</summary>
    private readonly ClickableTextureComponent _scrollBar;

    /// <summary>The textbox for naming a warp.</summary>
    private readonly TextBox _textBox;

    /// <summary>A clickable component for the textbox so it can be clicked and selected.</summary>
    private readonly ClickableComponent _textBoxComponent;

    /// <summary>The up arrow for scrolling up the list.</summary>
    private readonly ClickableTextureComponent _upArrow;

    /// <summary>The upper-right close menu button.</summary>
    private readonly ClickableTextureComponent _upperRightCloseButton;

    /// <inheritdoc cref="WarpHelper" />
    private readonly WarpHelper _warpHelper;
    
    /// <summary>Whether to close the menu after a warp.</summary>
    private readonly bool _closeMenuOnWarp;

    /// <summary>The current warp index.</summary>
    private int _currentItemIndex;

    /// <summary>The sorted and filtered list containing custom warps.</summary>
    private List<CustomWarp> _customWarps = new();

    /// <summary>The hover text.</summary>
    private string _hoverText = "";

    /// <summary>Whether a warp is currently being created.</summary>
    private bool _isAddingNewWarp;

    /// <summary>Whether a warp is currently being named.</summary>
    private bool _isNamingWarp;

    /// <summary>Whether a warp is being removed.</summary>
    private bool _isRemovingWarp;

    /// <summary>Whether the menu is currently being scrolled.</summary>
    private bool _isScrolling;

    /// <summary>Whether a warp should be accessible from any save file.</summary>
    private bool _makeGlobal;

    /// <summary>The tile location of the new warp.</summary>
    private Vector2 _newWarpTile;

    /// <summary>The rectangle behind the scrollbar element that can be clicked.</summary>
    private Rectangle _scrollBarRunner;
    
    // ///<summary>The search button.</summary>
    // private readonly ClickableTextureComponent _searchButton;

    /// <summary>The menu constructor.</summary>
    /// <param name="helper">Simplified API for writing mods.</param>
    /// <param name="warpHelper">A helper class for handling custom warp data.</param>
    /// <param name="closeMenuOnWarp">Whether to close the menu after a warp.</param>
    // TODO: handle numbers better instead of using arbitrary (and seemingly random) numbers?
    // TODO: better icons for add and remove warp buttons?
    // TODO: separate adding and naming a warp into separate (sub)menus?
    // TODO: handle search text box logic
    // TODO: handle controller logic?
    public VerticalListWarpMenu(IModHelper helper, WarpHelper warpHelper, bool closeMenuOnWarp = true) {
      this._helper = helper;
      this._warpHelper = warpHelper;
      this._closeMenuOnWarp = closeMenuOnWarp;

      this.xPositionOnScreen = Game1.uiViewport.Width / 2 - (800 + borderWidth * 2) / 2;
      this.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + borderWidth * 2) / 2;
      this.width = 800 + borderWidth * 2;
      this.height = 600 + borderWidth * 2;

      this._upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 24, this.yPositionOnScreen - 12, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
      this._upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 88, this.yPositionOnScreen + 16 + 96, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
      this._downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 88, this.yPositionOnScreen + this.height - 64 - 8, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
      this._scrollBar = new ClickableTextureComponent(new Rectangle(this._upArrow.bounds.X + 12, this._upArrow.bounds.Y + this._upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
      this._scrollBarRunner = new Rectangle(this._scrollBar.bounds.X, this._upArrow.bounds.Y + this._upArrow.bounds.Height + 4, this._scrollBar.bounds.Width, this.height - 64 - this._upArrow.bounds.Height - 28 - 96 - 8);

      this._addWarpButton = new ClickableTextureComponent("Add", new Rectangle(this.xPositionOnScreen + this.width - 248, this.yPositionOnScreen + 24, 64, 64), "", I18n.Strings_Addwarp_Tooltip(), Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f);
      this._removeWarpButton = new ClickableTextureComponent("Remove", new Rectangle(this.xPositionOnScreen + this.width - 248 + 64 + 8, this.yPositionOnScreen + 24, 64, 64), "", I18n.Strings_Removewarp_Tooltip(), Game1.mouseCursors, new Rectangle(192, 256, 64, 64), 1f);
      // this._addWarpButton = new ClickableTextureComponent("Add", new Rectangle(this.xPositionOnScreen + this.width - 248, this.yPositionOnScreen + this.height - 2, 64, 64), "", "Add warp", Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f);
      // this._removeWarpButton = new ClickableTextureComponent("Remove", new Rectangle(this.xPositionOnScreen + this.width - 248 + 64 + 8, this.yPositionOnScreen + this.height - 2, 64, 64), "", "Remove warp", Game1.mouseCursors, new Rectangle(192, 256, 64, 64), 1f);

      this._allWarpsTab = new ClickableTextureComponent("all-warps", new Rectangle(this.xPositionOnScreen - 64, this.yPositionOnScreen + 116, 64, 64), "", I18n.Strings_Tabs_All(), Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f);
      this._globalWarpsTab = new ClickableTextureComponent("global-warps", new Rectangle(this.xPositionOnScreen - 64, this.yPositionOnScreen + 116 + 64, 64, 64), "", I18n.Strings_Tabs_Global(), Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f);
      this._localWarpsTab = new ClickableTextureComponent("local-warps", new Rectangle(this.xPositionOnScreen - 64, this.yPositionOnScreen + 116 + 128, 64, 64), "", I18n.Strings_Tabs_Local(), Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f);

      this._textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor) {
        X = Game1.uiViewport.Width / 2 - (600 + borderWidth * 2) / 2,
        Y = Game1.viewport.Height / 2,
        Width = 600 + borderWidth * 2 - 16,
        Height = 192
      };
      this._textBox.OnEnterPressed += this.TextBoxEnter;
      this._textBoxComponent = new ClickableComponent(new Rectangle(this._textBox.X, this._textBox.Y, 192, 48), "");
      this._makeWarpGlobalButton = new ClickableTextureComponent("Global", new Rectangle(this._textBox.X + 32, this._textBox.Y + 96 - 6, 36, 36), "", I18n.Strings_Globalwarp_Tooltip(), Game1.mouseCursors, new Rectangle(!this._makeGlobal ? 227 : 236, 425, 9, 9), 4f);
      // this._searchButton = new ClickableTextureComponent("Search", new Rectangle(this.xPositionOnScreen + this.width - 248 + 64 + 16, this.yPositionOnScreen + 24, 64, 64), "", "Search", Game1.mouseCursors, new Rectangle(208, 321, 14, 15), 4f);

      this._dropDown = new DropdownElement(helper.Reflection, new List<Tuple<string, string>> {
        new("date-descending", I18n.Strings_Sorting_Date_Descending()),
        new("date-ascending", I18n.Strings_Sorting_Date_Ascending()),
        new("name-ascending", I18n.Strings_Sorting_Warpname_Ascending()),
        new("name-descending", I18n.Strings_Sorting_Warpname_Descending())
      }, _lastSelectedSortingOption);

      this.ReloadWarps();

      for (int i = 0; i < MAX_VISIBLE_ITEMS; i++)
        this._customWarpButtons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16 + i * ((this.height - 256) / 4) + 96, this.width - 32 - 96, (this.height - 256) / 4), i.ToString()));
    }

    /// <summary>Handles updating menu element positions when game window size changes.</summary>
    /// <param name="oldBounds">The bounds before the window size changed.</param>
    /// <param name="newBounds">The bounds after the window size changed.</param>
    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) {
      base.gameWindowSizeChanged(oldBounds, newBounds);

      this._upperRightCloseButton.bounds = new Rectangle(this.xPositionOnScreen + this.width - 24, this.yPositionOnScreen - 12, 48, 48);
      this._upArrow.bounds = new Rectangle(this.xPositionOnScreen + this.width - 88, this.yPositionOnScreen + 16 + 96, 44, 48);
      this._downArrow.bounds = new Rectangle(this.xPositionOnScreen + this.width - 88, this.yPositionOnScreen + this.height - 64 - 8, 44, 48);
      this._scrollBar.bounds = new Rectangle(this._upArrow.bounds.X + 12, this._upArrow.bounds.Y + this._upArrow.bounds.Height + 4, 24, 40);
      this._scrollBarRunner = new Rectangle(this._scrollBar.bounds.X, this._upArrow.bounds.Y + this._upArrow.bounds.Height + 4, this._scrollBar.bounds.Width, this.height - 64 - this._upArrow.bounds.Height - 28 - 96 - 8);

      this._addWarpButton.bounds = new Rectangle(this.xPositionOnScreen + this.width - 248, this.yPositionOnScreen + 24, 64, 64);
      this._removeWarpButton.bounds = new Rectangle(this.xPositionOnScreen + this.width - 248 + 64 + 8, this.yPositionOnScreen + 24, 64, 64);
      // this._addWarpButton.bounds = new Rectangle(this.xPositionOnScreen + this.width - 248, this.yPositionOnScreen + this.height - 2, 64, 64);
      // this._removeWarpButton.bounds = new Rectangle(this.xPositionOnScreen + this.width - 248 + 64 + 8, this.yPositionOnScreen + this.height - 2, 64, 64);

      this._allWarpsTab.bounds = new Rectangle(this.xPositionOnScreen - 64, this.yPositionOnScreen + 116, 64, 64);
      this._globalWarpsTab.bounds = new Rectangle(this.xPositionOnScreen - 64, this.yPositionOnScreen + 116 + 64, 64, 64);
      this._localWarpsTab.bounds = new Rectangle(this.xPositionOnScreen - 64, this.yPositionOnScreen + 116 + 128, 64, 64);

      this._textBoxComponent.bounds = new Rectangle(this._textBox.X, this._textBox.Y, 192, 48);
      this._makeWarpGlobalButton.bounds = new Rectangle(this._textBox.X + 32, this._textBox.Y + 96 - 6, 36, 36);
      // this._searchButton.bounds = new Rectangle(this.xPositionOnScreen + this.width - 248 - 8 - 64, this.yPositionOnScreen + 24, 64, 64);

      for (int i = 0; i < MAX_VISIBLE_ITEMS; i++)
        this._customWarpButtons[i].bounds = new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16 + i * ((this.height - 256) / 4) + 96, this.width - 32 - 96, (this.height - 256) / 4);
    }

    /// <summary>Handle mouse scroll wheel action.</summary>
    /// <param name="direction">Which direction the mouse wheel was scrolled.</param>
    public override void receiveScrollWheelAction(int direction) {
      if (this._dropDown.IsExpanded || this._isAddingNewWarp || this._isNamingWarp)
        return;

      if (direction > 0 && this._currentItemIndex > 0) {
        this.SimulateUpArrowPressed();
        Game1.playSound("shiny4");
      }
      else if (direction < 0 && this._currentItemIndex < Math.Max(0, this._customWarps.Count - MAX_VISIBLE_ITEMS)) {
        this.SimulateDownArrowPressed();
        Game1.playSound("shiny4");
      }
    }

    /// <summary>Handle left-click.</summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="playSound"></param>
    public override void receiveLeftClick(int x, int y, bool playSound = true) {
      if (!this._isAddingNewWarp && !this._isNamingWarp) {
        this._dropDown.TryClick(x, y);

        if (this._scrollBar.containsPoint(x, y))
          this._isScrolling = true;

        if (this._upperRightCloseButton.containsPoint(x, y))
          this.exitThisMenu();

        if (this._downArrow.containsPoint(x, y) && this._currentItemIndex < Math.Max(0, this._customWarps.Count - MAX_VISIBLE_ITEMS)) {
          this.SimulateDownArrowPressed();
          Game1.playSound("shwip");
        }

        if (this._upArrow.containsPoint(x, y) && this._currentItemIndex > 0) {
          this.SimulateUpArrowPressed();
          Game1.playSound("shwip");
        }

        if (this._allWarpsTab.containsPoint(x, y)) {
          this.HandleSwitchTab(this._allWarpsTab.name);
          Game1.playSound("smallSelect");
        }

        if (this._globalWarpsTab.containsPoint(x, y)) {
          this.HandleSwitchTab(this._globalWarpsTab.name);
          Game1.playSound("smallSelect");
        }

        if (this._localWarpsTab.containsPoint(x, y)) {
          this.HandleSwitchTab(this._localWarpsTab.name);
          Game1.playSound("smallSelect");
        }

        if (this._addWarpButton.containsPoint(x, y)) {
          this._isAddingNewWarp = !this._isAddingNewWarp;
          Game1.playSound("drumkit6");
        }

        if (this._removeWarpButton.containsPoint(x, y)) {
          this._isRemovingWarp = !this._isRemovingWarp;
          Game1.playSound("drumkit6");
        }

        // if (this._searchButton.containsPoint(x, y)) {
        //   // TODO: search button handling
        //   Game1.playSound("drumkit6");
        // }
        for (int i = 0; i < this._customWarpButtons.Count; i++) {
          if (this._currentItemIndex + i >= this._customWarps.Count || !this._customWarpButtons[i].containsPoint(x, y))
            continue;

          CustomWarp warp = this._customWarps[this._currentItemIndex + i];
          if (this._isRemovingWarp) {
            Game1.player.currentLocation.createQuestionDialogue(I18n.Strings_Removewarp_Questiontext(warp.WarpName), Game1.player.currentLocation.createYesNoResponses(), (_, answer) => {
              if (answer == "Yes") this._warpHelper.TryRemove(warp.WarpUniqueId, warp.IsGlobal);
            });
          }
          else {
            LocationRequest location = Game1.getLocationRequest(warp.MapName);
            location.OnWarp += () => Game1.player.position.Set(new Vector2(warp.TileX, warp.TileY) * 64f);
            Game1.warpFarmer(location, warp.TileX, warp.TileY, Game1.player.FacingDirection);
          }
          
          Game1.playSound("drumkit6");
          
          if (this._closeMenuOnWarp)
            this.exitThisMenu();
        }

        return;
      }

      if (this._isAddingNewWarp && !this._isNamingWarp) {
        // ReSharper disable twice PossibleLossOfFraction
        this._newWarpTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
        this._isNamingWarp = !this._isNamingWarp;
        return;
      }

      this._textBox.SelectMe();

      if (this._makeWarpGlobalButton.containsPoint(x, y)) {
        this._makeGlobal = !this._makeGlobal;
        Game1.playSound("drumkit6");
      }
    }

    /// <summary>Handle left-click release.</summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="playSound"></param>
    public override void releaseLeftClick(int x, int y) {
      if (this._isAddingNewWarp || this._isNamingWarp)
        return;

      this._isScrolling = false;

      if (this._dropDown.TryClick(x, y))
        this.HandleSortCustomWarps();
    }

    /// <summary>Handle left-click hold.</summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="playSound"></param>
    public override void leftClickHeld(int x, int y) {
      if (this._isAddingNewWarp || this._isNamingWarp || !this._isScrolling)
        return;

      int newY = this._scrollBar.bounds.Y;
      this._scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - 64 - 12 - this._scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this._upArrow.bounds.Height + 20));
      float percentage = (y - this._scrollBarRunner.Y) / (float)this._scrollBarRunner.Height;
      this._currentItemIndex = Math.Min(Math.Max(0, this._customWarps.Count - MAX_VISIBLE_ITEMS), Math.Max(0, (int)(this._customWarps.Count * percentage)));
      this.SetScrollBarToCurrentIndex();

      if (newY != this._scrollBar.bounds.Y)
        Game1.playSound("shiny4");
    }

    /// <summary>Handle hovering over elements.</summary>
    /// <param name="x">The cursor x-coordinate.</param>
    /// <param name="y">The cursor y-coordinate.</param>
    public override void performHoverAction(int x, int y) {
      this._hoverText = "";

      if (!this._isAddingNewWarp && !this._isNamingWarp) {
        this._upperRightCloseButton.tryHover(x, y, 0.25f);
        this._upArrow.tryHover(x, y, 0.25f);
        this._downArrow.tryHover(x, y, 0.25f);
        this._dropDown.TryHover(x, y);
        this._addWarpButton.tryHover(x, y, 0.25f / 4f);
        this._removeWarpButton.tryHover(x, y, 0.25f / 4f);
        // this._searchButton.tryHover(x, y, 0.25f);

        if (this._allWarpsTab.containsPoint(x, y)) this._hoverText = this._allWarpsTab.hoverText;
        if (this._globalWarpsTab.containsPoint(x, y)) this._hoverText = this._globalWarpsTab.hoverText;
        if (this._localWarpsTab.containsPoint(x, y)) this._hoverText = this._localWarpsTab.hoverText;
        if (this._addWarpButton.containsPoint(x, y)) this._hoverText = this._addWarpButton.hoverText;
        if (this._removeWarpButton.containsPoint(x, y)) this._hoverText = this._removeWarpButton.hoverText;
        // if (this._searchButton.containsPoint(x, y)) {
        //   this._hoverText = this._searchButton.hoverText;
        // }
        return;
      }

      // if (this._isAddingNewWarp && !this._isNamingWarp) {
      //   Vector2 tileLocation = new((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
      //   this._hoverText = $"({tileLocation.X}, {tileLocation.Y})";
      //   return;
      // }

      if (this._textBoxComponent.containsPoint(x, y))
        this._textBox.SelectMe();

      // if (this._makeWarpGlobalButton.containsPoint(x, y))
      //   this._hoverText = this._makeWarpGlobalButton.hoverText;
    }

    /// <summary>Handle key press.</summary>
    /// <param name="key">The key that was pressed.</param>
    public override void receiveKeyPress(Keys key) {
      if (key == Keys.Escape)
        // this._isNamingWarp = false;
        // this._isAddingNewWarp = false;
        this.exitThisMenu();
    }

    /// <summary>Handles automatically selecting the text box when naming (sub)menu is active.</summary>
    /// <param name="time">The game time.</param>
    public override void update(GameTime time) {
      if (this._isNamingWarp)
        this._textBox.SelectMe();
    }

    /// <summary>Handles drawing the menu.</summary>
    /// <param name="b">The SpriteBatch.</param>
    public override void draw(SpriteBatch b) {
      if (!this._isAddingNewWarp && !this._isNamingWarp) {
        if (!Game1.options.showMenuBackground)
          b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

        // menu background 
        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), this.xPositionOnScreen, this.yPositionOnScreen + 96, this.width - 96, MAX_VISIBLE_ITEMS * (this.height - 256) / 4 + 4 + 32, Color.White, 4f);

        if (this._isRemovingWarp)
          SpriteText.drawStringWithScrollBackground(b, I18n.Strings_Removewarp_Tooltip(), Game1.viewport.Width / 2 - SpriteText.getWidthOfString(I18n.Strings_Removewarp_Tooltip()) / 2, 16);

        for (int i = 0; i < this._customWarpButtons.Count; i++) {
          if (this._currentItemIndex + i >= this._customWarps.Count)
            continue;

          ClickableComponent currentWarpButton = this._customWarpButtons[i];
          CustomWarp currentWarp = this._customWarps[this._currentItemIndex + i];

          // warp item background
          Color warpBackgroundColor = this.GetWarpBackgroundColor(currentWarpButton);
          IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this._customWarpButtons[i].bounds.X, this._customWarpButtons[i].bounds.Y, this._customWarpButtons[i].bounds.Width, this._customWarpButtons[i].bounds.Height + 4, warpBackgroundColor, 4f, false);

          // warp name
          b.DrawString(Game1.smallFont, currentWarp.WarpName, new Vector2(currentWarpButton.bounds.X + 16 + 8, currentWarpButton.bounds.Y + 24), Color.Black);
          b.DrawString(Game1.smallFont, currentWarp.MapName, new Vector2(currentWarpButton.bounds.X + 16 + 8, currentWarpButton.bounds.Y + 58), Color.Black);

          DateTime date = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
          string dateString = date.AddSeconds(currentWarp.DateAdded).ToLocalTime().ToString(CultureInfo.CurrentCulture);
          b.DrawString(Game1.smallFont, dateString, new Vector2(currentWarpButton.bounds.X + this.width - 248 + 64 + 8 - Game1.smallFont.MeasureString(dateString).X + 24, currentWarpButton.bounds.Y + 24), Color.Black);
        }

        const float tabRotation = (float)(-90f * (Math.PI / 180f));

        // ReSharper disable PossibleLossOfFraction
        b.Draw(Game1.mouseCursors, new Vector2(_lastSelectedTab == this._allWarpsTab.name ? this._allWarpsTab.bounds.X + 8 : this._allWarpsTab.bounds.X, this._allWarpsTab.bounds.Y + 64), new Rectangle(16, 368, 16, 16), Color.White, tabRotation, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        b.Draw(Game1.mouseCursors, new Vector2(_lastSelectedTab == this._allWarpsTab.name ? this._allWarpsTab.bounds.X + this._allWarpsTab.bounds.Width / 2 - 2 + 8 : this._allWarpsTab.bounds.X + this._allWarpsTab.bounds.Width / 2 - 2, this._allWarpsTab.bounds.Y + 40), new Rectangle(52, 372, 8, 10), Color.White, tabRotation, Vector2.Zero, 3f, SpriteEffects.None, 1f);
        Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2(_lastSelectedTab == this._allWarpsTab.name ? this._allWarpsTab.bounds.X + this._allWarpsTab.bounds.Width / 2 - 18 + 8 : this._allWarpsTab.bounds.X + this._allWarpsTab.bounds.Width / 2 - 18, this._allWarpsTab.bounds.Y + 18), 1f, 2f, 2, Game1.player);
        // b.Draw(Game1.mouseCursors, new Vector2(_lastSelectedTab == this._allWarpsTab.name ? this._allWarpsTab.bounds.X + (this._allWarpsTab.bounds.Width / 2) - 16 + 8 : this._allWarpsTab.bounds.X + (this._allWarpsTab.bounds.Width / 2) - 16, this._allWarpsTab.bounds.Y + 12), new Rectangle(660, 83, 12, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

        b.Draw(Game1.mouseCursors, new Vector2(_lastSelectedTab == this._globalWarpsTab.name ? this._globalWarpsTab.bounds.X + 8 : this._globalWarpsTab.bounds.X, this._globalWarpsTab.bounds.Y + 64), new Rectangle(16, 368, 16, 16), Color.White, tabRotation, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        b.Draw(Game1.mouseCursors, new Vector2(_lastSelectedTab == this._globalWarpsTab.name ? this._globalWarpsTab.bounds.X + this._globalWarpsTab.bounds.Width / 2 - 12 + 8 : this._globalWarpsTab.bounds.X + this._globalWarpsTab.bounds.Width / 2 - 12, this._globalWarpsTab.bounds.Y + 48), new Rectangle(52, 372, 8, 10), Color.White, tabRotation, Vector2.Zero, 4f, SpriteEffects.None, 1f);

        b.Draw(Game1.mouseCursors, new Vector2(_lastSelectedTab == this._localWarpsTab.name ? this._localWarpsTab.bounds.X + 8 : this._localWarpsTab.bounds.X, this._localWarpsTab.bounds.Y + 64), new Rectangle(16, 368, 16, 16), Color.White, tabRotation, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2(_lastSelectedTab == this._localWarpsTab.name ? this._localWarpsTab.bounds.X + this._localWarpsTab.bounds.Width / 2 - 18 + 8 : this._localWarpsTab.bounds.X + this._localWarpsTab.bounds.Width / 2 - 18, this._localWarpsTab.bounds.Y + 3 + 2), 1f, 3f, 2, Game1.player);
        // ReSharper restore PossibleLossOfFraction

        // dropdown menu for sorting options
        this._dropDown.Draw(b, this.xPositionOnScreen + 16, this.yPositionOnScreen + 36);

        // scrollbar and arrows if there are enough items
        if (this._customWarps.Count > MAX_VISIBLE_ITEMS) {
          IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this._scrollBarRunner.X, this._scrollBarRunner.Y, this._scrollBarRunner.Width, this._scrollBarRunner.Height, Color.White, 4f);
          this._upArrow.draw(b);
          this._downArrow.draw(b);
          this._scrollBar.draw(b);
        }

        // rest of the menu buttons
        this._addWarpButton.draw(b);
        this._removeWarpButton.draw(b);
        // this._searchButton.draw(b);
        this._upperRightCloseButton.draw(b);
      }
      else if (this._isAddingNewWarp && !this._isNamingWarp) {
        SpriteText.drawStringWithScrollBackground(b, I18n.Strings_Addwarp_Tooltip(), Game1.viewport.Width / 2 - SpriteText.getWidthOfString(I18n.Strings_Addwarp_Tooltip()) / 2, 16);

        // highlight tile over mouse cursor
        // ReSharper disable twice PossibleLossOfFraction
        Vector2 tileLocation = new((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f), new Rectangle(194, 388, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
      }
      else {
        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), this._textBox.X, this._textBox.Y, this._textBox.Width + 16, 96 + 16 + this._makeWarpGlobalButton.bounds.Height + 10, Color.White, 4f, false);
        SpriteText.drawStringWithScrollBackground(b, I18n.Strings_Namewarp_Tooltip(), Game1.viewport.Width / 2 - SpriteText.getWidthOfString(I18n.Strings_Namewarp_Tooltip()) / 2, 16);

        this._textBox.Draw(b);
        // SpriteText.drawString(b, "Name warp:", this._textBox.X, this._textBox.Y - 64 - 12);
        // b.DrawString(Game1.dialogueFont, "Name warp:", new Vector2(this._textBox.X, this._textBox.Y - 64 - 12), Color.Black);
        b.Draw(this._makeWarpGlobalButton.texture, new Vector2(this._makeWarpGlobalButton.bounds.X, this._makeWarpGlobalButton.bounds.Y), new Rectangle(this._makeGlobal ? 236 : 227, 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        b.DrawString(Game1.smallFont, I18n.Strings_Globalwarp_Tooltip(), new Vector2(this._makeWarpGlobalButton.bounds.X + this._makeWarpGlobalButton.bounds.Width + 16, this._makeWarpGlobalButton.bounds.Y), Color.Black);
      }

      if (!string.IsNullOrWhiteSpace(this._hoverText))
        IClickableMenu.drawHoverText(b, this._hoverText, Game1.smallFont);

      this.drawMouse(b);
    }

    /// <summary>Update the custom warp list and load the last selected tab.</summary>
    private void ReloadWarps() {
      this._customWarpsDefault.Clear();
      this._customWarps.Clear();

      foreach (CustomWarp warp in this._warpHelper.CustomWarps.Values)
        this._customWarpsDefault.Add(warp);

      this.HandleSwitchTab(_lastSelectedTab);
    }

    /// <summary>Get the background color for the warp item.</summary>
    /// <param name="currentWarpButton">The warp button.</param>
    private Color GetWarpBackgroundColor(ClickableComponent currentWarpButton) {
      if (!currentWarpButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
        return Color.White;

      if (this._isRemovingWarp)
        return new Color(255, 127, 127);

      return !this._isScrolling && !this._dropDown.IsExpanded ? Color.Wheat : Color.White;
    }

    /// <summary>Handles the <see cref="TextBox" /> enter input.</summary>
    /// <param name="sender">The textbox.</param>
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

      Guid uniqueId = Guid.NewGuid();
      if (!this._warpHelper.TryAdd(uniqueId, new CustomWarp(sender.Text, mapName, (int)this._newWarpTile.X, (int)this._newWarpTile.Y, this._makeGlobal, isBuilding, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), uniqueId), this._makeGlobal)) {
        // realistically this shouldn't happen but if it does, it's not going to happen twice in a row
        uniqueId = Guid.NewGuid();
        this._warpHelper.TryAdd(uniqueId, new CustomWarp(sender.Text, mapName, (int)this._newWarpTile.X, (int)this._newWarpTile.Y, this._makeGlobal, isBuilding, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), uniqueId), this._makeGlobal);
      }

      this._isAddingNewWarp = false;
      this._isNamingWarp = false;
      this.ReloadWarps();
    }

    /// <summary>Sorts custom warps.</summary>
    private void HandleSortCustomWarps() {
      _lastSelectedSortingOption = this._dropDown.SelectedValue;

      string[] args = this._dropDown.SelectedValue.Split('-');
      string sortType = args[0];
      int sortDirection = args[1] == "ascending" ? 1 : -1;

      switch (sortType) {
        case "name":
          this._customWarps.Sort((warp1, warp2) => sortDirection * string.Compare(warp1.WarpName, warp2.WarpName, StringComparison.InvariantCultureIgnoreCase));
          break;
        case "date":
          this._customWarps.Sort((warp1, warp2) => sortDirection * warp1.DateAdded.CompareTo(warp2.DateAdded));
          break;
      }

      // switch (this._dropDown.SelectedValue) {
      //   case "name-ascending":
      //     this._customWarps.Sort((warp1, warp2) => string.Compare(warp1.WarpName, warp2.WarpName, StringComparison.InvariantCultureIgnoreCase));
      //     break;
      //   case "name-descending":
      //     this._customWarps.Sort((warp1, warp2) => -string.Compare(warp1.WarpName, warp2.WarpName, StringComparison.InvariantCultureIgnoreCase));
      //     break;
      //   case "date-ascending":
      //     this._customWarps.Sort((warp1, warp2) => warp1.DateAdded.CompareTo(warp2.DateAdded));
      //     break;
      //   case "date-descending":
      //     this._customWarps.Sort((warp1, warp2) => -warp1.DateAdded.CompareTo(warp2.DateAdded));
      //     break;
      // }
    }

    /// <summary>Handle switching to a different tab.</summary>
    /// <param name="newTab">The name of the new tab.</param>
    private void HandleSwitchTab(string newTab) {
      _lastSelectedTab = newTab;

      this._customWarps = newTab switch {
        "all-warps" => this._customWarpsDefault,
        "global-warps" => this._customWarpsDefault.Where(warp => warp.IsGlobal).ToList(),
        "local-warps" => this._customWarpsDefault.Where(warp => !warp.IsGlobal).ToList(),
        _ => this._customWarps
      };

      this.HandleSortCustomWarps();

      // reset index on tab switch
      this._currentItemIndex = 0;
      this.SetScrollBarToCurrentIndex();
    }

    /// <summary>Set the scrollbar to the current selected warp index.</summary>
    private void SetScrollBarToCurrentIndex() {
      if (this._customWarps.Count <= 0)
        return;

      this._scrollBar.bounds.Y = this._scrollBarRunner.Height / Math.Max(1, this._customWarps.Count - MAX_VISIBLE_ITEMS + 1) * this._currentItemIndex + this._upArrow.bounds.Bottom + 4;
      if (this._currentItemIndex == this._customWarps.Count - MAX_VISIBLE_ITEMS) this._scrollBar.bounds.Y = this._downArrow.bounds.Y - this._scrollBar.bounds.Height - 4;
    }

    /// <summary>Simulate pressing the up arrow.</summary>
    private void SimulateUpArrowPressed() {
      this._upArrow.scale = this._upArrow.baseScale;
      this._currentItemIndex--;
      this.SetScrollBarToCurrentIndex();
    }

    /// <summary>Simulate pressing the down arrow.</summary>
    private void SimulateDownArrowPressed() {
      this._downArrow.scale = this._downArrow.baseScale;
      this._currentItemIndex++;
      this.SetScrollBarToCurrentIndex();
    }
  }
}