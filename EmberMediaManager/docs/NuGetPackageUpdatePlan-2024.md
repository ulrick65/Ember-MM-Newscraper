# NuGet Package Update Plan - December 2025

**Solution:** Ember Media Manager  
**Target Framework:** .NET Framework 4.8  
**Plan Created:** December 26, 2025  
**Current Status:** ✅ All Phases Complete (except SQLite - Deferred)

---

## Executive Summary

This document outlines the phased approach to updating 21 NuGet packages in the Ember Media Manager solution. The strategy prioritizes safety, testing efficiency, and minimal disruption to the codebase.

### Update Philosophy
- **Phased Approach:** Updates grouped by risk level and dependency relationships
- **Test-Driven:** Each phase requires validation before proceeding
- **Rollback Ready:** Each phase uses separate branches for easy rollback
- **Documentation First:** Track decisions and outcomes for future reference

### Final Results
- **19 packages** successfully updated or removed
- **5 SQLite packages** intentionally deferred (stable, no security issues)
- **0 breaking changes** encountered
- **All 31 projects** build successfully

---

## Update Phases Overview

| Phase | Package Count | Risk Level | Status | Duration |
|-------|--------------|------------|--------|----------|
| Phase 1 | 12 packages | Low | ✅ Complete | 1 day |
| Phase 2 | 4 packages | Medium | ✅ Complete | 1 day |
| Phase 3C | 2 packages | Low | ✅ Complete (Removed) | 1 hour |
| Phase 3B | 1 package | Medium | ✅ Complete | 1 hour |
| Phase 3A | 5 packages | High | ⏸️ Deferred | TBD |

---

## Phase 1: Foundation Layer Updates (Low Risk)

### Packages to Update

| Package Name | Current | Target | Projects Affected |
|-------------|---------|--------|-------------------|
| System.Buffers | 4.5.1 | 4.6.1 | EmberAPI |
| System.Memory | 4.5.5 | 4.6.3 | EmberAPI |
| System.Numerics.Vectors | 4.5.0 | 4.6.1 | EmberAPI |
| System.ValueTuple | 4.5.0 | 4.6.1 | EmberAPI |
| System.Threading.Tasks.Extensions | 4.5.4 | 4.6.3 | EmberAPI |
| System.Runtime.CompilerServices.Unsafe | 6.0.0 | 6.1.2 | EmberAPI |
| System.Security.Cryptography.Algorithms | 4.3.0 | 4.3.1 | EmberAPI, EmberMediaManager, scraper.TMDB.Data |
| System.Security.Cryptography.X509Certificates | 4.3.0 | 4.3.2 | scraper.TMDB.Data |
| System.Runtime | 4.3.0 | 4.3.1 | EmberAPI, EmberMediaManager |
| System.Runtime.Extensions | 4.3.0 | 4.3.1 | EmberAPI, EmberMediaManager |
| System.Net.Primitives | 4.3.0 | 4.3.1 | EmberAPI, EmberMediaManager |
| System.Xml.ReaderWriter | 4.3.0 | 4.3.1 | EmberAPI, EmberMediaManager |

### Benefits
- Security improvements in cryptography packages
- Performance enhancements in memory management
- Bug fixes and stability improvements
- Foundation for modern .NET compatibility

### Implementation Steps

#### Step 1: Preparation
- [x] Create backup of entire solution
- [x] Commit all uncommitted changes to Git
- [x] Document current package versions
- [x] Create branch: nuget-updates-phase1
- [x] Switch to new branch

#### Step 2: Update Packages
- [x] Open NuGet Package Manager for Solution
- [x] Go to Updates tab
- [x] Select all Phase 1 packages
- [x] Click Update
- [x] Review and accept license agreements
- [x] Save all packages.config files

#### Step 3: Build Validation
- [x] Clean solution (Build > Clean Solution)
- [x] Rebuild solution (Build > Rebuild Solution)
- [x] Verify 0 errors, document any warnings
- [x] Build in Debug configuration
- [x] Build in Release configuration

