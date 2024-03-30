using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModCommon.Extensions;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using WheresMyItems.Common.Configs;
using WheresMyItems.Common.Enums;
using SObject = StardewValley.Object;

namespace WheresMyItems.Common.Menus;

// TODO: text entry menu doesn't always open on the first try?
/// <summary>The item search menu.</summary>
public class ItemSearchMenu : IClickableMenu {
  private const int ID_ELEMENT_TEXTBOX = 10000;
  private const int ID_BUTTON_PREVIOUS_QUERY = 10001;

  /// <inheritdoc cref="ModConfig" />
  private readonly ModConfig _config;

  /// <summary>The previous search query button.</summary>
  private readonly ClickableTextureComponent _previousSearchButton;

  /// <summary>The textbox.</summary>
  private readonly TextBox _textBox;

  /// <summary>The clickable component for the textbox.</summary>
  private readonly ClickableComponent _textBoxClickableComponent;

  /// <summary>A dictionary containing the chests and items with the tile as the key.</summary>
  public readonly IDictionary<Vector2, Tuple<Chest, Item[]>> QueryResults = new Dictionary<Vector2, Tuple<Chest, Item[]>>();

  /// <summary>The hover text.</summary>
  private string _hoverText = string.Empty;

  /// <summary>The previous search query.</summary>
  public string PreviousSearchQuery;

  /// <summary>The menu constructor.</summary>
  /// <param name="config">The mod configuration.</param>
  /// <param name="previousSearchQuery">The previous search query.</param>
  public ItemSearchMenu(ModConfig config, string previousSearchQuery = "") {
    this._config = config;
    this.PreviousSearchQuery = previousSearchQuery;

    this._textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor) {
      X = Game1.uiViewport.Width / 2 - Game1.uiViewport.Width / 4, // X = Game1.viewport.Width / 2 - Game1.tileSize * 4,
      Y = Game1.uiViewport.Height - Game1.tileSize * 2,
      Width = Game1.uiViewport.Width / 2, // Width = Game1.tileSize * 8,
      Height = Game1.tileSize * 3,
      Selected = !Game1.options.SnappyMenus
    };
    this._textBox.OnEnterPressed += this.TextBoxEnter;
    this._textBoxClickableComponent = new ClickableComponent(new Rectangle(this._textBox.X, this._textBox.Y, this._textBox.Width, 96), "clickable textbox") {
      myID = ID_ELEMENT_TEXTBOX,
      leftNeighborID = ID_BUTTON_PREVIOUS_QUERY,
      fullyImmutable = true
    };
    this._previousSearchButton = new ClickableTextureComponent(new Rectangle(this._textBox.X - 96, this._textBox.Y - 8, 64, 64), Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4f) {
      myID = ID_BUTTON_PREVIOUS_QUERY,
      rightNeighborID = ID_ELEMENT_TEXTBOX,
      fullyImmutable = true,
      hoverText = I18n.Strings_Previoussearch_Tooltip(string.IsNullOrWhiteSpace(this.PreviousSearchQuery) ? I18n.Strings_Previoussearch_Nopreviousquery() : this.PreviousSearchQuery)
    };

    ModEntry.StaticMonitor.Log($"name={Game1.player.Name}, uniqueId={Game1.player.UniqueMultiplayerID} opened ItemSearchMenu");

