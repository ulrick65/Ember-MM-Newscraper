# BL-CQ-002: Null Check Audit for .Count Calls

| Field | Value |
|-------|-------|
| **ID** | BL-CQ-002 |
| **Created** | January 2, 2026 |
| **Priority** | High |
| **Effort** | 2-3 hours |
| **Status** | Open |
| **Category** | Code Quality (CQ) |
| **Related Files** | Multiple - see audit below |

---

## Problem

Multiple places in the codebase call `.Count` on collections without first checking if the collection is `Nothing`. This causes `ArgumentNullException` when the collection is null.

### Example (Fixed)

**File:** `EmberMediaManager\frmMain.vb` - `MoveGenres` method

Before:
    
    For i As Integer = 0 To pnlGenre.Count - 1

After (fixed Jan 2, 2026):

    If pnlGenre Is Nothing OrElse pnlGenre.Count = 0 Then Exit Sub
    
    For i As Integer = 0 To pnlGenre.Count - 1

---

## Audit Required

Search for patterns like:

    .Count - 1
    .Count > 0
    .Count = 0
    .Count()

And verify null checks exist before the call.

### PowerShell Search Script

    Get-ChildItem -Path . -Filter "*.vb" -Recurse | 
        Select-String -Pattern "\.Count\s*[->=<]" |
        Select-Object Path, LineNumber, Line

---

## Fix Pattern

### For Arrays

    If arrayVar Is Nothing OrElse arrayVar.Count = 0 Then Exit Sub
    ' OR
    If arrayVar Is Nothing OrElse arrayVar.Length = 0 Then Exit Sub

### For Lists/Collections

    If listVar Is Nothing OrElse listVar.Count = 0 Then Exit Sub
    ' OR using LINQ
    If listVar Is Nothing OrElse Not listVar.Any() Then Exit Sub

### For Iteration

    If collection IsNot Nothing Then
        For Each item In collection
            ' ...
        Next
    End If

---

## Known Occurrences

| File | Method | Status |
|------|--------|--------|
| frmMain.vb | MoveGenres | ✅ Fixed Jan 2, 2026 |
| (audit needed) | | |

---

## Testing

After fixes, run full scraping session and monitor logs for `ArgumentNullException` or similar null reference errors.

---

## Notes

- This is defensive programming - the app may work most of the time but fail on edge cases
- Particularly important for arrays initialized as `Nothing` (like `pnlGenre() As Panel = Nothing`)
- Consider using `?.` null-conditional operator where appropriate (VB 14+)

---

*Created: January 2, 2026*