﻿using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;

namespace Common.Integrations.GenericModConfigMenu {

	public interface IGenericModConfigMenuApi {

		void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

		void RegisterLabel(IManifest mod, string labelName, string labelDesc);

		void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);

		void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<KeybindList> optionGet, Action<KeybindList> optionSet);

		void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);

		void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);

		void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);

		void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);

		void SetDefaultIngameOptinValue(IManifest mod, bool optedIn);
	}
}