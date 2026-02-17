using AoE4KeybindsOverlay.Models;
using AoE4KeybindsOverlay.Services.Keybinding;

namespace AoE4KeybindsOverlay.Tests.Services;

public class KeybindingMatcherTests
{
    private static KeybindingMatcher BuildMatcher(params Keybinding[] bindings)
    {
        var matcher = new KeybindingMatcher();
        matcher.Build(bindings);
        return matcher;
    }

    private static Keybinding CreateBinding(
        string commandId,
        string primaryCombo,
        string alternateCombo = "",
        string groupName = "test_group",
        BindingCategory category = BindingCategory.General)
    {
        return new Keybinding
        {
            CommandId = commandId,
            GroupName = groupName,
            Category = category,
            Primary = KeyCombination.Parse(primaryCombo),
            Alternate = KeyCombination.Parse(alternateCombo),
        };
    }

    #region FindExactMatch tests

    [Fact]
    public void FindExactMatch_ControlA_ReturnsCorrectBinding()
    {
        var binding = CreateBinding("select_all", "Control+A");
        var matcher = BuildMatcher(binding);

        var result = matcher.FindExactMatch("A", ModifierKeys.Ctrl);

        Assert.NotNull(result);
        Assert.Equal("select_all", result!.CommandId);
    }

    [Fact]
    public void FindExactMatch_UnboundCombo_ReturnsNull()
    {
        var binding = CreateBinding("select_all", "Control+A");
        var matcher = BuildMatcher(binding);

        var result = matcher.FindExactMatch("B", ModifierKeys.Ctrl);

        Assert.Null(result);
    }

    [Fact]
    public void FindExactMatch_MatchesAlternateCombo()
    {
        var binding = CreateBinding("pan_up", "Up", "Alt+W");
        var matcher = BuildMatcher(binding);

        var result = matcher.FindExactMatch("W", ModifierKeys.Alt);

        Assert.NotNull(result);
        Assert.Equal("pan_up", result!.CommandId);
    }

    [Fact]
    public void FindExactMatch_EmptyKey_ReturnsNull()
    {
        var binding = CreateBinding("select_all", "Control+A");
        var matcher = BuildMatcher(binding);

        var result = matcher.FindExactMatch("", ModifierKeys.None);

        Assert.Null(result);
    }

    [Fact]
    public void FindExactMatch_WrongModifiers_ReturnsNull()
    {
        var binding = CreateBinding("select_all", "Control+A");
        var matcher = BuildMatcher(binding);

        var result = matcher.FindExactMatch("A", ModifierKeys.Shift);

        Assert.Null(result);
    }

    #endregion

    #region GetPossibleCompletions tests

    [Fact]
    public void GetPossibleCompletions_CtrlHeld_ReturnsAllCtrlBindings()
    {
        var selectAll = CreateBinding("select_all", "Control+A");
        var copy = CreateBinding("copy", "Control+C");
        var plainKey = CreateBinding("attack_move", "A");
        var matcher = BuildMatcher(selectAll, copy, plainKey);

        var completions = matcher.GetPossibleCompletions(ModifierKeys.Ctrl);

        Assert.Equal(2, completions.Count);
        Assert.Contains(completions, b => b.CommandId == "select_all");
        Assert.Contains(completions, b => b.CommandId == "copy");
    }

    [Fact]
    public void GetPossibleCompletions_NoModifiers_ReturnsNoModifierBindings()
    {
        var selectAll = CreateBinding("select_all", "Control+A");
        var attackMove = CreateBinding("attack_move", "A");
        var matcher = BuildMatcher(selectAll, attackMove);

        var completions = matcher.GetPossibleCompletions(ModifierKeys.None);

        Assert.Single(completions);
        Assert.Equal("attack_move", completions[0].CommandId);
    }

    [Fact]
    public void GetPossibleCompletions_UnusedModifiers_ReturnsEmpty()
    {
        var selectAll = CreateBinding("select_all", "Control+A");
        var matcher = BuildMatcher(selectAll);

        var completions = matcher.GetPossibleCompletions(ModifierKeys.Alt);

        Assert.Empty(completions);
    }

    #endregion

    #region GetBindingsForKey tests

    [Fact]
    public void GetBindingsForKey_ReturnsAllBindingsUsingKey()
    {
        var selectAll = CreateBinding("select_all", "Control+A");
        var attackMove = CreateBinding("attack_move", "A");
        var copy = CreateBinding("copy", "Control+C");
        var matcher = BuildMatcher(selectAll, attackMove, copy);

        var aBindings = matcher.GetBindingsForKey("A");

        Assert.Equal(2, aBindings.Count);
        Assert.Contains(aBindings, b => b.CommandId == "select_all");
        Assert.Contains(aBindings, b => b.CommandId == "attack_move");
    }

    [Fact]
    public void GetBindingsForKey_CaseInsensitive()
    {
        var binding = CreateBinding("select_all", "Control+A");
        var matcher = BuildMatcher(binding);

        var result = matcher.GetBindingsForKey("a");

        Assert.Single(result);
        Assert.Equal("select_all", result[0].CommandId);
    }

    [Fact]
    public void GetBindingsForKey_UnknownKey_ReturnsEmpty()
    {
        var binding = CreateBinding("select_all", "Control+A");
        var matcher = BuildMatcher(binding);

        var result = matcher.GetBindingsForKey("Z");

        Assert.Empty(result);
    }

    [Fact]
    public void GetBindingsForKey_EmptyKey_ReturnsEmpty()
    {
        var binding = CreateBinding("select_all", "Control+A");
        var matcher = BuildMatcher(binding);

        var result = matcher.GetBindingsForKey("");

        Assert.Empty(result);
    }

    [Fact]
    public void GetBindingsForKey_IncludesAlternateKeyBindings()
    {
        var binding = CreateBinding("pan_up", "Up", "Alt+W");
        var matcher = BuildMatcher(binding);

        var wBindings = matcher.GetBindingsForKey("W");

        Assert.Single(wBindings);
        Assert.Equal("pan_up", wBindings[0].CommandId);
    }

    #endregion

    #region GetAll and GetBindingsForCategory tests

    [Fact]
    public void GetAll_ReturnsAllBindings()
    {
        var b1 = CreateBinding("a", "A");
        var b2 = CreateBinding("b", "B");
        var b3 = CreateBinding("c", "C");
        var matcher = BuildMatcher(b1, b2, b3);

        Assert.Equal(3, matcher.GetAll().Count);
    }

    [Fact]
    public void GetBindingsForCategory_ReturnsOnlyMatchingCategory()
    {
        var camera = CreateBinding("zoom_in", "MouseWheelUp", category: BindingCategory.Camera);
        var general = CreateBinding("toggle_hud", "H", category: BindingCategory.General);
        var matcher = BuildMatcher(camera, general);

        var cameraBindings = matcher.GetBindingsForCategory(BindingCategory.Camera);

        Assert.Single(cameraBindings);
        Assert.Equal("zoom_in", cameraBindings[0].CommandId);
    }

    #endregion
}
