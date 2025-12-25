# Ember Media Manager - Development Guide

**Solo Developer Reference** - This document serves as a personal development handbook for maintaining the Ember Media Manager project.

---

## Quick Reference

- **Framework:** .NET Framework 4.8
- **Language:** Visual Basic .NET (VB.NET)
- **IDE:** Visual Studio 2019+
- **Total Projects:** 28+ addon projects
- **Coding Standards:** See `.github/copilot-instructions.md`

---

## Development Environment Setup

### Prerequisites
- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- Git for version control

### Initial Setup
1. Clone the repository: `git clone https://github.com/ulrick65/Ember-MM-Newscraper.git`
2. Open `Ember Media Manager.sln` in Visual Studio
3. Restore NuGet packages (right-click solution > Restore NuGet Packages)
4. Build solution to verify everything compiles

### Upstream Sync
- Origin: `https://github.com/ulrick65/Ember-MM-Newscraper` (your fork)
- Upstream: `https://github.com/nagten/Ember-MM-Newscraper` (original repo)

To sync from upstream:

    git fetch upstream
    git checkout master
    git merge upstream/master
    git push origin master

---

## Project Structure

### Core Application
- Main Ember Media Manager application
- Core libraries and shared components

### Addon Categories
- **Scrapers (Data):** TMDB, IMDB, OFDB, TVDB, MoviepilotDE, OMDb, Trakttv
- **Scrapers (Images):** FanartTV, TMDB Poster, TVDB Images
- **Scrapers (Trailers):** TMDB, Apple, YouTube, Davestrailerpage, VideobusterDE
- **Scrapers (Themes):** TelevisionTunes, YouTube Theme
- **Generic Tools:** BulkRename, MediaFileManager, MovieExporter, MetadataEditor, TagManager, FilterEditor, ContextMenu, VideoSourceMapping, Mapping
- **Interfaces:** Kodi, Trakttv

---

## Development Workflow

### Making Changes
1. Create a feature branch: `git checkout -b feature/description`
2. Make changes following coding standards in `copilot-instructions.md`
3. Test affected addons thoroughly
4. Commit with descriptive messages: `git commit -m "Fix: TMDB scraper timeout issue"`
5. Push to origin: `git push origin feature/description`

### Testing Checklist
- [ ] Affected addon compiles without errors
- [ ] No warnings introduced (check Output window)
- [ ] Manual testing performed with real data
- [ ] Edge cases considered (missing data, network failures, etc.)
- [ ] No breaking changes to addon interfaces
- [ ] XML/NFO parsing validated (if applicable)

### Common Tasks

#### Adding a New Scraper Addon
1. Copy existing scraper project as template
2. Update namespace and assembly name
3. Implement required addon interfaces
4. Add project to solution
5. Test integration with core application

#### Updating Scraper API Integration
1. Document API changes in addon README or comments
2. Update HTTP requests and parsing logic
3. Test with multiple movies/shows
4. Handle API rate limiting appropriately
5. Update error handling for new API responses

#### Modifying Core Interfaces
1. **CAUTION:** Changes affect ALL addons
2. Review all 28+ addon projects for compatibility
3. Update affected addons simultaneously
4. Test thoroughly across multiple addon types

---

## Coding Standards Quick Reference

### Naming Conventions
- Private fields: `_httpClient`, `_movieData`
- Public properties: `FolderPath`, `MovieTitle`
- Parameters: `folderPath`, `movieId`
- Local variables: `nfoFiles`, `movieList`
- Methods: `ProcessFiles()`, `ValidateFolder()`

### VB.NET Best Practices
- Always use `Option Strict On` and `Option Explicit On`
- Prefer explicit types over `Object`
- Use `Using` statements for IDisposable objects
- Add XML documentation to public APIs
- Use specific exception types in Try-Catch blocks

### Code Example Pattern

    ' Good: Explicit types, proper disposal, clear naming
    Using httpClient As New HttpClient()
        Dim response As HttpResponseMessage = httpClient.GetAsync(apiUrl).Result
        If response.IsSuccessStatusCode Then
            Dim jsonData As String = response.Content.ReadAsStringAsync().Result
            ' Process data
        End If
    End Using

Full standards: See `.github/copilot-instructions.md`

---

## Debugging Tips

### Addon Not Loading
- Check addon is built to correct output directory
- Verify addon manifest file is present
- Review application logs for initialization errors
- Ensure .NET Framework 4.8 compatibility

### Scraper API Issues
- Test API endpoints directly with browser/Postman
- Check for API changes or deprecations
- Verify API keys/authentication still valid
- Review rate limiting and retry logic

### Build Errors Across Multiple Addons
- Likely a core interface change
- Check recent commits to core libraries
- Review shared dependency versions
- Clean and rebuild entire solution

---

## Release Process

### Pre-Release Checklist
- [ ] All projects build successfully
- [ ] No compiler warnings
- [ ] Version numbers updated in assemblies
- [ ] CHANGELOG.md updated with changes
- [ ] Test with fresh Ember installation
- [ ] Tag release in Git: `git tag v1.x.x`

### Creating Release
1. Build solution in Release configuration
2. Package required assemblies and addons
3. Create GitHub release with tag
4. Upload packaged files
5. Update release notes

---

## Common Issues and Solutions

### Issue: NuGet Restore Fails
**Solution:** Delete `packages` folder and restore again, or check NuGet.config sources

### Issue: Addon Interface Version Mismatch
**Solution:** Rebuild core libraries first, then rebuild all addons

### Issue: Scraper Returns No Results
**Solution:** Check API endpoint, verify response format hasn't changed, review error logs

### Issue: XML Parsing Errors
**Solution:** Validate XML structure, check encoding, handle malformed data gracefully

---

## Important Files

- `.github/copilot-instructions.md` - Comprehensive coding standards
- `Ember Media Manager.sln` - Main solution file
- Individual addon `.vbproj` files in `Addons/` directory

---

## Notes to Self

### When Updating After Long Break
1. Check for upstream changes: `git fetch upstream`
2. Review recent commits to understand what changed
3. Rebuild entire solution to catch any breaking changes
4. Test a few scrapers to verify functionality

### Before Major Refactoring
1. Create a backup branch
2. Document current behavior
3. Plan changes to minimize addon impact
4. Test incrementally, not all at once

### API Key Management
- Store API keys securely (not in source code)
- Document where keys are stored for each scraper
- Update keys before they expire

---

## Resources

- **Repository:** https://github.com/ulrick65/Ember-MM-Newscraper
- **Upstream:** https://github.com/nagten/Ember-MM-Newscraper
- **VB.NET Docs:** https://docs.microsoft.com/en-us/dotnet/visual-basic/
- **.NET Framework 4.8:** https://docs.microsoft.com/en-us/dotnet/framework/

---

**Last Updated:** 2025-12-25