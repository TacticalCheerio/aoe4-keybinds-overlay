using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Services.Keybinding;

/// <summary>
/// Provides keybinding profile management and efficient lookup operations.
/// </summary>
public interface IKeybindingService
{
    /// <summary>
    /// Gets the currently active keybinding profile, or null if no profile is loaded.
    /// </summary>
    BindingProfile? ActiveProfile { get; }

    /// <summary>
    /// Gets the names of all available profiles discovered in the profiles directory.
    /// </summary>
    IReadOnlyList<string> AvailableProfileNames { get; }

    /// <summary>
    /// Loads a single keybinding profile from the specified .rkp file path.
    /// </summary>
    /// <param name="filePath">The absolute path to the .rkp file.</param>
    /// <exception cref="System.IO.FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="RkpParseException">The file cannot be parsed.</exception>
    void LoadProfile(string filePath);

    /// <summary>
    /// Scans a directory for .rkp files and loads the first one found as the active profile.
    /// Populates <see cref="AvailableProfileNames"/> with all discovered profile file names.
    /// </summary>
    /// <param name="directoryPath">The directory path to scan for .rkp files.</param>
    void LoadProfilesDirectory(string directoryPath);

    /// <summary>
    /// Gets all keybindings in the active profile.
    /// </summary>
    /// <returns>All keybindings, or an empty list if no profile is loaded.</returns>
    IReadOnlyList<Models.Keybinding> GetAllBindings();

    /// <summary>
    /// Gets all keybindings that use the specified primary key (by Relic key name).
    /// </summary>
    /// <param name="relicKeyName">The Relic key name (e.g., "A", "F1", "MouseWheelUp").</param>
    /// <returns>All matching keybindings, or an empty list.</returns>
    IReadOnlyList<Models.Keybinding> GetBindingsForKey(string relicKeyName);

    /// <summary>
    /// Gets all keybindings whose modifier requirements match the currently active modifiers.
    /// Used to show which commands could be triggered if a primary key is pressed.
    /// </summary>
    /// <param name="activeModifiers">The currently held modifier keys.</param>
    /// <returns>All keybindings that could complete with the given modifiers.</returns>
    IReadOnlyList<Models.Keybinding> GetPossibleBindings(ModifierKeys activeModifiers);

    /// <summary>
    /// Finds the keybinding that exactly matches the given primary key and modifier combination.
    /// Checks both primary and alternate key combinations.
    /// </summary>
    /// <param name="primaryKey">The primary key name.</param>
    /// <param name="modifiers">The active modifier keys.</param>
    /// <returns>The matching keybinding, or null if no exact match exists.</returns>
    Models.Keybinding? FindExactMatch(string primaryKey, ModifierKeys modifiers);

    /// <summary>
    /// Gets all keybindings belonging to the specified category.
    /// </summary>
    /// <param name="category">The binding category to filter by.</param>
    /// <returns>All keybindings in the category, or an empty list.</returns>
    IReadOnlyList<Models.Keybinding> GetBindingsForCategory(BindingCategory category);
}
