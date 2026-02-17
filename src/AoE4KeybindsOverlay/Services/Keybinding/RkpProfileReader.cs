using System.IO;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Services.Keybinding;

/// <summary>
/// Maps the raw parsed .rkp AST (<see cref="RkpTable"/>) into the domain model (<see cref="BindingProfile"/>).
/// </summary>
/// <remarks>
/// <para>
/// The .rkp file structure is:
/// <code>
/// profile = {
///     bindingGroups = {
///         camera = { { command = "...", keycombos = { ... } }, ... },
///         hud_game = { ... },
///         archery_range = { ... },
///         ...
///     },
///     name = "ProfileName",
///     warnConflicts = true,
///     warnUnremapped = false,
/// }
/// </code>
/// </para>
/// <para>
/// Binding group names can appear multiple times (duplicates). This reader preserves all
/// occurrences as separate <see cref="BindingGroup"/> entries in the output list.
/// </para>
/// </remarks>
public static class RkpProfileReader
{
    /// <summary>
    /// Reads an .rkp file from disk and produces a <see cref="BindingProfile"/>.
    /// </summary>
    /// <param name="filePath">The path to the .rkp file.</param>
    /// <returns>A fully populated <see cref="BindingProfile"/>.</returns>
    /// <exception cref="RkpParseException">The file cannot be parsed.</exception>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    public static BindingProfile ReadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Keybinding profile not found: {filePath}", filePath);

        var source = File.ReadAllText(filePath);
        var rootTable = RkpParser.Parse(source);
        return MapProfile(rootTable, filePath);
    }

    /// <summary>
    /// Reads an .rkp file from a string and produces a <see cref="BindingProfile"/>.
    /// </summary>
    /// <param name="source">The full text content of an .rkp file.</param>
    /// <param name="filePath">The file path (used for metadata only).</param>
    /// <returns>A fully populated <see cref="BindingProfile"/>.</returns>
    public static BindingProfile ReadFromString(string source, string filePath = "<memory>")
    {
        var rootTable = RkpParser.Parse(source);
        return MapProfile(rootTable, filePath);
    }

    /// <summary>
    /// Maps the root AST table into a <see cref="BindingProfile"/>.
    /// </summary>
    private static BindingProfile MapProfile(RkpTable profileTable, string filePath)
    {
        var name = profileTable.GetString("name") ?? Path.GetFileNameWithoutExtension(filePath);
        var warnConflicts = profileTable.GetBool("warnConflicts") ?? true;
        var warnUnremapped = profileTable.GetBool("warnUnremapped") ?? false;

        var bindingGroups = new List<BindingGroup>();
        var bindingGroupsTable = profileTable.GetTable("bindingGroups");

        if (bindingGroupsTable is not null)
        {
            // Iterate all named entries in the bindingGroups table.
            // The same group name can appear multiple times (duplicates are preserved).
            foreach (var (groupName, groupValue) in bindingGroupsTable.NamedEntries())
            {
                if (groupValue is not RkpTable groupTable)
                    continue;

                var group = MapBindingGroup(groupName, groupTable);
                bindingGroups.Add(group);
            }
        }

        return new BindingProfile
        {
            Name = name,
            FilePath = filePath,
            WarnConflicts = warnConflicts,
            WarnUnremapped = warnUnremapped,
            BindingGroups = bindingGroups
        };
    }

    /// <summary>
    /// Maps a single binding group table (e.g., "camera") into a <see cref="BindingGroup"/>.
    /// </summary>
    private static BindingGroup MapBindingGroup(string groupName, RkpTable groupTable)
    {
        var category = BindingCategoryExtensions.FromGroupName(groupName);
        var bindings = new List<Models.Keybinding>();

        // Each anonymous table entry in the group is a command binding.
        foreach (var commandTable in groupTable.AnonymousTables())
        {
            var binding = MapKeybinding(commandTable, groupName, category);
            if (binding is not null)
                bindings.Add(binding);
        }

        return new BindingGroup
        {
            Name = groupName,
            Category = category,
            Bindings = bindings
        };
    }

    /// <summary>
    /// Maps a single command table into a <see cref="Models.Keybinding"/>.
    /// </summary>
    /// <remarks>
    /// Expected structure:
    /// <code>
    /// {
    ///     command = "zoom_in",
    ///     keycombos = {
    ///         { combo = "MouseWheelUp", event = "Press", repeatCount = -1, },
    ///         { combo = "", event = "Press", repeatCount = -1, },
    ///     },
    /// }
    /// </code>
    /// The <c>event</c> field is optional (absent in some profiles).
    /// </remarks>
    private static Models.Keybinding? MapKeybinding(RkpTable commandTable, string groupName, BindingCategory category)
    {
        var commandId = commandTable.GetString("command");
        if (string.IsNullOrWhiteSpace(commandId))
            return null;

        var keycombosTable = commandTable.GetTable("keycombos");

        KeyCombination primary;
        KeyCombination alternate;
        string? eventType = null;
        int repeatCount = -1;

        if (keycombosTable is not null)
        {
            var comboTables = keycombosTable.AnonymousTables().ToList();

            primary = MapKeyCombination(comboTables.ElementAtOrDefault(0));
            alternate = MapKeyCombination(comboTables.ElementAtOrDefault(1));

            // Pull event and repeatCount from the first combo entry
            if (comboTables.Count > 0)
            {
                eventType = comboTables[0].GetString("event");
                repeatCount = comboTables[0].GetInt("repeatCount") ?? -1;
            }
        }
        else
        {
            primary = KeyCombination.Parse("");
            alternate = KeyCombination.Parse("");
        }

        return new Models.Keybinding
        {
            CommandId = commandId,
            GroupName = groupName,
            Category = category,
            Primary = primary,
            Alternate = alternate,
            EventType = eventType,
            RepeatCount = repeatCount
        };
    }

    /// <summary>
    /// Maps a keycombo table entry into a <see cref="KeyCombination"/>.
    /// </summary>
    private static KeyCombination MapKeyCombination(RkpTable? comboTable)
    {
        if (comboTable is null)
            return KeyCombination.Parse("");

        var comboString = comboTable.GetString("combo") ?? "";
        return KeyCombination.Parse(comboString);
    }
}
