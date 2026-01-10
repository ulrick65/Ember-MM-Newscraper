# Logging System Analysis

| Document Info | |
|---------------|---|
| **Version** | 1.0 |
| **Created** | January 10, 2026 |
| **Updated** | January 10, 2026 |
| **Author** | Eric H. Anderson |
| **Purpose** | Analysis of current NLog implementation and improvement opportunities |

##### [← Return to Document Index](../DocumentIndex.md)

---

## Overview

This document analyzes the current logging system in Ember Media Manager to understand its architecture, configuration, and identify opportunities for improvement. The application uses NLog 6.0.7 for all logging functionality.

---

## Table of Contents

- [Current Implementation](#current-implementation)
- [Configuration Details](#configuration-details)
- [Log Targets](#log-targets)
- [Logging Levels](#logging-levels)
- [Usage Patterns](#usage-patterns)
- [Log File Format](#log-file-format)
- [Current Limitations](#current-limitations)
- [Improvement Opportunities](#improvement-opportunities)
- [Related Files](#related-files)

---

## [↑](#table-of-contents) Current Implementation

### Framework

- **Library:** NLog 6.0.7
- **Configuration:** XML-based (`EmberAPI\NLog.config`)
- **Shared:** Configuration is centralized and shared across all projects

### Key Features

- Async logging with queue limits
- CSV format for easy parsing
- Automatic archiving (30-day retention)
- Separate log files for language and help string errors
- Visual Studio debugger output integration

---

## [↑](#table-of-contents) Configuration Details

**Location:** [`EmberAPI\NLog.config`](../../EmberAPI/NLog.config)

### Global Settings

    <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
          autoReload="true" 
          throwExceptions="false"
          internalLogLevel="Error"
          internalLogFile="${basedir}/Log/nlog-internal.log">

| Setting | Value | Purpose |
|---------|-------|---------|
| `autoReload` | true | Config changes apply without restart |
| `throwExceptions` | false | Logging failures don't crash app |
| `internalLogLevel` | Error | NLog's own logging (troubleshooting) |
| `internalLogFile` | Log/nlog-internal.log | NLog internal error log |

### Variables

    <variable name="logDirectory" value="${basedir}/Log"/>

---

## [↑](#table-of-contents) Log Targets

### Main Application Log (AWf)

    <target name="AWf" xsi:type="AsyncWrapper" queueLimit="10000" overflowAction="Discard">

| Property | Value |
|----------|-------|
| Output | `Log/{shortdate}.csv` |
| Archive | `Log/archive/{shortdate}.{#}.csv` |
| Retention | 30 days |
| Queue Limit | 10,000 messages |
| Concurrent Writes | Enabled |

### Language String Errors (AWf_lang)

| Property | Value |
|----------|-------|
| Output | `Log/language_{shortdate}.csv` |
| Queue Limit | 5,000 messages |
| Purpose | Captures missing/invalid language strings |

### Help String Errors (AWf_help)

| Property | Value |
|----------|-------|
| Output | `Log/help_{shortdate}.csv` |
| Queue Limit | 5,000 messages |
| Purpose | Captures missing/invalid help strings |

### Visual Studio Debugger (VSDebugger)

| Property | Value |
|----------|-------|
| Type | Debugger |
| Format | Tab-delimited CSV |
| Purpose | Real-time debugging in VS Output window |

---

## [↑](#table-of-contents) Logging Levels

NLog supports six logging levels (lowest to highest severity):

| Level | Typical Usage |
|-------|---------------|
| **Trace** | Most verbose, detailed diagnostics |
| **Debug** | Debugging information |
| **Info** | Informational messages |
| **Warn** | Warning conditions |
| **Error** | Error conditions |
| **Fatal** | Critical errors |

### Current Rules

    <rules>
        <!-- Language string errors - separate log file -->
        <logger name="LanguageString" minlevel="Error" writeTo="AWf_lang" final="true" />
        
        <!-- Help string errors - separate log file -->
        <logger name="HelpString" minlevel="Error" writeTo="AWf_help" final="true"/>
        
        <!-- Ignore non-error LanguageString and HelpString messages -->
        <logger name="LanguageString" maxlevel="Warn" final="true" />
        <logger name="HelpString" maxlevel="Warn" final="true"/>
        
        <!-- All other logs go to main file and debugger -->
        <logger name="*" minlevel="Trace" writeTo="AWf,VSDebugger" />
    </rules>

**⚠️ Issue:** The catch-all rule uses `minlevel="Trace"`, which captures ALL log messages regardless of severity. This generates verbose output and large log files.

---

## [↑](#table-of-contents) Usage Patterns

### Standard Logger Declaration

Every class that logs declares a shared logger:

    Shared _Logger As Logger = LogManager.GetCurrentClassLogger()

Or:

    Shared logger As Logger = LogManager.GetCurrentClassLogger()

### Common Logging Calls

**Error with exception and method name:**

    logger.Error(ex, New StackFrame().GetMethod().Name)

**Informational message:**

    logger.Info("====Ember Media Manager exiting====")

**Warning with exception:**

    logger.Warn(ex, "Failed to export performance metrics to CSV")

**Trace level (verbose):**

    logger.Trace("Detailed diagnostic information")

### Exception Handling Pattern

    Try
        ' Operation
    Catch ex As Exception
        logger.Error(ex, New StackFrame().GetMethod().Name)
    End Try

---

## [↑](#table-of-contents) Log File Format

Logs are stored in CSV format with the following columns:

| Column | Layout | Example |
|--------|--------|---------|
| time | `${longdate}` | 2026-01-10 14:30:45.1234 |
| logger | `${logger}` | EmberAPI.Database |
| callsite | `${callsite}` | Save_Movie |
| threadid | `${threadid}` | 12 |
| level | `${uppercase:${level}}` | ERROR |
| message | `${message}` | Failed to save movie |
| exception | `${exception:format=tostring}` | System.IO.IOException... |

---

## [↑](#table-of-contents) Current Limitations

| Issue | Impact | Severity |
|-------|--------|----------|
| No UI settings for log level | Users cannot reduce verbosity | Medium |
| Trace level always enabled | Large log files, performance impact | Medium |
| No log viewer in application | Users must find CSV files manually | Low |
| Hardcoded 30-day retention | No user control over disk usage | Low |
| dlgErrorViewer shows assembly versions only | Doesn't display actual log content | Low |

---

## [↑](#table-of-contents) Improvement Opportunities

### Phase 1: Add Settings (2-3 hours)

1. Add `LogLevel` property to `Master.eSettings`
2. Add `LogRetentionDays` property to `Master.eSettings`
3. Add UI controls in dlgSettings (dropdown for level, number input for days)

### Phase 2: Dynamic Configuration (1-2 hours)

1. Modify NLog configuration programmatically on startup
2. Apply user's log level preference to the catch-all rule
3. Update archive settings based on retention preference

### Phase 3: Enhanced Log Viewer (2-3 hours)

1. Update `dlgErrorViewer` to read and display actual log files
2. Add filtering by level, date, logger
3. Add search functionality

### Recommended Settings Class Addition

    ' In clsAPISettings.vb
    Public Property GeneralLogLevel As String = "Info"  ' Trace, Debug, Info, Warn, Error, Fatal
    Public Property GeneralLogRetentionDays As Integer = 30

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| [`EmberAPI\NLog.config`](../../../EmberAPI/NLog.config) | Central NLog configuration |
| [`EmberMediaManager\dlgErrorViewer.vb`](../../dlgErrorViewer.vb) | Error log viewer dialog (assembly versions only) |
| [`EmberMediaManager\ApplicationEvents.vb`](../../ApplicationEvents.vb) | Startup/shutdown logging, unhandled exceptions |
| [`EmberAPI\clsAPISettings.vb`](../../../EmberAPI/clsAPISettings.vb) | Settings class (needs LogLevel property) |
| [`EmberMediaManager\dlgSettings.vb`](../../dlgSettings.vb) | Settings dialog (needs log level UI) |

---

*End of file*