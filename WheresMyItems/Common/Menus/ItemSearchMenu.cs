using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModCommon.Extensions;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using WheresMyItems.Common.Configs;
using WheresMyItems.Common.Enums;
using SObject = StardewValley.Object;

namespace WheresMyItems.Common.Menus {
  public class ItemSearchMenu : IClickableMenu {
    private static string _previousSearchString = string.Empty;
    private readonly IDictionary<Vector2, Chest> _chests = new Dictionary<Vector2, Chest>();
    private readonly ModConfig _config;
    private readonly IDictionary<Vector2, IEnumerable<Item>> _items = new Dictionary<Vector2, IEnumerable<Item>>();

    // private readonly IMonitor _monitor;

    private readonly ClickableTextureComponent _previousSearchButton;

    private readonly TextBox _textBox;
    private string _hoverText = string.Empty;

    public ItemSearchMenu(ModConfig config) {
      // this._monitor = monitor;
      this._config = config;
      this._textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor) {
        X = Game1.viewport.Width / 2 - Game1.viewport.Width / 4, // X = Game1.viewport.Width / 2 - Game1.tileSize * 4,
        Y = Game1.viewport.Height - Game1.tileSize * 2,
        Width = Game1.viewport.Width / 2, // Width = Game1.tileSize * 8,
        Height = Game1.tileSize * 3,
        Selected = true
      };
      this._textBox.OnEnterPressed += this.TextBoxEnter;
      this._previousSearchButton = new ClickableTextureComponent(new Rectangle(this._textBox.X - 96, this._textBox.Y - 8, 64, 64), Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4f) {
        hoverText = I18n.Strings_Previoussearch_Tooltip(string.IsNullOrWhiteSpace(_previousSearchString) ? I18n.Strings_Previoussearch_Nopreviousquery() : _previousSearchString)
      };
    }

    // TODO: handle list of keys that don't add any text to the text box and return without doing anything? e.g. F-keys, Alt, ..
    public override void receiveKeyPress(Keys key) {
      if (key.Equals(Keys.Escape)) {
        this.HandleSearchItems(true);
        // this.HandleMenuCloseHighlighting();
        this.exitThisMenu();
      }

      this.HandleSearchItems();
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true) {
      if (!this._previousSearchButton.containsPoint(x, y)) 
        return;
      
      Game1.playSound("smallSelect");
      this._textBox.Text = _previousSearchString;
      this.HandleSearchItems();
    }

    public override void performHoverAction(int x, int y) {
      this._hoverText = string.Empty;
      
      this._previousSearchButton.tryHover(x, y, 0.25f);

      if (this._previousSearchButton.containsPoint(x, y))
        this._hoverText = this._previousSearchButton.hoverText;
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) {
      base.gameWindowSizeChanged(oldBounds, newBounds);

      this._textBox.X = Game1.viewport.Width / 2 - Game1.viewport.Width / 4;
      this._textBox.Y = Game1.viewport.Height - Game1.tileSize * 2;
      this._textBox.Width = Game1.viewport.Width / 2;
      this._textBox.Height = Game1.tileSize * 3;
      this._previousSearchButton.bounds = new Rectangle(this._textBox.X - 96, this._textBox.Y - 8, 64, 64);
    }

