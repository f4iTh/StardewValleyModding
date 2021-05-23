using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace WheresMyItems {

	public class ModMenu : IClickableMenu {
		private readonly TextBox textBox;
		private readonly TextBoxEvent textBoxEvent;

		public ModMenu(int x, int y, int width, int height, bool showUpperRightCloseButton = false)
			: base(x, y, width, height) {
			this.textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor) {
				//X = Game1.viewport.Width / 2 - Game1.tileSize * 4,
				X = Game1.viewport.Width / 2 - Game1.viewport.Width / 4,
				Y = Game1.viewport.Height - Game1.tileSize * 2,
				//Width = Game1.tileSize * 8,
				Width = Game1.viewport.Width / 2,
				Height = Game1.tileSize * 3,
				Selected = true
			};
			this.textBoxEvent = this.TextBoxEnter;
			this.textBox.OnEnterPressed += this.textBoxEvent;
		}

		public override void clickAway() {
			Game1.exitActiveMenu();
		}

		public override void receiveRightClick(int x, int y, bool playSound = true) {
		}

		private void TextBoxEnter(TextBox sender) {
			if (sender.Text.Length < 0)
				return;
			//IEnumerable<KeyValuePair<Vector2, Object>> chests =
			//	from pair in Game1.currentLocation.objects.Pairs
			//	where pair.Value is Chest
			//	select pair;
			//foreach(KeyValuePair<Vector2, Object> o in chests) {
			//	Chest chest = o.Value as Chest;
			//	IEnumerable<Item> items =
			//		from item in chest.items.ToArray()
			//		where !string.IsNullOrEmpty(textBox.Text) && item.Name.ToLower().Contains(textBox.Text.ToLower())
			//		select item;
			//	foreach(Item item in items) {
			//	}
			//}
			this.DoSearch();
			Game1.playSound("bigDeSelect");
			Game1.exitActiveMenu();
		}

		public override void receiveKeyPress(Keys key) {
			if (key.Equals(Keys.Escape)) {
				Game1.playSound("bigDeSelect");
				Game1.exitActiveMenu();
				this._items.Clear();
			}

			this.DoSearch();
		}

		private void DoSearch() {
			if(string.IsNullOrEmpty(this.textBox.Text)) {
				this._items.Clear();
				return;
			}
			foreach (KeyValuePair<Vector2, Object> obj in Game1.currentLocation.objects.Pairs.Where(x => x.Value is Chest).ToArray()) {
				List<Item> itemList = new List<Item>();
				foreach (Item item in ((Chest) obj.Value).items.ToArray()) {
					if (item.Name.ToLower().Contains(this.textBox.Text.ToLower())) {
						if (!itemList.Contains(item)) 
							itemList.Add(item);
						Game1.player.currentLocation.temporarySprites.Add(
							new TemporaryAnimatedSprite(Game1.animationsName, new Rectangle(0, 320, Game1.tileSize, Game1.tileSize), 60f, 8, 0, obj.Key * Game1.tileSize, false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
					} else {
						if (itemList.Contains(item)) 
							itemList.Remove(item);
						if (this._items.ContainsKey(obj.Key)) this._items[obj.Key] = this._items[obj.Key].Where(n => n.Name.ToLower().Contains(this.textBox.Text.ToLower())).ToList();
						//else 
						//	_items.Add(obj.Key, itemList);
					}
				}
				if(!this._items.ContainsKey(obj.Key)) this._items.Add(obj.Key, itemList);
			}
		}

		private IDictionary<Vector2, List<Item>> _items = new Dictionary<Vector2, List<Item>>();
		public override void draw(SpriteBatch b) {
			base.draw(b);
			this.textBox.Draw(b);
			foreach(KeyValuePair<Vector2, List<Item>> pair in this._items) {
				int count = 0;
				foreach(Item item in pair.Value) {
					Vector2 pos = Game1.GlobalToLocal(Game1.viewport, (pair.Key * Game1.tileSize) + new Vector2(count * 40f, 0f));
					// no texture if not fruit tree or crop, dumb
					//if (WheresMyItems.Integrations.JsonAssets.IsLoaded && WheresMyItems.Integrations.JsonAssets.TryGetCustomSpriteSheet(item, out Texture2D texture, out Rectangle sourceRect, true)) {
					//	b.Draw(texture, pos, sourceRect, Color.White, 0f, new Vector2(8f, 8f), 2f, SpriteEffects.None, 1f);
					//} else {
					//	b.Draw(Game1.objectSpriteSheet, pos, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, item.ParentSheetIndex, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), 2f, SpriteEffects.None, 1f);
					//}
					b.Draw(Game1.objectSpriteSheet, pos, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, item.ParentSheetIndex, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), 2f, SpriteEffects.None, 1f);
					count++;
				}
			}
		}
	}
}