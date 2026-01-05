# BL-CQ-001: IMDB strPosterURL Thread Safety

| Field | Value |
|-------|-------|
| **ID** | BL-CQ-001 |
| **Created** | January 2, 2026 |
| **Priority** | High |
| **Effort** | 3-4 hours |
| **Status** | Open |
| **Category** | Code Quality (CQ) |
| **Related Files** | `Addons\scraper.Data.IMDB\Scraper\clsScrapeIMDB.vb` |

---

## Problem

The class-level field `strPosterURL` is shared state that gets overwritten during parallel scraping operations:

    Private strPosterURL As String = String.Empty  ' Shared class-level state - RACE CONDITION

This field is set in `GetMovieInfo`, `GetTVShowInfo`, and `GetTVEpisodeInfo` methods, then consumed later in `bwIMDB_RunWorkerCompleted`. With parallel scraping, Thread A's poster URL can be overwritten by Thread B before Thread A's event fires.

---

## Root Cause

Shared mutable state at class level being accessed from multiple threads without synchronization.

**Also affects these class-level fields:**
- `json_IMBD_next_data`
- `json_IMDB_Search_Results_next_data`

---

## Recommended Fix

### Step 1: Change ParsePosterURL from Sub to Function

    Private Function ParsePosterURL(ByRef json_data As IMDBJson) As String
        If json_data IsNot Nothing AndAlso
           json_data.props.PageProps.MainColumnData.PrimaryImage IsNot Nothing Then
            Return json_data.props.PageProps.MainColumnData.PrimaryImage.Url
        End If
        Return String.Empty
    End Function

### Step 2: Add PosterURL field to Results structure

    Private Structure Results
        Dim Result As Object
        Dim ResultType As SearchType
        Dim PosterURL As String  ' Add this
    End Structure

### Step 3: Update bwIMDB_DoWork to pass poster URL through results

    Case SearchType.SearchDetails_Movie
        Dim r As MediaContainers.Movie = GetMovieInfo(Args.Parameter, True, Args.Options_Movie)
        e.Result = New Results With {
            .ResultType = SearchType.SearchDetails_Movie,
            .Result = r,
            .PosterURL = localPosterURL  ' Pass through results
        }

### Step 4: Update bwIMDB_RunWorkerCompleted to use passed URL

    Case SearchType.SearchDetails_Movie
        Dim movieInfo As MediaContainers.Movie = DirectCast(Res.Result, MediaContainers.Movie)
        RaiseEvent SearchInfoDownloaded_Movie(Res.PosterURL, movieInfo)  ' Use Res.PosterURL

### Step 5: Use local variables in all Get*Info methods

    Dim localPosterURL As String = String.Empty
    ' ... later ...
    If getposter Then
        localPosterURL = ParsePosterURL(json_IMBD_next_data)
    End If

---

## Testing

1. Test with parallel movie scraping (50+ movies)
2. Verify no poster URL mismatches in search results dialogs
3. Verify correct posters appear in UI after scraping

---

## Notes

- The `strPosterURL = String.Empty` line at the start of methods is defensive coding but doesn't fix the race condition
- Consider same pattern for `json_IMBD_next_data` fields if issues persist
- This is a pre-existing issue exposed by the parallel scraping improvements in Phase 2-2

---

*Created: January 2, 2026*