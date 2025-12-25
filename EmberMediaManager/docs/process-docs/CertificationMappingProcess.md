# Certification Mapping System in Ember Media Manager

The **Certification Mapping** feature in Ember Media Manager provides a simple one-to-one mapping system that standardizes certification/rating names from various scraper sources into a consistent format.

## Overview

The Certification Mapping system solves the problem of inconsistent certification names across different scraper sources. For example, different scrapers might return "PG-13", "USA:PG-13", "Rated PG-13", or "PG13" for the same rating. This system automatically translates these variations into your preferred standard format.

## Core Components

### 1. Data Model (clsXMLSimpleMapping.vb)

The system uses a simple mapping structure located in `EmberAPI\XML Serialization\clsXMLSimpleMapping.vb`:

#### clsXMLSimpleMapping Class
The main container that manages certification mappings:
- **Mappings**: `List(Of SimpleMapping)` - List of input-to-output mapping rules
- **FileNameFullPath**: `String` - Path to the XML file storing mappings

Key methods:
- `Load()` - Loads mappings from XML file
- `Save()` - Persists mappings to XML file
- `RunMapping(ByRef listToBeMapped As List(Of String), Optional addNewInputs As Boolean = True)` - Core mapping logic for lists
- `RunMapping(ByRef singleString As String, Optional addNewInputs As Boolean = True)` - Core mapping logic for single strings
- `Sort()` - Sorts mappings alphabetically
- `CloneDeep()` - Creates a deep copy for editing without affecting live data

#### SimpleMapping Class
Represents a single mapping rule:
- **Input**: `String` - The certification string from scrapers (e.g., "USA:PG-13")
- **MappedTo**: `String` - The target certification string (e.g., "PG-13")

### 2. User Interface (dlgSimpleMapping.vb)

Located in `Addons\generic.EmberCore.Mapping\dlgSimpleMapping.vb`, this Windows Forms dialog provides a simple grid-based editor:

#### Single Panel Interface
- **DataGridView**: Two-column grid with "Input" and "Mapped to" columns
- **Operations**:
  - Add new mappings directly in grid
  - Edit existing mappings inline
  - Delete mappings by removing rows
  - All changes saved when clicking OK

#### Features
- Simple, straightforward interface
- Direct editing in grid cells
- Automatic sorting when saved
- No visual indicators for new entries (unlike Genre Mapping)

### 3. Menu Integration (genericMapping.vb)

The certification mapping editor is accessible through:
- Main menu: Tools → Certification Mapping
- System tray context menu: Tools → Certification Mapping

After closing the dialog with OK:
- Saves changes to XML file
- Triggers database cleanup to standardize existing certifications
- Shows count of items changed in database

## How the Mapping Process Works

### During Movie Scraping

When scrapers retrieve certification data, the mapping process is triggered automatically:

#### For Movies (clsAPINFO.vb - MergeDataScraperResults_Movie)

Located around line 122, the process is:

1. **Check conditions**: Verify certification scraping is enabled and not locked
2. **Apply language filter**: If certification language limit is set, filter to that language
3. **Apply mapping**: Call `APIXML.CertificationMapping.RunMapping(DBMovie.Movie.Certifications)`
4. **Assign to movie**: Store the result in database

The actual code flow is:
- Check if certification field is not locked (`Not Master.eSettings.MovieLockCert`)
- Check if scraping certifications is enabled (`Master.eSettings.MovieScraperCert`)
- Check if this is the first scraper result with certifications (`Not new_Certification`)
- Apply language filter if configured (e.g., only USA certifications)
- Run the mapping transformation
- Store the result

### The RunMapping Algorithm

The `RunMapping(ByRef listToBeMapped As List(Of String), Optional addNewInputs As Boolean = True)` method processes certification lists:

#### Step 1: Process Each Input Certification
For each certification string in the input list:

1. **Look for existing mapping**: Search in `Mappings` list for matching `Input`
2. **If mapping found**: Add `MappedTo` value to result list
3. **If no mapping found and addNewInputs is True**:
   - Add original certification to result (no modification)
   - Create new `SimpleMapping` with Input = MappedTo (1:1 mapping)
   - Mark for saving

#### Step 2: Cleanup and Sort
If any new items were added:
- Sort `Mappings` list alphabetically by Input
- Call `Save()` to persist changes immediately

#### Step 3: Finalize Result
- Remove duplicates using `Distinct()`
- Sort result list alphabetically
- Sort original input list for comparison

#### Step 4: Determine If Changes Occurred
- Compare input list with result list using `SequenceEqual()`
- Replace input list with result list (passed by reference)
- Return `True` if lists differ, `False` if identical

### Example Mapping Flow

**Scenario**: TMDB scraper returns US certification for a PG-13 movie

**Input from scraper**: `["USA:PG-13"]`

**Existing mappings**:
- "USA:PG-13" → "PG-13"
- "USA:R" → "R"

**Step-by-step process**:

