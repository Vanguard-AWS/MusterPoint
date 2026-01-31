# Unit Details View Implementation

## Overview
A comprehensive unit details modal has been added to the list builder that allows players to view complete unit information before adding them to their army lists.

## New Components Created

### 1. **UnitDetailsModal.razor** (Modal Component)
A reusable modal component that displays complete unit information including:

#### Display Sections:
- **Unit Header**: Category, base points, and squad size
- **Keywords**: Both unit and faction keywords with color-coded tags
- **Unit Composition**: Breakdown of unit roles (e.g., "1 Sergeant + 4-9 Intercessors")
- **Model Profiles**: Statistical profile tables (M, T, Sv, W, Ld, OC)
- **Ranged Weapons**: Complete weapon stat tables with range, attacks, BS, S, AP, D, type, and special rules
- **Melee Weapons**: Melee weapon stat tables with attacks, WS, S, AP, and D
- **Abilities**: Unit and faction special abilities with description and type badges
- **Wargear Options**: Available weapon/equipment upgrades with point costs and selection limits

#### Features:
- Scrollable modal body (max-height: 70vh) for large datasheets
- Clean table layouts with proper spacing
- Tag-based keyword display
- Special rules highlighted in italics
- Responsive font sizing
- "Add to List" button to quickly add units from the modal
- Close button to dismiss the modal

### 2. **UnitDetailsModal.razor.css** (Modal Styling)
Professional styling for the modal including:
- Responsive modal dimensions (max-width: 900px)
- Custom scrollbar styling
- Table formatting with centered headers
- Color-coded ability badges
- Hover effects and transitions
- Mobile-responsive design

## Enhanced Components

### BuildListStep.razor (Updated)
**New Features:**
1. **Clickable Unit Cards**: Left sidebar units are now clickable to view details
2. **Clickable List Entries**: Units in the current list can be clicked to view their details
3. **Modal Integration**: Shows unit details in a beautiful modal overlay
4. **Two-way Access**: 
   - Click units in "Available Units" to view ? Add button in modal
   - Click units in current list to view their stats

**New Methods:**
- `ShowUnitDetails(Unit unit)` - Opens modal with selected unit
- `ShowUnitDetailsFromEntry(ListEntry entry)` - Shows details for units in current list
- `CloseUnitDetails()` - Closes the modal
- `AddSelectedUnitFromModal()` - Adds unit from within the modal

**New Properties:**
- `selectedUnitForDetails` - Currently displayed unit
- `showUnitDetailsModal` - Modal visibility state

### BuildListStep.razor.css (Updated)
**Enhanced Styling:**
- Unit card hover effects with slight lift animation
- Improved category toggle buttons with color transitions
- Better scrollbar styling for unit lists
- Table container improvements
- Mobile responsiveness enhancements
- Link-style unit names in the current list

## User Experience Improvements

### Workflow:
1. **Browse Units** ? Click on any unit name to view full details
2. **View Complete Stats** ? See all weapons, profiles, abilities, and options
3. **Make Informed Decision** ? Decide if unit fits your army
4. **Quick Add** ? "Add to List" button directly in modal, or close and use sidebar button
5. **Reference** ? Click list units to quickly check stats during building

### Visual Enhancements:
- Smooth animations when expanding categories
- Hover states indicating clickable elements
- Color-coded tabs (info for faction, warning for unit abilities)
- Clean, organized table layouts
- Professional modal design

## Technical Details

### Data Display:
- Uses the full Unit model with all weapon, ability, and composition data
- Renders dynamic content (weapon stats, abilities) with proper formatting
- Handles optional fields (special rules, selection limits)
- Supports both numeric and dice-based values (D3, D6, etc.)

### Accessibility:
- Semantic HTML structure
- Proper heading hierarchy in modal
- Clear button labels
- Disabled state management
- Keyboard navigation support through modal

### Performance:
- Modal component is lightweight
- No expensive calculations
- Data already loaded from DatasheetService
- Efficient state management

## Future Enhancements

Potential additions to consider:
1. **Favorite Units** - Mark units as favorites
2. **Unit Comparison** - Compare multiple units side-by-side
3. **Point Calculator** - Show cost variations based on model count
4. **Wargear Calculator** - Calculate upgrade costs in real-time
5. **Export** - Print or export unit details
6. **Search** - Full-text search within modal content
7. **Keyboard Navigation** - Arrow keys to cycle through units
8. **Mobile Layout** - Optimized modal for smaller screens

## Files Modified/Created:
- ? Created: `MusterPoint.Client\Pages\UnitDetailsModal.razor`
- ? Created: `MusterPoint.Client\Pages\UnitDetailsModal.razor.css`
- ? Updated: `MusterPoint.Client\Pages\BuildListStep.razor`
- ? Updated: `MusterPoint.Client\Pages\BuildListStep.razor.css`
