# Genre Mapping System in Ember Media Manager

The **Genre Mapping** feature in Ember Media Manager is a system that standardizes and translates genre names from various scrapers into a consistent, customizable set of genres.

## Overview

The Genre Mapping system solves a common problem in media management: different scraper sources use different genre names (e.g., `Abenteuer` vs `Adventure`, `Sci-Fi` vs `Science Fiction`). This system automatically translates and standardizes these variations into a unified genre vocabulary.

In addition to handling genres coming from scrapers at runtime, Ember also **bootstraps** the mapping file from the database on startup (important for first-run scenarios, profile migrations, or older libraries).

## Core Components

### 1. Data Model (clsXMLGenreMapping.vb)

The system uses three main classes located in `EmberAPI\XML Serialization\clsXMLGenreMapping.vb`:

#### clsXMLGenreMapping Class

The main container that manages the entire mapping system:
- **Genres**: `List(Of GenreProperty)` - List of standardized genre names with optional images
- **Mappings**: `List(Of GenreMapping)` - Mapping rules (search strings to target genres)
- **DefaultImage**: `String` - Fallback image filename for genres without a specific image (default: `default.jpg`)
- **FileNameFullPath**: `String` - Path to the XML file storing mappings (settings folder, file: `Core.Mapping.Genres.xml`)

