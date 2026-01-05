# BL-UX-002: Edit Images Quick Access - TV Shows

| Field | Value |
|-------|-------|
| **ID** | BL-UX-002 |
| **Created** | January 3, 2026 |
| **Priority** | Medium |
| **Effort** | 1-2 hours |
| **Status** | ✅ Completed |
| **Category** | UI/UX Improvements (UX) |
| **Related Files** | `frmMain.vb`, `frmMain.Designer.vb`, `dlgImgSelect.vb` |
| **Depends On** | BL-UX-001 (pattern established) |

---

## Problem

Same issue as movies - editing TV Show images requires going through the full Edit dialog. The `dlgImgSelect` dialog provides a better experience but is only accessible during scraping.

---

## Goal

Add an "Edit Images..." option to the TV Show context menu that:
1. Scrapes fresh images from configured sources (TMDB, TVDB, Fanart.tv, etc.)
2. Opens `dlgImgSelect` directly with all image types
3. Saves selected images without modifying NFO or other metadata

---

## Implementation

### Step 1: Add Menu Item to Designer

**File:** `frmMain.Designer.vb`

Find the `cmnuShow` ContextMenuStrip definition and add a new item after `cmnuShowEdit`:

    'cmnuShowEditImages
    '
    Me.cmnuShowEditImages = New System.Windows.Forms.ToolStripMenuItem()
    Me.cmnuShowEditImages.Name = "cmnuShowEditImages"
    Me.cmnuShowEditImages.Size = New System.Drawing.Size(200, 22)
    Me.cmnuShowEditImages.Text = "Edit Images..."

Add to the `cmnuShow.Items.AddRange` array, after `cmnuShowEdit`:

    Me.cmnuShowEdit,
    Me.cmnuShowEditImages,  ' <-- Add this line

Add the field declaration in the fields region:

    Friend WithEvents cmnuShowEditImages As System.Windows.Forms.ToolStripMenuItem

---

### Step 2: Add Click Handler

**File:** `frmMain.vb`

Add this method (place near other `cmnuShow*_Click` handlers):

    ''' <summary>
    ''' Opens the image selection dialog for the selected TV show without full edit dialog.
    ''' Scrapes fresh images from configured sources and allows direct image selection.
    ''' </summary>
    Private Sub cmnuShowEditImages_Click(ByVal sender As Object, ByVal e As EventArgs) _
            Handles cmnuShowEditImages.Click
        Try
            Cursor = Cursors.WaitCursor

            ' Validate selection
            If dgvTVShows.SelectedRows.Count <> 1 Then
                Cursor = Cursors.Default
                Exit Sub
            End If

            Dim indX As Integer = dgvTVShows.SelectedRows(0).Index
            Dim ID As Long = Convert.ToInt64(dgvTVShows.Item("idShow", indX).Value)

            ' Load full TV show data from database
            Dim tmpDBElement As Database.DBElement = Master.DB.Load_TVShow(ID, True, False)
            If tmpDBElement Is Nothing Then
                Cursor = Cursors.Default
                Exit Sub
            End If

            ' Check if show is online
            If Not tmpDBElement.IsOnline AndAlso Not FileUtils.Common.CheckOnlineStatus_TVShow(tmpDBElement, True) Then
                Cursor = Cursors.Default
                Exit Sub
            End If

            ' Set up scrape modifiers for ALL image types
            Dim nScrapeModifiers As New Structures.ScrapeModifiers With {
                .MainBanner = Master.eSettings.TVShowBannerAnyEnabled,
                .MainCharacterArt = Master.eSettings.TVShowCharacterArtAnyEnabled,
                .MainClearArt = Master.eSettings.TVShowClearArtAnyEnabled,
                .MainClearLogo = Master.eSettings.TVShowClearLogoAnyEnabled,
                .MainExtrafanarts = Master.eSettings.TVShowExtrafanartsAnyEnabled,
                .MainFanart = Master.eSettings.TVShowFanartAnyEnabled,
                .MainKeyart = Master.eSettings.TVShowKeyartAnyEnabled,
                .MainLandscape = Master.eSettings.TVShowLandscapeAnyEnabled,
                .MainPoster = Master.eSettings.TVShowPosterAnyEnabled
            }

            ' Scrape images from configured sources
            Dim nSearchResultsContainer As New MediaContainers.SearchResultsContainer
            If Not ModulesManager.Instance.ScrapeImage_TVShow(tmpDBElement, nSearchResultsContainer, nScrapeModifiers, True) Then

                ' Check if any images were found
                Dim bImagesFound As Boolean = nSearchResultsContainer.MainBanners.Count > 0 OrElse
                                              nSearchResultsContainer.MainCharacterArts.Count > 0 OrElse
                                              nSearchResultsContainer.MainClearArts.Count > 0 OrElse
                                              nSearchResultsContainer.MainClearLogos.Count > 0 OrElse
                                              nSearchResultsContainer.MainFanarts.Count > 0 OrElse
                                              nSearchResultsContainer.MainKeyarts.Count > 0 OrElse
                                              nSearchResultsContainer.MainLandscapes.Count > 0 OrElse
                                              nSearchResultsContainer.MainPosters.Count > 0

                If bImagesFound Then
                    ' Open the image selection dialog
                    Using dlgImgS As New dlgImgSelect
                        If dlgImgS.ShowDialog(tmpDBElement, nSearchResultsContainer, nScrapeModifiers) = DialogResult.OK Then
                            ' Apply selected images back to the TV show
                            tmpDBElement.ImagesContainer = dlgImgS.Result.ImagesContainer

                            ' Save to database
                            Master.DB.Save_TVShow(tmpDBElement, False, False, True, False, False)

                            ' Save images to disk (images only, no NFO)
                            tmpDBElement.ImagesContainer.SaveAllImages(tmpDBElement, False)

                            ' Refresh the display
                            RefreshRow_TVShow(tmpDBElement.ID)
                            LoadInfo_TVShow(tmpDBElement.ID)
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

