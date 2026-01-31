# 40K Datasheet JSON Schema

## Complete Unit Structure

All datasheets in `wwwroot/data/units.json` follow this exact structure:

```json
{
  "id": "unique-identifier",
  "name": "Unit Name",
  "faction": "Space Marines",
  "keywords": ["INFANTRY", "BATTLELINE", "GRENADES", "IMPERIUM", "TACTICUS"],
  "factionKeywords": ["ADEPTUS ASTARTES"],
  "basePoints": 80,
  "minSize": 5,
  "maxSize": 10,
  "unitComposition": [
    {
      "role": "Intercessor Sergeant",
      "count": 1
},
    {
 "role": "Intercessors",
      "count": "4-9"
    }
  ],
  "profiles": [
    {
  "name": "Intercessor",
      "movement": "6\"",
      "toughness": 4,
    "save": "3+",
      "wounds": 2,
      "leadership": "6+",
      "objectiveControl": 2
    }
  ],
  "rangedWeapons": [
    {
      "name": "Bolt Rifle",
      "range": "24\"",
      "attacks": 2,
      "ballisticSkill": "3+",
      "strength": 4,
      "armorPenetration": -1,
      "damage": 1,
      "type": "assault heavy",
      "special": "optional - hazardous, torrent, etc."
    }
  ],
  "meleeWeapons": [
    {
  "name": "Close Combat Weapon",
      "attacks": 3,
      "weaponSkill": "3+",
  "strength": 4,
      "armorPenetration": 0,
 "damage": 1,
      "special": "optional"
}
  ],
  "abilities": [
    {
      "name": "Ability Name",
      "description": "Full ability text",
   "type": "faction or unit"
    }
  ],
  "wargearOptions": [
    {
      "name": "Option Name",
      "description": "What this option allows",
      "options": ["Choice 1", "Choice 2"],
      "pointsCost": 0,
      "selectionLimit": "optional - e.g., 'per 5 models'"
    }
  ]
}
```

## Key Field Details

### Root Level
| Field | Type | Example | Notes |
|-------|------|---------|-------|
| `id` | string | "intercessor-squad" | Unique identifier, use kebab-case |
| `name` | string | "Intercessor Squad" | Display name |
| `faction` | string | "Space Marines" | Which faction |
| `keywords` | string[] | ["INFANTRY", "BATTLELINE"] | Unit keywords |
| `factionKeywords` | string[] | ["ADEPTUS ASTARTES"] | Faction keywords |
| `basePoints` | number | 80 | Base cost in points |
| `minSize` | number | 5 | Minimum models in unit |
| `maxSize` | number | 10 | Maximum models in unit |

### Profiles (Stats Line)
| Field | Type | Example | Notes |
|-------|------|---------|-------|
| `name` | string | "Intercessor" | Model type name |
| `movement` | string | "6\"" | Include quotes symbol |
| `toughness` | number | 4 | T value |
| `save` | string | "3+" | Armor save |
| `wounds` | number | 2 | W per model |
| `leadership` | string | "6+" | Ld value |
| `objectiveControl` | number | 2 | OC rating |

### Ranged Weapons
| Field | Type | Example | Notes |
|-------|------|---------|-------|
| `name` | string | "Bolt Rifle" | Weapon name |
| `range` | string | "24\"" | Include quotes symbol |
| `attacks` | number\|string | 2 or "D3" | A value |
| `ballisticSkill` | string | "3+" | BS |
| `strength` | number | 4 | S value |
| `armorPenetration` | number | -1 | AP (negative = -AP) |
| `damage` | number\|string | 1 or "D3" | D value |
| `type` | string | "assault heavy" | Weapon type |
| `special` | string | "hazardous" | Optional special rules |

### Melee Weapons
| Field | Type | Example | Notes |
|-------|------|---------|-------|
| `name` | string | "Power Fist" | Weapon name |
| `attacks` | number | 3 | A value |
| `weaponSkill` | string | "3+" | WS |
| `strength` | number | 8 | S value |
| `armorPenetration` | number | -2 | AP |
| `damage` | number\|string | 2 or "D3" | D value |
| `special` | string | "devastating wounds" | Optional |

### Abilities
| Field | Type | Example | Notes |
|-------|------|---------|-------|
| `name` | string | "Objective Secured" | Ability name |
| `description` | string | "Full text..." | Complete description |
| `type` | string | "faction" or "unit" | Category |

### Wargear Options
| Field | Type | Example | Notes |
|-------|------|---------|-------|
| `name` | string | "Upgrade Sergeant" | Option group name |
| `description` | string | "Can be replaced with..." | What it does |
| `options` | string[] | ["Chainsword", "Hammer"] | Specific choices |
| `pointsCost` | number | 0 | Cost in points |
| `selectionLimit` | string | "per 5 models" | Optional limit |

## Important Notes

1. **String vs Number**: Some fields that look numeric (like attack rolls) should be strings if they're variable (e.g., "D3", "3+")
2. **Negative AP**: Armor penetration uses negative numbers (-1 for -1 AP)
3. **Quotes in Strings**: Range and movement include the " symbol: "24\"" 
4. **Variable Counts**: Unit composition can use strings for ranges: "4-9"
5. **No Detachment Field**: Keep faction/keywords only, no detachment
6. **Optional Fields**: `special` fields in weapons are optional
7. **Weapons Separation**: Keep `rangedWeapons` and `meleeWeapons` strictly separate

## Example: Adding a New Unit

```json
{
  "id": "terminator-squad",
  "name": "Terminator Squad",
  "faction": "Space Marines",
  "keywords": ["INFANTRY", "ELITE", "GRENADES", "IMPERIUM"],
  "factionKeywords": ["ADEPTUS ASTARTES"],
  "basePoints": 210,
  "minSize": 3,
  "maxSize": 10,
  "unitComposition": [
    {"role": "Terminator Sergeant", "count": 1},
    {"role": "Terminators", "count": "2-9"}
  ],
  "profiles": [
    {
"name": "Terminator",
  "movement": "5\"",
      "toughness": 5,
      "save": "2+",
      "wounds": 3,
      "leadership": "8+",
      "objectiveControl": 1
    }
  ],
  "rangedWeapons": [...],
  "meleeWeapons": [...],
  "abilities": [...],
  "wargearOptions": [...]
}
```
