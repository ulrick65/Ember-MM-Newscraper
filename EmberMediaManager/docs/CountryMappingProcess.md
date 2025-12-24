# Country Mapping System in Ember Media Manager

The **Country Mapping** feature in Ember Media Manager provides a simple one-to-one mapping system that standardizes country names from various scraper sources into a consistent format.

## Overview

The Country Mapping system solves the problem of inconsistent country names across different scraper sources. For example, different scrapers might return "USA", "United States", "US", "United States of America", or "U.S.A." for the same country. This system automatically translates these variations into your preferred standard format.

## Core Components

### 1. Data Model (clsXMLSimpleMapping.vb)

The system uses the same simple mapping structure as Certification Mapping, located in `EmberAPI\XML Serialization\clsXMLSimpleMapping.vb`:

#### clsXMLSimpleMapping Class
The main container that manages country mappings:
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
- **Input**: `String` - The country string from scrapers (e.g., "USA", "U.S.A.")
- **MappedTo**: `String` - The target country string (e.g., "United States")

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
- Shared implementation with Certification, Status, and Studio mappings

### 3. Menu Integration (genericMapping.vb)

The country mapping editor is accessible through:
- Main menu: Tools → Country Mapping
- System tray context menu: Tools → Country Mapping

After closing the dialog with OK:
- Saves changes to XML file
- Triggers database cleanup to standardize existing countries
- Shows count of items changed in database

## How the Mapping Process Works

### During Movie Scraping

When scrapers retrieve country data, the mapping process is triggered automatically:

#### For Movies (clsAPINFO.vb - MergeDataScraperResults_Movie)

Located around line 165, the process is:

1. **Check conditions**: Verify country scraping is enabled and not locked
2. **Apply count limit**: If country limit is set, limit number of countries
3. **Assign to movie**: Store scraped countries
4. **Apply mapping**: Call `APIXML.CountryMapping.RunMapping(DBMovie.Movie.Countries)`
5. **Store result**: Mapped countries saved to database

The actual code flow is:
- Check if country field is not locked (`Not Master.eSettings.MovieLockCountry`)
- Check if scraping countries is enabled (`Master.eSettings.MovieScraperCountry`)
- Check if this is the first scraper result with countries (`Not new_Countries`)
- Apply count limit if configured
- Run the mapping transformation
- Store the result

### The RunMapping Algorithm

The `RunMapping(ByRef listToBeMapped As List(Of String), Optional addNewInputs As Boolean = True)` method processes country lists:

#### Step 1: Process Each Input Country
For each country string in the input list:

1. **Look for existing mapping**: Search in `Mappings` list for matching `Input`
2. **If mapping found**: Add `MappedTo` value to result list
3. **If no mapping found and addNewInputs is True**:
   - Add original country to result (no modification)
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

**Scenario**: TMDB scraper returns countries for an international production

**Input from scraper**: `["USA", "UK", "Deutschland"]`

**Existing mappings**:
- "USA" → "United States"
- "UK" → "United Kingdom"
- "Deutschland" → "Germany"

**Step-by-step process**:

1. Process "USA":
   - Found in mappings
   - Add "United States" to result
   - Result: `["United States"]`

2. Process "UK":
   - Found in mappings
   - Add "United Kingdom" to result
   - Result: `["United States", "United Kingdom"]`

3. Process "Deutschland":
   - Found in mappings
   - Add "Germany" to result
   - Result: `["United States", "United Kingdom", "Germany"]`

4. Cleanup and finalize:
   - No new mappings created
   - Result sorted alphabetically

**Final result**: Movie has countries `["Germany", "United Kingdom", "United States"]`

### Country Count Limit

Before mapping is applied, an optional count limit can be applied:

**Setting**: `Master.eSettings.MovieScraperCountryLimit`

**Process**:
1. Apply `FilterCountLimit()` before mapping
2. Keeps only first N countries from scraper result
3. Then applies mapping to limited list

**Example**:
- Input: `["USA", "UK", "France", "Germany", "Italy"]`
- Limit: 3
- After limit: `["USA", "UK", "France"]`
- After mapping: `["United States", "United Kingdom", "France"]`

### Database Cleanup Process

When countries are modified or mappings change, the cleanup process ensures consistency:

