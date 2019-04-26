using StardewModdingAPI;
using System;

namespace Common.Integrations
{
	internal abstract class BaseIntegration : IModIntegration
	{
		protected string ModID { get; }

		protected IModRegistry ModRegistry { get; }

		protected IMonitor Monitor { get; }

		public string Label { get; }

		public bool IsLoaded { get; protected set; }

		protected BaseIntegration(string label, string modID, string minVersion, IModRegistry modRegistry, IMonitor monitor)
		{
			this.Label = label;
			this.ModID = modID;
			this.ModRegistry = modRegistry;
			this.Monitor = monitor;

			IManifest manifest = modRegistry.Get(this.ModID)?.Manifest;
			if (manifest == null)
				return;
			if (manifest.Version.IsOlderThan(minVersion))
			{
				monitor.Log($"Detected {label} {manifest.Version}, but need {minVersion} or later. Disabled integration with this mod.", LogLevel.Warn);
				return;
			}
			this.IsLoaded = true;
		}

		protected TInterface GetValidatedApi<TInterface>() where TInterface : class
		{
			TInterface api = this.ModRegistry.GetApi<TInterface>(this.ModID);
			if (api == null)
			{
				this.Monitor.Log($"Detected {this.Label}, but couldn't fetch its API. Disabled integration with this mod.", LogLevel.Warn);
				return null;
			}
			return api;
		}

		protected void AssertLoaded()
		{
			if (!this.IsLoaded)
				throw new InvalidOperationException($"The {this.Label} integration isn't loaded.");
		}
	}
}
