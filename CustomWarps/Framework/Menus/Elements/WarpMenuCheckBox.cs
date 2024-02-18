namespace CustomWarps.Framework.Menus.Elements {
/*
    public class WarpMenuCheckBox : OptionsElement {
        private readonly Action<bool> setValue;

        private bool isChecked;

        public WarpMenuCheckBox(string label, bool initialValue, Action<bool> setValue)
            : base(label, -1, -1, 9 * Game1.pixelZoom, 9 * Game1.pixelZoom) {
            this.isChecked = initialValue;
            this.setValue = setValue;
        }

        public override void receiveLeftClick(int x, int y) {
            if (this.greyedOut)
                return;
            Game1.soundBank.PlayCue("drumkit6");
            base.receiveLeftClick(x, y);
            this.isChecked = !this.isChecked;
            this.setValue(this.isChecked);
        }

        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null) {
            b.Draw(Game1.mouseCursors, new Vector2(slotX + this.bounds.X, slotY + this.bounds.Y), this.isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked, Color.White * (this.greyedOut ? 0.33f : 1f), 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.4f);
            base.draw(b, slotX, slotY, context);
        }
    }
*/
}