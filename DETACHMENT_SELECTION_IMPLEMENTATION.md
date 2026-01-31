# Detachment Selection Implementation - Summary

## Overview
Added detachment selection to Step 2 of the list builder. Users can now select a detachment associated with their chosen faction after selecting the faction and before building their army list.

## Files Created

### 1. **MusterPoint.Client\Models\Detachment.cs**
New model classes for handling detachment data:
- `Detachment` - Main detachment object with ID, name, faction, rule, and enhancements
- `DetachmentRule` - Contains the detachment's core rule (name and description)
- `Enhancement` - Individual enhancements/relics available in a detachment

### 2. **MusterPoint.Client\Services\DetachmentService.cs**
Service for loading detachment data from JSON files:
- `GetDetachmentsByFactionAsync()` - Loads all detachments for a specific faction
- `GetDetachmentByIdAsync()` - Loads a specific detachment by ID
- Handles index.json loading and dynamic detachment file loading
- Error handling and logging

## Files Modified

### 1. **MusterPoint.Client\Models\UserList.cs**
- Added `Detachment` property to store selected detachment ID

### 2. **MusterPoint.Client\Pages\ConfigureListStep.razor**
- Added detachment selection dropdown
- Integrated DetachmentService for loading faction-specific detachments
- Added loading state while detachments are being fetched
- Required detachment selection before allowing user to continue
- Passes detachment selection via `OnDetachmentChanged` callback

### 3. **MusterPoint.Client\Pages\ListBuilder.razor**
- Added `selectedDetachment` state variable
- Added `HandleDetachmentChanged()` method
- Pass detachment selection to ConfigureListStep
- Pass detachment selection to BuildListStep
- Load detachment when editing existing list

### 4. **MusterPoint.Client\Pages\BuildListStep.razor**
- Added `SelectedDetachment` parameter
- Updated `SaveList()` to include detachment in UserList
- Updated `LoadListForEditing()` to load detachment from saved list

### 5. **MusterPoint.Client\Program.cs**
- Registered `DetachmentService` in DI container

## User Flow

1. **Step 1**: User selects a faction (Space Marines, etc.)
2. **Step 2**: User now:
   - Enters list name
   - **Selects detachment** (new) ? Loading detachments for chosen faction
   - Selects game size (1000/2000/3000 points)
3. **Step 3**: Builds army list with units

## Data Flow

```
ConfigureListStep.razor
  ?
  DetachmentService.GetDetachmentsByFactionAsync()
    ?
    Load: /data/space-marine-detachments/index.json
      ?
      For each detachment in faction:
        Load: /data/space-marine-detachments/{detachment-id}.json
     ?
          Returns: Detachment objects with rules and enhancements
    ?
  Dropdown populated with available detachments
    ?
  User selects detachment
    ?
  OnDetachmentChanged event fired
    ?
  ListBuilder stores: selectedDetachment
    ?
  BuildListStep receives: selectedDetachment
    ?
  SaveList includes: currentList.Detachment = SelectedDetachment
```

## Detachment Structure

Each detachment loads from JSON with:
```json
{
  "id": "gladius-task-force",
  "name": "Gladius Task Force",
  "faction": "Space Marines",
  "factionKeyword": "ADEPTUS ASTARTES",
  "detachmentRule": {
    "name": "Combat Doctrines",
    "description": "..."
  },
  "enhancements": [
    {
    "id": "artificer-armour",
    "name": "Artificer Armour",
      "points": 10,
      "restrictions": "ADEPTUS ASTARTES model only",
      "description": "..."
    }
  ]
}
```

## UI Components

### Detachment Dropdown
- Displays in ConfigureListStep between List Name and Game Size fields
- Shows loading state while fetching detachments
- Displays empty state if no detachments available
- Required field (Continue button disabled until selected)
- Clean dropdown with faction-specific detachments

## Features

? **Dynamic Loading** - Detachments loaded based on selected faction
? **Service-Based** - Centralized DetachmentService handles all loading
? **Error Handling** - Graceful fallbacks if detachments fail to load
? **State Persistence** - Detachment selection saved with list
? **Edit Support** - Loads detachment when editing existing list
? **User Feedback** - Loading states and empty states handled

## Integration Ready

The detachment selection is now fully integrated and ready for:
- Displaying detachment rules in the list builder (future)
- Showing enhancements available in the selected detachment (future)
- Validating army composition against detachment restrictions (future)
- Exporting list with detachment information (future)

## Space Marine Detachments Available

1. **Gladius Task Force** - Combat Doctrines system
2. **Anvil Siege Force** - Shield of the Imperium rule

More detachments can be added by:
1. Creating new JSON file in `space-marine-detachments/` folder
2. Adding entry to `index.json`
3. No code changes needed!

## Next Steps

To further enhance:
1. Display selected detachment's rule in the list builder
2. Show available enhancements from the detachment
3. Allow enhancement selection during list building
4. Display detachment info in list details
5. Add detachment validation rules
6. Support for additional factions and their detachments
