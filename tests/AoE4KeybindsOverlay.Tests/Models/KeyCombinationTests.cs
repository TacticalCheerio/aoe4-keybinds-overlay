using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Tests.Models;

public class KeyCombinationTests
{
    #region Parse tests

    [Fact]
    public void Parse_EmptyString_ReturnsIsEmpty()
    {
        var combo = KeyCombination.Parse("");

        Assert.True(combo.IsEmpty);
        Assert.Equal(string.Empty, combo.PrimaryKey);
        Assert.Equal(ModifierKeys.None, combo.Modifiers);
    }

    [Fact]
    public void Parse_SingleKey_ReturnsPrimaryKeyWithNoModifiers()
    {
        var combo = KeyCombination.Parse("A");

        Assert.Equal("A", combo.PrimaryKey);
        Assert.Equal(ModifierKeys.None, combo.Modifiers);
        Assert.False(combo.IsEmpty);
    }

    [Fact]
    public void Parse_ControlPlusA_ReturnsCtrlModifier()
    {
        var combo = KeyCombination.Parse("Control+A");

        Assert.Equal("A", combo.PrimaryKey);
        Assert.Equal(ModifierKeys.Ctrl, combo.Modifiers);
    }

    [Fact]
    public void Parse_ControlShiftR_ReturnsCtrlAndShiftModifiers()
    {
        var combo = KeyCombination.Parse("Control+Shift+R");

        Assert.Equal("R", combo.PrimaryKey);
        Assert.Equal(ModifierKeys.Ctrl | ModifierKeys.Shift, combo.Modifiers);
    }

    [Fact]
    public void Parse_AltF3_ReturnsAltModifier()
    {
        var combo = KeyCombination.Parse("Alt+F3");

        Assert.Equal("F3", combo.PrimaryKey);
        Assert.Equal(ModifierKeys.Alt, combo.Modifiers);
    }

    [Fact]
    public void Parse_MouseWheelUp_ReturnsMouseKeyWithNoModifiers()
    {
        var combo = KeyCombination.Parse("MouseWheelUp");

        Assert.Equal("MouseWheelUp", combo.PrimaryKey);
        Assert.Equal(ModifierKeys.None, combo.Modifiers);
        Assert.False(combo.IsEmpty);
    }

    [Fact]
    public void Parse_ControlAlt3_ReturnsCtrlAndAltModifiers()
    {
        var combo = KeyCombination.Parse("Control+Alt+3");

        Assert.Equal("3", combo.PrimaryKey);
        Assert.Equal(ModifierKeys.Ctrl | ModifierKeys.Alt, combo.Modifiers);
    }

    #endregion

    #region Matches tests

    [Fact]
    public void Matches_ExactMatch_ReturnsTrue()
    {
        var combo = KeyCombination.Parse("Control+A");

        Assert.True(combo.Matches("A", ModifierKeys.Ctrl));
    }

    [Fact]
    public void Matches_WrongKey_ReturnsFalse()
    {
        var combo = KeyCombination.Parse("Control+A");

        Assert.False(combo.Matches("B", ModifierKeys.Ctrl));
    }

    [Fact]
    public void Matches_WrongModifiers_ReturnsFalse()
    {
        var combo = KeyCombination.Parse("Control+A");

        Assert.False(combo.Matches("A", ModifierKeys.Shift));
    }

    [Fact]
    public void Matches_EmptyCombo_ReturnsFalse()
    {
        var combo = KeyCombination.Parse("");

        Assert.False(combo.Matches("A", ModifierKeys.None));
    }

    #endregion

    #region IsPartialMatch tests

    [Fact]
    public void IsPartialMatch_MatchingModifiers_ReturnsTrue()
    {
        var combo = KeyCombination.Parse("Control+A");

        Assert.True(combo.IsPartialMatch(ModifierKeys.Ctrl));
    }

    [Fact]
    public void IsPartialMatch_NoModifiers_ReturnsFalseForComboRequiringModifiers()
    {
        var combo = KeyCombination.Parse("Control+A");

        Assert.False(combo.IsPartialMatch(ModifierKeys.None));
    }

    [Fact]
    public void IsPartialMatch_EmptyCombo_ReturnsFalse()
    {
        var combo = KeyCombination.Parse("");

        Assert.False(combo.IsPartialMatch(ModifierKeys.None));
    }

    #endregion

    #region DisplayString tests

    [Fact]
    public void DisplayString_ControlA_ReturnsFormattedString()
    {
        var combo = KeyCombination.Parse("Control+A");

        Assert.Equal("Ctrl + A", combo.DisplayString);
    }

    [Fact]
    public void DisplayString_ShiftF1_ReturnsFormattedString()
    {
        var combo = KeyCombination.Parse("Shift+F1");

        Assert.Equal("Shift + F1", combo.DisplayString);
    }

    [Fact]
    public void DisplayString_Empty_ReturnsEmptyString()
    {
        var combo = KeyCombination.Parse("");

        Assert.Equal(string.Empty, combo.DisplayString);
    }

    [Fact]
    public void DisplayString_MouseWheelUp_ReturnsFormattedName()
    {
        var combo = KeyCombination.Parse("MouseWheelUp");

        Assert.Equal("Mouse Wheel Up", combo.DisplayString);
    }

    #endregion
}
