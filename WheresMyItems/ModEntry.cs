using System;
using System.Collections.Generic;
using System.Linq;
using ActivateSprinklers.ModCommon.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModCommon.Extensions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using WheresMyItems.Common.Configs;
using WheresMyItems.Common.Enums;
using WheresMyItems.Common.Menus;

namespace WheresMyItems {
  /// <summary>The mod entry point.</summary>
  public class ModEntry : Mod {
    /// <inheritdoc cref="IMonitor"/>
    internal static IMonitor StaticMonitor;
    
    /// <summary>The mod configuration.</summary>
    private ModConfig _config;

    /// <summary>The chest tiles that match the current search query.</summary>
    private readonly IDictionary<long, IEnumerable<Vector2>> _chestTiles = new Dictionary<long, IEnumerable<Vector2>>();

    /// <summary>The previous search queries.</summary>
    private readonly IDictionary<long, string> _previousSearchQuery = new Dictionary<long, string>();

    /// <summary>The mod entry point method.</summary>
    /// <param name="helper">The mod helper.</param>
    public override void Entry(IModHelper helper) {
      I18n.Init(helper.Translation);

      StaticMonitor = this.Monitor;
      this._config = this.Helper.ReadConfig<ModConfig>();

      helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
      helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
      helper.Events.Display.MenuChanged += this.HandleMenuChanged;
      helper.Events.Display.RenderedWorld += this.DrawGuideArrowsAfterMenuClose;
    }

    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
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

    /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void HandleMenuChanged(object sender, MenuChangedEventArgs e) {
      if (this._config.GuideArrowOption != GuideArrowOption.UntilNextMenu)
        return;

      if (e.NewMenu == null && e.OldMenu is ItemSearchMenu itemSearchMenu) {
        this._previousSearchQuery.Add(Game1.player.UniqueMultiplayerID, itemSearchMenu.PreviousSearchQuery);
        this._chestTiles.Add(Game1.player.UniqueMultiplayerID, itemSearchMenu.QueryResults.Keys);
        return;
      }

      this._previousSearchQuery.Remove(Game1.player.UniqueMultiplayerID);
      if (this._chestTiles.Remove(Game1.player.UniqueMultiplayerID))
        this.Monitor.Log($"removed search arrows for name={Game1.player.Name}, uniqueId={Game1.player.UniqueMultiplayerID}");
    }

    /// <inheritdoc cref="IDisplayEvents.RenderedWorld" />
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void DrawGuideArrowsAfterMenuClose(object sender, RenderedWorldEventArgs e) {
      if (this._config.GuideArrowOption != GuideArrowOption.UntilNextMenu)
        return;

      if (Game1.activeClickableMenu != null || this._chestTiles == null || !this._chestTiles.Any())
        return;

      foreach (KeyValuePair<long, IEnumerable<Vector2>> pair in this._chestTiles) {
        if (pair.Key != Game1.player.UniqueMultiplayerID)
          continue;
      
        foreach (Vector2 tile in pair.Value) {
          Vector2 playerCenterPosition = Game1.player.Position + new Vector2(32f, -28f);
          double rotation = Game1.GlobalToLocal(Game1.viewport, playerCenterPosition).CalculateAngleToTarget(Game1.GlobalToLocal(tile * Game1.tileSize + new Vector2(32f, 24f)));
          Vector2 finalPosition = Game1.GlobalToLocal(Game1.viewport, playerCenterPosition + new Vector2((float)(Game1.tileSize * Math.Cos(rotation)), (float)(Game1.tileSize * Math.Sin(-rotation))));
          
          e.SpriteBatch.Draw(Game1.mouseCursors, Game1Utils.IsCurrentTargetUiScreen() ? Utility.ModifyCoordinatesForUIScale(finalPosition) : finalPosition, new Rectangle(76, 72, 40, 44), Color.White, (float)rotation.RadiansToStardewRotation(), new Vector2(20, 22), Game1Utils.IsCurrentTargetUiScreen() ? Utility.ModifyCoordinateForUIScale(1f) : 1f, SpriteEffects.None, 1f);
        }
      }
    }

    /// <inheritdoc cref="IInputEvents.ButtonsChanged" />
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e) {
      if (!Context.IsWorldReady || Game1.player.currentLocation == null || Game1.activeClickableMenu != null || !Context.CanPlayerMove)
        return;

      if (!this._config.KeyboardToggleButton.JustPressed() && (this._config.GamepadToggleButton == null || !this._config.GamepadToggleButton.JustPressed())) 
        return;

      this._previousSearchQuery.TryGetValue(Game1.player.UniqueMultiplayerID, out string previousSearchQuery);
      Game1.activeClickableMenu = new ItemSearchMenu(this._config, previousSearchQuery);
    }
  }
}