using AdjustBabyChance.Common.Configs;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace AdjustBabyChance {
  /// <summary>
  /// The mod entry point.
  /// </summary>
  public class ModEntry : Mod {
    /// <summary>
    /// The mod configuration.
    /// </summary>
    private static ModConfig _config;
    /// <inheritdoc cref="IMonitor"/>
    internal static IMonitor InternalMonitor;
    /// <summary>
    /// The <see cref="Harmony"/> instance.
    /// </summary>
    private Harmony _harmony;

    /// <summary>
    /// The mod entry point method.
    /// </summary>
    /// <param name="helper">The mod helper.</param>
    public override void Entry(IModHelper helper) {
      I18n.Init(helper.Translation);

      InternalMonitor = this.Monitor;
      _config = helper.ReadConfig<ModConfig>();

      helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

      helper.ConsoleCommands.Add("setbabychance", I18n.Command_Getchance_Description(), this.SetBabyChanceCommand);
      helper.ConsoleCommands.Add("getbabychance", I18n.Command_Getchance_Description(), this.GetBabyChanceCommand);

      if (_config.QuestionChance is < 0f or > 1f) {
        switch (_config.QuestionChance) {
          case < 0f:
            this.Monitor.Log(I18n.Errors_Value_Under(_config.QuestionChance, "value"), LogLevel.Error);
            break;
          case > 1f:
            this.Monitor.Log(I18n.Errors_Value_Over(_config.QuestionChance, "value"), LogLevel.Error);
            break;
        }

        _config.QuestionChance = 0.05f;
        this.Helper.WriteConfig(_config);
      }

      this._harmony = new Harmony("com.f4iTh.AdjustBabyChance");
      this._harmony.PatchAll();
    }

    /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
      new GenericModConfig(
        this.Helper.ModRegistry,
        this.ModManifest,
        () => _config,
        () => {
          _config = new ModConfig();
          this.Helper.WriteConfig(_config);
        },
        () => this.Helper.WriteConfig(_config)
      ).Register();
    }

    /// <summary>
    /// Prints the current baby chance to the console.
    /// </summary>
    /// <param name="command">The command string.</param>
    /// <param name="args">The command arguments.</param>
    private void GetBabyChanceCommand(string command, string[] args) {
      this.Monitor.Log(I18n.Command_Getchance_Output(_config.QuestionChance), LogLevel.Info);
    }

    
    /// <summary>
    /// Sets the baby question chance.
    /// </summary>
    /// <param name="command">The command string.</param>
    /// <param name="args">The command arguments.</param>
    private void SetBabyChanceCommand(string command, string[] args) {
      if (!float.TryParse(args[0], out float newChance)) {
        this.Monitor.Log(I18n.Errors_Value_Invalid(newChance, "value"), LogLevel.Error);
        return;
      }

      switch (newChance) {
        case < 0f:
          this.Monitor.Log(I18n.Errors_Value_Under(newChance, "value"), LogLevel.Error);
          return;
        case > 1f:
          this.Monitor.Log(I18n.Errors_Value_Over(newChance, "value"), LogLevel.Error);
          return;
      }

      _config.QuestionChance = newChance;
      this.Helper.WriteConfig(_config);
      this.Monitor.Log(I18n.Command_Setchance_Output(newChance), LogLevel.Info);
    }

    /// <summary>
    /// A helper method used in the transpiler patch for getting the current question chance.
    /// </summary>
    /// <returns>The current baby question chance.</returns>
    // ReSharper disable once UnusedMember.Local
    private static double GetQuestionChance() {
      return _config.QuestionChance;
    }
  }
}