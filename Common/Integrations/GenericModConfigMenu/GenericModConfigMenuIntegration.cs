using System;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ModCommon.Integrations.GenericModConfigMenu {
  internal class GenericModConfigMenuIntegration<TConfig> : BaseIntegration
    where TConfig : new() {
    private readonly IManifest ConsumerManifest;

    private readonly Func<TConfig> GetConfig;
    private readonly IGenericModConfigMenuApi ModApi;

    private readonly Action Reset;

    private readonly Action SaveAndApply;

    public GenericModConfigMenuIntegration(IModRegistry modRegistry, IMonitor monitor, IManifest consumerManifest, Func<TConfig> getConfig, Action reset, Action saveAndApply)
      : base("Generic Mod Config Menu", "spacechase0.GenericModConfigMenu", "1.3.3", modRegistry, monitor) {
      this.ConsumerManifest = consumerManifest;
      this.GetConfig = getConfig;
      this.Reset = reset;
      this.SaveAndApply = saveAndApply;

      if (!this.IsLoaded) return;
      this.ModApi = this.GetValidatedApi<IGenericModConfigMenuApi>();
      this.IsLoaded = this.ModApi != null;
    }

    public GenericModConfigMenuIntegration<TConfig> RegisterConfig(bool canConfigureInGame) {
      this.AssertLoaded();

      this.ModApi.RegisterModConfig(this.ConsumerManifest, this.Reset, this.SaveAndApply);

      if (canConfigureInGame)
        this.ModApi.SetDefaultIngameOptinValue(this.ConsumerManifest, true);

      return this;
    }

    public GenericModConfigMenuIntegration<TConfig> AddLabel(string label, string description = null) {
      this.AssertLoaded();

      this.ModApi.RegisterLabel(this.ConsumerManifest, label, description);

      return this;
    }

    public GenericModConfigMenuIntegration<TConfig> AddCheckbox(string label, string description, Func<TConfig, bool> get, Action<TConfig, bool> set, bool enable = true) {
      this.AssertLoaded();

      if (enable)
        this.ModApi.RegisterSimpleOption(
          this.ConsumerManifest,
          label,
          description,
          () => get(this.GetConfig()),
          val => set(this.GetConfig(), val)
        );

      return this;
    }

    public GenericModConfigMenuIntegration<TConfig> AddDropdown(string label, string description, Func<TConfig, string> get, Action<TConfig, string> set, string[] choices, bool enable = true) {
      this.AssertLoaded();

      if (enable)
        this.ModApi.RegisterChoiceOption(
          this.ConsumerManifest,
          label,
          description,
          () => get(this.GetConfig()),
          val => set(this.GetConfig(), val),
          choices
        );

      return this;
    }

    public GenericModConfigMenuIntegration<TConfig> AddTextbox(string label, string description, Func<TConfig, string> get, Action<TConfig, string> set, bool enable = true) {
      this.AssertLoaded();

      if (enable)
        this.ModApi.RegisterSimpleOption(
          this.ConsumerManifest,
          label,
          description,
          () => get(this.GetConfig()),
          val => set(this.GetConfig(), val)
        );

      return this;
    }

    public GenericModConfigMenuIntegration<TConfig> AddNumberField(string label, string description, Func<TConfig, int> get, Action<TConfig, int> set, int min, int max, bool enable = true) {
      this.AssertLoaded();

      if (enable)
        this.ModApi.RegisterClampedOption(
          this.ConsumerManifest,
          label,
          description,
          () => get(this.GetConfig()),
          val => set(this.GetConfig(), val),
          min,
          max
        );

      return this;
    }

    public GenericModConfigMenuIntegration<TConfig> AddNumberField(string label, string description, Func<TConfig, float> get, Action<TConfig, float> set, float min, float max, bool enable = true) {
      this.AssertLoaded();

      if (enable)
        this.ModApi.RegisterClampedOption(
          this.ConsumerManifest,
          label,
          description,
          () => get(this.GetConfig()),
          val => set(this.GetConfig(), val),
          min,
          max
        );

      return this;
    }

    public GenericModConfigMenuIntegration<TConfig> AddKeyBinding(string label, string description, Func<TConfig, KeybindList> get, Action<TConfig, KeybindList> set, bool enable = true) {
      this.AssertLoaded();

      if (enable)
        this.ModApi.RegisterSimpleOption(
          this.ConsumerManifest,
          label,
          description,
          () => get(this.GetConfig()),
          val => set(this.GetConfig(), val)
        );

      return this;
    }
  }
}