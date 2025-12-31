# Ember Media Manager - Localization System

## Overview

Ember Media Manager uses an XML-based localization system that provides multilingual support throughout the application. The system is built around numeric string IDs mapped to localized text values, with a fallback mechanism to ensure the UI never displays blank text.

## File Structure

### Language Files Location

    EmberMediaManager\Langs\
    ├── English_(en_US).xml      # Primary English language file
    ├── Debug_(db_DB).xml        # Debug file for development
    ├── German_(de_DE).xml       # German translation (example)
    ├── Languages.xml            # Master list of available languages with ISO codes
    └── [Other language files]

### File Naming Convention

Language files follow the pattern: `{LanguageName}_({locale}).xml`

Examples:
- `English_(en_US).xml`
- `German_(de_DE).xml`
- `Debug_(db_DB).xml`

## Core Components

### 1. Localization Class (`clsAPILocalization.vb`)

The `Localization` class in `EmberAPI\clsAPILocalization.vb` handles all language operations:

- **LoadAllLanguage(language)** - Loads the specified language file at startup
- **LoadLanguage(language)** - Loads a specific language for a module
- **GetString(ID, default)** - Retrieves a localized string by ID

### 2. Master.eLang

The global localization instance accessible throughout the application:

    Master.eLang.GetString(ID, "Fallback text")

### 3. Language File Structure

    <?xml version="1.0" encoding="utf-8"?>
    <strings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
             xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <string id="1">&amp;File</string>
      <string id="2">E&amp;xit</string>
      <string id="3">&amp;Edit</string>
      <!-- ... more strings ... -->
    </strings>

## How the System Works

### Initialization Flow

1. Application starts in `ApplicationEvents.vb`
2. User's language preference is loaded from settings (`Master.eSettings.GeneralLanguage`)
3. `Master.eLang.LoadAllLanguage()` loads the corresponding XML file
4. Strings are parsed and cached in memory (`htArrayStrings`)

### String Retrieval Process

    ' Code example
    Dim message As String = Master.eLang.GetString(132, "Scraping Media (Movies Missing Items - Auto):")

The `GetString` method:

1. Looks up the string by numeric ID in the loaded language
2. If found and not empty → returns the localized text
3. If not found or empty → returns the fallback text
4. Logs a warning if the ID is missing (for development tracking)

### Fallback Mechanism

The fallback system ensures the UI always displays meaningful text:

    Public Function GetString(ByVal ID As Integer, ByVal strDefault As String) As String
        ' Lookup ID in loaded language strings
        x1 = From x As LanguageString In htStrings.string Where (x.id = ID)
        
        If x1.Count = 0 Then
            ' ID not found - log warning and return default
            lang_logger.Warn("Missing language_string: {0} - {1} : '{2}'", Assembly, ID, strDefault)
            Return strDefault
        Else
            ' Return localized value or default if empty
            If Not String.IsNullOrEmpty(x1(0).Value) Then
                Return x1(0).Value
            Else
                Return strDefault
            End If
        End If
    End Function

## Language Files Comparison

### English File (`English_(en_US).xml`)

- Contains actual English text for each string ID
- Current highest ID: **1492**
- Missing ID: **1414** (gap in sequence)
- Some IDs have empty values (placeholders for future use)

Example:
    <string id="1400">Add folder as a new movie source</string>
    <string id="1401">Add folder as a new tv show source</string>

### Debug File (`Debug_(db_DB).xml`)

- Contains the ID number as the value for each string
- Current highest ID: **1713** (pre-allocated for future strings)
- Used for development and debugging

Example:
    <string id="1400">1400</string>
    <string id="1401">1401</string>

**Purpose of Debug File:**

When running with Debug language selected:
- UI displays the numeric ID instead of text
- Developers can instantly identify which string ID corresponds to each UI element
- Helps locate missing translations or incorrect ID usage

## Adding New Strings

### Step 1: Choose an Unused ID

- Check the English file for the highest used ID (currently 1492)
- Use the next available ID (1493+)
- Avoid gaps - keep IDs sequential when possible

### Step 2: Add to English Language File