#### Step 4: Functional Testing
- [x] Test EmberAPI core functionality
- [x] Test database connection and basic CRUD operations
- [x] Test TMDB scraper (uses cryptography packages)
- [x] Test IMDB scraper
- [x] Test movie metadata scraping
- [x] Test TV show metadata scraping
- [x] Verify no exceptions in application logs

#### Step 5: Commit and Review
- [x] Stage all changed files
- [x] Commit with message: "Phase 1: Update foundation System.* packages"
- [x] Push branch to remote
- [x] Create pull request (optional)
- [x] Document any issues in "Issues Encountered" section below

### Testing Checklist

#### Critical Paths to Test
- [x] Application starts without errors
- [x] Database connectivity works
- [x] Movie scraping from TMDB works
- [x] Movie scraping from IMDB works
- [x] Image downloading works
- [x] NFO file parsing works
- [x] Async operations complete successfully

#### Performance Verification
- [x] Application startup time (should be same or faster)
- [x] Scraper response times (should be same or faster)
- [x] Memory usage (check Task Manager during operations)

### Issues Encountered

**Date:** December 26, 2024  
**Issue:** Build failed with "Type 'X' is not defined" errors for EmberAPI types (Interfaces, MediaContainers, Structures, etc.) in scraper.Data.TMDB and other addon projects.  
**Resolution:** Root cause was a prior "Code cleanup" commit (442d7516) that removed required `Imports EmberAPI` statements from addon files. Fixed by running `git revert 442d7516 --no-commit` to restore the imports while preserving NuGet updates.  
**Notes:** Lesson learned - always build and test after running Code Cleanup tools before committing. Code analyzers may incorrectly identify project reference imports as "unused".

---

## Phase 2: Modern Microsoft Stack (Medium Risk)

### Packages to Update

| Package Name | Current | Target | Projects Affected |
|-------------|---------|--------|-------------------|
| Microsoft.Bcl.AsyncInterfaces | 9.0.1 | 10.0.1 | EmberAPI |
| System.IO.Pipelines | 9.0.1 | 10.0.1 | EmberAPI |
| System.Text.Encodings.Web | 9.0.1 | 10.0.1 | EmberAPI |
| System.Text.Json | 9.0.1 | 10.0.1 | EmberAPI |

### Benefits
- Latest Microsoft compatibility layer
- Continued support and bug fixes
- Better async/await support
- Improved JSON serialization performance

### Considerations
- These packages work as a cohesive unit
- Must be updated together
- Already using v9.x, so moving to v10 is natural progression
- Breaking changes between 9.x and 10.x are typically minimal for .NET Framework

### Implementation Steps

#### Step 1: Preparation
- [x] Verify Phase 1 is completed and stable
- [x] Create branch: nuget-updates-phase2
- [x] Switch to new branch

#### Step 2: Update Packages
- [x] Open NuGet Package Manager for Solution
- [x] Update all four Phase 2 packages together
- [x] Verify dependency resolution is clean
- [x] Save all packages.config files

#### Step 3: Build Validation
- [x] Clean solution
- [x] Rebuild solution
- [x] Verify 0 errors
- [x] Build in Debug and Release configurations

#### Step 4: Functional Testing
- [x] Test JSON serialization in EmberAPI
- [x] Test async operations in scrapers
- [x] Test HTTP pipeline operations
- [x] Verify no exceptions in application logs
- [x] Test data import/export features
- [x] Test API responses from scrapers

#### Step 5: Performance Testing
- [x] Compare JSON deserialization speed (before/after)
- [x] Check memory usage during large operations
- [x] Verify no performance degradation

#### Step 6: Commit and Review
- [x] Stage all changed files
- [x] Commit with message: "Phase 2: Update modern Microsoft stack packages"
- [x] Push branch to remote
- [x] Document any issues below

### Testing Checklist

