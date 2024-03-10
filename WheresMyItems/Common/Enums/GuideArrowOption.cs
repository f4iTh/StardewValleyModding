namespace WheresMyItems.Common.Enums {
  /// <summary>
  /// The guide arrow option.
  /// </summary>
  public enum GuideArrowOption {
    /// <summary>
    /// Do not show any guide arrows.
    /// </summary>
    None,
    /// <summary>
    /// Show guide arrows only while the item search menu is open.
    /// </summary>
    WhileMenuOpen,
    /// <summary>
    /// Show guide arrows until (any) new menu gets opened.
    /// </summary>
    UntilNextMenu
  }
}