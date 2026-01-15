# BL-CC-002: Audit My.* Namespace Usage

| Field | Value |
|-------|-------|
| **ID** | BL-CC-002 |
| **Created** | January 14, 2026 |
| **Updated** | January 14, 2026 |
| **Category** | Code Cleanup (CC) |
| **Priority** | Low |
| **Effort** | 4-6 hours |
| **Status** | 📋 Open |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

Audit and document all usages of VB.NET `My.*` namespace features (`My.Computer`, `My.Settings`, `My.Application`, `My.Resources`) in preparation for .NET 8 migration. These features are not available in .NET Core/.NET 5+.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Audit Strategy](#audit-strategy)
- [Replacement Patterns](#replacement-patterns)
- [Implementation Details](#implementation-details)
- [Related Files](#related-files)
- [Notes](#notes)
- [Change History](#change-history)

---

## [↑](#table-of-contents) Problem Description

The `My` namespace is a VB.NET-specific feature that provides convenient shortcuts for common operations. However, these features are **not available** in .NET Core/.NET 5+ and must be replaced before migration.

### Features to Audit

| Feature | Common Usage | Available in .NET 8? |
|---------|--------------|---------------------|
| `My.Computer.FileSystem` | File/folder operations | ❌ No |
| `My.Computer.Network` | Network operations | ❌ No |
| `My.Settings` | Application settings | ❌ No |
| `My.Application` | Application info | ❌ No |
| `My.Resources` | Embedded resources | ⚠️ Limited |
| `My.User` | Current user info | ❌ No |

---

## [↑](#table-of-contents) Audit Strategy

### PowerShell Search Script

Run this to find all `My.*` usages:

    Get-ChildItem -Path "C:\Dev\Ember-MM-Newscraper" -Filter "*.vb" -Recurse | 
        Select-String -Pattern "My\.(Computer|Settings|Application|Resources|User)" |
        Select-Object Path, LineNumber, Line |
        Format-Table -AutoSize -Wrap

### Expected Results Categories

1. **My.Computer.FileSystem** — File operations
2. **My.Settings** — Persisted settings
3. **My.Application.Info** — Version, assembly info
4. **My.Resources** — Embedded images, strings

---

## [↑](#table-of-contents) Replacement Patterns

### My.Computer.FileSystem

| Original | Replacement |
|----------|-------------|
| `My.Computer.FileSystem.CopyFile(src, dest)` | `System.IO.File.Copy(src, dest)` |
| `My.Computer.FileSystem.DeleteFile(path)` | `System.IO.File.Delete(path)` |
| `My.Computer.FileSystem.FileExists(path)` | `System.IO.File.Exists(path)` |
| `My.Computer.FileSystem.CreateDirectory(path)` | `System.IO.Directory.CreateDirectory(path)` |
| `My.Computer.FileSystem.GetFiles(path)` | `System.IO.Directory.GetFiles(path)` |
| `My.Computer.FileSystem.ReadAllText(path)` | `System.IO.File.ReadAllText(path)` |
| `My.Computer.FileSystem.WriteAllText(path, text)` | `System.IO.File.WriteAllText(path, text)` |

### My.Settings

**Before:**

    My.Settings.LastUsedPath = somePath
    My.Settings.Save()
    Dim path = My.Settings.LastUsedPath

**After (Option 1 — JSON file):**

    Public Class AppSettings
        Public Property LastUsedPath As String
        
        Private Shared _instance As AppSettings
        Private Shared ReadOnly SettingsPath As String = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EmberMediaManager", "settings.json")
        
        Public Shared Function Load() As AppSettings
            If _instance Is Nothing Then
                If File.Exists(SettingsPath) Then
                    Dim json = File.ReadAllText(SettingsPath)
                    _instance = JsonConvert.DeserializeObject(Of AppSettings)(json)
                Else
                    _instance = New AppSettings()
                End If
            End If
            Return _instance
        End Function
        
        Public Sub Save()
            Dim json = JsonConvert.SerializeObject(Me, Formatting.Indented)
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath))
            File.WriteAllText(SettingsPath, json)
        End Sub
    End Class

### My.Application.Info

| Original | Replacement |
|----------|-------------|
| `My.Application.Info.Version` | `Assembly.GetExecutingAssembly().GetName().Version` |
| `My.Application.Info.ProductName` | `Assembly.GetExecutingAssembly().GetCustomAttribute(Of AssemblyProductAttribute)()?.Product` |
| `My.Application.Info.CompanyName` | `Assembly.GetExecutingAssembly().GetCustomAttribute(Of AssemblyCompanyAttribute)()?.Company` |

### My.Resources

| Original | Replacement |
|----------|-------------|
| `My.Resources.SomeImage` | Use `ResourceManager` or embedded resource stream |

**After:**

    Private Shared _resourceManager As ResourceManager
    
    Public Shared ReadOnly Property Resources As ResourceManager
        Get
            If _resourceManager Is Nothing Then
                _resourceManager = New ResourceManager("EmberMediaManager.Resources", Assembly.GetExecutingAssembly())
            End If
            Return _resourceManager
        End Get
    End Property
    
    ' Usage:
    Dim img = CType(Resources.GetObject("SomeImage"), Image)

---

## [↑](#table-of-contents) Implementation Details

### Approach

1. **Run audit script** to identify all usages
2. **Categorize by type** (FileSystem, Settings, etc.)
3. **Create helper class** with replacement methods (optional)
4. **Replace incrementally** — can be done before or during .NET 8 migration

### Helper Class (Optional)

Create a compatibility shim that works on both .NET Framework and .NET 8:

    Public Module MyCompat
        Public Function FileExists(path As String) As Boolean
            Return System.IO.File.Exists(path)
        End Function
        
        Public Sub CopyFile(source As String, dest As String)
            System.IO.File.Copy(source, dest, overwrite:=True)
        End Sub
        
        ' ... etc
    End Module

This allows search-and-replace of `My.Computer.FileSystem.` with `MyCompat.` as a first step.

---

## [↑](#table-of-contents) Related Files

| File | Relevance |
|------|-----------|
| `EmberAPI\clsCommon.vb` | Likely contains `My.*` usage |
| `EmberMediaManager\My Project\Settings.settings` | Current settings definition |
| `EmberMediaManager\My Project\Settings.Designer.vb` | Auto-generated settings code |

---

## [↑](#table-of-contents) Notes

- This audit can be done **before** .NET 8 migration
- Replacements work on .NET Framework 4.8 as well
- Consider doing replacements incrementally to reduce migration risk

### Prerequisite For

- [BL-FR-005: Migrate to .NET 8 LTS](BL-FR-005-DotNet8Migration.md)

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 14, 2026 | Created |

---

*End of file*