**Cleanup trigger**:
- Executes when user clicks OK in Country Mapping dialog
- Runs in background worker (`bwCleanDatabase`)
- Shows progress bar during operation

**Cleanup method** (`Master.DB.Cleanup_Countries()`):
1. Retrieves all countries currently assigned to movies in database
2. Applies current mapping rules to all countries
3. Updates movies with newly mapped country values
4. Removes duplicate entries
5. Returns count of items changed

**User feedback**:
- Progress bar shows operation is running
- Message box displays: "X item(s) changed"
- Database immediately reflects new mapping configuration

## Data Persistence (XML Structure)

All mappings are stored in `Core.Mapping.Countries.xml` in the settings directory.

**XML structure example**:

`<simplemapping>`
`  <mapping input="USA">United States</mapping>`
`  <mapping input="US">United States</mapping>`
`  <mapping input="U.S.A.">United States</mapping>`
`  <mapping input="United States of America">United States</mapping>`
`  <mapping input="UK">United Kingdom</mapping>`
`  <mapping input="United Kingdom of Great Britain">United Kingdom</mapping>`
`  <mapping input="England">United Kingdom</mapping>`
`  <mapping input="Deutschland">Germany</mapping>`
`  <mapping input="BRD">Germany</mapping>`
`  <mapping input="West Germany">Germany</mapping>`
`  <mapping input="Frankreich">France</mapping>`
`  <mapping input="España">Spain</mapping>`
`  <mapping input="Italia">Italy</mapping>`
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

### Use Case 1: Standardize English Country Names

**Problem**: Scrapers return various English variations of country names

**Solution**: Map all variations to single standard English name

**Example mappings**:
- "USA" → "United States"
- "US" → "United States"
- "U.S.A." → "United States"
- "United States of America" → "United States"
- "UK" → "United Kingdom"
- "Great Britain" → "United Kingdom"
- "England" → "United Kingdom"

**Result**: Consistent English country names across all movies

### Use Case 2: Translate Foreign Country Names

**Problem**: International scrapers return country names in local languages

**Solution**: Map foreign language names to English equivalents

**Example mappings**:
- "Deutschland" → "Germany"
- "Frankreich" → "France"
- "España" → "Spain"
- "Italia" → "Italy"
- "Ö sterreich" → "Austria"
- "Schweiz" → "Switzerland"
- "Belgien" → "Belgium"
- "Nederland" → "Netherlands"

**Result**: All country names displayed in English regardless of scraper language

### Use Case 3: Use Local Language Country Names

**Problem**: Want to display countries in your local language

**Solution**: Map English names to your local language

**Example mappings for German**:
- "United States" → "Vereinigte Staaten"
- "United Kingdom" → "Vereinigtes Königreich"
- "France" → "Frankreich"
- "Spain" → "Spanien"
- "Italy" → "Italien"

**Result**: Country names displayed in your preferred language

### Use Case 4: Handle Historical Country Names

**Problem**: Older movies reference countries that no longer exist or have changed names

**Solution**: Map historical names to current names or keep historical accuracy

**Example mappings**:
- "Soviet Union" → "Russia" (or keep as "Soviet Union" for historical accuracy)
- "USSR" → "Soviet Union"
- "West Germany" → "Germany" (or keep as "West Germany")
- "East Germany" → "Germany" (or keep as "East Germany")
- "Yugoslavia" → "Yugoslavia" (historical preservation)
- "Czechoslovakia" → "Czechoslovakia" (historical preservation)

**Result**: Consistent handling of historical country references

### Use Case 5: Consolidate Regional Variations

**Problem**: Scrapers return specific regions instead of country names

**Solution**: Map regions to parent country or keep regional specificity

**Example mappings**:
- "England" → "United Kingdom" (consolidate)
- "Scotland" → "United Kingdom" (consolidate)
- "Wales" → "United Kingdom" (consolidate)
- "Bavaria" → "Germany" (consolidate)
- "Catalonia" → "Spain" (consolidate)

**Alternative - preserve regions**:
- "England" → "England"
- "Scotland" → "Scotland"
- "Wales" → "Wales"

**Result**: Your choice of consolidation vs. regional specificity

## Integration with Other Systems

### Scraper Integration

All data scrapers automatically use the country mapping system:

