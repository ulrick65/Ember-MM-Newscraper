# Status Mapping System in Ember Media Manager

The **Status Mapping** feature in Ember Media Manager provides a simple one-to-one mapping system that standardizes movie status values into a consistent format for library organization and filtering.

## Overview

The Status Mapping system helps you categorize and organize your movie library by standardizing status values. Movie status typically indicates things like "Watched", "Unwatched", "To Watch", "Favorite", or custom organizational categories. This system allows you to map various status indicators into your preferred categorization scheme.

## Core Components

### 1. Data Model (clsXMLSimpleMapping.vb)

The system uses the same simple mapping structure as Certification, Country, and Studio Mapping, located in `EmberAPI\XML Serialization\clsXMLSimpleMapping.vb`:

#### clsXMLSimpleMapping Class
The main container that manages status mappings:
- **Mappings**: `List(Of SimpleMapping)` - List of input-to-output mapping rules
- **FileNameFullPath**: `String` - Path to the XML file storing mappings

Key methods:
- `Load()` - Loads mappings from XML file
- `Save()` - Persists mappings to XML file
- `RunMapping(ByRef singleString As String, Optional addNewInputs As Boolean = True)` - Core mapping logic for single status string
- `Sort()` - Sorts mappings alphabetically
- `CloneDeep()` - Creates a deep copy for editing without affecting live data

#### SimpleMapping Class
Represents a single mapping rule:
- **Input**: `String` - The status string from various sources (e.g., "seen", "0", "1")
- **MappedTo**: `String` - The target status string (e.g., "Watched", "Unwatched")

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
- Shared implementation with Certification, Country, and Studio mappings

### 3. Menu Integration (genericMapping.vb)

The status mapping editor is accessible through:
- Main menu: Tools → Status Mapping
- System tray context menu: Tools → Status Mapping

After closing the dialog with OK:
- Saves changes to XML file
- Triggers database cleanup to standardize existing status values
- Shows count of items changed in database

## How the Mapping Process Works

### Status in Ember Media Manager

Status is a flexible metadata field that can be used for various purposes:

**Common uses**:
- Watch status tracking (Watched/Unwatched)
- Collection management (Owned/Wishlist)
- Quality indicators (To Upgrade/Good Quality)
- Custom categories (Favorite/Archive/Delete)
- Organizational tags (Kids Movies/Date Night/Party)

**Storage**:
- Stored as string in database per movie
- Single value per movie (not a list)
- User-definable values
- Can be set manually or via import

### The RunMapping Algorithm

The `RunMapping(ByRef singleString As String, Optional addNewInputs As Boolean = True)` method processes individual status strings:

Note: Status uses the single-string version of `RunMapping` since each movie has only one status value.

#### Step 1: Process Input Status
For the status string:

1. **Look for existing mapping**: Search in `Mappings` list for matching `Input`
2. **If mapping found**: Return `MappedTo` value
3. **If no mapping found and addNewInputs is True**:
   - Return original status unchanged
   - Create new `SimpleMapping` with Input = MappedTo (1:1 mapping)
   - Mark for saving

#### Step 2: Cleanup and Sort
If new item was added:
- Sort `Mappings` list alphabetically by Input
- Call `Save()` to persist changes immediately

#### Step 3: Determine If Changes Occurred
- Compare input with result
- Replace input string with result (passed by reference)
- Return `True` if strings differ, `False` if identical

### Example Mapping Flow

**Scenario**: Importing movies from external source with numeric status codes

**Input status**: `"1"`

**Existing mappings**:
- "0" → "Unwatched"
- "1" → "Watched"
- "2" → "Favorite"

**Step-by-step process**:

1. Process "1":
   - Found in mappings
   - Return "Watched"
   - Result: "Watched"

2. Cleanup and finalize:
   - No new mappings created
   - Status standardized

**Final result**: Movie status set to "Watched"

**Alternative scenario** - New status value:

**Input status**: `"to-upgrade"`

**No existing mapping**:

1. Process "to-upgrade":
   - NOT found in mappings
   - Return "to-upgrade" unchanged
   - Create mapping: "to-upgrade" → "to-upgrade"
   - Save immediately

2. User can later edit in Status Mapping dialog:
   - Change "to-upgrade" → "to-upgrade" to "To Upgrade" (proper casing)
   - Or change to existing category

### Database Cleanup Process

When status values are modified or mappings change, the cleanup process ensures consistency:

**Cleanup trigger**:
- Executes when user clicks OK in Status Mapping dialog
- Runs in background worker (`bwCleanDatabase`)
- Shows progress bar during operation

**Cleanup method** (`Master.DB.Cleanup_Status()`):
1. Retrieves all status values currently assigned to movies in database
2. Applies current mapping rules to all status values
3. Updates movies with newly mapped status values
4. Returns count of items changed

**User feedback**:
- Progress bar shows operation is running
- Message box displays: "X item(s) changed"
- Database immediately reflects new mapping configuration

## Data Persistence (XML Structure)

All mappings are stored in `Core.Mapping.Status.xml` in the settings directory.

**XML structure example**:

