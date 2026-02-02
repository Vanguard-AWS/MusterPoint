# Points Limit Validation Implementation

## Overview
Added points limit validation to alert users when their army list exceeds the intended point value.

## What Was Added

### ListValidationService Update
- **New Parameter**: `ValidateList()` method now accepts an optional `pointsLimit` parameter (defaults to 2000)
- **New Validation Rule**: `ValidatePointsLimit()` method checks if total list points exceed the limit
- **Warning Level**: Points overage is displayed as a **Warning** (yellow alert), not an error, so users can still build over-pointed lists

### BuildListStep.razor Update
- Updated `ValidateCurrentList()` to pass the `PointsLimit` parameter to the validation service
- Validation now runs with the correct point limit context

## Validation Rule Details

**Rule 6: Points Limit Check**
- Triggers when: `totalPoints > pointsLimit`
- Severity: **Warning** (yellow alert)
- Message Format: `"Your list exceeds the {pointsLimit} point limit by {overage} points. ({totalPoints} / {pointsLimit} pts)"`
- Example: If limit is 2000 and list is 2050: `"Your list exceeds the 2000 point limit by 50 points. (2050 / 2000 pts)"`

## User Experience

### When Points Are Over
The validation alert panel shows:
- **Yellow Warning Alert** with the message
- Clear indication of how many points over the limit
- Current total vs. limit comparison

### When Points Are Within Limit
The validation alert shows:
- **Green Success Message** if all other validations pass
- No points-related warnings

## Technical Details

- Points validation uses the `UserList.CalculateTotalPoints()` method
- The validation runs whenever:
  - Units are added
  - Units are removed
  - Model counts are changed
  - The list is loaded
- It's a **warning**, not an error, so players can intentionally build lists that exceed points for casual play or testing

## Integration with Existing Validation

This validation integrates seamlessly with the existing 6 validation rules:
1. ? At least one CHARACTER
2. ? Only one of each named EPIC HERO
3. ? Up to 3 units of each datasheet (6 for BATTLELINE/DEDICATED TRANSPORT)
4. ? Enhancement limits (1 per CHARACTER, 3 total, all different)
5. ? Exactly one Warlord
6. **? NEW - Points limit warning**

All validations display together in the single alert panel at the top of the Current List section.
