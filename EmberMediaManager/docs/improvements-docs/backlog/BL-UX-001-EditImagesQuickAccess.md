# BL-UX-001: Edit Images Quick Access

| Field | Value |
|-------|-------|
| **ID** | BL-UX-001 |
| **Created** | January 3, 2026 |
| **Priority** | Medium |
| **Effort** | 1-2 hours |
| **Status** | ✅ Completed |
| **Category** | UI/UX Improvements (UX) |
| **Related Files** | `frmMain.vb`, `frmMain.Designer.vb`, `dlgImgSelect.vb` |

---

## Problem

Currently, to edit images for a movie, the user must:
1. Right-click movie → Edit...
2. Click the "Images" tab
3. Make changes

This is cumbersome when you only want to update images. The `dlgImgSelect` dialog (used during scraping) provides a much better image editing experience with all image types visible at once, but it's only accessible during a scrape operation.

---

## Goal

Add an "Edit Images..." option to the movie context menu that:
1. Scrapes fresh images from configured sources (TMDB, Fanart.tv, etc.)
2. Opens `dlgImgSelect` directly with all image types
3. Saves selected images without modifying NFO or other metadata

---

## Implementation

### Step 1: Add Menu Item to Designer

**File:** `frmMain.Designer.vb`

Find the `cmnuMovie` ContextMenuStrip definition and add a new item after `cmnuMovieEdit`:

    'cmnuMovieEditImages
    '
    Me.cmnuMovieEditImages = New System.Windows.Forms.ToolStripMenuItem()
    Me.cmnuMovieEditImages.Name = "cmnuMovieEditImages"
    Me.cmnuMovieEditImages.Size = New System.Drawing.Size(200, 22)
    Me.cmnuMovieEditImages.Text = "Edit Images..."

Add to the `cmnuMovie.Items.AddRange` array, after `cmnuMovieEdit`:

    Me.cmnuMovieEdit,
    Me.cmnuMovieEditImages,  ' <-- Add this line
    Me.cmnuMovieEditMetaData,

Add the field declaration in the fields region:

    Friend WithEvents cmnuMovieEditImages As System.Windows.Forms.ToolStripMenuItem

---

### Step 2: Add Click Handler

**File:** `frmMain.vb`