`<simplemapping>`
`  <mapping input="0">Unwatched</mapping>`
`  <mapping input="1">Watched</mapping>`
`  <mapping input="seen">Watched</mapping>`
`  <mapping input="unseen">Unwatched</mapping>`
`  <mapping input="true">Watched</mapping>`
`  <mapping input="false">Unwatched</mapping>`
`  <mapping input="yes">Watched</mapping>`
`  <mapping input="no">Unwatched</mapping>`
`  <mapping input="favorite">Favorite</mapping>`
`  <mapping input="fav">Favorite</mapping>`
`  <mapping input="wishlist">Wishlist</mapping>`
`  <mapping input="to-watch">To Watch</mapping>`
`  <mapping input="archive">Archive</mapping>`
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

### Use Case 1: Numeric to Text Conversion

**Problem**: Importing from system that uses numeric status codes

**Solution**: Map numbers to descriptive text

**Example mappings**:
- "0" → "Unwatched"
- "1" → "Watched"
- "2" → "Favorite"
- "3" → "To Watch"
- "4" → "Archive"

**Result**: Human-readable status values in your library

### Use Case 2: Boolean to Status Conversion

**Problem**: Importing from system that uses true/false or yes/no

**Solution**: Map boolean values to watch status

**Example mappings**:
- "true" → "Watched"
- "false" → "Unwatched"
- "yes" → "Watched"
- "no" → "Unwatched"
- "1" → "Watched"
- "0" → "Unwatched"

**Result**: Consistent watch status tracking

### Use Case 3: Standardize Variations

**Problem**: Multiple ways to express same status from different sources

**Solution**: Map all variations to single standard value

**Example mappings**:
- "seen" → "Watched"
- "watched" → "Watched"
- "viewed" → "Watched"
- "unseen" → "Unwatched"
- "not watched" → "Unwatched"
- "fav" → "Favorite"
- "favorite" → "Favorite"
- "favourite" → "Favorite"

**Result**: Consistent status values regardless of import source

### Use Case 4: Custom Organization System

**Problem**: Want to organize library with custom categories

**Solution**: Create your own status vocabulary and map to it

**Example mappings**:
- "kids" → "Family Friendly"
- "date-night" → "Romantic"
- "party" → "Group Viewing"
- "solo" → "Personal Watch"
- "rewatch" → "Rewatch Later"

**Result**: Custom organization matching your viewing patterns

### Use Case 5: Quality Management

**Problem**: Track which movies need quality upgrades

**Solution**: Use status to indicate quality level

**Example mappings**:
- "sd" → "To Upgrade"
- "720p" → "Good Quality"
- "1080p" → "High Quality"
- "4k" → "Best Quality"
- "upgrade" → "To Upgrade"
- "acceptable" → "Good Quality"

**Result**: Easy identification of upgrade candidates

### Use Case 6: Collection Management

**Problem**: Track owned vs. wishlist movies

**Solution**: Use status for collection tracking

**Example mappings**:
- "owned" → "Owned"
- "wishlist" → "Wishlist"
- "borrowed" → "Borrowed"
- "rental" → "Rental"
- "buy" → "To Purchase"

**Result**: Inventory management within media library

## Integration with Other Systems

### Import/Export Integration

Status mappings are particularly useful when importing/exporting movie data:

**Import scenarios**:
- Importing from CSV files
- Migrating from other media managers
- Syncing with external databases
- Processing batch metadata updates

**Export scenarios**:
- Exporting to different format systems
- Creating reports with standardized status
- Sharing library information

### Database Integration

Status is stored directly in the movie database:

**Storage**:
- Single string field per movie
- No separate status table (unlike genres, countries, etc.)
- Stored in main movie record

**Filtering and sorting**:
- Can filter movie list by status
- Can sort by status
- Can use status in custom filters

### Interface Integration

Status values appear throughout the Ember interface:

**Movie list**:
- Status column displays mapped value
- Can be edited inline
- Can be filtered

**Movie details**:
- Status field shows mapped value
- Can be manually edited
- Dropdown shows available status values

**Batch operations**:
- Can set status on multiple movies at once
- Mapping applied automatically to batch updates

## Technical Implementation Details

### Shared Implementation

Status Mapping shares the same implementation as Certification, Country, and Studio mappings:
- Same `clsXMLSimpleMapping` base class
- Same `dlgSimpleMapping` dialog with type parameter
- Same `SimpleMapping` data structure
- Same database cleanup pattern

**Enum distinction**:
The `MappingType` enum in `dlgSimpleMapping.vb` distinguishes the mapping types:
- `StatusMapping` = Status mappings
- `CertificationMapping` = Certification mappings
- `CountryMapping` = Country mappings
- `StudioMapping` = Studio mappings

### Single String Processing

Unlike other mapping types that process lists, Status mapping processes single strings:

**Why single string**:
- Each movie has exactly one status value
- Status is not a multi-valued field
- Simplifies user experience
- Reduces complexity

**Implementation**:
- Uses overloaded `RunMapping(ByRef singleString As String)` method
- No need for list operations like `Distinct()` or multi-item sorting
- Direct string comparison and replacement
- Simpler logic flow

