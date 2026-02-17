namespace AoE4KeybindsOverlay.Models;

/// <summary>
/// Represents high-level categories for key bindings.
/// </summary>
public enum BindingCategory
{
    /// <summary>
    /// Camera control bindings.
    /// </summary>
    Camera,

    /// <summary>
    /// Control group bindings.
    /// </summary>
    ControlGroups,

    /// <summary>
    /// Unit command bindings.
    /// </summary>
    UnitCommands,

    /// <summary>
    /// General game bindings.
    /// </summary>
    General,

    /// <summary>
    /// Game menu bindings.
    /// </summary>
    GameMenu,

    /// <summary>
    /// Observer/replay bindings.
    /// </summary>
    Observer,

    /// <summary>
    /// Unit selection bindings.
    /// </summary>
    UnitSelection,

    /// <summary>
    /// Unit control bindings.
    /// </summary>
    UnitControl,

    /// <summary>
    /// Building production bindings.
    /// </summary>
    Buildings,

    /// <summary>
    /// Unit and building ability bindings.
    /// </summary>
    Abilities,

    /// <summary>
    /// Unknown or unmapped category.
    /// </summary>
    Unknown
}

/// <summary>
/// Extension methods for BindingCategory.
/// </summary>
public static class BindingCategoryExtensions
{
    /// <summary>
    /// Gets a human-readable display name for the category.
    /// </summary>
    public static string DisplayName(this BindingCategory category)
    {
        return category switch
        {
            BindingCategory.Camera => "Camera",
            BindingCategory.ControlGroups => "Control Groups",
            BindingCategory.UnitCommands => "Unit Commands",
            BindingCategory.General => "General",
            BindingCategory.GameMenu => "Game Menu",
            BindingCategory.Observer => "Observer",
            BindingCategory.UnitSelection => "Unit Selection",
            BindingCategory.UnitControl => "Unit Control",
            BindingCategory.Buildings => "Buildings",
            BindingCategory.Abilities => "Abilities",
            BindingCategory.Unknown => "Unknown",
            _ => category.ToString()
        };
    }

    /// <summary>
    /// Gets the default color for this category as a hex ARGB string.
    /// </summary>
    public static string DefaultColor(this BindingCategory category)
    {
        return category switch
        {
            BindingCategory.Camera => "#FF4A90E2",          // Blue
            BindingCategory.ControlGroups => "#FF9B59B6",    // Purple
            BindingCategory.UnitCommands => "#FF2ECC71",     // Green
            BindingCategory.General => "#FF95A5A6",          // Gray
            BindingCategory.GameMenu => "#FF34495E",         // Dark Blue
            BindingCategory.Observer => "#FFE67E22",         // Orange
            BindingCategory.UnitSelection => "#FF1ABC9C",    // Teal
            BindingCategory.UnitControl => "#FFF39C12",      // Yellow
            BindingCategory.Buildings => "#FFE74C3C",        // Red
            BindingCategory.Abilities => "#FF16A085",        // Dark Teal
            BindingCategory.Unknown => "#FF7F8C8D",          // Light Gray
            _ => "#FFFFFFFF"                                 // White
        };
    }

    /// <summary>
    /// Maps a Relic binding group name to a BindingCategory.
    /// </summary>
    /// <param name="rkpGroupName">The Relic .rkp file group name.</param>
    /// <returns>The corresponding BindingCategory.</returns>
    public static BindingCategory FromGroupName(string rkpGroupName)
    {
        if (string.IsNullOrWhiteSpace(rkpGroupName))
            return BindingCategory.Unknown;

        var normalized = rkpGroupName.ToLowerInvariant().Trim();

        return normalized switch
        {
            "camera" => BindingCategory.Camera,
            "hud_control_groups" => BindingCategory.ControlGroups,
            "hud_dynamic_classic" => BindingCategory.UnitCommands,
            "hud_dynamic_modern" => BindingCategory.UnitCommands,
            "hud_game" => BindingCategory.General,
            "hud_menu" => BindingCategory.GameMenu,
            "hud_replay" => BindingCategory.Observer,
            "hud_selection_orders" => BindingCategory.UnitSelection,
            "hud_unit_control" => BindingCategory.UnitControl,
            "abilities" => BindingCategory.Abilities,
            "unit_abilities" => BindingCategory.Abilities,
            _ => IsBuildingGroup(normalized) ? BindingCategory.Buildings : BindingCategory.Unknown
        };
    }

    /// <summary>
    /// Checks if a group name represents a building or unit-specific group.
    /// Any group that doesn't match the known HUD/system prefixes is assumed to be
    /// a building or unit-specific group (there are 70+ of these in AoE4).
    /// </summary>
    private static bool IsBuildingGroup(string normalizedGroupName)
    {
        // If it starts with "hud_" it's a system group, not a building
        // If it starts with "build_menu_" it's a build menu group -> Buildings
        if (normalizedGroupName.StartsWith("build_menu_"))
            return true;

        // Everything that isn't a known system group and hasn't matched above
        // is a building/unit-specific group (archery_range, barracks, town_center,
        // abbey_of_kings, golden_tent_golden_horde, etc.)
        return !normalizedGroupName.StartsWith("hud_");
    }
}