Add this method (place near other `cmnuMovie*_Click` handlers):

    ''' <summary>
    ''' Opens the image selection dialog for the selected movie without full edit dialog.
    ''' Scrapes fresh images from configured sources and allows direct image selection.
    ''' </summary>
    Private Sub cmnuMovieEditImages_Click(ByVal sender As Object, ByVal e As EventArgs) _
            Handles cmnuMovieEditImages.Click
        Try
            Cursor = Cursors.WaitCursor

            ' Validate selection
            If currMovie Is Nothing OrElse currMovie.ID = -1 Then
                Cursor = Cursors.Default
                Exit Sub
            End If

            ' Load full movie data from database
            Dim tmpDBElement As Database.DBElement = Master.DB.Load_Movie(currMovie.ID)
            If tmpDBElement Is Nothing Then
                Cursor = Cursors.Default
                Exit Sub
            End If

            ' Check if movie is online
            If Not tmpDBElement.IsOnline AndAlso Not FileUtils.Common.CheckOnlineStatus_Movie(tmpDBElement, True) Then
                Cursor = Cursors.Default
                Exit Sub
            End If

            ' Set up scrape modifiers for ALL image types
            Dim nScrapeModifiers As New Structures.ScrapeModifiers With {
                .MainBanner = Master.eSettings.MovieBannerAnyEnabled,
                .MainClearArt = Master.eSettings.MovieClearArtAnyEnabled,
                .MainClearLogo = Master.eSettings.MovieClearLogoAnyEnabled,
                .MainDiscArt = Master.eSettings.MovieDiscArtAnyEnabled,
                .MainExtrafanarts = Master.eSettings.MovieExtrafanartsAnyEnabled,
                .MainExtrathumbs = Master.eSettings.MovieExtrathumbsAnyEnabled,
                .MainFanart = Master.eSettings.MovieFanartAnyEnabled,
                .MainKeyart = Master.eSettings.MovieKeyartAnyEnabled,
                .MainLandscape = Master.eSettings.MovieLandscapeAnyEnabled,
                .MainPoster = Master.eSettings.MoviePosterAnyEnabled
            }

            ' Scrape images from configured sources
            Dim nSearchResultsContainer As New MediaContainers.SearchResultsContainer
            If Not ModulesManager.Instance.ScrapeImage_Movie(tmpDBElement, nSearchResultsContainer, nScrapeModifiers, True) Then

                ' Check if any images were found
                Dim bImagesFound As Boolean = nSearchResultsContainer.MainBanners.Count > 0 OrElse
                                              nSearchResultsContainer.MainClearArts.Count > 0 OrElse
                                              nSearchResultsContainer.MainClearLogos.Count > 0 OrElse
                                              nSearchResultsContainer.MainDiscArts.Count > 0 OrElse
                                              nSearchResultsContainer.MainFanarts.Count > 0 OrElse
                                              nSearchResultsContainer.MainKeyarts.Count > 0 OrElse
                                              nSearchResultsContainer.MainLandscapes.Count > 0 OrElse
                                              nSearchResultsContainer.MainPosters.Count > 0

                If bImagesFound Then
                    ' Open the image selection dialog
                    Using dlgImgS As New dlgImgSelect
                        If dlgImgS.ShowDialog(tmpDBElement, nSearchResultsContainer, nScrapeModifiers) = DialogResult.OK Then
                            ' Apply selected images back to the movie
                            tmpDBElement.ImagesContainer = dlgImgS.Result.ImagesContainer

                            ' Save to database
                            Master.DB.Save_Movie(tmpDBElement, False, False, True, False, False)

                            ' Save images to disk (images only, no NFO)
                            tmpDBElement.ImagesContainer.SaveAllImages(tmpDBElement, False)

                            ' Refresh the display
                            RefreshRow_Movie(tmpDBElement.ID)
                            LoadInfo_Movie(tmpDBElement.ID)
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

### Step 3: Add Language String (Optional)

If using language resources, add to the appropriate language file:

| ID | English Text |
|----|--------------|
| (new) | Edit Images... |

Or use hardcoded text initially and add to language system later.

---

### Step 4: Enable/Disable Logic

In `cmnuMovie_Opened`, add visibility/enabled logic if needed:

    cmnuMovieEditImages.Enabled = (dgvMovies.SelectedRows.Count = 1)

This ensures the menu item is only enabled when exactly one movie is selected.

---

## Menu Structure After Change

    Right-click on movie
            ↓
    ┌─────────────────────────┐
    │ Edit...                 │
    │ Edit Images...          │  ← NEW
    │ Edit Metadata...        │
    │ ─────────────────────── │
    │ Scrape                  │
    │ ...                     │
    └─────────────────────────┘

---

## Testing Checklist

- [x] Menu item appears in movie context menu
- [x] Menu item disabled when no movie selected
- [x] Menu item disabled when multiple movies selected
- [x] Clicking opens `dlgImgSelect` with scraped images
- [x] All enabled image types appear in dialog
- [x] Selecting images and clicking OK saves them
- [x] Images appear on disk in correct locations
- [x] Main window refreshes to show new images
- [x] Canceling dialog makes no changes
- [x] Works for movies with no existing images
- [x] Works for movies with existing images (replace)
- [x] Error handling for offline movies

---

## Future Enhancements

- Add similar functionality for TV Shows (`cmnuShowEditImages`)
- Add similar functionality for Movie Sets (`cmnuMovieSetEditImages`)
- Add keyboard shortcut (e.g., Ctrl+I)
- Add to main Edit menu as well as context menu

---

*Created: January 2, 2026*
*Completed: January 3, 2026*