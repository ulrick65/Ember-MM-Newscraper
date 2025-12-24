# Studio Mapping System in Ember Media Manager

The **Studio Mapping** feature in Ember Media Manager provides a simple one-to-one mapping system that standardizes studio/production company names from various scraper sources into a consistent format.

## Overview

The Studio Mapping system solves the problem of inconsistent studio names across different scraper sources. For example, different scrapers might return "20th Century Fox", "Twentieth Century Fox", "20th Century Studios", "Fox", or "20CF" for the same studio. This system automatically translates these variations into your preferred standard format, which is especially important for studio icon/logo display.

## Core Components

### 1. Data Model (clsXMLSimpleMapping.vb)

The system uses the same simple mapping structure as Certification and Country Mapping, located in `EmberAPI\XML Serialization\clsXMLSimpleMapping.vb`:

#### clsXMLSimpleMapping Class
The main container that manages studio mappings:
- **Mappings**: `List(Of SimpleMapping)` - List of input-to-output mapping rules
- **FileNameFullPath**: `String` - Path to the XML file storing mappings

Key methods:
- `Load()` - Loads mappings from XML file
- `Save()` - Persists mappings to XML file
- `RunMapping(ByRef listToBeMapped As List(Of String), Optional addNewInputs As Boolean = True)` - Core mapping logic for lists
- `Sort()` - Sorts mappings alphabetically
- `CloneDeep()` - Creates a deep copy for editing without affecting live data

#### SimpleMapping Class
Represents a single mapping rule:
- **Input**: `String` - The studio string from scrapers (e.g., "20th Century Fox")
- **MappedTo**: `String` - The target studio string (e.g., "20th Century Studios")

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
- Shared implementation with Certification, Country, and Status mappings

### 3. Menu Integration (genericMapping.vb)

The studio mapping editor is accessible through:
- Main menu: Tools → Studio Mapping
- System tray context menu: Tools → Studio Mapping

After closing the dialog with OK:
- Saves changes to XML file
- Triggers database cleanup to standardize existing studios
- Shows count of items changed in database

## How the Mapping Process Works

### During Movie Scraping

When scrapers retrieve studio data, the mapping process is triggered automatically:

#### For Movies (clsAPINFO.vb - MergeDataScraperResults_Movie)

Located around line 285, the process is:

1. **Check conditions**: Verify studio scraping is enabled and not locked
2. **Copy studios to temporary list**: Work with copy of scraped studios
3. **Apply mapping**: Call `APIXML.StudioMapping.RunMapping(_studios)`
4. **Filter by icon availability** (optional): If "WithImgOnly" setting enabled, remove studios without icons
5. **Apply count limit**: If studio limit is set, limit number of studios
6. **Assign to movie**: Store result in database

The actual code flow is:
- Check if studio field is not locked (`Not Master.eSettings.MovieLockStudio`)
- Check if scraping studios is enabled (`Master.eSettings.MovieScraperStudio`)
- Check if this is the first scraper result with studios (`Not new_Studio`)
- Create temporary list from scraped studios
- Run the mapping transformation
- Optionally filter to only studios with icons
- Apply count limit if configured
- Store the result if any studios remain

### The RunMapping Algorithm

The `RunMapping(ByRef listToBeMapped As List(Of String), Optional addNewInputs As Boolean = True)` method processes studio lists:

#### Step 1: Process Each Input Studio
For each studio string in the input list:

1. **Look for existing mapping**: Search in `Mappings` list for matching `Input`
2. **If mapping found**: Add `MappedTo` value to result list
3. **If no mapping found and addNewInputs is True**:
   - Add original studio to result (no modification)
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

**Scenario**: TMDB scraper returns studios for a major Hollywood production

**Input from scraper**: `["Twentieth Century Fox", "TSG Entertainment", "Lightstorm Entertainment"]`

**Existing mappings**:
- "Twentieth Century Fox" → "20th Century Studios"
- "TSG Entertainment" → "TSG Entertainment"

**Studio icon setting**: Enabled (only show studios with icons)

**Step-by-step process**:

1. Process "Twentieth Century Fox":
   - Found in mappings
   - Add "20th Century Studios" to result
   - Result: `["20th Century Studios"]`

2. Process "TSG Entertainment":
   - Found in mappings
   - Add "TSG Entertainment" to result
   - Result: `["20th Century Studios", "TSG Entertainment"]`

