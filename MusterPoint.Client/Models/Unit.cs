namespace MusterPoint.Client.Models;

/// <summary>
/// Represents a 40K unit datasheet with all its stats, weapons, abilities, and options.
/// Matches the schema defined in DATASHEET_SCHEMA.md
/// </summary>
public class Unit
{
    /// <summary>Unique identifier in kebab-case (e.g., "intercessor-squad")</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Display name of the unit</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Faction name (e.g., "Space Marines", "Astra Militarum")</summary>
    public string Faction { get; set; } = string.Empty;

    /// <summary>Unit category (e.g., "Characters", "Battleline", "Dedicated Transports")</summary>
    public string? Category { get; set; }

    /// <summary>Unit keywords (e.g., ["INFANTRY", "BATTLELINE"])</summary>
    public List<string> Keywords { get; set; } = new();

    /// <summary>Faction keywords (e.g., ["ADEPTUS ASTARTES"])</summary>
    public List<string> FactionKeywords { get; set; } = new();

    /// <summary>Base points cost for minimum unit size</summary>
    public int BasePoints { get; set; }

    /// <summary>Minimum number of models in the unit</summary>
    public int MinSize { get; set; }

    /// <summary>Maximum number of models in the unit</summary>
    public int MaxSize { get; set; }

    /// <summary>Composition breakdown (e.g., "1 Sergeant + 4-9 Troops")</summary>
    public List<UnitCompositionRole> UnitComposition { get; set; } = new();

    /// <summary>Model profiles with stat lines (M, T, Sv, W, Ld, OC)</summary>
    public List<UnitProfile> Profiles { get; set; } = new();

    /// <summary>All ranged weapons available to the unit</summary>
    public List<RangedWeapon> RangedWeapons { get; set; } = new();

    /// <summary>All melee weapons available to the unit</summary>
    public List<MeleeWeapon> MeleeWeapons { get; set; } = new();

    /// <summary>Special abilities and rules for the unit</summary>
    public List<Ability> Abilities { get; set; } = new();

    /// <summary>Wargear options and upgrades</summary>
    public List<WargearOption> WargearOptions { get; set; } = new();
}

/// <summary>Represents a role in the unit composition (e.g., "Sergeant" or "Troops")</summary>
public class UnitCompositionRole
{
    /// <summary>Role name (e.g., "Intercessor Sergeant")</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Count (number or range like "4-9")</summary>
    public object? Count { get; set; } // Can be int or string like "4-9"
}

/// <summary>Represents a model's stat line: M, T, Sv, W, Ld, OC</summary>
public class UnitProfile
{
    /// <summary>Model type name (e.g., "Intercessor")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Movement characteristic (e.g., "6\"")</summary>
    public string Movement { get; set; } = string.Empty;

    /// <summary>Toughness value</summary>
    public int Toughness { get; set; }

    /// <summary>Armor save (e.g., "3+")</summary>
    public string Save { get; set; } = string.Empty;

    /// <summary>Wounds per model</summary>
    public int Wounds { get; set; }

    /// <summary>Leadership value (e.g., "6+")</summary>
    public string Leadership { get; set; } = string.Empty;

    /// <summary>Objective Control rating</summary>
    public int ObjectiveControl { get; set; }
}

/// <summary>Represents a ranged weapon with full stats</summary>
public class RangedWeapon
{
    /// <summary>Weapon name (e.g., "Bolt Rifle")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Range (e.g., "24\"")</summary>
    public string Range { get; set; } = string.Empty;

    /// <summary>Attacks characteristic (number or "D3")</summary>
    public object? Attacks { get; set; } // Can be int or string "D3"

    /// <summary>Ballistic skill (e.g., "3+")</summary>
    public string BallisticSkill { get; set; } = string.Empty;

    /// <summary>Strength value</summary>
    public int Strength { get; set; }

    /// <summary>Armor Penetration (negative = -AP)</summary>
    public int ArmorPenetration { get; set; }

    /// <summary>Damage (number or "D3")</summary>
    public object? Damage { get; set; } // Can be int or string "D3"

    /// <summary>Weapon type (e.g., "assault heavy", "pistol", "grenade")</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Special rules (e.g., "hazardous", "torrent")</summary>
    public string? Special { get; set; }
}

/// <summary>Represents a melee weapon with full stats</summary>
public class MeleeWeapon
{
    /// <summary>Weapon name (e.g., "Power Fist")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Attacks characteristic</summary>
    public int Attacks { get; set; }

    /// <summary>Weapon Skill (e.g., "3+")</summary>
    public string WeaponSkill { get; set; } = string.Empty;

    /// <summary>Strength value</summary>
    public int Strength { get; set; }

    /// <summary>Armor Penetration</summary>
    public int ArmorPenetration { get; set; }

    /// <summary>Damage (number or "D3")</summary>
    public object? Damage { get; set; } // Can be int or string

    /// <summary>Special rules (e.g., "devastating wounds")</summary>
    public string? Special { get; set; }
}

/// <summary>Represents a special ability or rule</summary>
public class Ability
{
    /// <summary>Ability name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Full ability description/rules text</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Type of ability ("faction" or "unit")</summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>Represents a wargear option/upgrade</summary>
public class WargearOption
{
    /// <summary>Option group name (e.g., "Upgrade Sergeant Weapon")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description of what this option does</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Available choices for this option</summary>
    public List<string> Options { get; set; } = new();

    /// <summary>Points cost for this option</summary>
    public int PointsCost { get; set; }

  /// <summary>Optional selection limit (e.g., "per 5 models")</summary>
    public string? SelectionLimit { get; set; }
}
