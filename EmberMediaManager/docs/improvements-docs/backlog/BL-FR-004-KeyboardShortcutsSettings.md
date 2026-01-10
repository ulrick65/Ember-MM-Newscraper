# BL-FR-004: Keyboard Shortcuts Settings Page

| Field | Value |
|-------|-------|
| **ID** | BL-FR-004 |
| **Created** | January 10, 2026 |
| **Priority** | Low |
| **Effort** | 8-16 hours |
| **Status** | 📋 Open |
| **Category** | Feature Requests |
| **Related Files** | [`frmMain.vb`](../../../EmberMediaManager/frmMain.vb), [`frmMain.Designer.vb`](../../../EmberMediaManager/frmMain.Designer.vb), [`dlgSettings.vb`](../../../EmberMediaManager/dlgSettings.vb) |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

Create a settings page to display and allow modification of keyboard shortcuts. Currently all shortcuts are hardcoded in Designer files (menu shortcuts) and event handlers (control-specific keys).

---

## Table of Contents

- [Problem Description](#problem-description)
- [Current Implementation](#current-implementation)
- [Solution Summary](#solution-summary)
- [Implementation Details](#implementation-details)
- [Testing Summary](#testing-summary)
- [Related Files](#related-files)
- [Notes](#notes)

---

## [↑](#table-of-contents) Problem Description

Users cannot customize keyboard shortcuts in the application. All shortcuts are hardcoded, making it impossible to:

- View a complete list of available shortcuts
- Modify shortcuts to match user preferences
- Resolve conflicts with other software or accessibility tools
- Add shortcuts to frequently used actions that don't have them

---

## [↑](#table-of-contents) Current Implementation

Keyboard shortcuts are implemented in two ways:

### Menu Shortcuts

Defined in Designer files via the `ShortcutKeys` property on `ToolStripMenuItem` controls:

    Me.mnuMainEditSettings.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.S), System.Windows.Forms.Keys)

### Control Event Handlers

Handled via `KeyDown` and `KeyPress` events in code-behind files:

    Private Sub dgvMovies_KeyDown(sender As Object, e As KeyEventArgs) Handles dgvMovies.KeyDown
        If e.KeyCode = Keys.Enter Then
            ' Action
        End If
    End Sub

### Type-Ahead Search

The main form uses a `KeyBuffer` field with `tmrKeyBuffer` timer for type-ahead search functionality in list views.

### Known Hardcoded Shortcuts

| Shortcut | Location | Action |
|----------|----------|--------|
| Ctrl+A | TextBoxes | Select all text |
| Enter | DataGridViews | Open edit dialog |
| Delete | Various lists | Remove selected item |
| Escape | Dialogs | Close dialog |
| Type characters | Main lists | Type-ahead search/filter |

---

## [↑](#table-of-contents) Solution Summary

### Phase 1: Catalog and Display (4-6 hours)

1. Create a new settings panel for keyboard shortcuts
2. Catalog all existing hardcoded shortcuts
3. Display shortcuts in a read-only list initially

### Phase 2: Make Editable (4-6 hours)

1. Create a `KeyboardShortcuts` settings class
2. Add UI to capture and modify key combinations
3. Refactor handlers to read from settings instead of hardcoded values

### Phase 3: Persistence (2-4 hours)

1. Save custom shortcuts to settings file
2. Load and apply on startup
3. Add "Reset to Defaults" option

---

## [↑](#table-of-contents) Implementation Details

### Settings Class Structure

    Public Class KeyboardShortcut
        Public Property Action As String
        Public Property Keys As Keys
        Public Property Description As String
        Public Property Category As String ' Menu, Grid, Dialog, etc.
    End Class

    Public Class KeyboardShortcutSettings
        Public Property Shortcuts As New List(Of KeyboardShortcut)
    End Class

### Settings Panel

Add a new panel to `dlgSettings.vb`:

- TreeView node: "General" → "Keyboard Shortcuts"
- ListView showing: Action, Shortcut, Category
- "Edit" button to capture new key combination
- "Reset" button to restore defaults

### Refactoring Pattern

Replace hardcoded checks:

    ' Before
    If e.KeyCode = Keys.Enter Then

    ' After
    If e.KeyCode = Master.eSettings.KeyboardShortcuts.GetKey("EditSelected") Then

---

## [↑](#table-of-contents) Testing Summary

| Status | Test Scenario |
|:------:|---------------|
| ⬜ | All existing shortcuts display in settings |
| ⬜ | Shortcuts can be modified and saved |
| ⬜ | Modified shortcuts work after restart |
| ⬜ | Reset to defaults restores original shortcuts |
| ⬜ | Duplicate shortcut detection/warning |
| ⬜ | Type-ahead search still functions |

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| [`EmberMediaManager\frmMain.vb`](../../../EmberMediaManager/frmMain.vb) | Main form with KeyDown/KeyPress handlers |
| [`EmberMediaManager\frmMain.Designer.vb`](../../../EmberMediaManager/frmMain.Designer.vb) | Menu shortcut definitions |
| [`EmberMediaManager\dlgSettings.vb`](../../../EmberMediaManager/dlgSettings.vb) | Settings dialog to add new panel |
| [`EmberAPI\clsAPISettings.vb`](../../../EmberAPI/clsAPISettings.vb) | Settings class to extend |

---

## [↑](#table-of-contents) Notes

- Consider using a library like `InputSimulator` for complex key capture
- Some shortcuts may be framework-controlled (Tab, Arrow keys) and not customizable
- The type-ahead search feature should remain functional regardless of shortcut changes
- May want to support multiple shortcut schemes (e.g., "Default", "Compact", "Custom")

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 10, 2026 | Created |

---

*End of file*