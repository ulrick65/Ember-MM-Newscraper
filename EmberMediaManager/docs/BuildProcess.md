# Ember Media Manager - Build Process Documentation

## Table of Contents

- [Overview](#overview)
- [Build Configuration](#build-configuration)
- [Project Structure](#project-structure)
- [Output Directory Layout](#output-directory-layout)
- [How Projects Build](#how-projects-build)
- [Modifying Build Output Paths](#modifying-build-output-paths)
- [Packaging and Distribution](#packaging-and-distribution)
- [Troubleshooting](#troubleshooting)

---

## Overview

Ember Media Manager uses **MSBuild** (Microsoft Build Engine) to compile the solution. The build system is configured through individual `.vbproj` and `.csproj` project files, which define where each project outputs its compiled assemblies based on the selected **Configuration** (Debug/Release) and **Platform** (x86/x64/AnyCPU).

**Key Characteristics:**
- Target Framework: **.NET Framework 4.8**
- Build Tool: **MSBuild** (part of Visual Studio)
- No custom build scripts or post-build events
- All file placement is handled via `<OutputPath>` settings in project files
- Modular architecture with core application and plugin-based addons

---

## Build Configuration

### Supported Configurations

The solution supports **six build configurations**, which are combinations of:

**Configurations:**
- `Debug` - Includes debug symbols, no optimization
- `Release` - No debug symbols, full optimization

**Platforms:**
- `x86` - 32-bit
- `x64` - 64-bit  
- `AnyCPU` - Platform-agnostic

This results in six total configurations:
- `Debug|x86`
- `Debug|x64`
- `Debug|AnyCPU`
- `Release|x86`
- `Release|x64`
- `Release|AnyCPU`

### Configuration Differences

#### Debug Builds

Debug builds include the following settings:
- **Debug symbols:** Enabled via `<DebugSymbols>true</DebugSymbols>`
- **Debug type:** Full via `<DebugType>full</DebugType>`
- **Optimization:** Disabled via `<Optimize>false</Optimize>`
- **Defines:** Both `DEBUG` and `TRACE` constants defined

**Reference:** See `EmberMediaManager\EmberMediaManager.vbproj`, lines 53-72 for x86 Debug configuration

#### Release Builds

Release builds use these settings:
- **Debug symbols:** Disabled (no DebugSymbols element)
- **Debug type:** None via `<DebugType>None</DebugType>`
- **Optimization:** Enabled via `<Optimize>true</Optimize>`
- **Defines:** Only `TRACE` constant defined

**Reference:** See `EmberMediaManager\EmberMediaManager.vbproj`, lines 73-91 for x86 Release configuration

---

## Project Structure

### Core Projects

The solution contains four core projects:

| Project | Type | Output | Description |
|---------|------|--------|-------------|
| **EmberMediaManager** | WinExe | Ember Media Manager.exe | Main application executable |
| **EmberAPI** | Library | EmberAPI.dll | Core API shared by all addons |
| **KodiAPI** | Library | KodiAPI.dll | Kodi/XBMC integration library |
| **EmberAPI_Test** | Library | EmberAPI_Test.dll | Unit tests for EmberAPI |

### Addon Projects (Modules)

Addons are organized into categories based on their functionality:

#### Generic Addons (generic.EmberCore.*)

These provide core functionality extensions:
- **BulkRename** - Bulk file renaming functionality
- **MediaFileManager** - Media file management operations
- **MovieExport** - Export movie data to various formats
- **MetadataEditor** - Edit media metadata
- **TagManager** - Manage media tags
- **FilterEditor** - Create and edit media list filters
- **ContextMenu** - Context menu integration
- **VideoSourceMapping** - Map video sources
- **Mapping** - General mapping utilities

#### Interface Addons (generic.Interface.*)

External service integrations:
- **Kodi** - Kodi/XBMC interface
- **Trakttv** - Trakt.tv integration

#### Scraper Addons (scraper.*)

**Data Scrapers** - Retrieve movie/TV metadata:
- scraper.Data.TMDB - The Movie Database scraper
- scraper.Data.TVDB - TheTVDB scraper
- scraper.Data.IMDB - IMDb scraper
- scraper.Data.OFDB - Online-Filmdatenbank scraper
- scraper.Data.MoviepilotDE - Moviepilot.de scraper
- scraper.Data.OMDb - OMDb API scraper
- scraper.Data.Trakttv - Trakt.tv data scraper

**Image Scrapers** - Download posters and artwork:
- scraper.Image.TMDB - TMDB poster scraper
- scraper.Image.FanartTV - Fanart.tv image scraper
- scraper.Image.TVDB - TVDB image scraper

**Trailer Scrapers** - Download movie trailers:
- scraper.Trailer.TMDB - TMDB trailer scraper
- scraper.Trailer.YouTube - YouTube trailer scraper
- scraper.Trailer.Apple - Apple trailer scraper
- scraper.Trailer.Davestrailerpage - Dave's trailer page scraper
- scraper.Trailer.VideobusterDE - Videobuster.de trailer scraper

**Theme Scrapers** - Download TV theme music:
- scraper.Theme.TelevisionTunes - Television Tunes theme scraper
- scraper.Theme.YouTube - YouTube theme scraper

---

## Output Directory Layout

### Build Output Root Directories

All projects build into a common root directory at the solution level, named according to the configuration and platform. The directory structure looks like this:

- Solution Root/
  - EmberMM - Debug - x86/
  - EmberMM - Debug - x64/
  - EmberMM - Debug - AnyCPU/
  - EmberMM - Release - x86/
  - EmberMM - Release - x64/
  - EmberMM - Release - AnyCPU/

**Reference:** See `EmberMediaManager\EmberMediaManager.vbproj`, line 57 (and similar lines for other configurations)

### Structure Within Each Build Directory

Taking `EmberMM - Release - x86` as an example, the directory contains:

**Root level files:**
- Ember Media Manager.exe (main executable)
- EmberAPI.dll (core API library)
- KodiAPI.dll (Kodi integration library)
- Multiple .dll files (third-party dependencies from NuGet packages)
- Multiple .xml files (XML documentation)
- Configuration files (.config files):
  - Ember Media Manager.exe.config
  - EmberAPI.dll.config
  - NLog.config

**Subdirectories:**
- Bin/ - Additional binary resources
- DB/ - Database files and scripts
- Defaults/ - Default settings and templates
- Images/ - Application images and icons
- Langs/ - Additional language files
- Modules/ - Addon plugins (detailed below)
- Themes/ - UI themes
- Translations/ - Translation files
- x64/ - 64-bit native dependencies (e.g., SQLite)
- x86/ - 32-bit native dependencies

**Reference:** Structure inferred from installer script `BuildSetup\1.4_InstallerScript.nsi`, lines 120-148

### Modules Subdirectory Structure

Each addon is built into its own subdirectory under the Modules folder. The structure follows this pattern:

- Modules/
  - generic.embercore.bulkrenamer/
    - generic.EmberCore.BulkRename.dll
    - addon-specific dependencies
  - generic.embercore.mediafilemanager/
    - generic.EmberCore.MediaFileManager.dll
    - addon-specific dependencies
  - scraper.data.tmdb/
    - scraper.Data.TMDB.dll
    - TMDbLib.dll (addon-specific NuGet package)
    - other dependencies
  - scraper.image.fanarttv/
    - scraper.Image.FanartTV.dll
    - addon-specific dependencies
  - (additional addons...)

**Important Notes:**
- Folder names are **lowercase** versions of the assembly name
- Each addon gets its own isolated folder
- EmberAPI.dll is **NOT** copied to addon folders (referenced from root)
- Addon-specific NuGet packages **ARE** copied to their respective folders

**Reference:** See `Addons\generic.EmberCore.BulkRename\generic.EmberCore.BulkRename.vbproj`, line 39 for the output path format

---

## How Projects Build

### Understanding MSBuild's Two-Stage Build Process

Before diving into how each project builds, it's important to understand that MSBuild uses a **two-stage build process**:

#### Stage 1: Compilation (Intermediate Files)

**Location Pattern:** `<ProjectFolder>\obj\<Platform>\<Configuration>\`

**Example:** `Addons\scraper.Data.TMDB\obj\x86\Release\`

MSBuild first compiles source code into **intermediate build artifacts**:
- Compiled assemblies (`.dll` or `.exe`) - temporary copies
- Debug symbols (`.pdb` files) if configured
- Build cache files (`.cache`) for dependency resolution
- Compiled resources in `TempPE\` folder
- Assembly manifest and metadata files

**Purpose:**
- Enables incremental builds (only recompile changed files)
- Isolates each project's intermediate files
- Supports parallel builds without conflicts
- Stores compilation metadata for faster subsequent builds

**Important:** These are NOT the final output! The `obj` folder contains temporary compilation artifacts.

#### Stage 2: Copy to Output (Final Files)

**Location:** Defined by `<OutputPath>` in project file

**Example:** `EmberMM - Release - x86\Modules\scraper.data.tmdb\`

After compilation, MSBuild copies files to the final output location:
- Final compiled assemblies from `obj` folder
- All dependencies (NuGet packages marked `Private=True`)
- Configuration files (`.config`, `NLog.config`, etc.)
- XML documentation files
- Content files marked for copying

**This is your deployment directory** - only these files should be distributed.

#### Visual Comparison

For `scraper.Data.TMDB` building in `Release|x86`:

**Intermediate Files (can safely delete):**
- Location: `Addons\scraper.Data.TMDB\obj\x86\Release\`
- Contains: `scraper.Data.TMDB.dll` (temp), `.pdb`, `.cache` files
- Purpose: MSBuild internal use only

**Final Output Files (deploy these):**
- Location: `EmberMM - Release - x86\Modules\scraper.data.tmdb\`
- Contains: `scraper.Data.TMDB.dll` (final), `TMDbLib.dll`, `Newtonsoft.Json.dll`, `NLog.dll`
- Purpose: Application deployment

#### Cleaning Intermediate Files

Visual Studio's "Clean Solution" removes both `obj` folders and the final output directories. To manually clean only intermediate files, use this PowerShell command:

`Get-ChildItem -Path . -Recurse -Directory -Filter "obj" | Remove-Item -Recurse -Force`

This removes all `obj` folders recursively. **Note:** Deleting `obj` folders is safe and sometimes necessary to resolve build issues. MSBuild will recreate them on the next build.

---

### Main Application Build Process

**Project:** `EmberMediaManager\EmberMediaManager.vbproj`

#### Output Path Configuration

Each configuration defines its output path using MSBuild variables. The path is defined as:

`<OutputPath>..\EmberMM - $(Configuration) - $(Platform)\</OutputPath>`

This resolves to paths like:
- `..\EmberMM - Debug - x86\`
- `..\EmberMM - Release - x64\`
- `..\EmberMM - Release - AnyCPU\`

**Reference:** See `EmberMediaManager\EmberMediaManager.vbproj`, lines 57, 75, 95, 108, 125, 138

#### Assembly Properties

The main application is configured with these properties:
- OutputType: WinExe
- AssemblyName: Ember Media Manager
- RootNamespace: Ember_Media_Manager
- TargetFrameworkVersion: v4.8

**Reference:** See `EmberMediaManager\EmberMediaManager.vbproj`, lines 10, 13, 12, 16

This produces the final executable: `Ember Media Manager.exe`

#### Dependencies Handling

The main application references many NuGet packages with the Private element set to True, which means they are **copied to the output directory**. For example, the NLog reference includes:

- Reference Include="NLog, Version=4.0.0.0, ..."
- HintPath: ..\packages\NLog.4.2.3\lib\net45\NLog.dll
- Private: True

**Reference:** See `EmberMediaManager\EmberMediaManager.vbproj`, lines 161-164

### Core Library Build Process

**Project:** `EmberAPI\EmberAPI.vbproj`

#### Output Path

EmberAPI uses the **same output path pattern** as the main application:

`<OutputPath>..\EmberMM - $(Configuration) - $(Platform)\</OutputPath>`

This ensures EmberAPI.dll is placed in the root directory alongside the main executable, where all addons can reference it.

#### Why This Matters

By placing EmberAPI.dll in the root build directory:
- All addons can reference a single copy
- Reduces duplication
- Ensures version consistency
- Simplifies deployment

### Addon Build Process

**Example Project:** `Addons\scraper.TMDB.Data\scraper.Data.TMDB.vbproj`

#### Output Path Pattern

Addons use a different output path that creates module-specific subdirectories:

`<OutputPath>..\..\EmberMM - $(Configuration) - $(Platform)\Modules\scraper.data.tmdb\</OutputPath>`

**Reference:** See `Addons\scraper.TMDB.Data\scraper.Data.TMDB.vbproj`, lines 54-57 for Debug x86 configuration

#### Path Breakdown

Let's break down the path for the TMDB scraper in Debug x86:
- `..\..\` - Navigate up two levels from the addon project directory
- `EmberMM - Debug - x86\` - Target the appropriate build configuration directory
- `Modules\` - Enter the Modules subdirectory
- `scraper.data.tmdb\` - Create/use addon-specific folder (note: lowercase)

#### EmberAPI Reference Configuration

All addons reference EmberAPI with Private set to False:

- ProjectReference Include="..\..\EmberAPI\EmberAPI.vbproj"
- Project: {208AA35E-C6AE-4D2D-A9DD-B6EFD19A4279}
- Name: EmberAPI
- Private: False

**Reference:** See `Addons\scraper.TMDB.Data\scraper.Data.TMDB.vbproj`, lines 280-284

**What this means:**
- `<Private>False</Private>` tells MSBuild NOT to copy EmberAPI.dll to the addon's output folder
- The addon will find EmberAPI.dll in the parent directory at runtime
- This prevents duplicate copies of EmberAPI.dll

#### Addon-Specific Dependencies

Addons may have their own NuGet dependencies with Private set to True. For example, the TMDB scraper uses TMDbLib:

- Reference Include="TMDbLib, Version=1.0.0.0, ..."
- HintPath: ..\..\packages\TMDbLib.1.8.1\lib\net45\TMDbLib.dll
- Private: True (implicitly or explicitly)

**Reference:** See `Addons\scraper.TMDB.Data\scraper.Data.TMDB.vbproj` in the References section

These addon-specific dependencies ARE copied to the module folder.

### Build Order and Dependencies

MSBuild automatically determines build order based on project references:

1. **EmberAPI** builds first (no dependencies on other projects)
2. **KodiAPI** may build in parallel or after EmberAPI
3. **EmberMediaManager** builds after EmberAPI (depends on it)
4. **All Addons** build after EmberAPI (all depend on it)

**Note:** The exact order within each tier may vary, but dependencies are always respected.

---

## Modifying Build Output Paths

### When You Might Need to Change Output Paths

You might want to modify build output paths to:
- Change the output directory structure
- Output to a different drive or location
- Create custom build configurations
- Support new platforms
- Integrate with custom deployment scripts

### How to Modify Output Paths

#### Step 1: Understand the Current Pattern

Main Application and Core Libraries use:
- `<OutputPath>..\EmberMM - $(Configuration) - $(Platform)\</OutputPath>`

Addons use:
- `<OutputPath>..\..\EmberMM - $(Configuration) - $(Platform)\Modules\module.name\</OutputPath>`

#### Step 2: Locate the Configuration

Open the project file (.vbproj or .csproj) in a text editor or Visual Studio. Find the PropertyGroup element for the configuration you want to modify. For example:

`<PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Release|x86'>`

**Reference:** See `EmberMediaManager\EmberMediaManager.vbproj`, line 73 for Release x86

#### Step 3: Modify the OutputPath

Change the OutputPath element within that PropertyGroup. For example, to output to a custom location:

From: `<OutputPath>..\EmberMM - $(Configuration) - $(Platform)\</OutputPath>`

To: `<OutputPath>C:\CustomBuild\EmberMM\$(Configuration)\$(Platform)\</OutputPath>`

**Warning:** Ensure you update ALL configurations (Debug x86, Release x86, Debug x64, etc.) to maintain consistency.

#### Step 4: Update All Related Projects

If you change the main application's output path, you must:
- Update EmberAPI.vbproj to use the same path
- Update KodiAPI.csproj to use the same path
- Update ALL addon projects to point to the new Modules subdirectory

**Critical:** All addons must output to a Modules subfolder relative to where EmberAPI.dll is built, or they won't find the core library at runtime.

#### Step 5: Test the Build

After making changes:
1. Clean the solution (Build > Clean Solution in Visual Studio)
2. Rebuild the entire solution (Build > Rebuild Solution)
3. Verify files are in the expected locations
4. Test the application runs correctly
5. Verify all addons load properly

### Using Solution-Level Configuration (Alternative Approach)

Instead of editing each project file individually, you can use a `Directory.Build.props` file at the solution root to centralize output path configuration. This file does not currently exist in the solution but could be added.

**Benefits:**
- Single location for all output path configuration
- Easier maintenance
- Consistent across all projects

**To implement this:**

Create a file named `Directory.Build.props` in the solution root directory with content like:

- Project element
- PropertyGroup element
- EmberOutputRoot element: $(MSBuildThisFileDirectory)Build\
- EmberConfiguration element: $(Configuration)
- EmberPlatform element: $(Platform)
- Close PropertyGroup
- Close Project

Then update each project file to use these properties:

For main app: `<OutputPath>$(EmberOutputRoot)$(EmberConfiguration) - $(EmberPlatform)\</OutputPath>`

For addons: `<OutputPath>$(EmberOutputRoot)$(EmberConfiguration) - $(EmberPlatform)\Modules\module.name\</OutputPath>`

**Note:** This approach requires MSBuild 15.0 or later (Visual Studio 2017+), which is supported in your environment.

---

## Packaging and Distribution

### Installer Script

The solution includes an NSIS (Nullsoft Scriptable Install System) installer script for creating distributable installers.

**Location:** `BuildSetup\1.4_InstallerScript.nsi`

**Reference:** See `BuildSetup\1.4_InstallerScript.nsi` for complete installer configuration

### What the Installer Packages

The installer script copies files from the build output directory to create an installer. It includes:

**From the build root:**
- Main executable (${emm_filename})
- All DLL files (*.dll)
- Configuration files (*.config)
- XML documentation (*.xml)
- NLog.config

**Subdirectories copied:**
- Bin/ - Binary resources
- DB/ - Database files
- Defaults/ - Default settings
- Images/ - Application images
- Modules/ - All addon plugins (the entire folder structure)
- Themes/ - UI themes
- Translations/ - Translation files
- x64/ - 64-bit native dependencies
- x86/ - 32-bit native dependencies

**Reference:** See `BuildSetup\1.4_InstallerScript.nsi`, lines 116-148

### How to Build the Installer

To create an installer:

1. Build the solution in Release configuration for your target platform (typically x86)
2. Ensure the build output directory is complete
3. Install NSIS (if not already installed) from https://nsis.sourceforge.io/
4. Open the installer script: `BuildSetup\1.4_InstallerScript.nsi`
5. Verify the emm_root, emm_folder, and emm_filename variables point to your build output
6. Right-click the .nsi file and choose "Compile NSIS Script"
7. The installer executable will be created in the BuildSetup directory

### Installer Variables to Configure

Key variables at the top of the installer script that you may need to modify:

- `emm_appname` - Application name (Ember Media Manager)
- `emm_root` - Root path to build output
- `emm_folder` - Build folder name (e.g., EmberMM - Release - x86)
- `emm_filename` - Executable name (Ember Media Manager.exe)
- `emm_outfile` - Output installer filename

**Reference:** See `BuildSetup\1.4_InstallerScript.nsi`, lines 1-40 for variable definitions

### Manual Deployment (Without Installer)

You can also deploy manually by:

1. Build the solution in Release configuration
2. Copy the entire contents of the build output directory (e.g., EmberMM - Release - x86)
3. Deploy to target location
4. Ensure the directory structure remains intact (especially the Modules folder)
5. Ensure all subdirectories (x86, x64, Bin, DB, etc.) are copied

**Important:** The Modules folder structure must be preserved for addons to load correctly.

---

## Troubleshooting

### Common Build Issues

#### Issue: Addons don't appear in the application

**Symptoms:**
- Application builds successfully
- No errors during build
- Addons are not visible in the application

**Possible Causes:**
1. Addon DLLs were not built to the correct location
2. The Modules folder structure is incorrect
3. Addon folder names are not lowercase

**Solution:**
- Verify the addon's OutputPath in its .vbproj file
- Check that the Modules\addon.name\ folder exists in the build output
- Ensure folder names are lowercase (e.g., scraper.data.tmdb not Scraper.Data.TMDB)
- Check the build output window for warnings about copy failures

**Reference:** See the "Modules Subdirectory Structure" section above

#### Issue: Missing dependency DLLs

**Symptoms:**
- Application crashes on startup
- FileNotFoundException for a specific DLL
- AddOn fails to load with dependency error

**Possible Causes:**
1. NuGet package not restored before build
2. Reference marked as Private=False when it should be Private=True
3. Build output path incorrect

**Solution:**
- Restore NuGet packages (Right-click solution > Restore NuGet Packages)
- Check the Reference element for the missing DLL in the project file
- Verify Private element is set correctly:
  - EmberAPI references in addons should be Private=False
  - NuGet packages should be Private=True (or omit the element, True is default)
- Rebuild the solution

**Reference:** See "Dependencies Handling" and "EmberAPI Reference Configuration" sections above

#### Issue: Wrong files in output directory

**Symptoms:**
- Build output contains files from wrong configuration (Debug files in Release)
- Old files not being replaced
- Duplicate or conflicting DLL versions

**Solution:**
- Clean the solution (Build > Clean Solution)
- Delete the build output directories manually
- Rebuild the solution (Build > Rebuild Solution)
- Verify you're building the correct configuration in Visual Studio's toolbar

#### Issue: Build fails with path too long error

**Symptoms:**
- Build error mentioning path length exceeds maximum
- Error code related to file system limitations

**Possible Causes:**
- Output path is too deeply nested
- Project file location is too deeply nested

**Solution:**
- Shorten the output path in project files
- Move the solution to a location with shorter path (e.g., C:\Dev\Ember\)
- Consider using the Directory.Build.props approach with shorter paths

### Verifying Build Output

After building, verify the structure by checking:

1. **Main executable exists:**
   - Path: `EmberMM - Release - x86\Ember Media Manager.exe`

2. **Core library exists:**
   - Path: `EmberMM - Release - x86\EmberAPI.dll`

3. **Addons exist in their folders:**
   - Path: `EmberMM - Release - x86\Modules\scraper.data.tmdb\scraper.Data.TMDB.dll`

4. **Native dependencies exist:**
   - Path: `EmberMM - Release - x86\x86\SQLite.Interop.dll`

5. **Configuration files exist:**
   - Path: `EmberMM - Release - x86\Ember Media Manager.exe.config`
   - Path: `EmberMM - Release - x86\NLog.config`

### Getting Build Output Information

To see detailed build information in Visual Studio:

1. Go to Tools > Options > Projects and Solutions > Build and Run
2. Set "MSBuild project build output verbosity" to Normal or Detailed
3. Rebuild the solution
4. Review the Output window (View > Output) for detailed build information

This shows:
- Exactly where each file is being copied
- Which references are being resolved
- Any warnings or informational messages
- Build timing information

### Clean Build Procedure

If you encounter persistent build issues, perform a clean build:

1. Close Visual Studio
2. Delete all build output directories (EmberMM - Debug - x86, etc.)
3. Delete all bin and obj folders in each project directory
4. Delete the .vs hidden folder in the solution directory
5. Open Visual Studio
6. Restore NuGet packages (Right-click solution > Restore NuGet Packages)
7. Rebuild the entire solution (Build > Rebuild Solution)

---

## Summary

The Ember Media Manager build process is straightforward and relies entirely on MSBuild project file configuration:

**Key Points:**
- All output paths are defined in individual project files via the OutputPath element
- Main application and core libraries build to a common root directory
- Addons build to isolated subdirectories under Modules
- EmberAPI.dll is shared from the root (Private=False in addon references)
- No post-build events or custom scripts are used
- The installer packages the entire build output structure

**To modify the build:**
- Edit OutputPath elements in project files
- Maintain consistency across all projects
- Ensure addon paths remain relative to EmberAPI.dll location
- Test thoroughly after changes

**For questions or issues:**
- Review project files directly (they are XML and human-readable)
- Check the Output window in Visual Studio for detailed build information
- Refer to this documentation for understanding the current structure

---

*Document Version: 1.0*  
*Last Updated: 2024*  
*Related Files: All .vbproj and .csproj files, BuildSetup\1.4_InstallerScript.nsi*