Add the new string to `English_(en_US).xml`:

    <string id="1493">Scraping Media (All TV Shows - Ask):</string>
    <string id="1494">Scraping Media (All TV Shows - Auto):</string>

### Step 3: Use in Code

Reference the string with `GetString`:

    tslLoading.Text = Master.eLang.GetString(1493, "Scraping Media (All TV Shows - Ask):")

**Important:** Always provide meaningful fallback text that matches the intended English text.

### Step 4: Update Debug File (Optional)

The Debug file typically has IDs pre-allocated. If your new ID exceeds the Debug file's range, add entries:

    <string id="1493">1493</string>
    <string id="1494">1494</string>

## Module-Specific Languages

Addon modules have their own language files stored in:

    Modules\Langs\{ModuleName}.{Language}.xml

The `LoadLanguage` method handles module-specific loading:

    ' For main application
    lPath = [AppPath]\Langs\{Language}.xml
    
    ' For modules
    lPath = [AppPath]\Modules\Langs\{Assembly}.{Language}.xml

## Common String IDs Reference

| ID Range | Category |
|----------|----------|
| 1-100 | Menu items, basic UI |
| 100-200 | Dialog messages, warnings |
| 200-400 | Settings, options |
| 400-600 | Scraper-related messages |
| 600-800 | TV Show specific |
| 800-1000 | Advanced features |
| 1000-1200 | Additional features |
| 1200-1400 | MovieSet, extended features |
| 1400-1500 | Context menu, recent additions |
| 1493+ | Available for new strings |

## Frequently Used Strings

| ID | Text | Usage |
|----|------|-------|
| 19 | Close | Dialog close button |
| 167 | Cancel | Cancel button |
| 179 | OK | OK button |
| 276 | Apply | Apply button |
| 569 | All | All items filter |
| 570 | [none] | No selection |
| 571 | [Disabled] | Disabled state |
| 1171 | Abort | Abort button |
| 1228 | Skip | Skip button |

## XML Special Characters

When adding strings with special characters, use XML entities:

| Character | Entity |
|-----------|--------|
| & | `&amp;` |
| < | `&lt;` |
| > | `&gt;` |
| " | `&quot;` |
| ' | `&apos;` |
| Newline | `{0}` (format placeholder) |

Example:
    <string id="1">&amp;File</string>  <!-- Displays as "&File" with F underlined -->

## Best Practices

### For Developers

1. **Always use GetString** - Never hardcode user-visible text
2. **Provide meaningful fallbacks** - Fallback text should be the intended English text
3. **Keep IDs sequential** - Avoid gaps in ID numbering
4. **Document new IDs** - Add comments for context when adding many strings
5. **Use format placeholders** - Use `{0}`, `{1}`, etc. for dynamic content

### For Translators

1. **Preserve placeholders** - Keep `{0}`, `{1}` placeholders intact
2. **Match string length** - Keep translations reasonably close in length to English
3. **Preserve XML entities** - Keep `&amp;` for ampersands, etc.
4. **Test in context** - Verify translations fit in the UI

## Troubleshooting

### String Shows as Number

- You're running in Debug language mode
- Switch to English or another language in settings

### String Shows Fallback Text

- The ID doesn't exist in the loaded language file
- Check the log file for "Missing language_string" warnings
- Add the string to the language file

### String is Blank

- The ID exists but has an empty value in the language file
- Add the text value to the language file

### Duplicate IDs

The Debug file has known duplicates that should be cleaned up:
- ID 1165 (appears twice)
- ID 1265 (appears twice)
- ID 1365 (appears twice)
- ID 1399 (appears twice)

## File Locations Summary

| File | Path | Purpose |
|------|------|---------|
| Localization Class | `EmberAPI\clsAPILocalization.vb` | Core localization logic |
| English Strings | `EmberMediaManager\Langs\English_(en_US).xml` | Primary language file |
| Debug Strings | `EmberMediaManager\Langs\Debug_(db_DB).xml` | Development debugging |
| Languages List | `EmberMediaManager\Langs\Languages.xml` | Available languages |
| Module Languages | `Modules\Langs\*.xml` | Addon-specific strings |

---

*Last Updated: December 2024*
*Current String ID Range: 1-1492 (English), 1-1713 (Debug)*
*Next Available ID: 1493*