using System;
using StardewModdingAPI;

namespace ModCommon.Integrations {
    internal abstract class BaseIntegration : IModIntegration {
        protected BaseIntegration(string label, string modId, string minVersion, IModRegistry modRegistry, IMonitor monitor) {
            this.Label = label;
            this.ModId = modId;
            this.ModRegistry = modRegistry;
            this.Monitor = monitor;

            IManifest manifest = modRegistry.Get(this.ModId)?.Manifest;
            if (manifest == null)
                return;
            if (manifest.Version.IsOlderThan(minVersion)) {
                monitor.Log($"Detected {label} {manifest.Version}, but need {minVersion} or later. Disabled integration with this mod.", LogLevel.Warn);
                return;
            }

            this.IsLoaded = true;
        }

        protected string ModId { get; }
        protected IModRegistry ModRegistry { get; }
        protected IMonitor Monitor { get; }
        public string Label { get; }
        public bool IsLoaded { get; protected set; }

        protected TInterface GetValidatedApi<TInterface>() where TInterface : class {
            TInterface api = this.ModRegistry.GetApi<TInterface>(this.ModId);

            if (api != null) return api;
            this.Monitor.Log($"Detected {this.Label}, but couldn't fetch its API. Disabled integration with this mod.", LogLevel.Warn);
            return null;
        }

        protected void AssertLoaded() {
            if (!this.IsLoaded)
                throw new InvalidOperationException($"The {this.Label} integration isn't loaded.");
        }
    }
}
