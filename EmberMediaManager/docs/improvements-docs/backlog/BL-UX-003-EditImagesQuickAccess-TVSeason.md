# BL-UX-003: Edit Images Quick Access - TV Seasons

| Field | Value |
|-------|-------|
| **ID** | BL-UX-003 |
| **Created** | January 4, 2026 |
| **Priority** | Medium |
| **Effort** | 1-2 hours |
| **Status** | ✅ Completed |
| **Category** | UI/UX Improvements (UX) |
| **Related Files** | `frmMain.vb`, `frmMain.Designer.vb`, `dlgImgSelect.vb` |
| **Depends On** | BL-UX-001, BL-UX-002 (pattern established) |

---

## Problem

Same issue as movies and TV shows - editing TV Season images requires going through the full Edit dialog. The `dlgImgSelect` dialog provides a better experience but is only accessible during scraping.

---

## Goal

Add an "Edit Images..." option to the TV Season context menu that:
1. Scrapes fresh images from configured sources (TMDB, TVDB, Fanart.tv, etc.)
2. Opens `dlgImgSelect` directly with all season image types
3. Saves selected images without modifying NFO or other metadata

---

## Implementation

### Step 1: Add Menu Item to Designer

**File:** `frmMain.Designer.vb`

Find the `cmnuSeason` ContextMenuStrip definition and add a new item after `cmnuSeasonEdit`:

    'cmnuSeasonEditImages
    '
    Me.cmnuSeasonEditImages = New System.Windows.Forms.ToolStripMenuItem()
    Me.cmnuSeasonEditImages.Name = "cmnuSeasonEditImages"
    Me.cmnuSeasonEditImages.Size = New System.Drawing.Size(200, 22)
    Me.cmnuSeasonEditImages.Text = "Edit Images..."

Add to the `cmnuSeason.Items.AddRange` array, after `cmnuSeasonEdit`:

    Me.cmnuSeasonEdit,
    Me.cmnuSeasonEditImages,  ' <-- Add this line

Add the field declaration in the fields region:

    Friend WithEvents cmnuSeasonEditImages As System.Windows.Forms.ToolStripMenuItem

---

### Step 2: Add Click Handler

**File:** `frmMain.vb`

