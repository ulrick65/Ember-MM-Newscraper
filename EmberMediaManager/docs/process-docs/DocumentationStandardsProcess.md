# Ember Media Manager - Documentation Standards

| | Document Info |
|---------------|---|
| **Version** | 1.0 |
| **Created** | January 4, 2026 |
| **Updated** | January 4, 2026 |
| **Author** | Eric H. Anderson |
| **Purpose** | Standards and guidelines for maintaining consistent project documentation |

##### [← Return to Document Index](../DocumentIndex.md)

---

## Overview

Want to contribute documentation that fits seamlessly with the rest of the project? You're in the right place!

This guide explains how to create and maintain documentation for Ember Media Manager. Following these standards ensures consistency across all documents, making them easier to navigate, maintain, and understand.

**Quick links:**
- Jump to [Document Structure](#document-structure) for the required layout
- See [Formatting Standards](#formatting-standards) for markdown conventions
- Check [Document Types](#document-types) to understand where your doc belongs

---

## Table of Contents

- [Document Structure](#document-structure)
- [Required Elements](#required-elements)
- [Formatting Standards](#formatting-standards)
- [Document Types](#document-types)
- [Folder Organization](#folder-organization)
- [Linking Guidelines](#linking-guidelines)
- [Tables and Lists](#tables-and-lists)
- [Status Indicators](#status-indicators)
- [Tone and Style](#tone-and-style)
- [Maintenance Checklist](#maintenance-checklist)
- [Templates](#templates)

---

## [↑](#table-of-contents) Document Structure

Every document follows the same basic structure. This consistency helps readers know where to find information regardless of which document they're reading.

### [↑](#table-of-contents) Required Document Order

1. **Title** — H1 header with document name
2. **Document Info Table** — Version, dates, author, purpose
3. **Return Link** — Link back to Document Index
4. **Horizontal Rule** — Visual separator
5. **Overview Section** — Welcoming introduction
6. **Table of Contents** — Navigation links
7. **Content Sections** — Each with TOC return link
8. **End Marker** — `*End of file*` or `*End of document*`

### [↑](#table-of-contents) Example Header Structure

    # Ember Media Manager - Your Document Title

    | | Document Info |
    |---------------|---|
    | **Version** | 1.0 |
    | **Created** | January 4, 2026 |
    | **Updated** | January 4, 2026 |
    | **Author** | Your Name |
    | **Purpose** | Brief description of what this document covers |

    ##### [← Return to Document Index](../DocumentIndex.md)

    ---

    ## Overview

    Your welcoming introduction goes here...

    ---

    ## Table of Contents

    - [Section One](#section-one)
    - [Section Two](#section-two)

    ---

    ## [↑](#table-of-contents) Section One

    Content...

    ---

    *End of file*

---

## [↑](#table-of-contents) Required Elements

These elements are **mandatory** for every document:

### [↑](#table-of-contents) Document Info Table

| Field | Required | Description |
|-------|:--------:|-------------|
| **Version** | ✅ | Semantic version (1.0, 1.1, 2.0, etc.) |
| **Created** | ✅ | Long-form date: `January 4, 2026` |
| **Updated** | ✅ | Long-form date of last modification |
| **Author** | ✅ | Original author name |
| **Purpose** | ✅ | One-line description of document's goal |

### [↑](#table-of-contents) Date Format

**Always use long-form dates:**

| Format | Example | Status |
|--------|---------|:------:|
| Long form | January 4, 2026 | ✅ |
| Long form | December 25, 2025 | ✅ |
| Numeric slash | 01/04/2026 | ❌ |
| ISO format | 2026-01-04 | ❌ |
| Abbreviated | Jan 4, 2026 | ❌ |

### [↑](#table-of-contents) Return to Document Index Link

Every document needs a link back to the Document Index immediately after the Document Info table:

    ##### [← Return to Document Index](../DocumentIndex.md)

**Important:** Adjust the path based on the document's location:

| Document Location | Path to Use |
|-------------------|-------------|
| `docs/` | `DocumentIndex.md` |
| `docs/process-docs/` | `../DocumentIndex.md` |
| `docs/improvements-docs/` | `../DocumentIndex.md` |
| `docs/analysis-docs/` | `../DocumentIndex.md` |
| `docs/improvements-docs/backlog/` | `../../DocumentIndex.md` |

### [↑](#table-of-contents) Section TOC Links

**Every section header below the Table of Contents needs a TOC return link** — this includes both main sections (`##`) and subsections (`###`).

Format for main sections:

    ## [↑](#table-of-contents) Section Name

Format for subsections:

    ### [↑](#table-of-contents) Subsection Name

** Every `##` and `###` header after the Table of Contents must have `[↑](#table-of-contents)` immediately after the `##` or `###` markers.

This applies to ALL sections. Even if two sections are adjacent, both need the link because as the sections may expand later, the link ensures easy navigation back to the TOC.

---

## [↑](#table-of-contents) Formatting Standards

### [↑](#table-of-contents) Horizontal Rules

Use horizontal rules (`---`) to separate major sections:
- After the Document Info table and return link
- After the Overview section
- After the Table of Contents
- Between major content sections
- Before the end marker

### [↑](#table-of-contents) Code and File References

Use inline code formatting for:
- File names: `dlgImgSelect.vb`
- Method names: `CreateListImages()`
- Variable names: `tSearchResultsContainer`
- Paths: `EmberMediaManager\docs\`

### [↑](#table-of-contents) Multi-line Code Examples

Use 4-space indentation instead of fenced code blocks:

    Private Sub Example()
        ' Your code here
        Dim value As String = "example"
    End Sub

### [↑](#table-of-contents) File Links

Whenever referencing a file, include a link if the file exists:

    See [dlgImgSelect.vb](../../EmberMediaManager/dlgImgSelect.vb) for the implementation.

For documentation files:

    Related: [ScrapingProcessMovies.md](ScrapingProcessMovies.md)

---

## [↑](#table-of-contents) Document Types

Documents are categorized by their purpose. Place each document in the appropriate folder.

### [↑](#table-of-contents) Process Documents (`process-docs/`)

How things work — architecture, workflows, system behavior.

| Document Type | Example | Purpose |
|---------------|---------|---------|
| System Process | `ScrapingProcessMovies.md` | How a feature works end-to-end |
| Mapping Process | `GenreMappingProcess.md` | How data transformation works |
| Build Process | `BuildProcess.md` | How to build the solution |

### [↑](#table-of-contents) Analysis Documents (`analysis-docs/`)

Investigation and research — understanding problems before solving them.

| Document Type | Example | Purpose |
|---------------|---------|---------|
| Performance Analysis | `PerformanceAnalysis.md` | Identifying bottlenecks |
| Code Analysis | `IMDBScraperAnalysis.md` | Understanding existing code |
| Cleanup Analysis | `SolutionCleanupAnalysis.md` | Technical debt assessment |

### [↑](#table-of-contents) Improvements Documents (`improvements-docs/`)

Plans and implementations — what we're changing and why.

| Document Type | Example | Purpose |
|---------------|---------|---------|
| Phase Plan | `PerformanceImprovements-Phase1.md` | Detailed implementation plan |
| Enhancement Plan | `NfoFileImprovements.md` | Planned improvements |
| Removal Plan | `AddonRemovalPlan.md` | Deprecation tracking |

### [↑](#table-of-contents) Backlog Documents (`improvements-docs/backlog/`)

Known issues and future work — tracked but not yet scheduled.

| Document Type | Example | Purpose |
|---------------|---------|---------|
| Known Issue | `BL-KI-002-EditSeasonImageSelectionBug.md` | Bug documentation |
| Future Enhancement | `BL-FE-001-FeatureName.md` | Feature request |

**Note:** See [FutureEnhancements.md](../improvements-docs/FutureEnhancements.md) for details on the backlog numbering scheme (BL-KI-XXX, BL-FE-XXX, etc.).

### [↑](#table-of-contents) Performance Data (`performance-data/`)

Metrics and measurements — baseline data for comparison.

| Document Type | Example | Purpose |
|---------------|---------|---------|
| Baseline Metrics | `PerformanceData-Movies.md` | Before/after measurements |
| Scraper Metrics | `IMDB-Baseline-2025-12-31.md` | Specific component data |

### [↑](#table-of-contents) Release Notes (`release-notes/`)

Version history — what changed in each release.

| Document Type | Example | Purpose |
|---------------|---------|---------|
| Release Notes | `ReleaseNotes-v1.12.0.0.md` | Changes in a version |

### [↑](#table-of-contents) Archived Documents (`archived-docs/`)

Completed or superseded work — kept for historical reference.

| Document Type | Example | Purpose |
|---------------|---------|---------|
| Completed Plan | `PackageUpdatePlan.md` | Finished initiatives |
| Superseded Doc | `BulkScrapingDocumentation.md` | Replaced by newer doc |

---

## [↑](#table-of-contents) Folder Organization

### [↑](#table-of-contents) Documentation Folder Structure

    EmberMediaManager/
    └── docs/
        ├── DocumentIndex.md          ← Master index (start here)
        ├── ProjectIndex.md           ← Project overview
        ├── ForkChangeLog.md          ← Change history
        │
        ├── process-docs/             ← How things work
        │   ├── BuildProcess.md
        │   ├── ScrapingProcessMovies.md
        │   ├── DocumentationProcess.md
        │   └── ...
        │
        ├── analysis-docs/            ← Investigation & research
        │   ├── PerformanceAnalysis.md
        │   └── ...
        │
        ├── improvements-docs/        ← Plans & implementations
        │   ├── PerformanceImprovements-Phase1.md
        │   ├── FutureEnhancements.md
        │   └── backlog/              ← Known issues & future work
        │       ├── BL-KI-002-*.md
        │       └── ...
        │
        ├── performance-data/         ← Metrics & measurements
        │   ├── PerformanceData-Movies.md
        │   └── ...
        │
        ├── release-notes/            ← Version history
        │   ├── ReleaseNotes-v1.12.0.0.md
        │   └── ...
        │
        └── archived-docs/            ← Completed/superseded
            ├── PackageUpdatePlan.md
            └── ...

### [↑](#table-of-contents) When Creating a New Document

1. **Determine the document type** — What is its purpose?
2. **Choose the correct folder** — See [Document Types](#document-types)
3. **If no folder exists** — Ask before creating a new folder
4. **Update DocumentIndex.md** — Add the new document to the index

---

## [↑](#table-of-contents) Linking Guidelines

### [↑](#table-of-contents) Internal Document Links

Always use relative paths:

    [ScrapingProcessMovies.md](ScrapingProcessMovies.md)           ← Same folder
    [DocumentIndex.md](../DocumentIndex.md)                        ← Parent folder
    [BL-KI-002.md](backlog/BL-KI-002-EditSeasonBug.md)            ← Child folder

### [↑](#table-of-contents) Code File Links

Link to source files when referencing code:

    See [clsAPIDatabase.vb](../../EmberAPI/clsAPIDatabase.vb) for database operations.

### [↑](#table-of-contents) External Links

Use descriptive text for external URLs:

    See [TMDB API Documentation](https://developers.themoviedb.org/3) for details.

### [↑](#table-of-contents) Anchor Links

For linking within a document:

    See [Document Structure](#document-structure) above.

---

## [↑](#table-of-contents) Tables and Lists

### [↑](#table-of-contents) Table Formatting

Use tables for structured data with clear headers:

    | Column 1 | Column 2 | Column 3 |
    |----------|----------|----------|
    | Data 1   | Data 2   | Data 3   |

For alignment:
- Left align (default): `|----------|`
- Center align: `|:--------:|`
- Right align: `|---------:|`

### [↑](#table-of-contents) Bulleted Lists

Use `-` for bullet points, with proper indentation for nesting:

    - First level item
      - Second level item
        - Third level item

### [↑](#table-of-contents) Numbered Lists

Use for sequential steps or ordered items:

    1. First step
    2. Second step
    3. Third step

---

## [↑](#table-of-contents) Status Indicators

Use emoji indicators instead of checkbox syntax for task lists and status tracking:

| Emoji | Meaning | When to Use |
|:-----:|---------|-------------|
| ✅ | Complete/Passed | Task finished, test passed |
| ❌ | Failed/Blocked | Task failed, issue blocking |
| ⏸️ | Deferred/Skipped | Postponed, not doing now |
| 📋 | Future/Planned | On roadmap, not started |
| ⚠️ | Needs Attention | Partial, requires review |

### [↑](#table-of-contents) Example Usage

    | Task | Status |
    |------|:------:|
    | Implement parallel downloads | ✅ |
    | Add cancellation support | ✅ |
    | Optimize database queries | ⏸️ |
    | Add unit tests | 📋 |

### [↑](#table-of-contents) In Text

    The image selection bug is now ✅ resolved.
    SQLite 2.0 update is ❌ blocked due to native DLL issues.
    Theme scraper removal is ⏸️ deferred to next release.

---

## [↑](#table-of-contents) Tone and Style

### [↑](#table-of-contents) Writing Tone

Write as if explaining to a colleague — friendly, welcoming, but professional.

**Avoid stiff phrasing:**

| ❌ Avoid | ✅ Use Instead |
|----------|---------------|
| "This document serves as..." | "Welcome! Here you'll find..." |
| "The purpose of this document is..." | "Need to understand X? You're in the right place." |
| "This file contains information about..." | "Looking for X? Start here." |
| "The following sections describe..." | "Here's what we cover:" |

### [↑](#table-of-contents) Voice and Perspective

- **Use "you" and "your"** — Speak directly to the reader
- **Use active voice** — "The scraper downloads images" not "Images are downloaded by the scraper"
- **Be concise** — Remove unnecessary words

### [↑](#table-of-contents) Overview Section Best Practices

Start with value — tell readers what they'll get:

    ## Overview

    Need to understand how movie scraping works? You're in the right place!

    This guide walks you through the complete scraping pipeline, from user
    initiation to database save. You'll learn about the module architecture,
    data flow, and key optimization points.

    **Quick links:**
    - Jump to [Entry Points](#entry-points) to see how scraping starts
    - Check [Performance Tips](#performance-tips) for optimization guidance

### [↑](#table-of-contents) Navigation Cues

Help readers find what they need:

    **New here?** Start with the [Project Index](ProjectIndex.md).

    **Looking for TV show scraping?** See [ScrapingProcessTvShows.md](ScrapingProcessTvShows.md).

---

## [↑](#table-of-contents) Maintenance Checklist

When creating or updating documentation, verify these items:

### [↑](#table-of-contents) New Document Checklist

| Item | Check |
|------|:-----:|
| Document Info table present with all fields | ⬜ |
| Dates in long form (January 4, 2026) | ⬜ |
| Return to Document Index link present | ⬜ |
| Return link path is correct for folder location | ⬜ |
| Overview section is welcoming, not stiff | ⬜ |
| Table of Contents includes all sections | ⬜ |
| Every `##` and `###` section has TOC return link | ⬜ |
| Horizontal rules separate major sections | ⬜ |
| File references include links where possible | ⬜ |
| Status indicators use emoji, not checkboxes | ⬜ |
| Document added to DocumentIndex.md | ⬜ |
| End marker present (`*End of file*`) | ⬜ |

### [↑](#table-of-contents) Update Checklist

| Item | Check |
|------|:-----:|
| Updated date changed in Document Info table | ⬜ |
| Version incremented if significant changes | ⬜ |
| TOC updated if sections added/removed | ⬜ |
| All new sections have TOC return links | ⬜ |
| Links verified (no broken links) | ⬜ |

---

## [↑](#table-of-contents) Templates

### [↑](#table-of-contents) Standard Document Template

    # Ember Media Manager - Document Title

    | | Document Info |
    |---------------|---|
    | **Version** | 1.0 |
    | **Created** | January 4, 2026 |
    | **Updated** | January 4, 2026 |
    | **Author** | Your Name |
    | **Purpose** | One-line description |

    ##### [← Return to Document Index](../DocumentIndex.md)

    ---

    ## Overview

    Welcome! Here you'll find...

    **Quick links:**
    - [Section One](#section-one)
    - [Section Two](#section-two)

    ---

    ## Table of Contents

    - [Section One](#section-one)
    - [Section Two](#section-two)
    - [See Also](#see-also)

    ---

    ## [↑](#table-of-contents) Section One

    Content goes here...

    ---

    ## [↑](#table-of-contents) Section Two

    More content...

    ---

    ## [↑](#table-of-contents) See Also

    - [Related Document](RelatedDocument.md)
    - [Another Document](AnotherDocument.md)

    ---

    *End of file*

### [↑](#table-of-contents) Bug/Issue Document Template (for `backlog/`)

    # BL-KI-XXX: Brief Issue Title

    | Field | Value |
    |-------|-------|
    | **ID** | BL-KI-XXX |
    | **Created** | January 4, 2026 |
    | **Priority** | High/Medium/Low |
    | **Effort** | Low/Medium/High/TBD |
    | **Status** | Open/In Progress/✅ Resolved |
    | **Category** | Known Issues (KI) / Future Enhancement (FE) |
    | **Related Files** | `file1.vb`, `file2.vb` |

    ---

    ## Problem Description

    Clear description of the issue...

    ---

    ## Steps to Reproduce

    1. Step one
    2. Step two
    3. **Observe:** What happens

    ---

    ## Expected Behavior

    What should happen instead...

    ---

    ## Resolution (if resolved)

    How it was fixed...

    ---

    *Created: January 4, 2026*

---

## [↑](#table-of-contents) See Also

- [DocumentIndex.md](../DocumentIndex.md) — Master documentation index
- [ProjectIndex.md](../ProjectIndex.md) — Project overview and structure
- [ForkChangeLog.md](../ForkChangeLog.md) — Change history and versioning

---

*End of file*