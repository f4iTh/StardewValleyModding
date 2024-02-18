using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace ModCommon.Integrations.PrismaticTools {
    internal class PrismaticToolsIntegration : BaseIntegration {
        private readonly IPrismaticToolsApi ModApi;

        public PrismaticToolsIntegration(IModRegistry modRegistry, IMonitor monitor)
            : base("Prismatic Tools", "stokastic.PrismaticTools", "1.3.0", modRegistry, monitor) {
            if (!this.IsLoaded)
                return;

            this.ModApi = this.GetValidatedApi<IPrismaticToolsApi>();
            this.IsLoaded = this.ModApi != null;
        }

        public bool IsScarecrow() {
            this.AssertLoaded();
            return this.ModApi.ArePrismaticSprinklersScarecrows;
        }

        public int GetSprinklerID() {
            this.AssertLoaded();
            return this.ModApi.SprinklerIndex;
        }

        public IEnumerable<Vector2> GetSprinklerCoverage() {
            this.AssertLoaded();
            return this.ModApi.GetSprinklerCoverage(Vector2.Zero);
        }
    }
}