#### JSON Operations
- [x] Movie metadata deserialization from TMDB
- [x] TV show metadata deserialization
- [x] NFO file generation and parsing
- [x] Configuration file read/write

#### Async Operations
- [x] Concurrent scraper requests
- [x] Batch metadata updates
- [x] Async file I/O operations

### Issues Encountered

**Date:** December 27, 2024  
**Issue:** None - Phase 2 completed without issues  
**Resolution:** N/A  
**Notes:** All four packages updated successfully. Application runs correctly with no errors or performance degradation.

---

## Phase 3C: .NET Standard/Core Compatibility Packages (Low Risk) ✅ COMPLETE

### Strategy: Try Removal First, Update if Required

These packages are typically transitive dependencies for .NET Standard compatibility. For a pure .NET Framework 4.8 project, they may not be needed.

### Packages Evaluated

| Package Name | Current | Available Update | Strategy | Result |
|-------------|---------|------------------|----------|--------|
| NETStandard.Library | 1.6.1 | 2.0.3 | Try remove first | ✅ Removed |
| Microsoft.NETCore.Platforms | 1.1.0 | 7.0.4 | Try remove first | ✅ Removed |

### Implementation Steps

#### Step 1: Preparation
- [x] Verify master branch is up to date
- [x] Create branch: nuget-updates-phase3c
- [x] Switch to new branch

#### Step 2: Identify Package Locations
- [x] Search all packages.config files for NETStandard.Library
- [x] Search all packages.config files for Microsoft.NETCore.Platforms
- [x] Document which projects reference these packages
- **Found in:** EmberAPI, EmberMediaManager (both with targetFramework="net45")

#### Step 3: Attempt Package Removal
- [x] Remove NETStandard.Library entries from packages.config files
- [x] Remove Microsoft.NETCore.Platforms entries from packages.config files
- [x] Clean solution
- [x] Rebuild solution

#### Step 4: Evaluate Results

**Build succeeded:**
- [x] Packages were transitive dependencies - removal is safe
- [x] Run application and test basic functionality
- [x] Commit removal: "Phase 3C: Remove unnecessary .NET Standard/Core packages"

#### Step 5: Validation
- [x] Application starts without errors
- [x] Basic scraping operations work
- [x] No new warnings or errors in build output

#### Step 6: Complete
- [x] Push branch to remote
- [x] Merge to master
- [x] Document results below

### Results

**NETStandard.Library:**
- [x] Attempted removal
- Result: Build succeeded - package not needed
- Final action: ✅ Removed

**Microsoft.NETCore.Platforms:**
- [x] Attempted removal
- Result: Build succeeded - package not needed
- Final action: ✅ Removed

### Issues Encountered

**Date:** December 27, 2024  
**Issue:** None - packages removed successfully  
**Resolution:** N/A  
**Notes:** Both packages were legacy transitive dependencies with targetFramework="net45". Not needed for .NET Framework 4.8 projects. Build succeeded with 31 projects, 0 errors after removal.

---

## Phase 3B: MovieCollection.OpenMovieDatabase (Medium Risk) ✅ COMPLETE

### Package Updated

| Package Name | Current | Target | Projects Affected |
|-------------|---------|--------|-------------------|
| MovieCollection.OpenMovieDatabase | 2.0.1 | 4.0.3 | scraper.Data.OMDb |

### Risk Assessment
- **Isolated Impact:** Only affects the OMDb scraper addon
- **Major Version Jump:** 2.x to 4.x likely has API changes
- **Third-Party Package:** Not Microsoft maintained
- **Rollback Safe:** Easy to revert if issues arise

### Implementation Steps

#### Step 1: Preparation
- [x] Verify Phase 3C is completed
- [x] Create branch: nuget-updates-phase3b-omdb
- [x] Switch to new branch

#### Step 2: Research Breaking Changes
- [x] Check NuGet page for release notes
- [x] Review any migration documentation
- [x] Identify expected code changes in scraper.Data.OMDb

