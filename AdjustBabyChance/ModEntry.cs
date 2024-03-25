using System;
using System.Reflection;
using AdjustBabyChance.Common.Configs;
using AdjustBabyChance.Common.IL;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace AdjustBabyChance;

/// <summary>The mod entry point.</summary>
public class ModEntry : Mod {
  /// <summary>The mod configuration.</summary>
  private static ModConfig _config;

  /// <inheritdoc cref="IMonitor" />
  internal static IMonitor InternalMonitor;

  /// <summary>The <see cref="Harmony" /> instance.</summary>
  private readonly Harmony _harmony = new("com.f4iTh.AdjustBabyChance");

  /// <summary>The mod entry point method.</summary>
  /// <param name="helper">The mod helper.</param>
  public override void Entry(IModHelper helper) {
    I18n.Init(helper.Translation);

    InternalMonitor = this.Monitor;
    _config = helper.ReadConfig<ModConfig>();

    helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

    helper.ConsoleCommands.Add("setbabychance", I18n.Command_Getchance_Description(), this.SetBabyChanceCommand);
    helper.ConsoleCommands.Add("getbabychance", I18n.Command_Getchance_Description(), this.GetBabyChanceCommand);

    if (_config.QuestionChance is >= 0f and <= 1f)
      return;

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

  /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
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

    this._harmony.PatchAll();

    if (!this.Helper.ModRegistry.IsLoaded("aedenthorn.FreeLove"))
      return;

    this.PatchFreeLoveQuestionChance();
  }

  /// <summary>Prints the current baby chance to the console.</summary>
  /// <param name="command">The command string.</param>
  /// <param name="args">The command arguments.</param>
  private void GetBabyChanceCommand(string command, string[] args) {
    this.Monitor.Log(I18n.Command_Getchance_Output(_config.QuestionChance), LogLevel.Info);
  }

  /// <summary>Sets the baby question chance.</summary>
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

  /// <summary>Attempts to patch the <c>Utility_pickPersonalFarmEvent_Prefix</c> prefix method from Free Love.</summary>
  private void PatchFreeLoveQuestionChance() {
    Type freeLoveModEntryType = AccessTools.TypeByName("FreeLove.ModEntry, FreeLove");
    if (freeLoveModEntryType == null) {
      this.Monitor.Log("Could not find FreeLove.ModEntry. Adjusted question chance might not work correctly.");
      return;
    }

    try {
      MethodInfo pickPersonalFarmEventPrefixMethod = AccessTools.Method(freeLoveModEntryType, "Utility_pickPersonalFarmEvent_Prefix");
      this._harmony.Patch(pickPersonalFarmEventPrefixMethod, transpiler: new HarmonyMethod(typeof(EventPatch).GetMethod("Transpiler", BindingFlags.Static | BindingFlags.NonPublic)));
      this.Monitor.Log($"Patched {pickPersonalFarmEventPrefixMethod}");
    }
    catch (Exception ex) {
      this.Monitor.Log($"Could not patch FreeLove.ModEntry::Utility_pickPersonalFarmEvent_Prefix.\n{ex}", LogLevel.Error);
    }
  }

  /// <summary>A helper method used in the transpiler patch for getting the current question chance.</summary>
  /// <returns>The current baby question chance.</returns>
  // ReSharper disable once UnusedMember.Local
  private static double GetQuestionChance() {
    return _config.QuestionChance;
  }
}