**Movie scrapers**:
- TMDB (scraper.TMDB.Data) - Returns abbreviated country codes
- IMDB (scraper.IMDB.Data) - Returns full country names
- OFDB (scraper.OFDB.Data) - Returns German country names
- OMDb (scraper.Data.OMDb) - Returns various country formats

**How scrapers work**:
1. Scraper extracts raw country strings from source
2. Adds to `scrapedmovie.Countries` list
3. Core API applies count limit if configured
4. Core API calls `RunMapping()` automatically
5. Mapped countries stored in database

### Export and Interface Integration

Country mappings affect exported data and external interfaces:

**Movie Export** (generic.EmberCore.MovieExport):
- HTML templates use mapped country names
- Ensures consistent display in exported pages

**Kodi Interface** (generic.Interface.Kodi):
- Mapped countries synchronized to Kodi library
- Kodi displays standardized country values

**Trakt.tv Interface** (generic.Interface.Trakttv):
- Uses mapped countries when syncing to Trakt

## Technical Implementation Details

### Shared Implementation

Country Mapping shares the same implementation as Certification, Status, and Studio mappings:
- Same `clsXMLSimpleMapping` base class
- Same `dlgSimpleMapping` dialog with type parameter
- Same `SimpleMapping` data structure
- Same database cleanup pattern

**Enum distinction**:
The `MappingType` enum in `dlgSimpleMapping.vb` distinguishes the mapping types:
- `CountryMapping` = Country mappings
- `CertificationMapping` = Certification mappings
- `StatusMapping` = Status mappings
- `StudioMapping` = Studio mappings

### Performance Considerations

**Sorting**:
- Countries sorted alphabetically
- Makes browsing easier in UI
- Consistent ordering in database

**String matching**:
- Exact string comparison
- Case-sensitive by default
- Fast lookup using `FirstOrDefault()`

**Database updates**:
- Background worker prevents UI freeze
- Batch updates for efficiency
- Shows progress to user

## Best Practices

**Standardization**:
- Choose one standard format for country names
- Use full country names for clarity (e.g., "United States" not "USA")
- Be consistent across all mappings

**International considerations**:
- If managing international library, use internationally recognized names
- Consider using English names as standard (most universal)
- Alternatively, use your local language consistently

**Historical accuracy**:
- For older movies, consider keeping historical country names
- Example: "Soviet Union" for movies from that era
- Document your approach for consistency

**Regional handling**:
- Decide on country vs. region specificity
- Be consistent with choice across library
- Consider user expectations for browsing/filtering

**Regular maintenance**:
- Periodically review new auto-created mappings
- Check for typos or inconsistencies
- Standardize format as new countries appear

## Troubleshooting

**Issue**: Countries not mapping correctly
**Cause**: Exact string mismatch (case-sensitive)
**Solution**: Check exact format from scraper, add exact match to mappings

**Issue**: Too many countries showing for each movie
**Cause**: Country limit not configured
**Solution**: Set `MovieScraperCountryLimit` in settings to reasonable number (e.g., 3-5)

**Issue**: Mappings not persisting
**Cause**: File permission error on XML file
**Solution**: Check write permissions on settings directory

**Issue**: Database cleanup not updating all movies
**Cause**: Movies may be locked or country field locked
**Solution**: Check lock status in movie metadata

**Issue**: Historical countries showing incorrectly
**Cause**: Auto-mapping created inappropriate modern equivalent
**Solution**: Manually edit mapping to preserve historical name

## Summary

The Country Mapping system provides straightforward control over country name standardization:

**Key benefits**:
- Simple one-to-one mapping of country strings
- Automatic discovery and 1:1 mapping of new countries
- Easy-to-use grid-based editor
- Immediate database synchronization via cleanup process
- Portable configuration via XML
- Integration with country count limit
- Support for multilingual country names
- Historical country name handling

**Typical workflow**:
1. Scrapers retrieve raw country data in various formats
2. Optional count limit applied to limit number of countries
3. Mapping system automatically translates to standard format
4. New countries auto-mapped 1:1 (e.g., "USA" → "USA")
5. User periodically reviews and customizes mappings
6. Database cleanup applies mappings to existing movies
7. Library maintains consistent country format

This system ensures that regardless of which scraper sources you use, what language they return countries in, or what format they use, your media library maintains clean and consistent country names that match your organizational preferences.