3. Process "Lightstorm Entertainment":
   - NOT found in mappings
   - Add "Lightstorm Entertainment" to result unchanged
   - Create mapping: "Lightstorm Entertainment" → "Lightstorm Entertainment"
   - Save immediately
   - Result: `["20th Century Studios", "TSG Entertainment", "Lightstorm Entertainment"]`

4. Filter by icon availability (if enabled):
   - Check `APIXML.StudioIcons` for "20th Century Studios" - Found
   - Check `APIXML.StudioIcons` for "TSG Entertainment" - Not found, remove
   - Check `APIXML.StudioIcons` for "Lightstorm Entertainment" - Not found, remove
   - Result: `["20th Century Studios"]`

**Final result**: Movie has studio "20th Century Studios" with icon displayed

### Studio-Specific Features

#### Icon Filtering

Studios can be filtered to only show those with available icon/logo images:

**Setting**: `Master.eSettings.MovieScraperStudioWithImgOnly`

**Icon storage**: `Images\Studios\` folder and subfolders

**Process**:
1. After mapping, check each studio against `APIXML.StudioIcons` dictionary
2. Dictionary keys are lowercase studio names
3. If studio not found in dictionary, remove from list
4. Only studios with icons remain

**Example**:
- Mapped studios: `["Warner Bros.", "Village Roadshow", "Unknown Studio"]`
- Icons available for: "Warner Bros.", "Village Roadshow"
- After filtering: `["Warner Bros.", "Village Roadshow"]`

#### Studio Count Limit

After mapping and optional icon filtering, a count limit can be applied:

**Setting**: `Master.eSettings.MovieScraperStudioLimit`

**Process**:
1. Apply `FilterCountLimit()` after mapping and icon filtering
2. Keeps only first N studios
3. Prevents overcrowding with too many studios

**Example**:
- Mapped studios: `["Warner Bros.", "Legendary", "DC Films", "RatPac Entertainment", "Atlas Entertainment"]`
- Limit: 3
- After limit: `["Warner Bros.", "Legendary", "DC Films"]`

### Database Cleanup Process

When studios are modified or mappings change, the cleanup process ensures consistency:

**Cleanup trigger**:
- Executes when user clicks OK in Studio Mapping dialog
- Runs in background worker (`bwCleanDatabase`)
- Shows progress bar during operation

**Cleanup method** (`Master.DB.Cleanup_Studios()`):
1. Retrieves all studios currently assigned to movies in database
2. Applies current mapping rules to all studios
3. Updates movies with newly mapped studio values
4. Removes duplicate entries
5. Returns count of items changed

**User feedback**:
- Progress bar shows operation is running
- Message box displays: "X item(s) changed"
- Database immediately reflects new mapping configuration

## Data Persistence (XML Structure)

All mappings are stored in `Core.Mapping.Studios.xml` in the settings directory.

**XML structure example**:

`<simplemapping>`
`  <mapping input="Twentieth Century Fox">20th Century Studios</mapping>`
`  <mapping input="20th Century Fox">20th Century Studios</mapping>`
`  <mapping input="Fox">20th Century Studios</mapping>`
`  <mapping input="Warner Bros. Pictures">Warner Bros.</mapping>`
`  <mapping input="Warner Brothers">Warner Bros.</mapping>`
`  <mapping input="WB">Warner Bros.</mapping>`
`  <mapping input="Universal Pictures">Universal</mapping>`
`  <mapping input="Universal Studios">Universal</mapping>`
`  <mapping input="Paramount Pictures">Paramount</mapping>`
`  <mapping input="Paramount Pictures Corporation">Paramount</mapping>`
`  <mapping input="Sony Pictures">Sony</mapping>`
`  <mapping input="Columbia Pictures">Columbia</mapping>`
`  <mapping input="Metro-Goldwyn-Mayer">MGM</mapping>`
`  <mapping input="MGM Studios">MGM</mapping>`
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

### Use Case 1: Standardize Major Studio Names

**Problem**: Major studios returned in various formats from different scrapers

**Solution**: Map all variations to single preferred studio name (matching icon filename)

**Example mappings**:
- "Twentieth Century Fox" → "20th Century Studios"
- "20th Century Fox" → "20th Century Studios"
- "Fox" → "20th Century Studios"
- "Warner Bros. Pictures" → "Warner Bros."
- "Warner Brothers" → "Warner Bros."
- "WB" → "Warner Bros."
- "Universal Pictures" → "Universal"
- "Universal Studios" → "Universal"

**Result**: Consistent studio names matching your icon collection

### Use Case 2: Map to Icon Filenames

**Problem**: Need studio names to exactly match icon filenames for display

**Solution**: Map all studio name variations to match PNG filenames in `Images\Studios\`

**Example** (if you have `warner-bros.png`):
- "Warner Bros." → "warner-bros"
- "Warner Bros. Pictures" → "warner-bros"
- "Warner Brothers" → "warner-bros"
- "WB" → "warner-bros"

**Result**: Studios automatically display correct icons

### Use Case 3: Handle Studio Mergers and Acquisitions

**Problem**: Studios have been acquired, merged, or rebranded

**Solution**: Map old studio names to current names or preserve historical accuracy

**Example mappings for current names**:
- "Twentieth Century Fox" → "20th Century Studios" (Disney acquisition)
- "Castle Rock Entertainment" → "Warner Bros." (merged)
- "New Line Cinema" → "Warner Bros." (subsidiary)
- "Miramax" → "Paramount" (or "Miramax" for independence)

**Alternative - historical preservation**:
- Keep original studio names for historical accuracy
- "Twentieth Century Fox" → "Twentieth Century Fox" (for older movies)

**Result**: Your choice of current vs. historical studio names

### Use Case 4: Consolidate Subsidiary Studios

**Problem**: Parent companies and subsidiaries creating cluttered studio lists

**Solution**: Map subsidiaries to parent company or keep distinct

**Example mappings for consolidation**:
- "New Line Cinema" → "Warner Bros."
- "Castle Rock Entertainment" → "Warner Bros."
- "DC Films" → "Warner Bros."
- "Lucasfilm" → "Disney"
- "Marvel Studios" → "Disney"
- "Pixar" → "Disney"

**Alternative - preserve distinctions**:
- "Marvel Studios" → "Marvel Studios"
- "Pixar" → "Pixar"
- "Lucasfilm" → "Lucasfilm"

**Result**: Your choice of consolidation vs. studio specificity

### Use Case 5: Remove Unwanted Studios

**Problem**: Scrapers return production companies that aren't display studios

**Solution**: Map unwanted production companies to empty string or remove via icon filtering

**Example approach 1 - Map to empty**:
- "Plan B Entertainment" → "" (producer, not studio)
- "Dune Entertainment" → "" (financing, not studio)

**Example approach 2 - Use icon filtering**:
- Simply don't create icons for unwanted studios
- Enable "StudioWithImgOnly" setting
- Only studios with icons will display

**Result**: Clean list showing only major distribution studios

## Integration with Other Systems

### Scraper Integration

All data scrapers automatically use the studio mapping system:

**Movie scrapers**:
- TMDB (scraper.TMDB.Data) - Returns full production company names
- IMDB (scraper.IMDB.Data) - Returns various studio formats
- OFDB (scraper.OFDB.Data) - Returns German studio names
- OMDb (scraper.Data.OMDb) - Returns studio/production company names

**How scrapers work**:
1. Scraper extracts raw studio strings from source
2. Adds to `scrapedmovie.Studios` list
3. Core API copies to temporary list
4. Core API calls `RunMapping()` on temporary list
5. Optionally filters by icon availability
6. Applies count limit if configured
7. Stores mapped studios in database

### Studio Icon System

Studio icons enhance the visual display of studio information:

**Icon location**: `Images\Studios\` and subfolders

**Supported formats**: PNG

**Icon loading** (clsAPIXML.vb - Init):
1. Scans `Images\Studios\` folder for PNG files
2. Scans all subfolders for organized icon collections
3. Builds `StudioIcons` dictionary with lowercase keys
4. Keys include subfolder name for organized collections
5. Example key: "major\warner-bros" for `major\warner-bros.png`

**Icon retrieval**:
1. Lowercase studio name used as lookup key
2. If found in dictionary, returns full path to PNG
3. If not found and using subfolders, tries folder\studio format
4. Displays icon alongside studio name in UI

**Organizing icons**:
- Can use flat structure: `Images\Studios\warner-bros.png`
- Can use subfolders: `Images\Studios\major\warner-bros.png`
- Subfolder name becomes part of mapping key

### Export and Interface Integration

Studio mappings affect exported data and external interfaces:

**Movie Export** (generic.EmberCore.MovieExport):
- HTML templates use mapped studio names
- Can display studio icons in exports
- Ensures consistent branding

**Kodi Interface** (generic.Interface.Kodi):
- Mapped studios synchronized to Kodi library
- Kodi displays standardized studio values

**Trakt.tv Interface** (generic.Interface.Trakttv):
- Uses mapped studios when syncing to Trakt

## Technical Implementation Details

### Shared Implementation

Studio Mapping shares the same implementation as Certification, Country, and Status mappings:
- Same `clsXMLSimpleMapping` base class
- Same `dlgSimpleMapping` dialog with type parameter
- Same `SimpleMapping` data structure
- Same database cleanup pattern

### Additional Studio Processing

Studio mapping has additional processing steps unique to studios:
1. **Temporary list**: Creates copy before mapping (preserves scraped data)
2. **Icon filtering**: Optional filtering based on icon availability
3. **Count limit**: Applied after mapping and filtering
4. **Validation**: Checks if any studios remain before marking as success

### Performance Considerations

**Icon dictionary**:
- Built once during initialization
- Fast lookup using lowercase keys
- Supports subfolder organization

**Filtering efficiency**:
- Icon filtering happens in memory
- Fast dictionary lookup for each studio
- Minimal performance impact

**Database updates**:
- Background worker prevents UI freeze
- Batch updates for efficiency
- Shows progress to user

## Best Practices

**Icon-driven mapping**:
- Organize your icon collection first
- Map studio names to match icon filenames exactly
- Use lowercase, hyphenated names for consistency
- Example: `warner-bros.png` matches mapping output "warner-bros"

**Major studios focus**:
- Consider mapping only major distribution studios
- Enable icon filtering to show only studios with icons
- Maps production companies to their distributors
- Keeps studio lists concise and visually appealing

**Historical considerations**:
- For classic movies, consider preserving original studio names
- Example: Keep "RKO Pictures" for 1940s films
- Helps maintain historical context

**Subsidiary handling**:
- Decide on parent company vs. subsidiary specificity
- Marvel fans might prefer "Marvel Studios" over "Disney"
- Pixar fans might prefer "Pixar" over "Disney"
- Balance brand identity with organizational clarity

**Regular maintenance**:
- Periodically review new auto-created mappings
- Check for new studios needing icons
- Update mappings when studios rebrand or merge
- Clean up unused or obsolete studios

**Icon collection management**:
- Use consistent dimensions (e.g., 400x150 pixels)
- Use transparent backgrounds for PNG files
- Name files descriptively and consistently
- Organize in subfolders if collection is large

## Troubleshooting

**Issue**: Studios not displaying icons
**Cause**: Studio name doesn't match icon filename
**Solution**: Check exact mapping output matches PNG filename (case-insensitive)

**Issue**: Too many studios showing per movie
**Cause**: Studio limit not configured or too high
**Solution**: Set `MovieScraperStudioLimit` to reasonable number (e.g., 2-3)

**Issue**: Important studios being filtered out
**Cause**: Icon filtering enabled but missing icons for those studios
**Solution**: Add PNG icons for studios you want to display, or disable icon filtering

**Issue**: Mappings not persisting
**Cause**: File permission error on XML file
**Solution**: Check write permissions on settings directory

**Issue**: Database cleanup not updating all movies
**Cause**: Movies may be locked or studio field locked
**Solution**: Check lock status in movie metadata

**Issue**: Subsidiary studios not mapping correctly
**Cause**: Need to map both parent and subsidiary names
**Solution**: Create mappings for all name variations to your chosen output

## Summary

The Studio Mapping system provides comprehensive control over studio name standardization with tight integration to icon display:

**Key benefits**:
- Simple one-to-one mapping of studio strings
- Automatic discovery and 1:1 mapping of new studios
- Easy-to-use grid-based editor
- Integration with studio icon system
- Optional filtering to only show studios with icons
- Studio count limiting for clean display
- Immediate database synchronization via cleanup process
- Portable configuration via XML
- Support for historical studio names
- Subsidiary and parent company handling

**Typical workflow**:
1. Organize studio icon collection in `Images\Studios\`
2. Configure icon filtering and studio limit in settings
3. Scrapers retrieve raw studio data
4. Mapping system translates to standard format matching icon filenames
5. Optional icon filtering removes studios without icons
6. Count limit applied for clean display
7. New studios auto-mapped 1:1
8. User periodically reviews and customizes mappings to match icons
9. Database cleanup applies mappings to existing movies
10. Library maintains consistent studio names with beautiful icon display

This system ensures that regardless of which scraper sources you use or what format they return studios in, your media library maintains clean, consistent studio names with matching icons for an enhanced visual experience.