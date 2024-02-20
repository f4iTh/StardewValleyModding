using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace AdjustBabyChance.Common.IL {
  [HarmonyPatch(typeof(Utility), "pickPersonalFarmEvent")]
  public static class EventPatch {
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      IEnumerable<CodeInstruction> codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
      List<CodeInstruction> origCodes = new(codeInstructions);
      List<CodeInstruction> codes = new(codeInstructions);
      
      List<int> indexes = new();
      try {
        // match all of the following:
        //    ldc.r8    0.05
        for (int i = 0; i < codes.Count; i++) {
          CodeInstruction instruction = codes[i];

          if (instruction.opcode != OpCodes.Ldc_R8 || Math.Abs((double)instruction.operand! - 0.05) > 0.001)
            continue;

          indexes.Add(i);
        }

        int indexAdjustment = 0;
        foreach (int index in indexes.Where(index => index != -1)) {
          codes.RemoveAt(index + indexAdjustment);
          codes.InsertRange(index + indexAdjustment, new[] {
            new CodeInstruction(OpCodes.Call, typeof(ModEntry).GetMethod("GetQuestionChance", BindingFlags.Static | BindingFlags.NonPublic)),
            new CodeInstruction(OpCodes.Conv_R8)
          });

          // add one to the old index
          indexAdjustment++;
        }

        return codes.AsEnumerable();
      }
      catch (Exception e) {
        ModEntry.InternalMonitor?.Log(e.ToString(), LogLevel.Error);
        // return original instructions on error as a fail-safe
        return origCodes;
      }
    }
  }
}
