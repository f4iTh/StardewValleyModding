namespace ActivateSprinklers.Common.Enums {
  /// <summary>Which side tiles to search for sprinklers.</summary>
  public enum AdjacentTileDirection {
    /// <summary>Do not search side tiles.</summary>
    None,

    /// <summary>Search left tile only.</summary>
    Left,

    /// <summary>Search right tile only.</summary>
    Right,

    /// <summary>Search both left and right tiles.</summary>
    LeftRight
  }
}