#### Step 3: Update Package
- [x] Open NuGet Package Manager
- [x] Update MovieCollection.OpenMovieDatabase to 4.0.3
- [x] Save packages.config

#### Step 4: Fix Compilation Errors
- [x] Build solution
- [x] Document any compilation errors in scraper.Data.OMDb
- **Result:** No compilation errors - API remained compatible
- [x] Rebuild until 0 errors

#### Step 5: Functional Testing
- [x] Test OMDb scraper - search by movie title
- [x] Test OMDb scraper - search by IMDb ID
- [x] Verify metadata fields populate correctly
- [x] Test error handling for invalid searches
- [x] Test API rate limiting behavior

#### Step 6: Commit and Complete
- [x] Stage all changed files
- [x] Commit with message: "Phase 3B: Update MovieCollection.OpenMovieDatabase to 4.0.3"
- [x] Push branch to remote
- [x] Merge to master

### Testing Checklist
- [x] OMDb API authentication works
- [x] Movie search by title returns results
- [x] Movie search by IMDb ID returns results
- [x] All metadata fields populated correctly (title, year, plot, rating, etc.)
- [x] Error handling for API failures works
- [x] Application handles missing OMDb API key gracefully

### Issues Encountered

**Date:** December 27, 2024  
**Issue:** None - update completed without issues  
**Resolution:** N/A  
**Notes:** Despite major version jump from 2.0.1 to 4.0.3, no breaking API changes affected the scraper.Data.OMDb project. Build succeeded with 0 errors, application runs correctly.

---

## Phase 3A: SQLite Database Packages (High Risk) - DEFERRED

### Packages to Update

| Package Name | Current | Target | Projects Affected |
|-------------|---------|--------|-------------------|
| System.Data.SQLite | 1.0.119 | 2.0.2 | All projects using database |
| System.Data.SQLite.Core | 1.0.119 | 2.0.2 | All projects using database |
| System.Data.SQLite.EF6 | 1.0.119 | 2.0.2 | All projects using Entity Framework |
| System.Data.SQLite.Linq | 1.0.119 | 2.0.2 | All projects using LINQ to SQL |
| Stub.System.Data.SQLite.Core.NetFramework | 1.0.119 | 2.0.2 | Framework compatibility |

### RECOMMENDATION: CONTINUE TO DEFER

#### Reasons to Skip
1. Current version 1.0.119 is stable and working
2. No known security vulnerabilities
3. No compelling features needed from 2.0.x
4. Major version change = high risk of breaking changes
5. Requires extensive database migration testing
6. Potential Entity Framework compatibility issues
7. Database layer is critical - highest risk component

#### If Update Becomes Necessary in Future

##### Prerequisites
- [ ] Research SQLite 2.0.x breaking changes
- [ ] Review release notes and migration guide
- [ ] Backup production database
- [ ] Create full database test suite
- [ ] Plan rollback strategy

##### Implementation Steps
- [ ] Create isolated test branch: nuget-updates-phase3a-sqlite
- [ ] Update all SQLite packages simultaneously
- [ ] Run comprehensive database tests
- [ ] Test EF6 migrations
- [ ] Test LINQ queries
- [ ] Performance test with production-sized database
- [ ] Test all CRUD operations across all addons

##### Testing Requirements
- [ ] Database connection and initialization
- [ ] Movie CRUD operations
- [ ] TV show CRUD operations
- [ ] Bulk import operations
- [ ] Database cleanup operations
- [ ] Entity Framework migrations
- [ ] LINQ query execution
- [ ] Database performance under load

### Issues Encountered

**Date:** December 27, 2024  
**Issue:** N/A - Phase intentionally deferred  
**Resolution:** N/A  
**Notes:** Decision to defer SQLite updates remains appropriate. Current version is stable, no security vulnerabilities, and major version update poses significant risk to critical database layer.

---

## Progress Tracking

