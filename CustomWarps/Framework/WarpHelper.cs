using System;
using System.Collections.Generic;
using System.Linq;
using CustomWarps.Framework.Models;
using StardewModdingAPI;

namespace CustomWarps.Framework {
  public class WarpHelper {
    private readonly IModHelper _helper;
    private readonly IMonitor _monitor;
    private Dictionary<string, CustomWarp> _globalWarps;
    private Dictionary<string, CustomWarp> _localWarps;
    public SortedDictionary<string, CustomWarp> CustomWarps;

    public WarpHelper(IModHelper helper, IMonitor monitor) {
      this._helper = helper;
      this._monitor = monitor;
      this._localWarps = this.LoadLocalCustomWarps();
      this._globalWarps = this.LoadGlobalCustomWarps();
      Dictionary<string, CustomWarp> dictionary = new();
      foreach (KeyValuePair<string, CustomWarp> gWarp in this._globalWarps.Where(gWarp => !dictionary.ContainsKey(gWarp.Key)))
        dictionary.Add(gWarp.Key, gWarp.Value);
      foreach (KeyValuePair<string, CustomWarp> lWarp in this._localWarps.Where(lWarp => !dictionary.ContainsKey(lWarp.Key)))
        dictionary.Add(lWarp.Key, lWarp.Value);
      this.CustomWarps = new SortedDictionary<string, CustomWarp>(dictionary);
    }

    public bool HasKey(string key) {
      return this.CustomWarps.ContainsKey(key);
    }

    public CustomWarp GetWarp(string key) {
      return !this.CustomWarps.ContainsKey(key) ? null : this.CustomWarps[key];
    }

    public void Add(string name, CustomWarp warp, bool isGlobal) {
      if (isGlobal) {
        if (!this._globalWarps.ContainsKey(name))
          this._globalWarps.Add(name, warp);
      }
      else {
        if (!this._localWarps.ContainsKey(name))
          this._localWarps.Add(name, warp);
      }

      this.SaveWarps(isGlobal);
    }

    public bool TryAdd(string name, CustomWarp warp, bool isGlobal) {
      if (isGlobal) {
        if (this._globalWarps.ContainsKey(name)) return false;
        this._globalWarps.Add(name, warp);
        this.SaveWarps(true);
      }
      else {
        if (this._localWarps.ContainsKey(name)) return false;
        this._localWarps.Add(name, warp);
        this.SaveWarps(false);
      }

      this.SaveWarps(isGlobal);
      return true;
    }

    public void Remove(string name, bool isGlobal) {
      if (!this.CustomWarps.ContainsKey(name)) return;
      if (isGlobal)
        this._globalWarps.Remove(name);
      else
        this._localWarps.Remove(name);
      this.CustomWarps.Remove(name);
      this.SaveWarps(isGlobal);
    }

    private void SaveWarps(bool isGlobal) {
      this._helper.Data.WriteJsonFile($"{(isGlobal ? "warps/global.json" : "warps/{Constants.SaveFolderName}.json")}",
        isGlobal ? this._globalWarps : this._localWarps);
      // if (isGlobal) this._helper.Data.WriteJsonFile("warps/global.json", this._globalWarps);
      // else this._helper.Data.WriteJsonFile($"warps/{Constants.SaveFolderName}.json", this._localWarps);
      this.Update();
    }

    private Dictionary<string, CustomWarp> LoadLocalCustomWarps() {
      return this._helper.Data.ReadJsonFile<Dictionary<string, CustomWarp>>($"warps/{Constants.SaveFolderName}.json") ?? new Dictionary<string, CustomWarp>();
    }

    private Dictionary<string, CustomWarp> LoadGlobalCustomWarps() {
      return this._helper.Data.ReadJsonFile<Dictionary<string, CustomWarp>>("warps/global.json") ?? new Dictionary<string, CustomWarp>();
    }

    private void Update() {
      this._localWarps.Clear();
      this._globalWarps.Clear();
      this.CustomWarps.Clear();

      this._localWarps = this.LoadLocalCustomWarps();
      this._globalWarps = this.LoadGlobalCustomWarps();

      try {
        Dictionary<string, CustomWarp> dictionary = new();
        foreach (KeyValuePair<string, CustomWarp> gWarp in this._globalWarps.Where(gWarp => !dictionary.ContainsKey(gWarp.Key)))
          dictionary.Add(gWarp.Key, gWarp.Value);
        foreach (KeyValuePair<string, CustomWarp> lWarp in this._localWarps.Where(lWarp => !dictionary.ContainsKey(lWarp.Key)))
          dictionary.Add(lWarp.Key, lWarp.Value);
        this.CustomWarps = new SortedDictionary<string, CustomWarp>(dictionary);
      }
      catch (ArgumentException ae) {
        this._monitor.Log($"Something went wrong!\n{ae}", LogLevel.Error);
      }
    }
  }
}