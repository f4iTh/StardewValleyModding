using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Linq;

namespace WheresMyItems
{
    public class ModMenu : IClickableMenu
    {
        private TextBox textBox;

        private TextBoxEvent e;

        public ModMenu(int x, int y, int width, int height, bool showUpperRightCloseButton = false)
            : base(x, y, width, height)
        {
            this.textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor)
            {
                X = Game1.viewport.Width / 2 - Game1.tileSize * 4,
                Y = Game1.viewport.Height - Game1.tileSize * 2,
                Width = Game1.tileSize * 8,
                Height = Game1.tileSize * 3
            };
            this.textBox.Selected = true;
            this.e = this.textBoxEnter;
            this.textBox.OnEnterPressed += this.e;
        }

        public override void clickAway()
        {
            Game1.exitActiveMenu();
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        private void textBoxEnter(TextBox sender)
        {
            if (sender.Text.Length >= 0)
            {
                foreach (var obj in Game1.currentLocation.objects.Pairs.Where(x => x.Value is Chest).ToArray())
                {
                    foreach (var item in (obj.Value as Chest).items.ToArray())
                    {
                        if (textBox.Text != string.Empty && item.Name.ToLower().Contains(textBox.Text.ToLower()))
                        {
                            Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.animationsName, new Rectangle(0, 320, Game1.tileSize, Game1.tileSize), 60f, 8, 0, obj.Key * 64, false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
                        }
                    }
                }
                Game1.playSound("bigDeSelect");
                Game1.exitActiveMenu();
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key.Equals(Keys.Escape))
            {
                Game1.playSound("bigDeSelect");
                Game1.exitActiveMenu();
            }

            foreach (var obj in Game1.currentLocation.objects.Pairs.Where(x => x.Value is Chest).ToArray())
            {
                foreach (var item in (obj.Value as Chest).items.ToArray())
                {
                    if (textBox.Text != string.Empty && textBox.Text.Length > 1 && item.Name.ToLower().Contains(textBox.Text.ToLower()))
                    {
                        Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.animationsName, new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), 60f, 8, 0, obj.Key * 64, false, false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
                    }
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            this.textBox.Draw(b);
        }
    }
}