Add this method (place near other `cmnuSeason*_Click` handlers):

    ''' <summary>
    ''' Opens the image selection dialog for the selected TV season without full edit dialog.
    ''' Scrapes fresh images from configured sources and allows direct image selection.
    ''' </summary>
    Private Sub cmnuSeasonEditImages_Click(ByVal sender As Object, ByVal e As EventArgs) _
            Handles cmnuSeasonEditImages.Click
        Try
            Cursor = Cursors.WaitCursor

            ' Validate selection
            If dgvTVSeasons.SelectedRows.Count <> 1 Then
                Cursor = Cursors.Default
                Exit Sub
            End If

            Dim indX As Integer = dgvTVSeasons.SelectedRows(0).Index
            Dim ID As Long = Convert.ToInt64(dgvTVSeasons.Item("idSeason", indX).Value)

            ' Load full TV season data from database
            Dim tmpDBElement As Database.DBElement = Master.DB.Load_TVSeason(ID, True, False)
            If tmpDBElement Is Nothing Then
                Cursor = Cursors.Default
                Exit Sub
            End If

            ' Check if show is online
            If Not tmpDBElement.IsOnline AndAlso Not FileUtils.Common.CheckOnlineStatus_TVShow(tmpDBElement, True) Then
                Cursor = Cursors.Default
                Exit Sub
            End If

            ' Set up scrape modifiers for ALL season image types
            Dim nScrapeModifiers As New Structures.ScrapeModifiers With {
                .SeasonBanner = Master.eSettings.TVSeasonBannerAnyEnabled,
                .SeasonFanart = Master.eSettings.TVSeasonFanartAnyEnabled,
                .SeasonLandscape = Master.eSettings.TVSeasonLandscapeAnyEnabled,
                .SeasonPoster = Master.eSettings.TVSeasonPosterAnyEnabled
            }

            ' Scrape images from configured sources
            Dim nSearchResultsContainer As New MediaContainers.SearchResultsContainer
            If Not ModulesManager.Instance.ScrapeImage_TV(tmpDBElement, nSearchResultsContainer, nScrapeModifiers, True) Then

                ' Check if any images were found
                Dim bImagesFound As Boolean = nSearchResultsContainer.SeasonBanners.Count > 0 OrElse
                                              nSearchResultsContainer.SeasonFanarts.Count > 0 OrElse
                                              nSearchResultsContainer.SeasonLandscapes.Count > 0 OrElse
                                              nSearchResultsContainer.SeasonPosters.Count > 0

                If bImagesFound Then
                    ' Open the image selection dialog
                    Using dlgImgS As New dlgImgSelect
                        If dlgImgS.ShowDialog(tmpDBElement, nSearchResultsContainer, nScrapeModifiers) = DialogResult.OK Then
                            ' Apply selected images back to the TV season
                            Images.SetPreferredImages(tmpDBElement, dlgImgS.Result)

                            ' Save to database (downloads images and persists)
                            Master.DB.Save_TVSeason(tmpDBElement, False, True, True)

                            ' Refresh the display
                            RefreshRow_TVSeason(tmpDBElement.ID)
                            LoadInfo_TVSeason(tmpDBElement.ID)
                        End If
                    End Using
                Else
                    MessageBox.Show(Master.eLang.GetString(970, "No Fanarts found"),
                                    String.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End If

        Catch ex As Exception
            logger.Error(ex, New StackFrame().GetMethod().Name)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

---

### Step 3: Enable/Disable Logic

In `cmnuSeason_Opened`, add visibility/enabled logic:

    cmnuSeasonEditImages.Enabled = (dgvTVSeasons.SelectedRows.Count = 1)

This ensures the menu item is only enabled when exactly one TV season is selected.

---

## Key Differences from TV Show Implementation

| Aspect | TV Show | TV Season |
|--------|---------|-----------|
| DataGridView | `dgvTVShows` | `dgvTVSeasons` |
| ID Column | `idShow` | `idSeason` |
| Load Method | `Master.DB.Load_TVShow()` | `Master.DB.Load_TVSeason()` |
| Scrape Method | `ScrapeImage_TVShow()` | `ScrapeImage_TV()` |
| Save Method | `Master.DB.Save_TVShow()` | `Master.DB.Save_TVSeason()` |
| Refresh Method | `RefreshRow_TVShow()` | `RefreshRow_TVSeason()` |
| Load Info Method | `LoadInfo_TVShow()` | `LoadInfo_TVSeason()` |
| Image Types | Banner, CharacterArt, ClearArt, ClearLogo, Extrafanarts, Fanart, Keyart, Landscape, Poster | Banner, Fanart, Landscape, Poster |
| Image Apply | Direct assignment to ImagesContainer | `Images.SetPreferredImages()` |

---

## Menu Structure After Change

    Right-click on TV season
            ↓
    ┌─────────────────────────┐
    │ Edit...                 │
    │ Edit Images...          │  ← NEW
    │ ─────────────────────── │
    │ Scrape                  │
    │ ...                     │
    └─────────────────────────┘

---

## Testing Checklist

- [x] Menu item appears in TV season context menu
- [x] Menu item disabled when no season selected
- [ ] Menu item disabled when multiple seasons selected
- [x] Clicking opens `dlgImgSelect` with scraped images
- [x] All enabled image types appear in dialog (Poster, Banner, Fanart, Landscape)
- [x] Selecting images and clicking OK saves them
- [x] Images appear on disk in correct locations
- [x] Main window refreshes to show new images
- [x] Canceling dialog makes no changes
- [x] Works for seasons with no existing images
- [x] Works for seasons with existing images (replace)
- [x] Error handling for offline shows

---

## Notes

- Uses `ScrapeImage_TV()` instead of a season-specific method (same as `bwTVSeasonScraper_DoWork`)
- Uses `Images.SetPreferredImages()` to apply images (consistent with scraper flow)
- Season images are: Poster, Banner, Fanart, Landscape (no CharacterArt, ClearArt, etc.)

---

*Created: January 4, 2026*
*Completed: January 4, 2026*