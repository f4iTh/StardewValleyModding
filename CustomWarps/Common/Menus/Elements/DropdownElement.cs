using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace CustomWarps.Common.Menus.Elements {
  // https://github.com/Pathoschild/StardewMods/blob/develop/ChestsAnywhere/Menus/Components/SimpleDropdown.cs
  /// <summary>
  /// A dropdown menu element.
  /// </summary>
  public class DropdownElement {
    /// <summary>
    /// The underlying dropdown element.
    /// </summary>
    private readonly OptionsDropDown _dropDown;
    /// <summary>
    /// Whether the dropdown element is expanded.
    /// </summary>
    private readonly IReflectedField<bool> _isExpandedField;
    /// <summary>
    /// The dropdown menu options.
    /// </summary>
    private readonly List<Tuple<string, string>> _options;

    /// <summary>
    /// The constructor for creating a dropdown.
    /// </summary>
    /// <param name="reflectionHelper">An API for accessing inaccessible code</param>
    /// <param name="options">The dropdown options.</param>
    /// <param name="selected">Which option to select.</param>
    public DropdownElement(IReflectionHelper reflectionHelper, List<Tuple<string, string>> options, string selected = default) {
      List<string> optionKeys = new();
      List<string> optionLabels = new();

      foreach (Tuple<string, string> option in options) {
        optionKeys.Add(option.Item1);
        optionLabels.Add(option.Item2);
      }

      this._options = options;
      this._dropDown = new OptionsDropDown(null, int.MinValue) {
        dropDownOptions = optionKeys,
        dropDownDisplayOptions = optionLabels
      };
      this._isExpandedField = reflectionHelper.GetField<bool>(this._dropDown, Constants.TargetPlatform == GamePlatform.Android ? "dropDownOpen" : "clicked");

      this.TrySelect(selected);
    }

    /// <summary>
    /// The bounds of the dropdown element.
    /// </summary>
    private Rectangle Bounds => this._dropDown.bounds;
    /// <summary>
    /// The currently selected value.
    /// </summary>
    public string SelectedValue => this._options[this._dropDown.selectedOption].Item1;
    /// <summary>
    /// Whether the dropdown element is expanded.
    /// </summary>
    public bool IsExpanded => this._isExpandedField.GetValue();

    /// <summary>
    /// Handle selecting an option.
    /// </summary>
    /// <param name="key">The option string.</param>
    public bool TrySelect(string key) {
      int selectedIndex = this._options.FindIndex(option => option.Item1.Equals(key));
      if (selectedIndex == -1)
        return false;

      this._dropDown.selectedOption = selectedIndex;
      return true;
    }

    /// <summary>
    /// Handle clicking the dropdown menu.
    /// </summary>
    /// <param name="x">The cursor x-coordinate.</param>
    /// <param name="y">The cursor y-coordinate.</param>
    public bool TryClick(int x, int y) {
      if (!this.IsExpanded) {
        if (!this.Bounds.Contains(x, y))
          return false;

        this._dropDown.receiveLeftClick(x, y);
        return true;
      }

      this._dropDown.leftClickReleased(x, y);
      return true;
    }

    /// <summary>
    /// Handle hovering over the dropdown menu.
    /// </summary>
    /// <param name="x">The cursor x-coordinate.</param>
    /// <param name="y">The cursor y-coordinate.</param>
    public bool TryHover(int x, int y) {
      if (!this.IsExpanded)
        return false;

      this._dropDown.leftClickHeld(x, y);

      this._dropDown.selectedOption = (int)Math.Max(Math.Min((y - this._dropDown.bounds.Y) / (float)this._dropDown.bounds.Height, this._dropDown.dropDownOptions.Count - 1), 0);
      return true;
    }

    /// <summary>
    /// Draw the dropdown element.
    /// </summary>
    /// <param name="b">The SpriteBatch.</param>
    /// <param name="x">The cursor x-coordinate.</param>
    /// <param name="y">The cursor y-coordinate.</param>
    public void Draw(SpriteBatch b, int x, int y) {
      if (x != this._dropDown.bounds.X || y != this._dropDown.bounds.Y) {
        this._dropDown.bounds = new Rectangle(x, y, this._dropDown.bounds.Width, this._dropDown.bounds.Height);
        this._dropDown.RecalculateBounds();
      }

      this._dropDown.draw(b, 0, 0);
    }
  }
}