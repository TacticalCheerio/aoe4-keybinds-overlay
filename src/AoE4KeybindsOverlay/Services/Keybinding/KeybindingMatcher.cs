using AoE4KeybindsOverlay.Models;

namespace AoE4KeybindsOverlay.Services.Keybinding;

/// <summary>
/// Provides efficient lookup of keybindings by primary key, modifier set, and category.
/// Call <see cref="Build"/> to index a set of keybindings, then use the query methods
/// for O(1) dictionary lookup followed by a small linear scan.
/// </summary>
public sealed class KeybindingMatcher
{
    /// <summary>
    /// Maps a primary key name (case-insensitive) to all keybindings that use that key
    /// in either their primary or alternate combination.
    /// </summary>
    private readonly Dictionary<string, List<Models.Keybinding>> _byPrimaryKey =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maps a modifier key set to all keybindings whose primary or alternate combination
    /// requires exactly that modifier set.
    /// </summary>
    private readonly Dictionary<ModifierKeys, List<Models.Keybinding>> _byModifiers = new();

    /// <summary>
    /// Maps a binding category to all keybindings in that category.
    /// </summary>
    private readonly Dictionary<BindingCategory, List<Models.Keybinding>> _byCategory = new();

    /// <summary>
    /// All indexed keybindings, in insertion order.
    /// </summary>
    private readonly List<Models.Keybinding> _allBindings = new();

    /// <summary>
    /// Builds the internal lookup indices from the given set of keybindings.
    /// Clears any previously indexed data.
    /// </summary>
    /// <param name="bindings">The keybindings to index.</param>
    public void Build(IEnumerable<Models.Keybinding> bindings)
    {
        _byPrimaryKey.Clear();
        _byModifiers.Clear();
        _byCategory.Clear();
        _allBindings.Clear();

        foreach (var binding in bindings)
        {
            _allBindings.Add(binding);

            // Index by primary key combination
            IndexKeyCombination(binding, binding.Primary);

            // Index by alternate key combination
            IndexKeyCombination(binding, binding.Alternate);

            // Index by category
            if (!_byCategory.TryGetValue(binding.Category, out var categoryList))
            {
                categoryList = new List<Models.Keybinding>();
                _byCategory[binding.Category] = categoryList;
            }
            categoryList.Add(binding);
        }
    }

    /// <summary>
    /// Indexes a single key combination for the given binding into the primary key
    /// and modifier dictionaries.
    /// </summary>
    private void IndexKeyCombination(Models.Keybinding binding, KeyCombination combo)
    {
        if (combo.IsEmpty)
            return;

        // Index by primary key name
        if (!_byPrimaryKey.TryGetValue(combo.PrimaryKey, out var keyList))
        {
            keyList = new List<Models.Keybinding>();
            _byPrimaryKey[combo.PrimaryKey] = keyList;
        }
        // Avoid duplicate entries when primary and alternate share the same key
        if (!keyList.Contains(binding))
            keyList.Add(binding);

        // Index by modifier set
        if (!_byModifiers.TryGetValue(combo.Modifiers, out var modList))
        {
            modList = new List<Models.Keybinding>();
            _byModifiers[combo.Modifiers] = modList;
        }
        if (!modList.Contains(binding))
            modList.Add(binding);
    }

    /// <summary>
    /// Gets all indexed keybindings.
    /// </summary>
    /// <returns>A read-only list of all keybindings.</returns>
    public IReadOnlyList<Models.Keybinding> GetAll() => _allBindings;

    /// <summary>
    /// Gets all keybindings that use the specified primary key name.
    /// </summary>
    /// <param name="relicKeyName">The Relic key name (case-insensitive).</param>
    /// <returns>All matching keybindings, or an empty list.</returns>
    public IReadOnlyList<Models.Keybinding> GetBindingsForKey(string relicKeyName)
    {
        if (string.IsNullOrEmpty(relicKeyName))
            return Array.Empty<Models.Keybinding>();

        return _byPrimaryKey.TryGetValue(relicKeyName, out var list)
            ? list
            : Array.Empty<Models.Keybinding>();
    }

    /// <summary>
    /// Gets all keybindings whose modifier requirements match the given active modifiers.
    /// This answers "which commands could fire if a primary key is pressed while these
    /// modifiers are held?"
    /// </summary>
    /// <param name="activeModifiers">The currently held modifier keys.</param>
    /// <returns>All keybindings that could complete with the given modifiers.</returns>
    public IReadOnlyList<Models.Keybinding> GetPossibleCompletions(ModifierKeys activeModifiers)
    {
        return _byModifiers.TryGetValue(activeModifiers, out var list)
            ? list
            : Array.Empty<Models.Keybinding>();
    }

    /// <summary>
    /// Finds the first keybinding that exactly matches the given primary key and modifier
    /// combination. Checks both primary and alternate key combinations.
    /// </summary>
    /// <param name="primaryKey">The primary key name (case-insensitive).</param>
    /// <param name="modifiers">The active modifier keys.</param>
    /// <returns>The first matching keybinding, or null if no exact match exists.</returns>
    public Models.Keybinding? FindExactMatch(string primaryKey, ModifierKeys modifiers)
    {
        if (string.IsNullOrEmpty(primaryKey))
            return null;

        if (!_byPrimaryKey.TryGetValue(primaryKey, out var candidates))
            return null;

        for (int i = 0; i < candidates.Count; i++)
        {
            var binding = candidates[i];
            if (binding.Primary.Matches(primaryKey, modifiers) ||
                binding.Alternate.Matches(primaryKey, modifiers))
            {
                return binding;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all keybindings belonging to the specified category.
    /// </summary>
    /// <param name="category">The binding category to filter by.</param>
    /// <returns>All keybindings in the category, or an empty list.</returns>
    public IReadOnlyList<Models.Keybinding> GetBindingsForCategory(BindingCategory category)
    {
        return _byCategory.TryGetValue(category, out var list)
            ? list
            : Array.Empty<Models.Keybinding>();
    }
}
