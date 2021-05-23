using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace ActivateSprinklers {
    public class ActivateSprinklers : Mod {
        private ModConfig _config;
        private IDictionary<int, Vector2[]> _customSprinklerCoverage;
        private ModIntegrations _integrations;

        private bool IsSprinkler(SObject obj) {
            return obj.IsSprinkler() || obj.bigCraftable.Value && this._customSprinklerCoverage.ContainsKey(obj.ParentSheetIndex);
        }

        private static bool IsReady() {
            return Context.IsWorldReady && Game1.currentLocation != null && Game1.player.CanMove && !Game1.player.hasMenuOpen.Value;
        }

        public override void Entry(IModHelper helper) {
            this._config = helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.GameLaunched += this.Initialize;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.GameLoop.UpdateTicking += this.OnGameTick;
        }

        private void Initialize(object sender, EventArgs e) {
            this._integrations = new ModIntegrations(this.Monitor, this.Helper.ModRegistry);
            this._customSprinklerCoverage = this.GetCustomSprinklerCoverage();
            new GenericModConfig(
                this.Helper.ModRegistry,
                this.Monitor,
                this.ModManifest,
                () => this._config,
                () => {
                    this._config = new ModConfig();
                    this.Helper.WriteConfig(this._config);
                    this.Helper.ReadConfig<ModConfig>();
                },
                () => {
                    this.Helper.WriteConfig(this._config);
                    this.Helper.ReadConfig<ModConfig>();
                }
            ).Register();
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e) {
            SButton actionButton;
            if (!IsReady() || (actionButton = e.Pressed.FirstOrDefault(button => button.IsActionButton())) == default)
                return;
            Vector2 tile = Game1.options.gamepadControls
                ? Game1.player.GetGrabTile()
                : this._config.InfiniteReach
                    ? Game1.currentCursorTile
                    : e.Cursor.GrabTile;
            if (!Game1.player.currentLocation.Objects.TryGetValue(tile, out SObject sprinkler) || !this.IsSprinkler(sprinkler)) return;
            if (this._integrations.PrismaticTools.IsLoaded && this._integrations.PrismaticTools.IsScarecrow() && sprinkler.ParentSheetIndex.Equals(this._integrations.PrismaticTools.GetSprinklerID()))
                this.Helper.Input.Suppress(actionButton);
            this.DoActivateSprinkler(sprinkler);
        }

        private void OnGameTick(object sender, UpdateTickingEventArgs e) {
            if (!IsReady())
                return;
            this._customSprinklerCoverage = this.GetCustomSprinklerCoverage();
        }

        private void DoActivateSprinkler(SObject sprinkler) {
            int oldPower = Game1.player.toolPower;
            float currentStamina = Game1.player.Stamina;
            WateringCan can = new WateringCan {WaterLeft = 100};
            Game1.player.toolPower = 0;
            foreach (Vector2 tile in GetCoverage(sprinkler, sprinkler.TileLocation, this._customSprinklerCoverage)) {
                can.DoFunction(Game1.player.currentLocation, (int) tile.X * 64, (int) tile.Y * 64, 0, Game1.player);
                can.WaterLeft = can.waterCanMax;
                Game1.player.Stamina = currentStamina;
            }

            Game1.player.toolPower = oldPower;
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        private IDictionary<int, Vector2[]> GetCustomSprinklerCoverage() {
            IDictionary<int, Vector2[]> sprinklerCoverage = new Dictionary<int, Vector2[]>();
            if (this._integrations.BetterSprinklers.IsLoaded)
                foreach (KeyValuePair<int, Vector2[]> kvp in this._integrations.BetterSprinklers.GetSprinklerTiles())
                    sprinklerCoverage[kvp.Key] = kvp.Value;
            if (this._integrations.LineSprinklers.IsLoaded)
                foreach (KeyValuePair<int, Vector2[]> kvp in this._integrations.LineSprinklers.GetSprinklerTiles())
                    sprinklerCoverage[kvp.Key] = kvp.Value;
            if (this._integrations.SimpleSprinkler.IsLoaded)
                foreach (KeyValuePair<int, Vector2[]> kvp in this._integrations.SimpleSprinkler.GetSprinklerTiles())
                    sprinklerCoverage[kvp.Key] = kvp.Value;
            return sprinklerCoverage;
        }

        private static IEnumerable<Vector2> GetCoverage(SObject sprinkler, Vector2 origin, IDictionary<int, Vector2[]> customSprinklerCoverage) {
            IEnumerable<Vector2> sprinklerCoverage = sprinkler.GetSprinklerTiles();
            if (customSprinklerCoverage.TryGetValue(sprinkler.ParentSheetIndex, out Vector2[] sprinklerCoverageTiles))
                sprinklerCoverage = new HashSet<Vector2>(sprinklerCoverage.Concat(sprinklerCoverageTiles.Select(tile => tile + origin)));
            return sprinklerCoverage;
        }
    }
}
