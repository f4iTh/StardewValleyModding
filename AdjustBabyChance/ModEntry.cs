using AdjustBabyChance.Common.Configs;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace AdjustBabyChance {
  public class ModEntry : Mod {
    private static ModConfig _config;
    internal static IMonitor InternalMonitor;

    private Harmony _harmony;

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

    private void GetBabyChanceCommand(string command, string[] args) {
      this.Monitor.Log(I18n.Command_Getchance_Output(_config.QuestionChance), LogLevel.Info);
    }

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

    // ReSharper disable once UnusedMember.Local
    private static double GetQuestionChance() {
      return _config.QuestionChance;
    }
  }
}