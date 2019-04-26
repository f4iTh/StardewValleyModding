using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace WheresMyItems
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private SButton toggleButton;

        private ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            
            helper.Events.Input.ButtonPressed += this.OnButtonPress;
            this.Config = this.Helper.ReadConfig<ModConfig>();

            toggleButton = this.Config.ToggleButton;
        }

        private void OnButtonPress(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player.currentLocation == null)
                return;
            if (toggleButton.Equals(e.Button))
                this.OpenMenu();
        }

        private void OpenMenu()
        {
			if (Game1.activeClickableMenu != null || !Context.CanPlayerMove)
				return;
            Game1.activeClickableMenu = new ModMenu(Game1.viewport.Width / 2 - (600 + WheresMyItems.ModMenu.borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + WheresMyItems.ModMenu.borderWidth * 2) / 2, 800 + WheresMyItems.ModMenu.borderWidth * 2, 600 + WheresMyItems.ModMenu.borderWidth * 2);
        }
    }
}