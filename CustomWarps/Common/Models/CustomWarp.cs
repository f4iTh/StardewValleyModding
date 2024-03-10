using System;

namespace CustomWarps.Common.Models {
  /// <summary>The custom warp model.</summary>
  public class CustomWarp {
    /// <summary>The constructor for a custom warp.</summary>
    /// <param name="warpName">The name of the warp.</param>
    /// <param name="mapName">the name of the map.</param>
    /// <param name="x">The x-coordinate of the tile.</param>
    /// <param name="y">The y-coordinate of the tile.</param>
    /// <param name="isGlobal">Whether the warp can be accessed from any save file.</param>
    /// <param name="isBuilding">Whether the warp is in a (farm) building.</param>
    /// <param name="dateAdded">When the warp was created.</param>
    /// <param name="warpUniqueId">The unique identifier of the warp.</param>
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

    /// <summary>The name of the warp.</summary>
    public string WarpName { get; set; }

    /// <summary>The name of the map.</summary>
    public string MapName { get; set; }

    /// <summary>The x-coordinate of the tile.</summary>
    public int TileX { get; set; }

    /// <summary>The y-coordinate of the tile.</summary>
    public int TileY { get; set; }

    /// <summary>Whether the warp can be accessed from any save file.</summary>
    public bool IsGlobal { get; set; }

    /// <summary>Whether the warp is in a (farm) building.</summary>
    public bool IsBuilding { get; set; }

    /// <summary>When the warp was created.</summary>
    public long DateAdded { get; set; }

    /// <summary>The unique identifier of the warp.</summary>
    public Guid WarpUniqueId { get; set; }
  }
}