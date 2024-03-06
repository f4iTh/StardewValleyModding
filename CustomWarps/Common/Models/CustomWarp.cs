using System;

namespace CustomWarps.Common.Models {
  public class CustomWarp {
    public CustomWarp(string warpName, string mapName, int x, int y, bool isGlobal, bool isBuilding, long dateAdded, Guid warpUniqueId) {
      this.WarpName = warpName;
      this.MapName = mapName;
      this.TileX = x;
      this.TileY = y;
      this.IsGlobal = isGlobal;
      this.IsBuilding = isBuilding;
      this.DateAdded = dateAdded;
      this.WarpUniqueId = warpUniqueId;
    }

    public string WarpName { get; set; }
    public string MapName { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
    public bool IsGlobal { get; set; }
    public bool IsBuilding { get; set; }
    public long DateAdded { get; set; }
    public Guid WarpUniqueId { get; set; }
  }
}