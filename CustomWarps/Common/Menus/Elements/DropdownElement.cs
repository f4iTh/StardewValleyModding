using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace CustomWarps.Common.Menus.Elements {
  // https://github.com/Pathoschild/StardewMods/blob/develop/ChestsAnywhere/Menus/Components/SimpleDropdown.cs
  public class DropdownElement {
    private readonly OptionsDropDown _dropDown;
    private readonly IReflectedField<bool> _isExpandedField;
    private readonly List<Tuple<string, string>> _options;

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

    private Rectangle Bounds => this._dropDown.bounds;

    public string SelectedValue => this._options[this._dropDown.selectedOption].Item1;
    public bool IsExpanded => this._isExpandedField.GetValue();

    public bool TrySelect(string key) {
      int selectedIndex = this._options.FindIndex(option => option.Item1.Equals(key));
      if (selectedIndex == -1)
        return false;

      this._dropDown.selectedOption = selectedIndex;
      return true;
    }

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

    public bool TryHover(int x, int y) {
      if (!this.IsExpanded)
        return false;

      this._dropDown.leftClickHeld(x, y);

      this._dropDown.selectedOption = (int)Math.Max(Math.Min((y - this._dropDown.bounds.Y) / (float)this._dropDown.bounds.Height, this._dropDown.dropDownOptions.Count - 1), 0);
      return true;
    }

    public void Draw(SpriteBatch b, int x, int y) {
      if (x != this._dropDown.bounds.X || y != this._dropDown.bounds.Y) {
        this._dropDown.bounds = new Rectangle(x, y, this._dropDown.bounds.Width, this._dropDown.bounds.Height);
        this._dropDown.RecalculateBounds();
      }

      this._dropDown.draw(b, 0, 0);
    }
  }
}