1. Process "USA:PG-13":
   - Found in mappings
   - Add "PG-13" to result
   - Result: `["PG-13"]`

2. Cleanup and finalize:
   - No new mappings created
   - Result: `["PG-13"]`

**Final result**: Movie has certification "PG-13"

**Alternative scenario** - New certification:

**Input from scraper**: `["UK:15"]`

**No existing mapping**:

1. Process "UK:15":
   - NOT found in mappings
   - Add "UK:15" to result unchanged
   - Create mapping: "UK:15" → "UK:15"
   - Save immediately

2. User can later edit in Certification Mapping dialog:
   - Change "UK:15" → "UK:15" to "15" or preferred format

### Database Cleanup Process

When certifications are modified or mappings change, the cleanup process ensures consistency:

**Cleanup trigger**:
- Executes when user clicks OK in Certification Mapping dialog
- Runs in background worker (`bwCleanDatabase`)
- Shows progress bar during operation

**Cleanup method** (`Master.DB.Cleanup_Certifications()`):
1. Retrieves all certifications currently assigned to movies in database
2. Applies current mapping rules to all certifications
3. Updates movies with newly mapped certification values
4. Returns count of items changed

**User feedback**:
- Progress bar shows operation is running
- Message box displays: "X item(s) changed"
- Database immediately reflects new mapping configuration

## Data Persistence (XML Structure)

All mappings are stored in `Core.Mapping.Certifications.xml` in the settings directory.

**XML structure example**:

`<simplemapping>`
`  <mapping input="USA:G">G</mapping>`
`  <mapping input="USA:PG">PG</mapping>`
`  <mapping input="USA:PG-13">PG-13</mapping>`
`  <mapping input="USA:R">R</mapping>`
`  <mapping input="USA:NC-17">NC-17</mapping>`
`  <mapping input="UK:U">U</mapping>`
`  <mapping input="UK:PG">PG</mapping>`
`  <mapping input="UK:12">12</mapping>`
`  <mapping input="UK:15">15</mapping>`
`  <mapping input="UK:18">18</mapping>`
`  <mapping input="Rated PG-13">PG-13</mapping>`
`  <mapping input="Rated R">R</mapping>`
`</simplemapping>`

**Load process**:
- Called during application initialization in `clsAPIXML.Init()`
- Deserializes XML into `clsXMLSimpleMapping` object
- Handles missing files by creating empty mapping
- Creates backup if XML is corrupted

**Save process**:
- Called after UI changes in dialog
- Called automatically when new mappings are auto-created
- Sorts all data alphabetically before serialization
- Overwrites existing file atomically

## Common Use Cases and Examples

### Use Case 1: Remove Country Prefixes

**Problem**: Scrapers return certifications with country codes

**Solution**: Map country-prefixed certifications to simple format

**Example mappings**:
- "USA:G" → "G"
- "USA:PG" → "PG"
- "USA:PG-13" → "PG-13"
- "USA:R" → "R"
- "USA:NC-17" → "NC-17"

**Result**: Clean certification names without country prefixes

### Use Case 2: Standardize Text Variations

**Problem**: Different scrapers use different text formats

**Solution**: Map all variations to single standard format

**Example mappings**:
- "Rated PG-13" → "PG-13"
- "Rated R" → "R"
- "Not Rated" → "Unrated"
- "NR" → "Unrated"

**Result**: Consistent certification format across all movies

### Use Case 3: International Certification Translation

**Problem**: Want to show local country certifications in different format

**Solution**: Map international certifications to preferred display format

**Example mappings**:
- "UK:U" → "Universal (UK)"
- "UK:PG" → "Parental Guidance (UK)"
- "UK:12" → "12+ (UK)"
- "UK:15" → "15+ (UK)"
- "UK:18" → "18+ (UK)"
- "Germany:6" → "FSK 6"
- "Germany:12" → "FSK 12"
- "Germany:16" → "FSK 16"

**Result**: More descriptive certification names for international content

### Use Case 4: Consolidate Similar Ratings

**Problem**: Multiple certifications that mean the same thing

**Solution**: Map similar certifications to single value

**Example mappings**:
- "TV-14" → "PG-13"
- "TV-MA" → "R"
- "M" → "PG-13"
- "GP" → "PG"

**Result**: Simplified certification vocabulary

### Use Case 5: Remove Unwanted Certifications

**Problem**: Scrapers return certifications you don't want to display

**Solution**: Map unwanted certifications to empty string or preferred default

**Example mappings**:
- "Unknown" → ""
- "Not Yet Rated" → ""
- "Unrated/Extended" → "Unrated"

**Result**: Clean database without placeholder certification values

## Integration with Other Systems

### Scraper Integration

All data scrapers automatically use the certification mapping system:

**Movie scrapers**:
- TMDB (scraper.TMDB.Data) - Returns certifications with country prefixes
- IMDB (scraper.IMDB.Data) - Returns various certification formats
- OFDB (scraper.OFDB.Data) - Returns German FSK ratings
- OMDb (scraper.Data.OMDb) - Returns "Rated X" format