In `cmnuShow_Opened`, add visibility/enabled logic:

    cmnuShowEditImages.Enabled = (dgvTVShows.SelectedRows.Count = 1)

This ensures the menu item is only enabled when exactly one TV show is selected.

---

## Key Differences from Movie Implementation

| Aspect | Movie | TV Show |
|--------|-------|---------|
| DataGridView | `dgvMovies` | `dgvTVShows` |
| ID Column | `idMovie` | `idShow` |
| Load Method | `Master.DB.Load_Movie()` | `Master.DB.Load_TVShow()` |
| Online Check | `CheckOnlineStatus_Movie()` | `CheckOnlineStatus_TVShow()` |
| Scrape Method | `ScrapeImage_Movie()` | `ScrapeImage_TVShow()` |
| Save Method | `Master.DB.Save_Movie()` | `Master.DB.Save_TVShow()` |
| Refresh Method | `RefreshRow_Movie()` | `RefreshRow_TVShow()` |
| Load Info Method | `LoadInfo_Movie()` | `LoadInfo_TVShow()` |
| Has CharacterArt | No | Yes |
| Has DiscArt | Yes | No |
| Has Extrathumbs | Yes | No |

---

## Menu Structure After Change

    Right-click on TV show
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

- [x] Menu item appears in TV show context menu
- [x] Menu item disabled when no show selected
- [x] Menu item disabled when multiple shows selected
- [x] Clicking opens `dlgImgSelect` with scraped images
- [x] All enabled image types appear in dialog (including CharacterArt)
- [x] Selecting images and clicking OK saves them
- [x] Images appear on disk in correct locations
- [x] Main window refreshes to show new images
- [x] Canceling dialog makes no changes
- [x] Works for shows with no existing images
- [x] Works for shows with existing images (replace)
- [x] Error handling for offline shows

---

## Future Enhancements

- Add similar functionality for TV Seasons (`cmnuSeasonEditImages`)
- Add similar functionality for TV Episodes (`cmnuEpisodeEditImages`)

---

*Created: January 2, 2026*
*Completed: January 4, 2026*