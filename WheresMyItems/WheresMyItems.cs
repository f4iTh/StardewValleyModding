using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace WheresMyItems {

	public class WheresMyItems : Mod {
		private ModIntegrations Integrations;

		private ModConfig Config;

		public override void Entry(IModHelper helper) {
			this.Config = this.Helper.ReadConfig<ModConfig>();

			IModEvents events = helper.Events;
			events.GameLoop.GameLaunched += this.Initialize;
			events.Input.ButtonsChanged += this.OnButtonsChanged;
		}

		private void Initialize(object sender, GameLaunchedEventArgs e) {
			this.Integrations = new ModIntegrations(this.Helper.ModRegistry, this.Monitor);

			new GenericModConfig(
				modRegistry: this.Helper.ModRegistry,
				monitor: this.Monitor,
				manifest: this.ModManifest,
				getConfig: () => this.Config,
				reset: () => {
					this.Config = new ModConfig();
					this.Helper.WriteConfig(this.Config);
					this.Helper.ReadConfig<ModConfig>();
				},
				saveAndApply: () => {
					this.Helper.WriteConfig(this.Config);
					this.Helper.ReadConfig<ModConfig>();
				}
			).Register();
		}

		private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e) {
			if (!Context.IsWorldReady || Game1.player.currentLocation == null || Game1.activeClickableMenu != null || !Context.CanPlayerMove || !this.Config.ToggleButton.JustPressed())
				return;
			Game1.activeClickableMenu = new ModMenu(
				Game1.viewport.Width / 2 - (600 + StardewValley.Menus.IClickableMenu.borderWidth * 2) / 2,
				Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2,
				800 + IClickableMenu.borderWidth * 2,
				600 + IClickableMenu.borderWidth * 2);
		}
	}
}