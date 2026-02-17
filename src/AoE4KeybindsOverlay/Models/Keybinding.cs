using System.Text.RegularExpressions;

namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents a single keybinding with primary and alternate key combinations.
/// </summary>
public sealed class Keybinding
{
    /// <summary>
    /// Gets the unique command identifier (e.g., "zoom_in", "produce_archer").
    /// </summary>
    public required string CommandId { get; init; }

    /// <summary>
    /// Gets the binding group name (e.g., "camera", "archery_range").
    /// </summary>
    public required string GroupName { get; init; }

    /// <summary>
    /// Gets the category this binding belongs to.
    /// </summary>
    public required BindingCategory Category { get; init; }

    /// <summary>
    /// Gets the primary key combination for this binding.
    /// </summary>
    public required KeyCombination Primary { get; init; }

    /// <summary>
    /// Gets the alternate key combination for this binding.
    /// </summary>
    public required KeyCombination Alternate { get; init; }

    /// <summary>
    /// Gets the event type for this binding (e.g., "Press", "Hold", "Release").
    /// </summary>
    public string? EventType { get; init; }

    /// <summary>
    /// Gets the repeat count for this binding. -1 indicates no repeat limit.
    /// </summary>
    public int RepeatCount { get; init; } = -1;

    /// <summary>
    /// Gets a formatted display name for this command.
    /// </summary>
    public string DisplayName => FormatCommandName(CommandId);

    /// <summary>
    /// Converts a command ID like "pick_all_military_buildings" to "Pick All Military Buildings".
    /// </summary>
    private static string FormatCommandName(string commandId)
    {
        if (string.IsNullOrWhiteSpace(commandId))
            return string.Empty;

        // Replace underscores with spaces
        var formatted = commandId.Replace('_', ' ');

        // Split by spaces and capitalize each word
        var words = formatted.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var capitalizedWords = words.Select(word =>
        {
            if (word.Length == 0)
                return word;

            // Handle common abbreviations
            if (IsAbbreviation(word))
                return word.ToUpperInvariant();

            // Capitalize first letter, lowercase the rest
            return char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant();
        });

        return string.Join(" ", capitalizedWords);
    }

    /// <summary>
    /// Checks if a word should be treated as an abbreviation.
    /// </summary>
    private static bool IsAbbreviation(string word)
    {
        var upperWord = word.ToUpperInvariant();
        return upperWord switch
        {
            "UI" => true,
            "HUD" => true,
            "AOE" => true,
            "RTS" => true,
            "HP" => true,
            "MP" => true,
            "XP" => true,
            _ => false
        };
    }
}
