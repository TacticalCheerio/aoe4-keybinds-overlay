using System.IO;
using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Services.Keybinding;

/// <summary>
/// Default implementation of <see cref="IKeybindingService"/>.
/// Uses <see cref="RkpProfileReader"/> to load .rkp profiles and <see cref="KeybindingMatcher"/>
/// for efficient keybinding lookups.
/// </summary>
public sealed class KeybindingService : IKeybindingService
{
    private readonly KeybindingMatcher _matcher = new();
    private readonly List<string> _availableProfileNames = new();

    /// <inheritdoc />
    public BindingProfile? ActiveProfile { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<string> AvailableProfileNames => _availableProfileNames;

    /// <inheritdoc />
    public void LoadProfile(string filePath)
    {
        var profile = RkpProfileReader.ReadFromFile(filePath);
        ActiveProfile = profile;
        _matcher.Build(profile.AllBindings);
    }

    /// <inheritdoc />
    public void LoadProfilesDirectory(string directoryPath)
    {
        _availableProfileNames.Clear();

        if (!Directory.Exists(directoryPath))
            return;

        var rkpFiles = Directory.GetFiles(directoryPath, "*.rkp", SearchOption.TopDirectoryOnly);

        foreach (var filePath in rkpFiles)
        {
            _availableProfileNames.Add(Path.GetFileNameWithoutExtension(filePath));
        }

        // Sort alphabetically for consistent ordering
        _availableProfileNames.Sort(StringComparer.OrdinalIgnoreCase);

        // Load the first profile as the active one, if any exist
        if (rkpFiles.Length > 0)
        {
            LoadProfile(rkpFiles[0]);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<Models.Keybinding> GetAllBindings()
    {
        return _matcher.GetAll();
    }

    /// <inheritdoc />
    public IReadOnlyList<Models.Keybinding> GetBindingsForKey(string relicKeyName)
    {
        return _matcher.GetBindingsForKey(relicKeyName);
    }

    /// <inheritdoc />
    public IReadOnlyList<Models.Keybinding> GetPossibleBindings(ModifierKeys activeModifiers)
    {
        return _matcher.GetPossibleCompletions(activeModifiers);
    }

    /// <inheritdoc />
    public Models.Keybinding? FindExactMatch(string primaryKey, ModifierKeys modifiers)
    {
        return _matcher.FindExactMatch(primaryKey, modifiers);
    }

    /// <inheritdoc />
    public IReadOnlyList<Models.Keybinding> GetBindingsForCategory(BindingCategory category)
    {
        return _matcher.GetBindingsForCategory(category);
    }
}
