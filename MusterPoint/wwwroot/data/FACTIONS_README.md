# Faction Data Structure

## Overview
This structure separates unit data by faction into individual JSON files for better organization and scalability.

## Directory Structure
```
wwwroot/data/
??? factions-index.json          # Master index of all factions
??? factions/
    ??? space-marines.json       # Space Marines units
    ??? astra-militarum.json     # (Future) Astra Militarum units
    ??? necrons.json   # (Future) Necron units
 ??? orks.json      # (Future) Ork units
    ??? [faction].json   # Add more as needed
```

## Adding a New Faction

### Step 1: Create Faction File
Create a new JSON file in `wwwroot/data/factions/` with the faction name in kebab-case:
- `astra-militarum.json`
- `necrons.json`
- `tyranids.json`
- etc.

Each file should be an array of unit objects:
```json
[
  {
    "id": "unit-id",
"name": "Unit Name",
    "faction": "Faction Name",
    "category": "Category",
    "keywords": [...],
    "factionKeywords": [...],
    "basePoints": 100,
    "minSize": 1,
    "maxSize": 1,
"unitComposition": [...],
    "profiles": [...],
    "rangedWeapons": [...],
    "meleeWeapons": [...],
    "abilities": [...],
    "wargearOptions": [...]
  }
]
```

### Step 2: Update Faction Index
Add an entry to `factions-index.json`:
```json
{
  "id": "astra-militarum",
  "name": "Astra Militarum",
  "path": "/data/factions/astra-militarum.json"
}
```

## DatasheetService Integration

The `DatasheetService` should be updated to:
1. Load `factions-index.json` first
2. Dynamically load faction files as needed
3. Combine all faction data when needed

### Example Implementation
```csharp
// Load all available factions
private async Task LoadAllFactions()
{
    var response = await Http.GetFromJsonAsync<FactionIndex>("/data/factions-index.json");
    foreach (var faction in response.Factions)
    {
 var units = await Http.GetFromJsonAsync<List<Unit>>(faction.Path);
        // Add to cache
    }
}
```

## Benefits
- ? **Scalable**: Easy to add new factions without bloating a single file
- ? **Maintainable**: Each faction is isolated and manageable
- ? **Organized**: Clear directory structure
- ? **Efficient**: Load only needed faction data
- ? **Future-proof**: Ready for pagination or lazy-loading

## Current Factions
- Space Marines ?

## Planned Factions
- Astra Militarum
- Necrons
- Tyranids
- Orks
- Genestealer Cults
- Dark Eldar/Drukhari
- Eldar/Craftworlds
- Tau Empire
- And more...
