# BL-CQ-006: Improve Logging Capabilities

| Field | Value |
|-------|-------|
| **ID** | BL-CQ-006 |
| **Created** | January 10, 2026 |
| **Priority** | Low |
| **Effort** | 4-6 hours |
| **Status** | 📋 Open |
| **Category** | Code Quality |
| **Related Files** | [`NLog.config`](../../../EmberAPI/NLog.config), [`clsAPISettings.vb`](../../../EmberAPI/clsAPISettings.vb), [`dlgSettings.vb`](../../../EmberMediaManager/dlgSettings.vb), [`dlgErrorViewer.vb`](../../../EmberMediaManager/dlgErrorViewer.vb) |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

Add user-configurable settings for logging level and retention, and enhance the log viewer to display actual log content. Currently all logging configuration is hardcoded with Trace level always enabled.

**Analysis Document:** [LoggingSystemAnalysis.md](../../analysis-docs/LoggingSystemAnalysis.md)

---

## Table of Contents

- [Problem Description](#problem-description)
- [Solution Summary](#solution-summary)
- [Implementation Details](#implementation-details)
- [Testing Summary](#testing-summary)
- [Related Files](#related-files)
- [Notes](#notes)

---

## [↑](#table-of-contents) Problem Description

The current logging system has several limitations:

1. **No user control over log level** - Trace level is always enabled, generating verbose output
2. **No control over retention** - Hardcoded to 30 days
3. **No log viewer** - `dlgErrorViewer` only shows assembly versions, not actual logs
4. **Large log files** - Trace-level logging creates unnecessarily large files

Users cannot:
- Reduce logging verbosity for production use
- Increase logging detail for troubleshooting
- View logs without navigating to the Log folder
- Control disk space usage from log retention

---

## [↑](#table-of-contents) Solution Summary

### Phase 1: Add Settings (2-3 hours)

1. Add `GeneralLogLevel` property to settings (default: "Info")
2. Add `GeneralLogRetentionDays` property to settings (default: 30)
3. Add UI controls in dlgSettings under General section

### Phase 2: Dynamic Configuration (1-2 hours)

1. Apply log level setting to NLog configuration on startup
2. Update archive settings based on retention days

### Phase 3: Enhanced Log Viewer (2-3 hours)

1. Update `dlgErrorViewer` to read and display log CSV files
2. Add level filtering and search

---

## [↑](#table-of-contents) Implementation Details

### Settings Class Changes

**File:** [`EmberAPI\clsAPISettings.vb`](../../../EmberAPI/clsAPISettings.vb)

Add properties:

    Public Property GeneralLogLevel As String = "Info"
    Public Property GeneralLogRetentionDays As Integer = 30

### Settings Dialog Changes

**File:** [`EmberMediaManager\dlgSettings.vb`](../../../EmberMediaManager/dlgSettings.vb)

Add to General settings panel:
- ComboBox for log level (Trace, Debug, Info, Warn, Error, Fatal)
- NumericUpDown for retention days (1-365)

### Apply Settings on Startup

**File:** [`EmberMediaManager\ApplicationEvents.vb`](../../../EmberMediaManager/ApplicationEvents.vb)

In `MyApplication_Startup`:

    ' Apply log level from settings
    Dim config = LogManager.Configuration
    For Each rule In config.LoggingRules
        If rule.LoggerNamePattern = "*" Then
            rule.SetLoggingLevels(
                LogLevel.FromString(Master.eSettings.GeneralLogLevel),
                LogLevel.Fatal)
        End If
    Next
    LogManager.ReconfigExistingLoggers()

### Enhanced Log Viewer

**File:** [`EmberMediaManager\dlgErrorViewer.vb`](../../../EmberMediaManager/dlgErrorViewer.vb)

Replace assembly version display with:
- DataGridView showing log entries
- ComboBox filter for log level
- DateTimePicker for date selection
- TextBox for search

---

## [↑](#table-of-contents) Testing Summary

| Status | Test Scenario |
|:------:|---------------|
| ⬜ | Log level setting saves and loads correctly |
| ⬜ | Changing log level to Error reduces log output |
| ⬜ | Changing log level to Trace increases log output |
| ⬜ | Retention days setting affects archive cleanup |
| ⬜ | Log viewer displays current day's log file |
| ⬜ | Log viewer filtering by level works |
| ⬜ | Log viewer search finds matching entries |
| ⬜ | Settings persist across application restart |

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| [`EmberAPI\NLog.config`](../../../EmberAPI/NLog.config) | NLog configuration (reference only, modified programmatically) |
| [`EmberAPI\clsAPISettings.vb`](../../../EmberAPI/clsAPISettings.vb) | Add LogLevel and RetentionDays properties |
| [`EmberMediaManager\dlgSettings.vb`](../../../EmberMediaManager/dlgSettings.vb) | Add log settings UI |
| [`EmberMediaManager\dlgSettings.Designer.vb`](../../../EmberMediaManager/dlgSettings.Designer.vb) | Designer for new controls |
| [`EmberMediaManager\dlgErrorViewer.vb`](../../../EmberMediaManager/dlgErrorViewer.vb) | Enhance to show actual logs |
| [`EmberMediaManager\ApplicationEvents.vb`](../../../EmberMediaManager/ApplicationEvents.vb) | Apply log level on startup |

**Analysis:** [LoggingSystemAnalysis.md](../../analysis-docs/LoggingSystemAnalysis.md)

---

## [↑](#table-of-contents) Notes

- NLog supports runtime configuration changes via `LogManager.Configuration`
- The CSV format makes log files easy to parse with DataGridView
- Consider adding a "Open Log Folder" button for quick access
- Phase 3 (enhanced viewer) could be deferred if time is limited

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 10, 2026 | Created |

---

*End of file*