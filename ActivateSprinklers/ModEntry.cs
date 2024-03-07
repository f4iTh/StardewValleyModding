using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ActivateSprinklers.Common.Configs;
using ActivateSprinklers.Common.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace ActivateSprinklers {
  public class ModEntry : Mod {
    private readonly HashSet<Vector2> _tilesCheckedController = new();
    private readonly HashSet<Vector2> _tilesCheckedKeyboard = new();

    private ModConfig _config;

    private IDictionary<int, Vector2[]> _customSprinklerCoverage;
    private bool _didGrabTileCheck;
    private ModIntegrations _integrations;

    public override void Entry(IModHelper helper) {
      I18n.Init(helper.Translation);

      this._config = helper.ReadConfig<ModConfig>();

      helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
      helper.Events.GameLoop.OneSecondUpdateTicking += this.HandleGetCustomSprinklerCoverage;
      helper.Events.GameLoop.UpdateTicking += this.HandleActionButtonHeldController;
      helper.Events.GameLoop.UpdateTicking += this.HandleActionButtonHeldKeyboard;
      // helper.Events.Input.ButtonsChanged += this.HandleActionButton;
      helper.Events.Input.ButtonReleased += this.HandleClearCheckedTiles;
    }

    private void OnGameLaunched(object sender, EventArgs e) {
      this._integrations = new ModIntegrations(this.Helper.ModRegistry);
      this._customSprinklerCoverage = this.GetCustomSprinklerCoverage();

      new GenericModConfig(
        this.Helper.ModRegistry,
        this.ModManifest,
        () => this._config,
        () => {
          this._config = new ModConfig();
          this.Helper.WriteConfig(this._config);
        },
        () => this.Helper.WriteConfig(this._config)
      ).Register();
    }

    private void HandleGetCustomSprinklerCoverage(object sender, OneSecondUpdateTickingEventArgs e) {
      if (!ModEntry.IsReady())
        return;

      this._customSprinklerCoverage = this.GetCustomSprinklerCoverage();
    }

    private void HandleActionButtonHeldController(object sender, UpdateTickingEventArgs e) {
      if (!ModEntry.IsReady() || !Game1.options.gamepadControls || this._didGrabTileCheck || (Game1.player.CurrentItem != null && (Utility.IsNormalObjectAtParentSheetIndex(Game1.player.CurrentItem, 915) || Game1.player.CurrentItem.Name.ToLower().Contains("sprinkler"))))
        return;

      // if (!this.Helper.Input.IsDown(SButton.ControllerA))
      //   return;

      GamePadState currentGamePadState = Game1.input.GetGamePadState();
      if (!currentGamePadState.IsButtonDown(Buttons.A))
        return;

      // looking directly at a sprinkler
      GamePadState oldGamePadState = Game1.oldPadState;
      if (!oldGamePadState.IsButtonDown(Buttons.A)) {
        Vector2 grabTile = Game1.player.GetGrabTile();
        if (oldGamePadState.IsButtonDown(Buttons.A) || !Game1.player.currentLocation.Objects.TryGetValue(grabTile, out SObject sprinkler) || !this.IsSprinkler(sprinkler))
          return;

        this._didGrabTileCheck = true;
        this.HandleActivateSprinkler(sprinkler);

        return;
      }

      if (this._config.AdjacentTileDirection == AdjacentTileDirection.None)
        return;

      // holding the action button and getting side tiles
      Vector2 playerPosition = Game1.player.Position / 64;
      foreach (Vector2 adjTile in this.GetAdjacentSideTiles(playerPosition).Where(tile => !this._tilesCheckedController.Contains(tile))) {
        if (!Game1.player.currentLocation.Objects.TryGetValue(adjTile, out SObject sideSprinkler) || !this.IsSprinkler(sideSprinkler))
          continue;

        // this.SuppressPrismaticToolsScarecrow(sideSprinkler, SButton.ControllerA);
        // this.SuppressRadioactiveToolsScarecrow(sideSprinkler, SButton.ControllerA);

        this.HandleActivateSprinkler(sideSprinkler);
        this._tilesCheckedController.Add(adjTile);
      }
    }

    private void HandleActionButtonHeldKeyboard(object sender, UpdateTickingEventArgs e) {
      if (!ModEntry.IsReady() || (Game1.player.CurrentItem != null && (Utility.IsNormalObjectAtParentSheetIndex(Game1.player.CurrentItem, 915) || Game1.player.CurrentItem.Name.ToLower().Contains("sprinkler"))))
        return;
      
      Vector2 tile = this.Helper.Input.GetCursorPosition().GrabTile;
      if (this._config.InfiniteReach)
        tile = Game1.currentCursorTile;

      if (this._tilesCheckedKeyboard.Contains(tile))
        return;

      KeyboardState keyboardState = Game1.input.GetKeyboardState();
      MouseState mouseState = Game1.input.GetMouseState();
      if (!Game1.isOneOfTheseKeysDown(keyboardState, Game1.options.actionButton) && mouseState.RightButton != ButtonState.Pressed)
        return;

      if (!Game1.player.currentLocation.Objects.TryGetValue(tile, out SObject sprinkler) || !this.IsSprinkler(sprinkler))
        return;

      this.HandleActivateSprinkler(sprinkler);
      this._tilesCheckedKeyboard.Add(tile);
    }

    // TODO: handle button logic here instead?
    // private void HandleActionButton(object sender, ButtonsChangedEventArgs e) {
    //   if (e.Held.Any(b => b.IsActionButton())) {
    //     // ...
    //   }
    //   
    //   if (e.Pressed.Any(button => button.IsActionButton())) {
    //     // ...
    //   }
    // }

    private void HandleClearCheckedTiles(object sender, ButtonReleasedEventArgs e) {
      if (!e.Button.IsActionButton())
        return;

      this._tilesCheckedController.Clear();
      this._tilesCheckedKeyboard.Clear();
      this._didGrabTileCheck = false;
    }

    private void HandleActivateSprinkler(SObject sprinkler) {
      Multiplayer multiplayer = (Multiplayer)typeof(Game1).GetField("multiplayer", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);

      GameLocation location = Game1.player.currentLocation;
      float currentStamina = Game1.player.Stamina;
      int oldPower = Game1.player.toolPower;
      Game1.player.toolPower = 0;
      WateringCan wateringCan = new() { WaterLeft = 100 };

      if (this._config.SprinklerAnimation == SprinklerAnimation.NewDayAnimation)
        ModEntry.ApplySprinklerAnimationCustomDelay(location, sprinkler, 100);

      foreach (Vector2 tile in ModEntry.GetCoverage(sprinkler, sprinkler.TileLocation, this._customSprinklerCoverage)) {
        if (tile == sprinkler.TileLocation)
          continue;

        if (location.terrainFeatures.ContainsKey(tile))
          location.terrainFeatures[tile].performToolAction(wateringCan, 0, tile, Game1.player.currentLocation);
        if (location.Objects.ContainsKey(tile))
          location.Objects[tile].performToolAction(wateringCan, Game1.player.currentLocation);
        location.performToolAction(wateringCan, (int)tile.X, (int)tile.Y);

        if (this._config.SprinklerAnimation == SprinklerAnimation.WateringCanAnimation)
          multiplayer?.broadcastSprites(location, new TemporaryAnimatedSprite(13, new Vector2(tile.X * 64f, tile.Y * 64f), Color.White, 10, Game1.random.NextDouble() < 0.5, 70f, 0, 64, (tile.Y * 64f + 32f) / 10000f - 0.01f) {
            delayBeforeAnimationStart = 100
          });

        wateringCan.WaterLeft = wateringCan.waterCanMax;
        Game1.player.stamina = currentStamina;
      }

      Game1.player.toolPower = oldPower;
    }

    private IDictionary<int, Vector2[]> GetCustomSprinklerCoverage() {
      if (this._integrations == null)
        return null;

      IDictionary<int, Vector2[]> sprinklerCoverage = new Dictionary<int, Vector2[]>();

      if (this._integrations.BetterSprinklersApi != null)
        foreach (KeyValuePair<int, Vector2[]> kvp in this._integrations.BetterSprinklersApi.GetSprinklerCoverage())
          sprinklerCoverage[kvp.Key] = kvp.Value;

      // if (this._integrations.LineSprinklersApi != null)
      //   foreach (KeyValuePair<int, Vector2[]> kvp in this._integrations.LineSprinklersApi.GetSprinklerCoverage())
      //     sprinklerCoverage[kvp.Key] = kvp.Value;

      // ReSharper disable once InvertIf
      if (this._integrations.SimpleSprinklerApi != null)
        foreach (KeyValuePair<int, Vector2[]> kvp in this._integrations.SimpleSprinklerApi.GetNewSprinklerCoverage())
          sprinklerCoverage[kvp.Key] = kvp.Value;

      return sprinklerCoverage;
    }

    private static IEnumerable<Vector2> GetCoverage(SObject sprinkler, Vector2 origin, IDictionary<int, Vector2[]> customSprinklerCoverage) {
      IEnumerable<Vector2> sprinklerCoverage = sprinkler.GetSprinklerTiles();

      if (customSprinklerCoverage.TryGetValue(sprinkler.ParentSheetIndex, out Vector2[] sprinklerCoverageTiles))
        sprinklerCoverage = new HashSet<Vector2>(sprinklerCoverage.Concat(sprinklerCoverageTiles.Select(tile => tile + origin)));

      return sprinklerCoverage;
    }

    // Not needed anymore?
    // private void SuppressPrismaticToolsScarecrow(SObject sprinkler, SButton actionButton) {
    //   if (!this._modRegistry.IsLoaded("stokastic.PrismaticTools"))
    //     return;
    //
    //   if (!this._integrations.PrismaticTools.IsLoaded)
    //     return;
    //
    //   if (!this._integrations.PrismaticTools.IsScarecrow())
    //     return;
    //
    //   if (!sprinkler.ParentSheetIndex.Equals(this._integrations.PrismaticTools.GetSprinklerId()))
    //     return;
    //
    //   this.Helper.Input.Suppress(actionButton);
    // }

    // Not needed anymore?
    // private void SuppressRadioactiveToolsScarecrow(SObject sprinkler, SButton actionButton) {
    //   if (!this._modRegistry.IsLoaded("kakashigr.RadioactiveTools"))
    //     return;
    //
    //   if (!this._integrations.RadioactiveTools.IsLoaded)
    //     return;
    //
    //   if (!this._integrations.RadioactiveTools.IsScarecrow())
    //     return;
    //
    //   if (!sprinkler.ParentSheetIndex.Equals(this._integrations.RadioactiveTools.GetSprinklerId()))
    //     return;
    //
    //   // this.Helper.Input.Suppress(actionButton);
    // }

    private IEnumerable<Vector2> GetAdjacentSideTiles(Vector2 pos) {
      List<Vector2> sideTiles = new();
      Vector2 position = new((int)pos.X, (int)pos.Y);

      // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
      switch (this._config.AdjacentTileDirection) {
        case AdjacentTileDirection.Left:
          switch (Game1.player.FacingDirection) {
            case 0:
              sideTiles.Add(new Vector2(-1f, 0f) + position);
              break;
            case 1:
              sideTiles.Add(new Vector2(0f, -1f) + position);
              break;
            case 2:
              sideTiles.Add(new Vector2(1f, 0f) + position);
              break;
            case 3:
              sideTiles.Add(new Vector2(0f, 1f) + position);
              break;
          }

          break;
        case AdjacentTileDirection.Right:
          switch (Game1.player.FacingDirection) {
            case 0:
              sideTiles.Add(new Vector2(1f, 0f) + position);
              break;
            case 1:
              sideTiles.Add(new Vector2(0f, 1f) + position);
              break;
            case 2:
              sideTiles.Add(new Vector2(-1f, 0f) + position);
              break;
            case 3:
              sideTiles.Add(new Vector2(0f, -1f) + position);
              break;
          }

          break;
        case AdjacentTileDirection.LeftRight:
          switch (Game1.player.FacingDirection) {
            case 0:
            case 2:
              sideTiles.Add(new Vector2(-1f, 0f) + position);
              sideTiles.Add(new Vector2(1f, 0f) + position);
              break;
            case 1:
            case 3:
              sideTiles.Add(new Vector2(0f, -1f) + position);
              sideTiles.Add(new Vector2(0f, 1f) + position);
              break;
          }

          break;
      }

      return sideTiles;
    }

    private bool IsSprinkler(SObject obj) {
      return obj.IsSprinkler() || (obj.bigCraftable.Value && this._customSprinklerCoverage.ContainsKey(obj.ParentSheetIndex));
    }

    // literally just Object::ApplySprinklerAnimation() with a custom delay and fewer loops
    private static void ApplySprinklerAnimationCustomDelay(GameLocation location, SObject sprinkler, int delay) {
      int radius = sprinkler.GetModifiedRadiusForSprinkler();
      if (radius < 0)
        return;

      switch (radius) {
        case 0:
          location.temporarySprites.Add(new TemporaryAnimatedSprite(29, sprinkler.TileLocation * 64f + new Vector2(0f, -48f), Color.White * 0.5f, 4, false, 60f, 10) {
            delayBeforeAnimationStart = delay
          });
          location.temporarySprites.Add(new TemporaryAnimatedSprite(29, sprinkler.TileLocation * 64f + new Vector2(48f, 0f), Color.White * 0.5f, 4, false, 60f, 10) {
            delayBeforeAnimationStart = delay,
            rotation = (float)Math.PI / 2f
          });
          location.temporarySprites.Add(new TemporaryAnimatedSprite(29, sprinkler.TileLocation * 64f + new Vector2(0f, 48f), Color.White * 0.5f, 4, false, 60f, 10) {
            delayBeforeAnimationStart = delay,
            rotation = (float)Math.PI
          });
          location.temporarySprites.Add(new TemporaryAnimatedSprite(29, sprinkler.TileLocation * 64f + new Vector2(-48f, 0f), Color.White * 0.5f, 4, false, 60f, 10) {
            delayBeforeAnimationStart = delay,
            // rotation = 4.712389f
            rotation = (float)Math.PI * 1.5f
          });
          break;
        case 1:
          location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 1984, 192, 192), 60f, 3, 10, sprinkler.TileLocation * 64f + new Vector2(-64f, -64f), false, false) {
            delayBeforeAnimationStart = delay,
            color = Color.White * 0.4f
          });
          break;
        default:
          float scale = radius / 2f;
          location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 2176, 320, 320), 60f, 4, 10, sprinkler.TileLocation * 64f + new Vector2(32f, 32f) + new Vector2(-160f, -160f) * scale, false, false) {
            delayBeforeAnimationStart = delay,
            color = Color.White * 0.4f,
            scale = scale
          });
          break;
      }
    }

    private static bool IsReady() {
      return Context.IsWorldReady && Game1.currentLocation != null && Game1.player.CanMove && !Game1.player.hasMenuOpen.Value;
    }
  }
}