Key methods:
- `Load()` - Loads mappings from XML file (and logs invalid genre names)
- `Save()` - Persists mappings to XML file
- `RunMapping(listToBeMapped, addNewInputs)` - Core runtime mapping logic
- `Sort()` - Sorts genres and mappings alphabetically
- `CloneDeep()` - Creates a deep copy for editing without affecting live data
- `AutoMatchImages(genresPath)` - Assigns images based on matching filenames in `Images\Genres\`

#### GenreProperty Class

Represents a standardized genre in your library:
- **Name**: `String` - The genre name (must be valid per rules described below)
- **Image**: `String` - Optional image filename (e.g., `action.jpg`)
- **isNew**: `Boolean` - Flag indicating newly discovered or auto-created genres needing review

#### GenreMapping Class

Represents a mapping rule that translates incoming genres:
- **SearchString**: `String` - The input genre from scrapers or database (e.g., `Abenteuer`, `Sci-Fi`, `Science Fiction`)
- **MappedTo**: `List(Of String)` - Target genre(s) this maps to (supports one-to-many mapping)
- **isNew**: `Boolean` - Flag indicating newly auto-created mappings needing review

### 2. User Interface (dlgGenreMapping.vb)

Located in `Addons\generic.EmberCore.Mapping\dlgGenreMapping.vb`, this Windows Forms dialog provides an editor with two main panels:

#### Left Panel - Genres Management
- **Genre List**: DataGridView displaying all standardized genres
- **Visual Indicators**:
  - Green text + bold font = New genres needing confirmation
  - Blue background = Genres without assigned images
  - White background = Genres with images
- **Operations**:
  - Add new genres manually
  - Remove genres (also removes from all mappings)
  - Rename genres (updates all mappings automatically)
  - Assign/change genre images
  - Confirm new genres to clear the `isNew` flag

#### Right Panel - Mappings Management
- **Mappings List**: DataGridView showing search string to genre mappings
- **Filter Dropdown**: Filter mappings by target genre or view all
- **Visual Indicators**:
  - Green text + bold font = New mappings needing confirmation
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
- **Change Image**: Browse to select new image from `Images\Genres\`
- **Remove Image**: Clear image assignment

### 3. Menu Integration (genericMapping.vb)

The genre mapping editor is accessible through:
- Main menu: Tools → Genre Mapping
- System tray context menu: Tools → Genre Mapping

After closing the dialog, the system triggers:
- UI refresh to reflect changes
- Database cleanup to remove orphaned genres

## Startup Initialization and Bootstrapping

On application startup, Ember initializes the mapping system in two phases:

### Phase 1: CacheXMLs (clsAPIXML.vb)

During startup, `APIXML.CacheXMLs()` performs the XML load and image folder scan:

- If `Core.Mapping.Genres.xml` exists, it is loaded via `GenreMapping.Load()`.
- If the older file `Core.Genres.xml` exists, it is migrated to `Core.Mapping.Genres.xml` and then loaded.
- The folder `Images\Genres\` is scanned for `.jpg` and `.png`.
- `GenreMapping.AutoMatchImages(genresPath)` is called to match any existing genres to image files.

Important: on first run, the mapping XML may not exist yet. That means the initial `AutoMatchImages` call can run with an empty `Genres` list (no effect). This is expected and is handled by Phase 2.

### Phase 2: Database Bootstrap (clsAPIDatabase.vb)

When the database is loaded, `Database.LoadAll_Genres()` enumerates genres from the database and ensures that:
- `GenreMapping.Mappings` contains an entry for each database genre string.
- Genres are validated and normalized into valid names.
- The mapping XML is created and saved if missing.
- Genre images are auto-linked after the genre list is populated (so images are correctly assigned on first run).

## Valid Genre Naming Rules and Auto-Fix Behavior

To prevent library issues and enforce a predictable image mapping strategy, genre names are validated.

### Validation Rules (clsXMLGenreMapping.vb)
A genre name is considered valid if:
- It is not null/empty/whitespace
- It contains only letters, digits, and hyphens (`-`)
- It does not start or end with a hyphen
- It contains no spaces or special characters

### Auto-Fix Rules
If an input genre contains only spaces as its invalid character(s), Ember will auto-fix it by:
- trimming
- converting spaces to hyphens
- collapsing duplicate hyphens
- trimming leading or trailing hyphens

Example:
- `Science Fiction` becomes `Science-Fiction`

If the input contains special characters (example: `War & Politics`), Ember does not attempt to auto-fix. Instead it creates an empty mapping to prevent repeated re-processing, and the genre is filtered out.

## How the Mapping Process Works (Runtime)

### During Movie/TV Show Scraping

When scrapers retrieve metadata, the genre mapping process is triggered automatically.

#### For Movies (clsAPINFO.vb - MergeDataScraperResults_Movie)

Typical flow:
1. Check conditions (enabled + not locked)
2. Apply mapping: call `APIXML.GenreMapping.RunMapping(scrapedmovie.Genres)`
3. Apply limit: `FilterCountLimit(Master.eSettings.MovieScraperGenreLimit, scrapedmovie.Genres)`
4. Assign to movie: `DBMovie.Movie.Genres = scrapedmovie.Genres`

#### For TV Shows (clsAPINFO.vb - MergeDataScraperResults_TVShow)

Similar flow:
- call `APIXML.GenreMapping.RunMapping(scrapedshow.Genres)`
- limit the list
- assign back to the show

### The RunMapping Algorithm

`RunMapping(listToBeMapped, addNewInputs)` processes each input genre string and produces a standardized output list.

High-level steps:
1. For each input string:
   - If a mapping exists, use its `MappedTo` list
   - If no mapping exists and `addNewInputs` is true:
     - Validate and possibly auto-fix the input
     - Create new Genre and Mapping entries as needed
2. If new items were created during the run:
   - Sort genres + mappings
   - Save immediately
3. Replace the input list with the mapped output list (distinct + sorted)
4. Return whether changes occurred

## Bootstrapping from Database: LoadAll_Genres (Startup)

In addition to scraper-driven discovery, Ember initializes the mapping system from the database.

### What LoadAll_Genres Does

`Database.LoadAll_Genres()` reads the database table `genre` (`strGenre`) and ensures the mapping system is aware of all genres already present in the users library.

For each database genre string:
- If there is no mapping entry:
  - If the genre string is valid:
    - Add to `GenreMapping.Genres` if missing
    - Add a 1:1 mapping (`SearchString` maps to itself)
  - If the genre string is invalid:
    - If it can be auto-fixed (spaces-only case):
      - Create the fixed genre name as a new `GenreProperty` with `isNew = True`
      - Create a mapping from the original invalid string to the fixed name, and mark the mapping `isNew = True`
    - If it cannot be auto-fixed (special characters):
      - Create an empty mapping for the invalid string (so it won’t be repeatedly re-added)
      - Do not add the string as a `GenreProperty` (so it is filtered out from results)

### Image Linking on First Run

Because `APIXML.CacheXMLs()` may run before the mapping XML exists (first run), `LoadAll_Genres()` performs image matching at the correct time:

- After genres are populated from the database and changes are detected, `GenreMapping.AutoMatchImages(genresPath)` runs.
- Then the XML is saved.

This ensures:
- The mapping XML is created immediately when needed.
- The saved XML already includes `GenreProperty.Image` values for any matching image files in `Images\Genres\`.

## Example Bootstrapping Flow

Scenario: Database contains genres: `Action`, `Science Fiction`, `War & Politics`

On first startup (no `Core.Mapping.Genres.xml` yet):
1. `APIXML.CacheXMLs()` scans images and calls `AutoMatchImages` but the genre list might be empty at this moment.
2. `Database.LoadAll_Genres()` reads database genres:
   - `Action` is valid: add `Action` genre and `Action -> Action` mapping
   - `Science Fiction` is invalid but auto-fixable: add `Science-Fiction` genre (marked `isNew = True`) and mapping `Science Fiction -> Science-Fiction` (marked `isNew = True`)
   - `War & Politics` is invalid and not auto-fixable: add empty mapping for `War & Politics` and do not add it as a genre
3. `AutoMatchImages` runs again (now with populated genres) and assigns images if files exist (example: `action.jpg`, `science-fiction.png`)
4. XML is saved, immediately persisting mappings and images

## Genre Image Association

Each genre can have a custom image.

- Image storage folder: `Images\Genres\` under the application directory
- Supported formats: `.jpg`, `.png`
- Auto-matching logic:
  - For any genre where `GenreProperty.Image` is empty, Ember checks for a filename match:
    - Genre name is converted to lowercase
    - Compared to filename without extension (lowercase)
  - If match found, `GenreProperty.Image` stores the matched filename

Example:
- Genre `science-fiction` matches `science-fiction.jpg` and stores `science-fiction.jpg`

## Genre Display in Main UI

### Genre Thumbnails (frmMain.vb)

When a movie or TV show is selected, genre images are displayed as thumbnails in the info panel via the `createGenreThumbs` method.

#### How It Works

1. **Panel Creation**: For each genre, a `Panel` container is created with a nested `PictureBox`
2. **Image Loading**: Genre images are retrieved via `APIXML.GetGenreImage(genreName)`
3. **Tooltips**: The `ToolTips` component displays the genre name on hover

#### Implementation Details

The `createGenreThumbs` method in `frmMain.vb`:
- Accepts a `List(Of String)` of genre names
- Creates dynamically sized panels (68x100) with picture boxes (62x94)
- Positions thumbnails horizontally from right to left in the info panel
- Applies the current theme's `GenrePanelColor` for background styling

#### Tooltip Configuration

Because the `PictureBox` is nested inside a `Panel`, tooltips must be set on **both** controls to ensure consistent hover behavior:

    ToolTips.SetToolTip(pbGenre(i), genres(i))
    ToolTips.SetToolTip(pnlGenre(i), genres(i))

This ensures the genre name tooltip appears whether the user hovers over the image or the panel border area.

#### Styled Tooltip Rendering

The `ToolTips` component uses custom owner-draw rendering for a modern dark-themed appearance:

**Setup** (in `Setup` method):

    With ToolTips
        .OwnerDraw = True
        .InitialDelay = 400
        .ReshowDelay = 100
    End With

**Custom Draw Handler** (`ToolTips_Draw`):
- Dark gradient background (RGB 70,70,74 → 45,45,48)
- Gray border (RGB 100,100,100)
- White centered text using Segoe UI font

This provides a consistent dark UI appearance that matches the application theme, rather than the default Windows tooltip style.

**Note:** The `IsBalloon` property is not used because it overrides custom `OwnerDraw` rendering and reverts to system colors.

#### Related Components

- **ToolTips**: Form-level `ToolTip` component declared in `frmMain.Designer.vb`
- **pbGenre()**: Array of `PictureBox` controls for genre images
- **pnlGenre()**: Array of `Panel` controls containing the picture boxes
- **MoveGenres()**: Repositions genre thumbnails when the info panel is resized

## Data Persistence (XML Structure)

All mappings are stored in `Core.Mapping.Genres.xml` in the settings directory.

The XML includes:
- Default image filename
- Genre list including optional image name and `isNew` flag
- Mapping list including original search strings, mapped targets, and `isNew` flag

Note: invalid search strings may exist in mappings with an empty `MappedTo` list. This is used intentionally to prevent repeated auto-generation and to filter out the invalid genre from results.

## Summary

The Genre Mapping system ensures:
- Scraper-provided genres are normalized and mapped consistently
- Existing database genres are bootstrapped into the mapping system on startup
- Invalid genre strings are either auto-fixed (spaces-only) or filtered out (special characters)
- Mappings and newly created genres are persisted immediately to avoid data loss
- Genre images are auto-linked reliably, including during first-run XML creation

This design ensures that regardless of which scraper sources you use (or what legacy genres exist in your database), your library maintains a clean, consistent, and customizable genre organization.