**How scrapers work**:
1. Scraper extracts raw certification strings from source
2. Adds to `scrapedmovie.Certifications` list
3. Core API applies language filter if configured
4. Core API calls `RunMapping()` automatically
5. Mapped certifications stored in database

### Certification Language Filter

Before mapping is applied, an optional language filter can be configured:

**Setting**: `Master.eSettings.MovieScraperCertLang`

**Options**:
- Specific country (e.g., "USA", "UK", "DE")
- "All" - No filtering, keep all certifications

**Process**:
1. Check if certification starts with configured country name
2. Keep only matching certifications
3. Apply mapping to filtered list

**Example**:
- Input: `["USA:PG-13", "UK:12", "Germany:12"]`
- Filter: "USA"
- After filter: `["USA:PG-13"]`
- After mapping: `["PG-13"]`

### Export and Interface Integration

Certification mappings affect exported data and external interfaces:

**Movie Export** (generic.EmberCore.MovieExport):
- HTML templates use mapped certification names
- Ensures consistent display in exported pages

**Kodi Interface** (generic.Interface.Kodi):
- Mapped certifications synchronized to Kodi library
- Kodi displays standardized certification values

**Trakt.tv Interface** (generic.Interface.Trakttv):
- Uses mapped certifications when syncing to Trakt

## Technical Implementation Details

### Memory Management

**Clone for editing**:
- Dialog operates on live data, not clone (unlike Genre Mapping)
- Changes committed to XML when OK button clicked
- Cancel button discards changes by not saving

**Simplicity**:
- No complex nested objects
- Simple list of input/output pairs
- Minimal memory footprint

### UI Implementation

**Direct grid editing**:
- DataGridView bound directly to data
- New rows created by typing in bottom empty row
- Rows deleted by removing from grid
- All edits reflected immediately in UI

**No confirmation workflow**:
- Unlike Genre Mapping, no "isNew" flags
- No visual indicators for new entries
- Simpler workflow for straightforward mappings

### Performance Considerations

**Sorting**:
- Mappings sorted alphabetically by Input
- Makes manual searching easier
- Implemented via `IComparable(Of SimpleMapping)` interface

**Background processing**:
- Database cleanup runs in background worker
- Prevents UI freeze during large operations
- Shows progress bar and completion message

**String comparison**:
- Uses exact string matching
- Case-sensitive by default
- Fast lookup performance

## Comparison with Genre Mapping

**Similarities**:
- Both use automatic discovery of new values
- Both save immediately when new mappings created
- Both trigger database cleanup on save

**Differences**:
- Certification uses 1:1 mapping (one input → one output)
- Genre supports 1:many mapping (one input → multiple outputs)
- Certification has simpler UI (single grid)
- Genre has dual-panel UI with visual indicators
- Certification operates on strings
- Genre has additional features (images, confirmation workflow)

## Best Practices

**Regular maintenance**:
- Periodically review certification mappings
- Check for new auto-created entries
- Standardize format consistently

**Consistent formatting**:
- Decide on certification format (with or without prefixes)
- Apply format consistently across all mappings
- Document your certification standard

**Country-specific handling**:
- If managing international library, consider including country in output
- Example: "PG-13 (USA)" vs just "PG-13"
- Use certification language filter to prioritize preferred country

**Backup strategy**:
- XML file is automatically backed up by Ember
- Small file, easy to manually backup
- Can share mapping files between installations

## Troubleshooting

**Issue**: Certifications not mapping correctly
**Cause**: Exact string mismatch (case-sensitive)
**Solution**: Check exact format in database, add exact match to mappings

**Issue**: Multiple certifications showing for same movie
**Cause**: Language filter not applied, or multiple countries in source
**Solution**: Enable certification language filter in settings

**Issue**: Mappings not persisting
**Cause**: File permission error on XML file
**Solution**: Check write permissions on settings directory

**Issue**: Database cleanup not updating all movies
**Cause**: Movies may be locked or certification field locked
**Solution**: Check lock status in movie metadata

## Summary

The Certification Mapping system provides straightforward control over certification standardization:

**Key benefits**:
- Simple one-to-one mapping of certification strings
- Automatic discovery and 1:1 mapping of new certifications
- Easy-to-use grid-based editor
- Immediate database synchronization via cleanup process
- Portable configuration via XML
- Integration with certification language filter

**Typical workflow**:
1. Scrapers retrieve raw certification data with country prefixes
2. Optional language filter applied
3. Mapping system automatically translates to standard format
4. New certifications auto-mapped 1:1 (e.g., "UK:15" → "UK:15")
5. User periodically reviews and customizes mappings
6. Database cleanup applies mappings to existing movies
7. Library maintains consistent certification format

This system ensures that regardless of which scraper sources you use or what format they return certifications in, your media library maintains a clean and consistent certification display that matches your preferences.