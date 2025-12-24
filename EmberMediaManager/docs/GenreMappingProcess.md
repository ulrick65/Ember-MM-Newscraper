# Genre Mapping System in Ember Media Manager

The **Genre Mapping** feature in Ember Media Manager is a comprehensive system that standardizes and translates genre names from various scrapers into a consistent, customizable set of genres.

## Overview

The Genre Mapping system solves a common problem in media management: different scraper sources use different genre names (e.g., "Abenteuer" vs "Adventure", "Sci-Fi" vs "Science Fiction"). This system automatically translates and standardizes these variations into a unified genre vocabulary.

## Core Components

### 1. Data Model (clsXMLGenreMapping.vb)

The system uses three main classes located in `EmberAPI\XML Serialization\clsXMLGenreMapping.vb`:

#### clsXMLGenreMapping Class
The main container that manages the entire mapping system:
- **Genres**: `List(Of GenreProperty)` - List of standardized genre names with optional images
- **Mappings**: `List(Of GenreMapping)` - List of mapping rules (search strings to target genres)
- **DefaultImage**: `String` - Fallback image for genres without a specific image (default: "default.jpg")
- **FileNameFullPath**: `String` - Path to the XML file storing mappings

Key methods:
- `Load()` - Loads mappings from XML file
- `Save()` - Persists mappings to XML file
- `RunMapping(ByRef listToBeMapped As List(Of String), Optional addNewInputs As Boolean = True)` - Core mapping logic
- `Sort()` - Sorts genres and mappings alphabetically
- `CloneDeep()` - Creates a deep copy for editing without affecting live data

#### GenreProperty Class
Represents a standardized genre in your library:
- **Name**: `String` - The genre name (e.g., "Action", "Drama", "Science Fiction")
- **Image**: `String` - Optional image filename (e.g., "action.jpg")
- **isNew**: `Boolean` - Flag indicating newly discovered genres needing review

#### GenreMapping Class
Represents a mapping rule that translates incoming genres:
- **SearchString**: `String` - The input genre from scrapers (e.g., "Abenteuer", "Sci-Fi")
- **MappedTo**: `List(Of String)` - Target genre(s) this maps to (supports one-to-many mapping)
- **isNew**: `Boolean` - Flag indicating newly auto-created mappings needing review

### 2. User Interface (dlgGenreMapping.vb)

Located in `Addons\generic.EmberCore.Mapping\dlgGenreMapping.vb`, this Windows Forms dialog provides a comprehensive editor with two main panels:

#### Left Panel - Genres Management
- **Genre List**: DataGridView displaying all standardized genres
- **Visual Indicators**:
  - Green text with bold font = New genres needing confirmation
  - Blue background = Genres without assigned images
  - White background = Genres with images
- **Operations**:
  - Add new genres manually
  - Remove genres (also removes from all mappings)
  - Rename genres (updates all mappings automatically)
  - Assign/change genre images
  - Confirm new genres to remove the "new" flag

#### Right Panel - Mappings Management
- **Mappings List**: DataGridView showing search string to genre mappings
- **Filter Dropdown**: Filter mappings by target genre or view all
- **Visual Indicators**:
  - Green text with bold font = New mappings needing confirmation
  - Blue background = Unmapped search strings
  - White background = Mapped search strings
- **Operations**:
  - Add new mappings manually
  - Remove mappings
  - Edit search strings inline
  - Check/uncheck genres using checkboxes in the left panel
  - Confirm new mappings

