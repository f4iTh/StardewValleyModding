using System;
using System.Collections.Generic;
using ActivateSprinklers.ModCommon.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using WheresMyItems.Common.Configs;
using WheresMyItems.Common.Enums;
using WheresMyItems.Common.Menus;

namespace WheresMyItems {
  public class WheresMyItems : Mod {
    // private ModIntegrations _integrations;
    private ModConfig _config;
    private IDictionary<Vector2, IEnumerable<Item>> _items = new Dictionary<Vector2, IEnumerable<Item>>();

    public override void Entry(IModHelper helper) {
      I18n.Init(helper.Translation);

      this._config = this.Helper.ReadConfig<ModConfig>();

      helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
      helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
      helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
      helper.Events.Display.RenderedWorld += this.DrawGuideArrowsAfterMenuClose;
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
      // this._integrations = new ModIntegrations(this.Helper.ModRegistry);

      new GenericModConfig(
        this.Helper.ModRegistry,
        this.ModManifest,
        () => this._config,
        () => {
          this._config = new ModConfig();
          this.Helper.WriteConfig(this._config);
        },
        () => { this.Helper.WriteConfig(this._config); }
      ).Register();
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e) {
      if (this._config.GuideArrowOption != GuideArrowOptions.UntilNextMenu)
        return;

      // ReSharper disable once ConvertIfStatementToSwitchStatement
      if (Game1.activeClickableMenu == null)
        return;

      if (Game1.activeClickableMenu is ItemSearchMenu itemSearchMenu) {
        this._items = this.Helper.Reflection.GetField<IDictionary<Vector2, IEnumerable<Item>>>(itemSearchMenu, "_items").GetValue();
        return;
      }

      if (this._items.Count > 0)
        this._items.Clear();
    }

    private void DrawGuideArrowsAfterMenuClose(object sender, RenderedWorldEventArgs e) {
      if (this._config.GuideArrowOption != GuideArrowOptions.UntilNextMenu)
        return;

      if (Game1.activeClickableMenu != null || this._items is not { Count: > 0 })
        return;

      foreach (KeyValuePair<Vector2, IEnumerable<Item>> obj in this._items) {
        double rotation = Game1.GlobalToLocal(Game1.viewport, Game1.player.Position + new Vector2(32f, -28f)).CalculateAngleToTarget(Game1.GlobalToLocal(obj.Key * Game1.tileSize + new Vector2(32f, 24f)));
        e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.player.Position + new Vector2(32f, -28f) + new Vector2((float)(Game1.tileSize * Math.Cos(rotation)), (float)(Game1.tileSize * Math.Sin(-rotation)))), new Rectangle(76, 72, 40, 44), Color.White, (float)rotation.RadiansToStardewRotation(), new Vector2(20, 22), 1f, SpriteEffects.None, 1f);
      }
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e) {
      if (!Context.IsWorldReady || Game1.player.currentLocation == null || Game1.activeClickableMenu != null || !Context.CanPlayerMove || !this._config.ToggleButton.JustPressed())
        return;

      Game1.activeClickableMenu = new ItemSearchMenu(this._config);
    }
  }
}