### Overall Progress

    Phase 1:  [##########] 100% Complete ✅
    Phase 2:  [##########] 100% Complete ✅
    Phase 3C: [##########] 100% Complete ✅ (Packages Removed)
    Phase 3B: [##########] 100% Complete ✅
    Phase 3A: [          ] Deferred ⏸️

### Timeline

| Phase | Start Date | Completion Date | Duration | Status |
|-------|-----------|----------------|----------|--------|
| Phase 1 | 12/26/2024 | 12/26/2024 | 1 day | ✅ Complete |
| Phase 2 | 12/27/2024 | 12/27/2024 | 1 day | ✅ Complete |
| Phase 3C | 12/27/2024 | 12/27/2024 | 1 hour | ✅ Complete |
| Phase 3B | 12/27/2024 | 12/27/2024 | 1 hour | ✅ Complete |
| Phase 3A | _____ | _____ | _____ | ⏸️ Deferred |

---

## Git Branch Strategy

### Branch Naming Convention
- Phase 1: nuget-updates-phase1
- Phase 2: nuget-updates-phase2
- Phase 3C: nuget-updates-phase3c
- Phase 3B: nuget-updates-phase3b-omdb
- Phase 3A: nuget-updates-phase3a-sqlite (if needed)

### Merge Strategy
1. Complete phase on feature branch
2. Test thoroughly
3. Push to remote repository
4. Create pull request (optional for review)
5. Merge to master after validation
6. Keep branch for 2 weeks before deletion (rollback safety)

### Branch History

| Branch | Created | Merged | Status |
|--------|---------|--------|--------|
| nuget-updates-phase1 | 12/26/2024 | 12/26/2024 | ✅ Merged & Deleted |
| nuget-updates-phase2 | 12/27/2024 | 12/27/2024 | ✅ Merged & Deleted |
| nuget-updates-phase3c | 12/27/2024 | 12/27/2024 | ✅ Merged |
| nuget-updates-phase3b-omdb | 12/27/2024 | 12/27/2024 | ✅ Merged |

---

## Pre-Update Checklist

Before starting any phase:
- [x] All current work is committed
- [x] Working directory is clean (git status)
- [x] Currently on master branch
- [x] Local master is up to date with remote
- [x] Full backup of solution exists
- [x] Visual Studio is closed
- [x] Documentation is ready

---

## Rollback Procedures

### If Phase Fails

#### Option 1: Git Reset (Preferred)

    git checkout master
    git branch -D nuget-updates-phaseX

#### Option 2: Manual Revert
1. Close Visual Studio
2. Restore packages.config files from backup
3. Delete packages folder in solution root
4. Restore NuGet packages using command: nuget restore "Ember Media Manager.sln"
5. Rebuild solution

#### Option 3: Package Downgrade
1. Use NuGet Package Manager
2. Uninstall updated packages
3. Install specific older versions
4. Rebuild solution

---

## Success Criteria

Each phase is considered successful when:
- All projects build without errors
- No new warnings introduced
- All functional tests pass
- Performance is same or better
- No exceptions in application logs
- Key user scenarios work correctly

---

## Resources and References

### Documentation
- NuGet Package Manager Documentation: https://learn.microsoft.com/en-us/nuget/
- .NET Framework 4.8 Compatibility: https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/

### Package-Specific Resources
- System.Data.SQLite: https://system.data.sqlite.org/
- System.Text.Json: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview
- MovieCollection.OpenMovieDatabase: https://www.nuget.org/packages/MovieCollection.OpenMovieDatabase

### Tools
- NuGet CLI: https://www.nuget.org/downloads
- Visual Studio Package Manager Console

---

## Notes and Observations

### General Notes

**Date:** December 26, 2024  
**Note:** Phase 1 completed successfully. Encountered build errors due to a prior code cleanup commit that removed `Imports EmberAPI` statements. Resolved by reverting that specific commit while preserving NuGet updates. Key takeaway: always test builds after code cleanup operations before committing.

**Date:** December 27, 2024  
**Note:** Phase 2 completed successfully. All four Microsoft stack packages (Microsoft.Bcl.AsyncInterfaces, System.IO.Pipelines, System.Text.Encodings.Web, System.Text.Json) updated from v9.0.1 to v10.0.1 without issues. Application runs correctly with no errors or performance degradation observed.

**Date:** December 27, 2024  
**Note:** Revised Phase 3 execution order. Phase 3C (.NET Standard/Core packages) moved first as lowest risk. Phase 3B (OMDb) second as isolated addon. Phase 3A (SQLite) remains deferred due to high risk to database layer.

**Date:** December 27, 2024  
**Note:** Phase 3C completed - successfully removed NETStandard.Library and Microsoft.NETCore.Platforms packages. These were legacy transitive dependencies not needed for .NET Framework 4.8.

**Date:** December 27, 2024  
**Note:** Phase 3B completed - MovieCollection.OpenMovieDatabase updated from 2.0.1 to 4.0.3 with no breaking changes. All phases complete except SQLite (intentionally deferred).

---

## Final Summary

### Packages Updated (17 total)
| Package | From | To |
|---------|------|-----|
| System.Buffers | 4.5.1 | 4.6.1 |
| System.Memory | 4.5.5 | 4.6.3 |
| System.Numerics.Vectors | 4.5.0 | 4.6.1 |
| System.ValueTuple | 4.5.0 | 4.6.1 |
| System.Threading.Tasks.Extensions | 4.5.4 | 4.6.3 |
| System.Runtime.CompilerServices.Unsafe | 6.0.0 | 6.1.2 |
| System.Security.Cryptography.Algorithms | 4.3.0 | 4.3.1 |
| System.Security.Cryptography.X509Certificates | 4.3.0 | 4.3.2 |
| System.Runtime | 4.3.0 | 4.3.1 |
| System.Runtime.Extensions | 4.3.0 | 4.3.1 |
| System.Net.Primitives | 4.3.0 | 4.3.1 |
| System.Xml.ReaderWriter | 4.3.0 | 4.3.1 |
| Microsoft.Bcl.AsyncInterfaces | 9.0.1 | 10.0.1 |
| System.IO.Pipelines | 9.0.1 | 10.0.1 |
| System.Text.Encodings.Web | 9.0.1 | 10.0.1 |
| System.Text.Json | 9.0.1 | 10.0.1 |
| MovieCollection.OpenMovieDatabase | 2.0.1 | 4.0.3 |

### Packages Removed (2 total)
| Package | Version | Reason |
|---------|---------|--------|
| NETStandard.Library | 1.6.1 | Not needed for .NET Framework 4.8 |
| Microsoft.NETCore.Platforms | 1.1.0 | Not needed for .NET Framework 4.8 |

### Packages Deferred (5 total)
| Package | Version | Reason |
|---------|---------|--------|
| System.Data.SQLite | 1.0.119 | Stable, high risk to update |
| System.Data.SQLite.Core | 1.0.119 | Stable, high risk to update |
| System.Data.SQLite.EF6 | 1.0.119 | Stable, high risk to update |
| System.Data.SQLite.Linq | 1.0.119 | Stable, high risk to update |
| Stub.System.Data.SQLite.Core.NetFramework | 1.0.119 | Stable, high risk to update |

---

## Approval and Sign-off

| Phase | Approved By | Date | Notes |
|-------|------------|------|-------|
| Phase 1 | ulrick65 | 12/26/2024 | Build successful, app runs correctly |
| Phase 2 | ulrick65 | 12/27/2024 | Build successful, app runs correctly |
| Phase 3C | ulrick65 | 12/27/2024 | Packages removed, build successful |
| Phase 3B | ulrick65 | 12/27/2024 | Build successful, app runs correctly |
| Phase 3A | N/A | N/A | Intentionally deferred |

---

**Project Complete: December 27, 2024**

**End of Document**