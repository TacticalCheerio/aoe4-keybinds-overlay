using System.IO;
using AoE4KeybindsOverlay.Models;
using AoE4KeybindsOverlay.Services.Keybinding;

namespace AoE4KeybindsOverlay.Tests.Services;

public class RkpProfileReaderTests
{
    private const string MinimalProfile = @"profile = {
    bindingGroups = {
        camera = {
            {
                command = ""zoom_in"",
                keycombos = {
                    {
                        combo = ""MouseWheelUp"",
                        repeatCount = -1,
                    },
                    {
                        combo = """",
                        repeatCount = -1,
                    },
                },
            },
        },
    },
    name = ""TestProfile"",
    warnConflicts = true,
}";

    [Fact]
    public void ReadFromString_MinimalProfile_ReturnsCorrectName()
    {
        var profile = RkpProfileReader.ReadFromString(MinimalProfile);

        Assert.Equal("TestProfile", profile.Name);
    }

    [Fact]
    public void ReadFromString_MinimalProfile_HasOneBindingGroup()
    {
        var profile = RkpProfileReader.ReadFromString(MinimalProfile);

        Assert.Single(profile.BindingGroups);
        Assert.Equal("camera", profile.BindingGroups[0].Name);
    }

    [Fact]
    public void ReadFromString_MinimalProfile_FirstBindingHasCorrectCommandId()
    {
        var profile = RkpProfileReader.ReadFromString(MinimalProfile);

        var firstBinding = profile.BindingGroups[0].Bindings[0];
        Assert.Equal("zoom_in", firstBinding.CommandId);
    }

    [Fact]
    public void ReadFromString_MinimalProfile_PrimaryComboIsMouseWheelUp()
    {
        var profile = RkpProfileReader.ReadFromString(MinimalProfile);

        var firstBinding = profile.BindingGroups[0].Bindings[0];
        Assert.Equal("MouseWheelUp", firstBinding.Primary.PrimaryKey);
        Assert.Equal(ModifierKeys.None, firstBinding.Primary.Modifiers);
        Assert.False(firstBinding.Primary.IsEmpty);
    }

    [Fact]
    public void ReadFromString_MinimalProfile_AlternateComboIsEmpty()
    {
        var profile = RkpProfileReader.ReadFromString(MinimalProfile);

        var firstBinding = profile.BindingGroups[0].Bindings[0];
        Assert.True(firstBinding.Alternate.IsEmpty);
    }

    [Fact]
    public void ReadFromString_MinimalProfile_WarnConflictsIsTrue()
    {
        var profile = RkpProfileReader.ReadFromString(MinimalProfile);

        Assert.True(profile.WarnConflicts);
    }

    [Fact]
    public void ReadFromString_MinimalProfile_CategoryIsCamera()
    {
        var profile = RkpProfileReader.ReadFromString(MinimalProfile);

        Assert.Equal(BindingCategory.Camera, profile.BindingGroups[0].Category);
    }

    [Fact]
    public void ReadFromString_MultipleGroups_ReturnsAllGroups()
    {
        const string source = @"profile = {
    bindingGroups = {
        camera = {
            {
                command = ""zoom_in"",
                keycombos = {
                    { combo = ""MouseWheelUp"", repeatCount = -1, },
                    { combo = """", repeatCount = -1, },
                },
            },
        },
        hud_game = {
            {
                command = ""toggle_hud"",
                keycombos = {
                    { combo = ""Control+H"", repeatCount = -1, },
                    { combo = """", repeatCount = -1, },
                },
            },
        },
    },
    name = ""MultiGroup"",
    warnConflicts = true,
}";

        var profile = RkpProfileReader.ReadFromString(source);

        Assert.Equal(2, profile.BindingGroups.Count);
        Assert.Equal("camera", profile.BindingGroups[0].Name);
        Assert.Equal("hud_game", profile.BindingGroups[1].Name);
    }

    #region Real file tests

    private const string RealTestRkpPath =
        @"C:\Users\cod4m\Documents\my games\Age of Empires IV\keyBindingProfiles\test.rkp";

    [Fact]
    public void ReadFromFile_RealTestRkp_ParsesWithoutException()
    {
        if (!File.Exists(RealTestRkpPath))
        {
            // Skip if the file does not exist in this environment
            return;
        }

        var profile = RkpProfileReader.ReadFromFile(RealTestRkpPath);

        Assert.NotNull(profile);
    }

    [Fact]
    public void ReadFromFile_RealTestRkp_HasBindingGroups()
    {
        if (!File.Exists(RealTestRkpPath))
        {
            return;
        }

        var profile = RkpProfileReader.ReadFromFile(RealTestRkpPath);

        Assert.NotEmpty(profile.BindingGroups);
    }

    [Fact]
    public void ReadFromFile_RealTestRkp_NameMatchesTest()
    {
        if (!File.Exists(RealTestRkpPath))
        {
            return;
        }

        var profile = RkpProfileReader.ReadFromFile(RealTestRkpPath);

        Assert.Equal("test", profile.Name);
    }

    [Fact]
    public void ReadFromFile_RealTestRkp_ContainsCameraGroup()
    {
        if (!File.Exists(RealTestRkpPath))
        {
            return;
        }

        var profile = RkpProfileReader.ReadFromFile(RealTestRkpPath);

        Assert.Contains(profile.BindingGroups, g => g.Name == "camera");
    }

    [Fact]
    public void ReadFromFile_RealTestRkp_AllBindingsHaveCommandIds()
    {
        if (!File.Exists(RealTestRkpPath))
        {
            return;
        }

        var profile = RkpProfileReader.ReadFromFile(RealTestRkpPath);

        foreach (var binding in profile.AllBindings)
        {
            Assert.False(string.IsNullOrWhiteSpace(binding.CommandId),
                $"Binding in group '{binding.GroupName}' has empty CommandId");
        }
    }

    #endregion
}
