using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomWarps.Common.Models;
using ModCommon.Extensions;
using StardewModdingAPI;

namespace CustomWarps.Common {
  public class WarpHelper {
    private readonly IDataHelper _dataHelper;

    public Dictionary<Guid, CustomWarp> CustomWarps;

    public WarpHelper(IDataHelper dataHelper) {
      this._dataHelper = dataHelper;
      this.CustomWarps = this.LoadCustomWarps(true).Merge(this.LoadCustomWarps(false));
    }

    private void SaveCustomWarps(bool isGlobal) {
      this._dataHelper.WriteJsonFile(Path.Combine("data", isGlobal ? "global.json" : $"{Constants.SaveFolderName}.json"), this.CustomWarps.Where(pair => pair.Value.IsGlobal == isGlobal).ToDictionary(pair => pair.Key, pair => pair.Value));
      this.Update();
    }

    private Dictionary<Guid, CustomWarp> LoadCustomWarps(bool isGlobal) {
      return this._dataHelper.ReadJsonFile<Dictionary<Guid, CustomWarp>>(Path.Combine("data", isGlobal ? "global.json" : $"{Constants.SaveFolderName}.json")) ?? new Dictionary<Guid, CustomWarp>();
    }

    public bool TryAdd(Guid id, CustomWarp warp, bool isGlobal) {
      if (!this.CustomWarps.TryAdd(id, warp))
        return false;

      this.SaveCustomWarps(isGlobal);
      return true;
    }

    public bool TryRemove(Guid id, bool isGlobal) {
      if (!this.CustomWarps.TryRemove(id))
        return false;

      this.SaveCustomWarps(isGlobal);
      return true;
    }

    private void Update() {
      this.CustomWarps.Clear();
      this.CustomWarps = this.LoadCustomWarps(true).Merge(this.LoadCustomWarps(false));
    }
  }
}