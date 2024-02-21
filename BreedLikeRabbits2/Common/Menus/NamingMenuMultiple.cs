using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace BreedLikeRabbits2.Common.Menus {
  // TODO: make menu reusable?
  public class NamingMenuMultiple : NamingMenu {
    public delegate void DoneNamingBehavior(string[] names, FarmAnimal[] newRabbits);

    private readonly DoneNamingBehavior _doneNaming;
    private readonly List<string> _names;
    private readonly FarmAnimal[] _newRabbits;
    private readonly IReflectedField<string> _title;
    private int _currentNameIndex;
    private int _namesRemaining;

    // TODO: get rid of reflectionhelper?
    public NamingMenuMultiple(DoneNamingBehavior behavior, FarmAnimal[] newRabbits, IReflectionHelper reflectionHelper, string defaultName = null) : base(null, string.Empty) {
      this._currentNameIndex = 0;
      this._doneNaming = behavior;
      this._title = reflectionHelper.GetField<string>(this, "title");
      this._names = new List<string>();
      this._newRabbits = newRabbits;
      this._namesRemaining = this._newRabbits.Length;
      this.textBox.Text = defaultName ?? Dialogue.randomName();
      this.UpdateTitleText();
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true) {
      // base.receiveLeftClick(x, y, playSound);
      this.textBox.Update();
      if (this.doneNamingButton.containsPoint(x, y)) {
        this.TextBoxEnter(this.textBox);
        Game1.playSound("smallSelect");
      }
      else if (this.randomButton.containsPoint(x, y)) {
        this.textBox.Text = Dialogue.randomName();
        this.randomButton.scale = this.randomButton.baseScale;
        Game1.playSound("drumkit6");
      }
    }

    public override void draw(SpriteBatch b) {
      // base.draw(b);
      b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
      SpriteText.drawStringWithScrollCenteredAt(b, this._title.GetValue(), Game1.uiViewport.Width / 2, Game1.uiViewport.Height / 2 - 128, this._title.GetValue());
      this.textBox.Draw(b);
      this.doneNamingButton.draw(b);
      this.randomButton.draw(b);

      // draw gender icon for current rabbit
      Rectangle genderIconRect = new(this._newRabbits[this._currentNameIndex].isMale() ? 128 : 144, 192, 16, 16);
      b.Draw(Game1.mouseCursors, new Vector2(this.randomButton.bounds.X + this.randomButton.bounds.Width, this.randomButton.bounds.Y - 16f), genderIconRect, Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);

      this.drawMouse(b);
    }

    private void TextBoxEnter(TextBox sender) {
      if (sender.Text.Length < this.minLength)
        return;

      if (this._namesRemaining > 1) {
        this._names.Add(sender.Text);
        this._namesRemaining--;
        this._currentNameIndex++;
        this.UpdateTitleText();
        this.textBox.Text = Dialogue.randomName();
        return;
      }

      if (this._doneNaming != null && this._namesRemaining == 1) {
        this._names.Add(sender.Text);
        this._namesRemaining--; // is this needed?
        this._currentNameIndex++; // is this needed?
        this._doneNaming(this._names.ToArray(), this._newRabbits);
        this.textBox.Selected = false;
        // this.exitFunction?.Invoke();
        Game1.exitActiveMenu();
      }

      Game1.exitActiveMenu();
    }

    private void UpdateTitleText() {
      this._title.SetValue(I18n.Strings_Namingmenu_Title(this._namesRemaining, this._namesRemaining > 1 ? "names" : "name"));
    }
  }
}