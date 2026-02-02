# List Validation Implementation Summary

## Overview
Added comprehensive list validation to the MusterPoint list builder that checks if the player's army composition follows all 40K game rules.

## Validation Rules Implemented

1. **At least one CHARACTER** - Lists must have at least one unit with the Characters category
2. **Only one EPIC HERO of each name** - Units with the EPIC HERO keyword can only be included once
3. **Unit Limits** - Up to 3 units of each datasheet (with exceptions)
   - **Exception**: Units with BATTLELINE or DEDICATED TRANSPORT keywords can have up to 6
4. **Enhancement Rules**:
   - Each CHARACTER can only have one Enhancement
   - Maximum of 3 Enhancements total in the list
   - All Enhancements must be different (no duplicates)
5. **Warlord Selection** - Exactly one CHARACTER must be designated as the Warlord

## Files Created/Modified

### New Files
- **`MusterPoint.Client\Services\ListValidationService.cs`** - Core validation logic
  - `ValidateList()` - Main method that runs all validations
  - `ValidationMessage` class - Represents each validation issue
- `ValidationSeverity` enum - Error, Warning, or Info severity levels
  - Helper methods for each validation rule

### Modified Files
- **`MusterPoint.Client\Pages\BuildListStep.razor`**
  - Added ListValidationService injection
  - Added validation alert section at the top of the Current List column
  - Integrated validation checks on all list modifications
  - Calls `ValidateCurrentList()` whenever the list changes

- **`MusterPoint.Client\Program.cs`**
  - Registered ListValidationService in the dependency injection container

## User Interface

### Validation Alert Display
The list builder now shows a validation alert panel above the current list with three possible states:

#### ? Invalid List (Red)
Shows when the list has validation errors. Displays:
- "Invalid List" header with exclamation icon
- Bullet list of all validation errors

#### ? Valid List (Green)
Shows when all validation rules are met. Displays:
- "List is Valid ?" message with checkmark icon

#### ?? Warnings (Yellow)
Shows any non-critical validation warnings (can be added for future rules)

## Validation Triggers
Validation runs automatically whenever:
- A unit is added to the list
- A unit is removed from the list
- Model count is changed (recalculation)
- Enhancement is selected/changed
- Warlord is selected/changed
- A list is loaded for editing

## User Experience
- Users can still break the rules (allow invalid lists)
- Clear, specific error messages tell users exactly what's wrong
- Validation updates in real-time as they build the list
- Multiple violations are all listed together so users can fix them all at once

## Example Validation Messages
- "You must include at least one CHARACTER unit in your list."
- "You can only include one Captain (EPIC HERO) in your list, but you have 2."
- "You can only include up to three units of Intercessor Squad, but you have 4."
- "You can include a maximum of three Enhancements in total, but you have 4."
- "You must select one CHARACTER to be your Warlord."