#### Genre Image Management
- **Image Preview**: PictureBox showing selected genre's image
- **Change Image**: Browse to select new image from `Images\Genres\` folder
- **Remove Image**: Clear image assignment

### 3. Menu Integration (genericMapping.vb)

The genre mapping editor is accessible through:
- Main menu: Tools → Genre Mapping
- System tray context menu: Tools → Genre Mapping

After closing the dialog, the system triggers:
- UI refresh to reflect changes
- Database cleanup to remove orphaned genres

## How the Mapping Process Works

### During Movie/TV Show Scraping

When scrapers retrieve metadata, the genre mapping process is triggered automatically:

#### For Movies (clsAPINFO.vb - MergeDataScraperResults_Movie)

Located around line 183, the process is:

1. **Check conditions**: Verify genre scraping is enabled and not locked
2. **Apply mapping**: Call `APIXML.GenreMapping.RunMapping(scrapedmovie.Genres)`
3. **Apply limit**: Call `FilterCountLimit(Master.eSettings.MovieScraperGenreLimit, scrapedmovie.Genres)`
4. **Assign to movie**: `DBMovie.Movie.Genres = scrapedmovie.Genres`

The actual code flow is:
- Check if genre field is not locked (`Not Master.eSettings.MovieLockGenre`)
- Check if scraping genres is enabled (`Master.eSettings.MovieScraperGenre`)
- Check if this is the first scraper result with genres (`Not new_Genres`)
- Run the mapping transformation
- Apply count limit if configured
- Store the result

#### For TV Shows (clsAPINFO.vb - MergeDataScraperResults_TVShow)

Located around line 555, follows similar logic:
- Check lock status and settings
- Call `APIXML.GenreMapping.RunMapping(scrapedshow.Genres)`
- Apply limit with `FilterCountLimit(Master.eSettings.TVScraperShowGenreLimit, scrapedshow.Genres)`
- Assign to TV show data

### The RunMapping Algorithm

The `RunMapping(ByRef listToBeMapped As List(Of String), Optional addNewInputs As Boolean = True)` method is the core of the mapping system:

#### Step 1: Process Each Input Genre
For each genre string in the input list:

1. **Look for existing mapping**: Search in `Mappings` list for matching `SearchString`
2. **If mapping found**: Add all genres from `MappedTo` list to result
3. **If no mapping found and addNewInputs is True**:
   - Check if genre exists in `Genres` list
   - If not, create new `GenreProperty` with `isNew = True`
   - Create new `GenreMapping` with 1:1 mapping (genre maps to itself)
   - Mark mapping as `isNew = True`
   - Add genre to result list

#### Step 2: Cleanup and Sort
If any new items were added:
- Sort `Genres` list alphabetically
- Sort `Mappings` list alphabetically
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

**Scenario**: OFDB scraper returns German genres for an action movie

**Input from scraper**: `["Abenteuer", "Drama", "Science-Fiction"]`

**Existing mappings**:
- "Abenteuer" → "Adventure"
- "Drama" → "Drama"

**Step-by-step process**:

1. Process "Abenteuer":
   - Found in mappings
   - Add "Adventure" to result
   - Result: `["Adventure"]`

2. Process "Drama":
   - Found in mappings
   - Add "Drama" to result
   - Result: `["Adventure", "Drama"]`

3. Process "Science-Fiction":
   - NOT found in mappings
   - Auto-create `GenreProperty`: Name="Science-Fiction", isNew=True
   - Auto-create `GenreMapping`: SearchString="Science-Fiction", MappedTo=["Science-Fiction"], isNew=True
   - Add "Science-Fiction" to result
   - Result: `["Adventure", "Drama", "Science-Fiction"]`
   - Save mappings to XML immediately

4. User opens Genre Mapping dialog:
   - "Science-Fiction" appears in green (new genre needing confirmation)
   - User can rename to "Science Fiction" (with space)
   - User can map to existing genre or keep as-is
   - User clicks Confirm to accept

**Final result**: Movie has genres `["Adventure", "Drama", "Science Fiction"]`

## Advanced Features

### Multi-Target Mapping (One-to-Many)

A single search string can map to multiple target genres:

**Use case**: Some scrapers combine genres with slashes or hyphens

**Example mappings**:
- "Action/Adventure" → `["Action", "Adventure"]`
- "Sci-Fi-Thriller" → `["Science Fiction", "Thriller"]`
- "Comedy Drama" → `["Comedy", "Drama"]`

**Process**:
1. Scraper returns "Action/Adventure"
2. Mapping found with `MappedTo = ["Action", "Adventure"]`
3. Both genres added to result
4. Movie ends up with two separate genres

### Genre Image Association

Each genre can have a custom image for visual displays throughout the application.

**Image storage**: `Images\Genres\` folder in application directory

**Supported formats**: JPG, PNG

**Image selection**:
1. Select genre in left panel of Genre Mapping dialog
2. Click "Change" button
3. Browse to select image file
4. Image filename stored in `GenreProperty.Image`

**Image retrieval** (clsAPIXML.vb - GetGenreImage):
1. Look up genre in `GenreMapping.Genres` list
2. Get image filename from `GenreProperty.Image`
3. Build full path: `Images\Genres\{filename}`
4. If image exists, load and return
5. If not found, return default genre image

**Default image**:
- Filename specified in `GenreMapping.DefaultImage` (default: "default.jpg")
- Falls back to `Images\Defaults\DefaultGenre.jpg` if not found

### Automatic Genre Discovery

New genres from scrapers are automatically detected and prepared for user review:

**When new genre detected**:
1. `RunMapping()` creates `GenreProperty` with `isNew = True`
2. Creates 1:1 `GenreMapping` with `isNew = True`
3. Saves immediately to XML
4. Genre appears in green text in UI

**User workflow**:
1. Open Genre Mapping dialog
2. New genres appear in green in left panel
3. New mappings appear in green in right panel
4. Review and decide:
   - Keep as-is and confirm
   - Rename to match existing standard
   - Map to different existing genre
   - Delete if unwanted
5. Click "Confirm" or "Confirm All" to clear new flags

**Benefits**:
- Never lose genre data from scrapers
- User maintains control over genre vocabulary
- Gradual refinement of mappings over time
- No need to pre-define all possible genres

### Data Persistence (XML Structure)

All mappings are stored in `Core.Mapping.Genres.xml` in the settings directory.

**XML structure example**:

`<core.genres>`
`  <defaultimage>default.jpg</defaultimage>`
`  `
`  <genre>`
`    <name>Action</name>`
`    <image>action.jpg</image>`
`    <isnew>False</isnew>`
`  </genre>`
`  `
`  <genre>`
`    <name>Adventure</name>`
`    <image>adventure.jpg</image>`
`    <isnew>False</isnew>`
`  </genre>`
`  `
`  <genre>`
`    <name>Science Fiction</name>`
`    <image>scifi.jpg</image>`
`    <isnew>True</isnew>`
`  </genre>`
`  `
`  <mapping>`
`    <searchstring>Abenteuer</searchstring>`
`    <mappedto>Adventure</mappedto>`
`    <isnew>False</isnew>`
`  </mapping>`
`  `
`  <mapping>`
`    <searchstring>Action</searchstring>`
`    <mappedto>Action</mappedto>`
`    <isnew>False</isnew>`
`  </mapping>`
`  `
`  <mapping>`
`    <searchstring>Sci-Fi</searchstring>`
`    <mappedto>Science Fiction</mappedto>`
`    <isnew>False</isnew>`
`  </mapping>`
`  `
`  <mapping>`
`    <searchstring>Action/Adventure</searchstring>`
`    <mappedto>Action</mappedto>`
`    <mappedto>Adventure</mappedto>`
`    <isnew>False</isnew>`
`  </mapping>`
`</core.genres>`

**Load process**:
- Called during application initialization in `clsAPIXML.Init()`
- Deserializes XML into `clsXMLGenreMapping` object
- Handles missing files by creating new empty mapping
- Migrates from old file location if needed
- Creates backup if XML is corrupted

**Save process**:
- Called after UI changes in dialog
- Called automatically when new mappings are created
- Sorts all data before serialization
- Overwrites existing file atomically

### Database Cleanup Process

When genres are removed or mappings change, orphaned genre entries may exist in the database.

**Cleanup trigger**:
- Executes when user clicks OK in Genre Mapping dialog
- Runs in background worker (`bwCleanDatabase`)
- Shows progress bar during operation

**Cleanup method** (`Master.DB.Cleanup_Genres()`):
1. Retrieves all genres currently assigned to movies/TV shows in database
2. Compares against valid genres in `GenreMapping.Genres` list
3. Removes genre assignments that no longer match any valid genre
4. Updates genre tables to remove unused entries
5. Returns count of items changed

**User feedback**:
- Progress bar shows operation is running
- Message box displays: "X item(s) changed"
- Database is immediately consistent with mapping configuration

## Common Use Cases and Examples

### Use Case 1: Language Translation

**Problem**: OFDB scraper returns German genre names

**Solution**: Create mappings for German to English translation

**Example mappings**:
- "Abenteuer" → "Adventure"
- "Komödie" → "Comedy"
- "Krimi" → "Crime"
- "Thriller" → "Thriller"
- "Horror" → "Horror"
- "Science Fiction" → "Science Fiction"

**Result**: All movies get English genre names regardless of scraper source

### Use Case 2: Genre Standardization

**Problem**: Different scrapers use different variations of the same genre

**Solution**: Map all variations to single standard name

**Example mappings**:
- "Sci-Fi" → "Science Fiction"
- "SciFi" → "Science Fiction"
- "Science-Fiction" → "Science Fiction"
- "SF" → "Science Fiction"

**Result**: Consistent "Science Fiction" genre across entire library

### Use Case 3: Genre Consolidation

**Problem**: Too many similar or overlapping genres

**Solution**: Map multiple specific genres to broader category

**Example mappings**:
- "Superhero" → "Action"
- "Martial Arts" → "Action"
- "Spy" → "Action"
- "Heist" → "Thriller"
- "Psychological" → "Thriller"

**Result**: Simplified genre list with fewer categories

### Use Case 4: Genre Splitting

**Problem**: Scraper returns combined genres

**Solution**: Use one-to-many mapping to split into separate genres

**Example mappings**:
- "Action/Adventure" → `["Action", "Adventure"]`
- "Comedy/Drama" → `["Comedy", "Drama"]`
- "Horror/Thriller" → `["Horror", "Thriller"]`
- "Sci-Fi/Fantasy" → `["Science Fiction", "Fantasy"]`

**Result**: Movies properly categorized with multiple distinct genres

### Use Case 5: Genre Filtering

**Problem**: Scraper returns genres you don't want to use

**Solution**: Map unwanted genres to empty list (will be filtered out)

**Note**: Currently mapping to empty list will create unmapped entry. Better approach is to map to a valid genre or delete the mapping after initial creation.

**Alternative**: Simply don't confirm new genres, and they won't be used in future

### Use Case 6: Custom Genre Creation

**Problem**: You want custom genres not provided by scrapers

**Solution**: Manually add genres and create mappings

**Steps**:
1. Click "Add" in Genres panel
2. Enter custom genre name (e.g., "Family Friendly")
3. Assign image if desired
4. Click "Add" in Mappings panel
5. Enter search criteria (e.g., "Family")
6. Check your custom genre in the checkboxes
7. Save

**Result**: Custom genre vocabulary tailored to your library organization

## Integration with Other Systems

### Scraper Integration

All data scrapers automatically use the genre mapping system:

**Movie scrapers**:
- TMDB (scraper.TMDB.Data)
- IMDB (scraper.IMDB.Data)
- OFDB (scraper.OFDB.Data)
- OMDb (scraper.Data.OMDb)
- Moviepilot DE (scraper.MoviepilotDE.Data)

**TV show scrapers**:
- TVDB (scraper.Data.TVDB)
- Trakt.tv (scraper.Trakttv.Data)

**How scrapers work**:
1. Scraper extracts raw genre strings from source
2. Adds to `scrapedmovie.Genres` or `scrapedshow.Genres` list
3. Core API calls `RunMapping()` automatically
4. Mapped genres stored in database
5. Original scraper data is not modified

### Export and Interface Integration

Genre mappings affect exported data and external interfaces:

**Movie Export** (generic.EmberCore.MovieExport):
- HTML templates use mapped genre names
- Genre images from mapping system used in visual exports

**Kodi Interface** (generic.Interface.Kodi):
- Mapped genres synchronized to Kodi library
- Ensures consistency between Ember and Kodi

**Trakt.tv Interface** (generic.Interface.Trakttv):
- Uses mapped genres when syncing to Trakt
- Maintains standard genre vocabulary

## Technical Implementation Details

### Memory Management

**Clone for editing**:
- Dialog creates deep clone: `tmpGenreXML = CType(APIXML.GenreMapping.CloneDeep, clsXMLGenreMapping)`
- All edits modify clone, not live data
- Live data only updated on OK button
- Cancel button discards clone

**Deep clone implementation**:
- Uses binary serialization
- Copies all nested objects
- Ensures complete independence from original

### UI Event Handling

**Genre renaming cascade**:
- When genre renamed in left panel
- `dgvGenres_CellValueChanged` event fires
- Automatically updates all mappings referencing old name
- Replaces old name with new name in all `MappedTo` lists
- Marks genre as confirmed (not new)

**Checkbox mapping**:
- When checkbox changed in left panel
- Only processes if mapping selected in right panel
- Adds/removes genre from mapping's `MappedTo` list
- Marks mapping as confirmed (not new)
- Refreshes right panel to update visual indicators

### Performance Considerations

**Sorting**:
- Genres and mappings sorted alphabetically
- Provides consistent ordering in UI
- Makes manual searching easier
- Implemented via `IComparable(Of T)` interface

**Filtering**:
- Dropdown filter allows viewing mappings by target genre
- Reduces visual clutter for large mapping sets
- Implemented by filtering DataGridView rows
- Original data unchanged, just view filtered

**Background processing**:
- Database cleanup runs in background worker
- Prevents UI freeze during large operations
- Reports progress to status bar
- Shows completion message when done

## Troubleshooting and Maintenance

### Common Issues

**Issue**: New genres keep appearing in green
**Cause**: Scrapers finding variations not yet mapped
**Solution**: Review and confirm or remap as needed

**Issue**: Genres missing after cleanup
**Cause**: Genre removed from mapping but still in database
**Solution**: Cleanup process automatically removes on next OK

**Issue**: Genre images not showing
**Cause**: Image file missing or incorrect filename
**Solution**: Check `Images\Genres\` folder, reassign image in dialog

**Issue**: Mappings not persisting
**Cause**: File permission error on XML file
**Solution**: Check write permissions on settings directory

### Best Practices

**Regular maintenance**:
- Periodically review new genres (green items)
- Confirm or remap promptly to keep mapping current
- Remove unused genres to keep list manageable

**Consistent naming**:
- Use standard genre names consistently
- Avoid abbreviations unless universally recognized
- Use proper capitalization (e.g., "Science Fiction" not "science fiction")

**Image management**:
- Use consistent image dimensions for uniform display
- Name image files descriptively (e.g., "action.jpg" not "image1.jpg")
- Keep images in `Images\Genres\` folder
- Use JPG for smaller file sizes, PNG for quality

**Backup strategy**:
- Settings folder is automatically backed up by Ember
- XML file is small and easily backed up manually
- Consider version control for custom mapping configurations

### Migration and Upgrades

**Version migration**:
- Old location: `Core.Genres.xml`
- New location: `Core.Mapping.Genres.xml`
- Automatic migration on first load
- Original file deleted after successful migration

**Sharing configurations**:
- XML file is portable
- Can share mapping files between installations
- Useful for teams or multi-computer setups
- Copy file to settings directory before first run

## Summary

The Genre Mapping system provides powerful control over how genres are standardized in your media library:

**Key benefits**:
- Automatic translation of genres from multiple scraper sources
- Consistent genre vocabulary across entire library
- Support for one-to-many mappings for complex genre relationships
- Visual genre images for enhanced UI
- Automatic discovery and flagging of new genres
- User-friendly editing interface with visual feedback
- Immediate database synchronization
- Portable configuration via XML

**Typical workflow**:
1. Scrapers retrieve raw genre data
2. Mapping system automatically translates to standard genres
3. New genres flagged for review (shown in green)
4. User periodically reviews and confirms/remaps new genres
5. Database cleanup removes orphaned entries
6. Library maintains consistent genre structure

This system ensures that regardless of which scraper sources you use or what languages they return genres in, your media library maintains a clean, consistent, and customized genre organization that matches your preferences.