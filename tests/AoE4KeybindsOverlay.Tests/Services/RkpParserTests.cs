using AoE4KeybindsOverlay.Services.Keybinding;

namespace AoE4KeybindsOverlay.Tests.Services;

public class RkpParserTests
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
    name = ""test"",
    warnConflicts = true,
}";

    [Fact]
    public void Parse_MinimalProfile_ReturnsRootTableWithExpectedEntries()
    {
        var rootTable = RkpParser.Parse(MinimalProfile);

        // Root table should have named entries for bindingGroups, name, warnConflicts
        Assert.NotNull(rootTable.Get("bindingGroups"));
        Assert.NotNull(rootTable.Get("name"));
        Assert.NotNull(rootTable.Get("warnConflicts"));
    }

    [Fact]
    public void Parse_MinimalProfile_NameIsTest()
    {
        var rootTable = RkpParser.Parse(MinimalProfile);

        var name = rootTable.GetString("name");
        Assert.Equal("test", name);
    }

    [Fact]
    public void Parse_MinimalProfile_WarnConflictsIsTrue()
    {
        var rootTable = RkpParser.Parse(MinimalProfile);

        var warnConflicts = rootTable.GetBool("warnConflicts");
        Assert.True(warnConflicts);
    }

    [Fact]
    public void Parse_MinimalProfile_CameraGroupHasOneCommand()
    {
        var rootTable = RkpParser.Parse(MinimalProfile);

        var bindingGroups = rootTable.GetTable("bindingGroups");
        Assert.NotNull(bindingGroups);

        var cameraGroup = bindingGroups!.GetTable("camera");
        Assert.NotNull(cameraGroup);

        // camera group should have 1 anonymous table (the command entry)
        var commandTables = cameraGroup!.AnonymousTables().ToList();
        Assert.Single(commandTables);
    }

    [Fact]
    public void Parse_MinimalProfile_CommandIsZoomIn()
    {
        var rootTable = RkpParser.Parse(MinimalProfile);

        var bindingGroups = rootTable.GetTable("bindingGroups")!;
        var cameraGroup = bindingGroups.GetTable("camera")!;
        var commandTable = cameraGroup.AnonymousTables().First();

        var commandId = commandTable.GetString("command");
        Assert.Equal("zoom_in", commandId);
    }

    [Fact]
    public void Parse_MinimalProfile_KeycombosHasTwoEntries()
    {
        var rootTable = RkpParser.Parse(MinimalProfile);

        var bindingGroups = rootTable.GetTable("bindingGroups")!;
        var cameraGroup = bindingGroups.GetTable("camera")!;
        var commandTable = cameraGroup.AnonymousTables().First();
        var keycombos = commandTable.GetTable("keycombos")!;

        var comboEntries = keycombos.AnonymousTables().ToList();
        Assert.Equal(2, comboEntries.Count);
    }

    [Fact]
    public void Parse_MinimalProfile_FirstComboIsMouseWheelUp()
    {
        var rootTable = RkpParser.Parse(MinimalProfile);

        var bindingGroups = rootTable.GetTable("bindingGroups")!;
        var cameraGroup = bindingGroups.GetTable("camera")!;
        var commandTable = cameraGroup.AnonymousTables().First();
        var keycombos = commandTable.GetTable("keycombos")!;
        var firstCombo = keycombos.AnonymousTables().First();

        Assert.Equal("MouseWheelUp", firstCombo.GetString("combo"));
        Assert.Equal(-1, firstCombo.GetInt("repeatCount"));
    }

    [Fact]
    public void Parse_MinimalProfile_SecondComboIsEmpty()
    {
        var rootTable = RkpParser.Parse(MinimalProfile);

        var bindingGroups = rootTable.GetTable("bindingGroups")!;
        var cameraGroup = bindingGroups.GetTable("camera")!;
        var commandTable = cameraGroup.AnonymousTables().First();
        var keycombos = commandTable.GetTable("keycombos")!;
        var secondCombo = keycombos.AnonymousTables().Skip(1).First();

        Assert.Equal("", secondCombo.GetString("combo"));
    }

    [Fact]
    public void Parse_SimpleAssignment_ReturnsStringValue()
    {
        var rootTable = RkpParser.Parse("data = { name = \"hello\" }");

        Assert.Equal("hello", rootTable.GetString("name"));
    }

    [Fact]
    public void Parse_IntegerValue_ReturnsCorrectInteger()
    {
        var rootTable = RkpParser.Parse("data = { count = 42 }");

        Assert.Equal(42, rootTable.GetInt("count"));
    }

    [Fact]
    public void Parse_BooleanFalse_ReturnsCorrectBoolean()
    {
        var rootTable = RkpParser.Parse("data = { enabled = false }");

        Assert.False(rootTable.GetBool("enabled"));
    }
}
