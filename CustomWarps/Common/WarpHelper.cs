using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomWarps.Common.Models;
using ModCommon.Extensions;
using StardewModdingAPI;

namespace CustomWarps.Common {
  /// <summary>A helper class for handling custom warp data.</summary>
  public class WarpHelper {
    /// <inheritdoc cref="IDataHelper" />
    private readonly IDataHelper _dataHelper;

    /// <summary>A dictionary containing all custom warps.</summary>
    public Dictionary<Guid, CustomWarp> CustomWarps;

    /// <summary>The constructor for the warp helper class.</summary>
    /// <param name="dataHelper">API for reading and storing data.</param>
    public WarpHelper(IDataHelper dataHelper) {
      this._dataHelper = dataHelper;
      this.CustomWarps = this.LoadCustomWarps(true).Merge(this.LoadCustomWarps(false));
    }

    /// <summary>Saves custom warps.</summary>
    /// <param name="isGlobal">Whether the warp can be accessed from any save file.</param>
    private void SaveCustomWarps(bool isGlobal) {
      this._dataHelper.WriteJsonFile(Path.Combine("data", isGlobal ? "global.json" : $"{Constants.SaveFolderName}.json"), this.CustomWarps.Where(pair => pair.Value.IsGlobal == isGlobal).ToDictionary(pair => pair.Key, pair => pair.Value));
      this.Update();
    }

    /// <summary>Loads custom warps.</summary>
    /// <param name="isGlobal">Whether the warp can be accessed from any save file.</param>
    private Dictionary<Guid, CustomWarp> LoadCustomWarps(bool isGlobal) {
      return this._dataHelper.ReadJsonFile<Dictionary<Guid, CustomWarp>>(Path.Combine("data", isGlobal ? "global.json" : $"{Constants.SaveFolderName}.json")) ?? new Dictionary<Guid, CustomWarp>();
    }

    /// <summary>Tries to add a custom warp to the dictionary.</summary>
    /// <param name="id">The unique identifier of the warp.</param>
    /// <param name="warp">The custom warp data.</param>
    /// <param name="isGlobal">Whether the warp can be accessed from any save file.</param>
    /// <returns>Whether adding the warp was successful.</returns>
    public bool TryAdd(Guid id, CustomWarp warp, bool isGlobal) {
      if (!this.CustomWarps.TryAdd(id, warp))
        return false;

      this.SaveCustomWarps(isGlobal);
      return true;
    }

    /// <summary>Tries to remove a custom warp from the dictionary.</summary>
    /// <param name="id">The unique identifier of the warp.</param>
    /// <param name="isGlobal">Whether the warp can be accessed from any save file.</param>
    /// <returns>Whether removing the warp was successful.</returns>
    public bool TryRemove(Guid id, bool isGlobal) {
      if (!this.CustomWarps.Remove(id))
        return false;

      this.SaveCustomWarps(isGlobal);
      return true;
    }

    /// <summary>Updates the custom warp dictionary.</summary>
    private void Update() {
      this.CustomWarps.Clear();
      this.CustomWarps = this.LoadCustomWarps(true).Merge(this.LoadCustomWarps(false));
    }
  }
}