### Manual Status Setting

Status is typically set manually by users, not scrapers:

**User actions**:
- Setting status during movie editing
- Batch status updates on multiple movies
- Importing status from external files
- Status changes during library maintenance

**No scraper integration**:
- Unlike genres, countries, etc., status is not scraped
- Status reflects user organization, not movie metadata
- Purely user-driven field

### Performance Considerations

**Fast lookups**:
- Single string comparison
- No list iteration needed
- Minimal processing overhead

**Database updates**:
- Background worker for cleanup
- Single field update per movie
- Fast execution even for large libraries

**Memory efficiency**:
- Single string value per movie
- No complex data structures
- Minimal memory footprint

## Best Practices

### Define Your Status Vocabulary

**Create a standard set**:
- Decide on your status categories upfront
- Keep the list manageable (5-10 categories)
- Use clear, descriptive names
- Document what each status means

**Example standard set**:
- "Unwatched" - Not yet seen
- "Watched" - Completed viewing
- "Favorite" - Highly rated personal favorites
- "To Watch" - Queued for viewing
- "Archive" - Watched but keeping for reference

### Consistent Naming

**Use proper capitalization**:
- "Watched" not "watched" or "WATCHED"
- "To Watch" not "to watch" or "TO WATCH"
- Consistent formatting makes filtering easier

**Avoid special characters**:
- Use alphanumeric names
- Spaces are fine, but avoid punctuation
- Makes database queries more reliable

### Plan for Imports

**Pre-define mappings**:
- If importing from external systems, set up mappings first
- Map common external values to your standard set
- Prevents auto-creation of unwanted status values

**Test with sample data**:
- Import small batch first
- Check status mapping results
- Adjust mappings before full import

### Regular Maintenance

**Periodic review**:
- Check for auto-created mappings
- Standardize any new entries
- Remove obsolete or duplicate status values

**Consolidate similar values**:
- If you find "Watched" and "watched", map both to standard format
- Clean up variations created by imports
- Keep status list concise

### Use for Workflows

**Create workflow stages**:
- "To Download"
- "Downloaded"
- "To Watch"
- "Watched"
- "To Delete"

**Tracking upgrades**:
- "SD - Upgrade Needed"
- "720p"
- "1080p"
- "4K"

**Collection management**:
- "Physical Copy"
- "Digital Copy"
- "Both Formats"
- "Wishlist"

## Troubleshooting

**Issue**: Status values not mapping correctly during import
**Cause**: Exact string mismatch (case-sensitive)
**Solution**: Check exact format of incoming status values, create exact mappings

**Issue**: Too many different status values in database
**Cause**: Auto-mapping creating variations instead of standardizing
**Solution**: Run database cleanup after defining mappings, consolidate similar values

**Issue**: Mappings not persisting
**Cause**: File permission error on XML file
**Solution**: Check write permissions on settings directory

**Issue**: Database cleanup not updating all movies
**Cause**: Movies may be locked
**Solution**: Check lock status, unlock movies if needed

**Issue**: Status dropdown showing unexpected values
**Cause**: Auto-created status values from imports
**Solution**: Review status mappings, consolidate to standard set, run cleanup

**Issue**: Status field blank after import
**Cause**: Source didn't have status field, or mapping removed value
**Solution**: Check import source, verify mappings aren't mapping to empty string

## Comparison with Other Mapping Types

**Similarities**:
- Same UI dialog and workflow
- Same auto-discovery mechanism
- Same XML storage format
- Same database cleanup process

**Key differences**:

| Feature | Status Mapping | Genre/Country/Studio/Cert |
|---------|----------------|---------------------------|
| Values per item | Single string | List of strings |
| Scraper integration | None (user-set) | Automatic during scraping |
| Common use case | Organization/workflow | Metadata standardization |
| Processing method | Single string | List processing |
| Typical source | Manual/import | Scraper data |

## Summary

The Status Mapping system provides flexible control over movie organization and workflow tracking:

**Key benefits**:
- Simple one-to-one mapping of status strings
- Automatic discovery and 1:1 mapping of new status values
- Easy-to-use grid-based editor
- Immediate database synchronization via cleanup process
- Portable configuration via XML
- Perfect for custom organization schemes
- Essential for import from external systems
- Enables workflow tracking and quality management

**Typical workflow**:
1. Define your standard status vocabulary
2. Set up mappings for expected import values
3. Import movies from external source or set manually
4. Status values automatically mapped to your standards
5. New status values auto-mapped 1:1
6. Periodically review and refine mappings
7. Run database cleanup to apply changes to existing movies
8. Maintain consistent status organization across library

**Best practices**:
- Keep status vocabulary concise (5-10 values)
- Use consistent capitalization
- Pre-define mappings before imports
- Regular maintenance to consolidate variations
- Use status for workflows and organization
- Document what each status category means
- Plan status vocabulary to match your library management style

This system ensures that your movie library maintains a clean, consistent status organization that supports your personal viewing habits, collection management needs, and quality tracking requirements, regardless of how movies are added to your library.