    this.InitializeSnappingBehavior();
  }

  /// <summary>Initializes snapping behavior.</summary>
  private void InitializeSnappingBehavior() {
    this.populateClickableComponentList();

    if (!Game1.options.SnappyMenus)
      return;

    this.snapToDefaultClickableComponent();
  }

  /// <summary>Add all clickable components to a list for snapping behavior.</summary>
  public override void populateClickableComponentList() {
    this.allClickableComponents = [];
    this.allClickableComponents.AddRange([this._textBoxClickableComponent, this._previousSearchButton]);
  }

  /// <summary>Snap to the default clickable component.</summary>
  public override void snapToDefaultClickableComponent() {
    this.setCurrentlySnappedComponentTo(ID_ELEMENT_TEXTBOX);
    this.snapCursorToCurrentSnappedComponent();
  }

  /// <summary>Snaps the cursor to the currently snapped component.</summary>
  public override void snapCursorToCurrentSnappedComponent() {
    // TODO: handle this better; this feels a bit too jank
    StackFrame frame = new(1, true);
    MethodBase method;
    // close the menu if TextEntryMenu::Close called this method
    if ((method = frame.GetMethod()) != null && method.Name == "Close" && method.Module.Name == "Stardew Valley.dll") {
      this.HandleCloseMenu();
      return;
    }
    base.snapCursorToCurrentSnappedComponent();
  }

  /// <summary>Handles updating menu element positions when game window size changes.</summary>
  /// <param name="oldBounds">The bounds before the window size changed.</param>
  /// <param name="newBounds">The bounds after the window size changed.</param>
  public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) {
    base.gameWindowSizeChanged(oldBounds, newBounds);

    this._textBox.X = Game1.uiViewport.Width / 2 - Game1.uiViewport.Width / 4;
    this._textBox.Y = Game1.uiViewport.Height - Game1.tileSize * 2;
    this._textBox.Width = Game1.uiViewport.Width / 2;
    this._textBox.Height = Game1.tileSize * 3;
    this._previousSearchButton.bounds = new Rectangle(this._textBox.X - 96, this._textBox.Y - 8, 64, 64);
  }

  /// <summary>Handle key press.</summary>
  /// <param name="key">The key that was pressed.</param>
  public override void receiveKeyPress(Keys key) {
    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
    switch (key) {
      case Keys.Escape:
        this.HandleCloseMenu();
        break;
    }

    this.HandleSearchItems();
    base.receiveKeyPress(key);
  }

  /// <summary>Handle gamepad key press.</summary>
  /// <param name="button">The button that was pressed.</param>
  public override void receiveGamePadButton(Buttons button) {
    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
    switch (button) {
      case Buttons.B:
      case Buttons.Start:
        this.HandleCloseMenu();
        break;
    }

    base.receiveGamePadButton(button);
  }

  /// <summary>Handle left-click.</summary>
  /// <param name="x">The x-coordinate.</param>
  /// <param name="y">The x-coordinate.</param>
  /// <param name="playSound">Whether to play sound.</param>
  public override void receiveLeftClick(int x, int y, bool playSound = true) {
    if (this._textBoxClickableComponent.containsPoint(x, y)) {
      this._textBox.SelectMe();
      if (Game1.options.SnappyMenus && !Game1.lastCursorMotionWasMouse) {
        Game1.showTextEntry(this._textBox);
      }
      Game1.playSound("smallSelect");
    }

    else if (this._previousSearchButton.containsPoint(x, y)) {
      this._textBox.Text = this.PreviousSearchQuery;
      this.HandleSearchItems();
      Game1.playSound("smallSelect");
    }
  }

  /// <summary>Handle hovering over elements.</summary>
  /// <param name="x">The x-coordinate.</param>
  /// <param name="y">The y-coordinate.</param>
  public override void performHoverAction(int x, int y) {
    this._hoverText = string.Empty;

    this._previousSearchButton.tryHover(x, y, 0.25f);

    if (this._previousSearchButton.containsPoint(x, y))
      this._hoverText = this._previousSearchButton.hoverText;
  }

  /// <summary>Handles menu update.</summary>
  /// <param name="time">The game time.</param>
  public override void update(GameTime time) {
    this._textBox.SelectMe();
  }

  /// <summary>Whether the menu is ready to close.</summary>
  public override bool readyToClose() {
    return false;
  }

  /// <summary>Handles drawing the menu.</summary>
  /// <param name="b">The spritebatch.</param>
  public override void draw(SpriteBatch b) {
    if (Game1.textEntry == null) {
      b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
    }

    this._textBox.Draw(b);
    this._previousSearchButton.draw(b);

    if (!string.IsNullOrWhiteSpace(this._hoverText))
      IClickableMenu.drawHoverText(b, this._hoverText, Game1.smallFont);

    foreach (KeyValuePair<Vector2, Tuple<Chest, Item[]>> pair in this.QueryResults) {
      Item[] items = pair.Value.Item2.ToArray();
      if (items.Length == 0)
        continue;

      if (this._config.ChestHighlightMethod == ChestHighlightMethod.PulsatingChest && pair.Value.Item1 != null) {
        // TODO: handle chest opening animation?
        ItemSearchMenu.DrawChestWithUiScale(pair.Value.Item1, b, (int)pair.Key.X, (int)pair.Key.Y, (float)(0.5f * Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250) + 0.5f));
      }
      
      // arrows pointing towards chests
      if (this._config.GuideArrowOption != GuideArrowOption.None) {
        Vector2 playerCenterPosition = Game1.player.Position + new Vector2(32f, -28f);
        double rotation = Game1.GlobalToLocal(Game1.uiViewport, playerCenterPosition).CalculateAngleToTarget(Game1.GlobalToLocal(Game1.uiViewport, pair.Key * Game1.tileSize + new Vector2(32f, 24f)));
        b.Draw(Game1.mouseCursors, Utility.ModifyCoordinatesForUIScale(Game1.GlobalToLocal(Game1.uiViewport, playerCenterPosition + new Vector2((float)(Game1.tileSize * Math.Cos(rotation)), (float)(Game1.tileSize * Math.Sin(-rotation))))), new Rectangle(76, 72, 40, 44), Color.White, (float)rotation.RadiansToStardewRotation(), new Vector2(20, 22), Utility.ModifyCoordinateForUIScale(1f), SpriteEffects.None, 1f);
      }

      if (this._config.ItemDisplayStyle == ItemDisplayStyle.None)
        continue;

      // handle drawing items
      int maxLength = this._config.MaxItemsDrawnOverChests == -1 ? items.Length : Math.Min(items.Length, this._config.MaxItemsDrawnOverChests);
      for (int i = 0; i < maxLength; i++) {
        if (i >= maxLength)
          break;

        Item item = items[i];
        Vector2 position = this._config.ItemDisplayStyle switch {
          ItemDisplayStyle.Horizontal => pair.Key * Game1.tileSize + new Vector2(i * 56f - (maxLength - 1) * 28f, -24f),
          ItemDisplayStyle.Vertical => pair.Key * Game1.tileSize + new Vector2(0f, -24f + i * 56f - (maxLength - 1) * 28f),
          _ => default
        };

        item.drawInMenu(b, Utility.ModifyCoordinatesForUIScale(Game1.GlobalToLocal(Game1.uiViewport, position)) - ItemSearchMenu.GetItemPositionOffsetForUiScale(new Vector2(32f, 32f)), Utility.ModifyCoordinateForUIScale(1f), 1f, 1f, StackDrawType.Hide);
      }
    }

    this.drawMouse(b);
  }

  /// <summary>Handles searching chests for items that match the search query.</summary>
  /// <param name="exitingMenu">Whether the menu should be exited.</param>
  private void HandleSearchItems(bool exitingMenu = false) {
    this.QueryResults.Clear();

    if (string.IsNullOrWhiteSpace(this._textBox.Text))
      return;

    foreach (KeyValuePair<Vector2, SObject> obj in Game1.player.currentLocation.Objects.Pairs.Where(pair => pair.Value is Chest)) {
      Inventory items = (obj.Value as Chest)?.Items;
      if (items == null || !items.Any())
        continue;

      Item[] itemList = this.GetItemsMatchingTextboxText(items).ToArray();
      if (!itemList.Any())
        continue;

      this.QueryResults.TryAdd(obj.Key, new Tuple<Chest, Item[]>((Chest)obj.Value, itemList));
      if (exitingMenu || this._config.ChestHighlightMethod != ChestHighlightMethod.TypingRipple)
        continue;

      Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.animationsName, new Rectangle(0, 320, Game1.tileSize, Game1.tileSize), 60f, 8, 0, obj.Key * Game1.tileSize + new Vector2(0f, -16f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
    }

    ModEntry.StaticMonitor.Log($"found a total of {this.QueryResults.Count} chest(s) matching the query");
  }

  /// <summary>Gets items matching the search query.</summary>
  /// <param name="items">The chest items.</param>
  private IEnumerable<Item> GetItemsMatchingTextboxText(IEnumerable<Item> items) {
    return items.Where(item => item.Name?.Contains(this._textBox.Text, StringComparison.OrdinalIgnoreCase) ?? item.DisplayName?.Contains(this._textBox.Text, StringComparison.OrdinalIgnoreCase) == true);
  }

  /// <summary>Handles the <see cref="TextBox" /> enter input.</summary>
  /// <param name="sender">The textbox.</param>
  private void TextBoxEnter(TextBox sender) {
    if (sender.Text.Length > 0) {
      this.HandleCloseMenu();
      return;
    }

    this.exitThisMenu();
  }

  /// <summary>Closes the menu and sets the previous search query.</summary>
  private void HandleCloseMenu() {
    this.PreviousSearchQuery = this._textBox.Text;
    this.HandleSearchItems(true);
    this.exitThisMenu();
  }
  
  /// <summary>Gets the item position offset to account for UI scaling.</summary>
  /// <param name="offset">The position offset.</param>
  private static Vector2 GetItemPositionOffsetForUiScale(Vector2 offset) {
    return offset - Utility.ModifyCoordinatesForUIScale(offset);
  }

  // TODO: simplify this instead of using the entire method?
  /// <summary>Draws a chest with proper UI scaling.</summary>
  /// <param name="chest">The chest to draw.</param>
  /// <param name="spriteBatch">The spritebatch.</param>
  /// <param name="x">The x-position.</param>
  /// <param name="y">The y-position.</param>
  /// <param name="alpha">The opacity of the chest.</param>
  /// <param name="scale">The base scale of the chest.</param>
  /// <remarks>Basically a modified <see cref="Chest.draw(SpriteBatch, int, int, float, bool)" /> that accounts for UI scaling.</remarks>
  private static void DrawChestWithUiScale(Chest chest, SpriteBatch spriteBatch, int x, int y, float alpha = 1f, float scale = 4f) {
    if (!chest.playerChest.Value)
      return;
  
    if (chest.playerChoiceColor.Equals(Color.Black)) {
      ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(chest.QualifiedItemId);
      spriteBatch.Draw(dataOrErrorItem.GetTexture(), Utility.ModifyCoordinatesForUIScale(Game1.GlobalToLocal(Game1.uiViewport, new Vector2(x * 64, (y - 1) * 64))), dataOrErrorItem.GetSourceRect(), chest.Tint * alpha, 0.0f, Vector2.Zero, Utility.ModifyCoordinateForUIScale(scale), SpriteEffects.None, (y * 64 + 4) / 10000f);
    }
    else {
      ParsedItemData data = ItemRegistry.GetData(chest.QualifiedItemId);
      if (data == null)
        return;
  
      Rectangle chestSourceRect = data.GetSourceRect(spriteIndex: chest.ParentSheetIndex);
      Texture2D texture = data.GetTexture();
      spriteBatch.Draw(texture, Utility.ModifyCoordinatesForUIScale(Game1.GlobalToLocal(Game1.uiViewport, new Vector2(x * 64, (y - 1) * 64))), chestSourceRect, chest.playerChoiceColor.Value * alpha, 0.0f, Vector2.Zero, Utility.ModifyCoordinateForUIScale(scale), SpriteEffects.None, (y * 64 + 4) / 10000f);
      spriteBatch.Draw(texture, Utility.ModifyCoordinatesForUIScale(Game1.GlobalToLocal(Game1.uiViewport, new Vector2(x * 64, y * 64 + 20))), new Rectangle(0, chest.ParentSheetIndex / 8 * 32 + 53, 16, 11), Color.White * alpha, 0.0f, Vector2.Zero, Utility.ModifyCoordinateForUIScale(scale), SpriteEffects.None, (y * 64 + 6) / 10000f);
    }
  }
}