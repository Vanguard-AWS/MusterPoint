# Space Marines Dropdown Implementation

## Summary
Updated the SelectFactionStep to replace the Space Marines button with a dropdown selector that includes:
- **Space Marines** (at the top)
- **Black Templars**
- **Blood Angels**
- **Dark Angels**
- **Deathwatch**
- **Imperial Fists**
- **Iron Hands**
- **Raven Guard**
- **Salamanders**
- **Space Wolves**
- **Ultramarines**
- **White Scars**

All other Imperium factions (Imperial Knights, Adeptus Mechanicus, Grey Knights, etc.) continue to display as regular faction cards below the dropdown.

## Changes Made

### 1. SelectFactionStep.razor
**Key Modifications:**

#### New Component State
```csharp
private List<string> SpaceMarineFactions = new();
private List<string> OtherImperiumFactions = new();

private readonly List<string> SpaceMarineSuccessorChapters = new()
{
    "Black Templars",
    "Blood Angels",
    "Dark Angels",
    "Deathwatch",
    "Imperial Fists",
    "Iron Hands",
    "Raven Guard",
    "Salamanders",
    "Space Wolves",
    "Ultramarines",
 "White Scars"
};
```

#### Updated Markup
- Replaced individual Space Marines button with a `<select>` dropdown
- Dropdown first option: "-- Select Space Marines Chapter --" (placeholder)
- Options: Space Marines, then all successor chapters in alphabetical order
- Other Imperium factions render as regular faction cards

#### New Event Handler
```csharp
private void HandleSpaceMarineChange(ChangeEventArgs e)
{
    var selectedValue = e.Value?.ToString();
    if (!string.IsNullOrEmpty(selectedValue))
    {
     _localSelectedFaction = selectedValue;
     SelectedFaction = selectedValue;
    }
}
```

#### Updated Initialization Logic
```csharp
// Separate Space Marines and successors from other Imperium factions
SpaceMarineFactions = allFactions
    .Where(f => f == "Space Marines" || SpaceMarineSuccessorChapters.Contains(f))
    .ToList();

OtherImperiumFactions = allFactions
    .Where(f => FactionCategoryMap.TryGetValue(f, out var category) && 
           category == "Imperium" && 
                !SpaceMarineFactions.Contains(f))
    .OrderBy(f => f)
    .ToList();
```

### 2. SelectFactionStep.razor.css
**New Styles Added:**

#### Dropdown Container
```css
.faction-dropdown-wrapper {
    width: 100%;
  margin-bottom: 0.5rem;
}
```

#### Dropdown Select Element
```css
.faction-dropdown {
    width: 100%;
    padding: 0.5rem 0.75rem;
    border: 2px solid var(--terminal-green);
    border-radius: 2px;
    background-color: rgba(0, 0, 0, 0.3);
    color: var(--terminal-green);
    cursor: pointer;
    transition: all var(--transition-fast);
    font-size: 0.85rem;
    font-family: var(--terminal-font);
    font-weight: bold;
  text-transform: uppercase;
    letter-spacing: 0.05em;
  appearance: none;
    /* Custom dropdown arrow */
    padding-right: 2rem;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='8' viewBox='0 0 12 8'%3E%3Cpath fill='%2300ff00' d='M1 1l5 5 5-5'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 0.5rem center;
    background-size: 1.2rem;
}
```

#### Interactive States
- **`:hover`** - Orange border and highlight, matching faction card hover behavior
- **`:focus`** - Orange accent to indicate focus
- **`.is-selected`** - Orange styling when a Space Marine faction is selected
- **`:disabled`** - Faded appearance when loading

#### Options Styling
```css
.faction-dropdown option {
    background-color: #1a1a1a;
    color: var(--terminal-green);
    padding: 0.5rem;
    font-weight: bold;
}

.faction-dropdown option:checked {
    background-color: rgba(255, 170, 0, 0.3);
    color: var(--terminal-orange);
}
```

## Visual Design

### Theme Consistency
- Uses existing CSS variables: `--terminal-green`, `--terminal-orange`, `--terminal-font`
- Matches faction card styling with 2px borders and terminal aesthetics
- Green text on dark background matching the terminal theme
- Custom SVG arrow icon in terminal green/orange

### Interactive Feedback
- **Hover:** Border changes to orange, background highlights with orange tint
- **Selected:** Matches selected faction card styling (orange borders and background)
- **Disabled:** Reduced opacity during loading
- **Focus:** Clear focus indication for accessibility

### Layout
- Dropdown spans full width of faction-options container
- Positioned above other Imperium faction cards
- Spacing consistent with faction cards (0.5rem gap)

## Backward Compatibility
? **Fully compatible** - No changes to:
- API contracts
- Parent component expectations
- Faction selection event flow
- Already-selected faction handling

## Testing Checklist
- [ ] Dropdown displays on page load
- [ ] Space Marines is shown at top of list
- [ ] All 11 successor chapters appear in dropdown
- [ ] Selection updates `_localSelectedFaction` correctly
- [ ] Continue button enables after selection
- [ ] Selected faction is passed to OnFactionSelected callback
- [ ] Hover states work on dropdown
- [ ] Other Imperium factions still display as cards
- [ ] Responsive behavior maintained on mobile
- [ ] Tab navigation works correctly

## Known Considerations
1. **Browser Support:** Uses CSS `appearance: none` which is widely supported but may have minor styling differences in older browsers
2. **Custom Arrow:** SVG arrow is inlined and uses terminal theme colors
3. **Option Styling:** Browser limitations may prevent full styling of dropdown options in some browsers
4. **Accessibility:** Dropdown is keyboard navigable and screen-reader friendly

## Future Enhancements
- Could add grouping/optgroup to organize chapters by founding/successor generation
- Could add faction icons to dropdown options (if browser support improves)
- Could implement custom dropdown component for full styling control