    public override void draw(SpriteBatch b) {
      b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
      this._textBox.Draw(b);
      this._previousSearchButton.draw(b); // TODO: draw hover tooltip?

      if (!string.IsNullOrWhiteSpace(this._hoverText))
        IClickableMenu.drawHoverText(b, this._hoverText, Game1.smallFont);

      foreach (KeyValuePair<Vector2, IEnumerable<Item>> pair in this._items) {
        Item[] items = pair.Value.ToArray();
        if (items.Length == 0)
          continue;

        if (this._config.ChestHighlightMethod == ChestHighlightMethod.PulsatingChest && this._chests.TryGetValue(pair.Key, out Chest chest))
          chest.draw(b, (int)pair.Key.X, (int)pair.Key.Y, (float)(0.5f * Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250) + 0.5f));

        // arrows pointing towards chests
        if (this._config.GuideArrowOption != GuideArrowOption.None) {
          double rotation = Game1.GlobalToLocal(Game1.viewport, Game1.player.Position + new Vector2(32f, -28f)).CalculateAngleToTarget(Game1.GlobalToLocal(pair.Key * Game1.tileSize + new Vector2(32f, 24f)));
          b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.player.Position + new Vector2(32f, -28f) + new Vector2((float)(Game1.tileSize * Math.Cos(rotation)), (float)(Game1.tileSize * Math.Sin(-rotation)))), new Rectangle(76, 72, 40, 44), Color.White, (float)rotation.RadiansToStardewRotation(), new Vector2(20, 22), 1f, SpriteEffects.None, 1f);
        }

        if (this._config.ItemDisplayStyle == ItemDisplayStyle.None)
          continue;

        int maxLength = this._config.MaxItemsDrawnOverChests == -1 ? items.Length : Math.Min(items.Length, this._config.MaxItemsDrawnOverChests);
        for (int i = 0; i < maxLength; i++) {
          if (i >= maxLength)
            break;

          Item item = items[i];
          Vector2 position = this._config.ItemDisplayStyle switch {
            ItemDisplayStyle.Horizontal => Game1.GlobalToLocal(Game1.viewport, pair.Key * Game1.tileSize + new Vector2(i * 56f - (maxLength - 1) * 28f, -24f)),
            ItemDisplayStyle.Vertical => Game1.GlobalToLocal(Game1.viewport, pair.Key * Game1.tileSize + new Vector2(0f, -24f + i * 56f - (maxLength - 1) * 28f)),
            _ => default
          };
          item.drawInMenu(b, position, 1f, 1f, 1f, StackDrawType.Hide);
        }

        // if (this._config.ItemDrawDirection is ItemDrawDirection.GridHorizontal) {
        //   int maxItemsPerLine = maxLength;
        //   int currentLine = 0;
        //   double totalRows = Math.Ceiling((double)items.Length / maxLength);
        //   bool lastLine = false;
        //
        //   for (int i = 0; i < items.Length; i++) {
        //     int remaining = items.Length - i;
        //
        //     if (i != 0 && i % maxLength == 0)
        //       currentLine++;
        //
        //     if (maxItemsPerLine * (currentLine + 1) > items.Length && !lastLine) {
        //       maxItemsPerLine = remaining;
        //       lastLine = true;
        //     }
        //
        //     Item item = items[i];
        //     float xAdj = i % maxLength * 56f - (maxItemsPerLine - 1) * 28f;
        //     float yAdj = currentLine * 56f - ((int)totalRows - 1) * 28f;
        //     Vector2 position = Game1.GlobalToLocal(Game1.viewport, pair.Key * Game1.tileSize + new Vector2(0, -24f) + new Vector2(xAdj, yAdj));
        //     item.drawInMenu(b, position, 1f, 1f, 1f, StackDrawType.Hide);
        //   }
        // }
      }

      this.drawMouse(b);
    }

    private void HandleSearchItems(bool exitingMenu = false) {
      this._items.Clear();
      this._chests.Clear();

      if (string.IsNullOrWhiteSpace(this._textBox.Text))
        return;

      foreach (KeyValuePair<Vector2, SObject> obj in Game1.player.currentLocation.Objects.Pairs.Where(pair => pair.Value is Chest)) {
        NetObjectList<Item> items = (obj.Value as Chest)?.items;
        if (items == null || !items.Any())
          continue;

        Item[] itemList = this.GetItemsMatchingTextboxText(items).ToArray();
        if (!itemList.Any())
          continue;

        this._chests.TryAdd(obj.Key, (Chest)obj.Value);
        this._items.TryAdd(obj.Key, itemList);
        if (exitingMenu || this._config.ChestHighlightMethod != ChestHighlightMethod.TypingRipple)
          continue;

        Rectangle sourceRect = new(0, 320, Game1.tileSize, Game1.tileSize);
        Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.animationsName, sourceRect, 60f, 8, 0, obj.Key * Game1.tileSize + new Vector2(0f, -16f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
      }
    }

    private IEnumerable<Item> GetItemsMatchingTextboxText(IEnumerable<Item> items) {
      return items.Where(item => item.Name.ToLower().Contains(this._textBox.Text.ToLower()));
    }

    private void TextBoxEnter(TextBox sender) {
      if (sender.Text.Length > 0) {
        _previousSearchString = this._textBox.Text;
        this.HandleSearchItems(true);
        // this.HandleMenuCloseHighlighting();
      }

      this.exitThisMenu();
    }

    // private void HandleMenuCloseHighlighting() {
    //   foreach (KeyValuePair<Vector2, Chest> obj in this._chests) {
    //     // add one final slow ripple to highlight the chests after closing the menu
    //     Rectangle sourceRect = new(0, 320, Game1.tileSize, Game1.tileSize);
    //     Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.animationsName, sourceRect, 120f, 8, 0, obj.Key * Game1.tileSize + new Vector2(0f, -16f), false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
    //   }
    // }
  }
}