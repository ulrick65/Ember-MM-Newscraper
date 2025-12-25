# MediaInfo Integration - Deep Analysis & Update Guide

## Overview

This document provides a comprehensive analysis of how Ember Media Manager integrates with the MediaInfo library for extracting media file metadata. It includes architectural details, deployment mechanisms, API usage patterns, and a step-by-step guide for safely updating the MediaInfo.dll to newer versions.

**Document Version:** 1.1  
**Last Updated:** December 24, 2025  
**Target Framework:** .NET Framework 4.8
**MediaInfo Version Updated and Tested to Version: 25.10.0.0

---

## Table of Contents

1. [Current Architecture](#current-architecture)
2. [DLL Location & Deployment](#dll-location--deployment)
3. [P/Invoke Declarations](#pinvoke-declarations)
4. [API Usage Pattern](#api-usage-pattern)
5. [Extracted Media Information](#extracted-media-information)
6. [How to Update MediaInfo.dll Safely](#how-to-update-mediainfodll-safely)
7. [Testing Checklist](#testing-checklist)
8. [Rollback Procedure](#rollback-procedure)
9. [Additional Notes](#additional-notes)

---

## Current Architecture

### DLL Location & Deployment

The MediaInfo.dll is deployed through a PostBuildEvent configured in the EmberAPI.vbproj file at line 568.

**PostBuildEvent Command:**

The command `if exist "$(ProjectDir)$(PlatformName)" xcopy "$(ProjectDir)$(PlatformName)" "$(TargetDir)Bin" /E /I /R /Y` copies platform-specific files to the Bin folder.

**File Paths:**
- **Source Location (x64):** `C:\Dev\Ember-MM-Newscraper\EmberAPI\x64\MediaInfo.DLL`
- **Source Location (x86):** `C:\Dev\Ember-MM-Newscraper\EmberAPI\x86\MediaInfo.DLL`
- **Destination:** `{OutputPath}\Bin\MediaInfo.DLL`

**Build Process:**
1. During build, MSBuild checks if the platform-specific folder exists (x64 or x86)
2. If found, all contents are copied to the output Bin folder
3. The application loads the DLL from the Bin subfolder relative to the executable

### P/Invoke Declarations

The application uses native DLL imports defined in `EmberAPI\clsAPIMediaInfo.vb` (lines 392-436). The following functions are imported from the native MediaInfo.dll:

**Core API Functions:**
- `MediaInfo_New()` - Creates a new MediaInfo handle
- `MediaInfo_Open(handle, filename)` - Opens a media file for analysis
- `MediaInfo_Get(handle, streamKind, streamNumber, parameter, kindOfInfo, kindOfSearch)` - Retrieves specific information
- `MediaInfo_Count_Get(handle, streamKind, streamNumber)` - Gets the count of streams
- `MediaInfo_Close(handle)` - Closes the currently open file
- `MediaInfo_Delete(handle)` - Destroys the MediaInfo handle

**ANSI Versions (for non-Windows platforms):**
- `MediaInfoA_Open(handle, filename)` - ANSI version of Open
- `MediaInfoA_Get(handle, streamKind, streamNumber, parameter, kindOfInfo, kindOfSearch)` - ANSI version of Get

**DllImport Attribute:**
All functions use the attribute `<DllImport("Bin\MediaInfo.DLL")>` which tells the CLR to load the DLL from the Bin subdirectory.

**Important Code Pattern:**

The application detects the platform at runtime. On Windows, UseAnsi is set to False (Unicode mode). On other platforms, UseAnsi is set to True (ANSI mode). This allows the application to work on both Windows and Mono/Linux environments.

### API Usage Pattern

**Standard Workflow:**

1. **Initialize Handle**
   - Call `MediaInfo_New()` to create a handle
   - Returns an IntPtr representing the MediaInfo instance

2. **Open Media File**
   - Call `Open(filePath)` wrapper method
   - Internally calls either `MediaInfo_Open` or `MediaInfoA_Open` based on platform

3. **Query Stream Counts**
   - Call `Count_Get(StreamKind.Visual)` for video streams
   - Call `Count_Get(StreamKind.Audio)` for audio streams
   - Call `Count_Get(StreamKind.Text)` for subtitle streams

4. **Extract Metadata**
   - Call `Get_(StreamKind, streamNumber, "ParameterName")` for each property
   - Examples: "Width", "Height", "Duration", "Format", "CodecID", "Language"

5. **Cleanup**
   - Call `MediaInfo_Close(Handle)` to close the file
   - Call `MediaInfo_Delete(Handle)` to free resources

**Example Usage Flow:**

The typical code pattern is: create handle with `MediaInfo_New()`, open file with `Open(sPath)`, get stream count with `Count_Get(StreamKind.Visual)`, loop through streams calling `Get_(StreamKind.Visual, v, "Width")` and similar for each property needed, then cleanup with `Close()`.

### Extracted Media Information

#### Video Stream Properties

**Basic Properties:**
- Width - Video width in pixels
- Height - Video height in pixels
- Codec - Video codec identifier (converted via `ConvertVFormat` function)
- Duration - Playback duration in various formats
- Aspect - Display aspect ratio
- Scantype - Progressive or Interlaced

**Advanced Properties:**
- Bitrate - Video bitrate (formatted via `FormatBitrate` function)
- MultiViewCount - For 3D detection (value > 1 indicates 3D)
- MultiViewLayout - 3D format layout information
- StereoMode - Converted stereo mode for Kodi (e.g., "left_right", "top_bottom")
- Language - Video track language
- Filesize - File size in bytes (or folder size for DVD/Blu-ray)

#### Audio Stream Properties

**Extracted Fields:**
- Codec - Audio codec (converted via `ConvertAFormat` function)
- Channels - Number of audio channels (formatted via `FormatAudioChannel`)
- Bitrate - Audio bitrate
- Language - Audio track language (ISO 639-2 code)
- LongLanguage - Full language name

**Special Handling:**
The `ConvertAFormat` function handles complex audio formats. It detects DTS-HD MA/HR from profile information, identifies Atmos audio (both TrueHD+Atmos and E-AC-3+Atmos), and maps codec IDs to friendly names using XML mapping files.

#### Subtitle Stream Properties

**Extracted Fields:**
- Language - Subtitle language (ISO 639-2 code)
- LongLanguage - Full language name
- Forced - Boolean flag for forced subtitles
- Type - "Embedded" for internal subtitles, "External" for separate files

#### Special Format Support

**DVD Support (VIDEO_TS folders):**
- Scans IFO files for stream information
- Uses `ScanLanguage` function for language detection
- Handles multiple VOB files
- Calculates total folder size

**Blu-ray Support (BDMV folders):**
- Scans BDMV/STREAM directory
- Extracts stream information from M2TS files
- Calculates total disc size

**ISO Image Support:**
- Mounts ISO using DAEMON Tools Lite or VirtualCloneDrive
- Configurable drive letter and tool path
- Timeout setting for mount completion
- Automatically detects VIDEO_TS or BDMV structure
- Unmounts after scanning

---

## How to Update MediaInfo.dll Safely

### Step 1: Download Latest MediaInfo DLL

**Official Source:**
- Website: https://mediaarea.net/en/MediaInfo/Download/Windows
- Select: "DLL without installer" package
- Download both x86 and x64 versions if supporting both architectures

**What You Need:**
- `MediaInfo.dll` - The native library file
- Optionally: Release notes and changelog

**Current Version Check:**
Right-click your existing DLL in Windows Explorer, select Properties, go to Details tab to see current version number.

### Step 2: Verify API Compatibility

**Stable API Functions:**
The MediaInfo C-style API has maintained backward compatibility for many years. The functions used by Ember MM are part of the stable ABI: `MediaInfo_New`, `MediaInfo_Open`, `MediaInfo_Get`, `MediaInfo_Count_Get`, `MediaInfo_Close`, and `MediaInfo_Delete`.

**Compatibility Notes:**
- Parameter names (e.g., "Width", "Format", "Duration") are stable
- Return value formats may include new options but maintain existing formats
- New codec support is additive, not breaking

**Changelog Review:**
Always check the MediaInfo changelog at https://mediaarea.net/MediaInfo/ChangeLog before updating to identify any breaking changes or new features.

### Step 3: Backup & Replace

**Windows Command Prompt Example:**

Navigate to `C:\Dev\Ember-MM-Newscraper\EmberAPI\x64`, then copy `MediaInfo.DLL` to `MediaInfo.DLL.backup`, then copy the new DLL from your download location to `MediaInfo.DLL`.

For x86 builds, navigate to `C:\Dev\Ember-MM-Newscraper\EmberAPI\x86` and repeat the process.

**PowerShell Alternative Example:**

Define source path as the current MediaInfo.DLL location, backup path as the same with .backup extension, and new DLL path as your download location. Use Copy-Item to backup the current file, then Copy-Item with -Force to replace with the new version.

### Step 4: Rebuild and Initial Testing

**Rebuild Process:**
1. Open solution in Visual Studio
2. Select Build > Rebuild Solution
3. Verify no build errors occur
4. Check Output window for PostBuildEvent execution
5. Verify `MediaInfo.DLL` copied to the output Bin folder

**Initial Smoke Test:**
1. Launch Ember Media Manager
2. Navigate to a movie in your library
3. Right-click > Edit Movie > File Info tab
4. Verify that video and audio information displays correctly
5. Check for any error messages or missing data

### Step 5: Comprehensive Testing Scenarios

#### Test 1: Basic Video Scan
**Purpose:** Verify core functionality  
**Test Files:**
- Standard MP4 file (H.264/AAC)
- Standard MKV file (H.264/AC3)

**Verification:**
- Video codec detected correctly
- Resolution (width/height) accurate
- Duration calculated properly
- Audio codec identified
- File size shown

#### Test 2: Multi-Stream Files
**Purpose:** Verify multi-track detection  
**Test Files:**
- MKV with multiple audio tracks (different languages)
- MKV with embedded subtitles
- File with forced subtitles

**Verification:**
- All audio tracks listed
- Audio languages detected correctly
- All subtitle tracks listed
- Forced subtitle flag accurate
- Language codes (ISO 639-2) correct

#### Test 3: Modern Codecs
**Purpose:** Verify new codec support  
**Test Files:**
- HEVC/H.265 encoded file
- VP9 encoded file (if available)
- AV1 encoded file (if available)
- HDR10 or Dolby Vision content (if available)

**Verification:**
- Codec names recognized
- Codec names map to correct flag images
- HDR metadata detected (if applicable)

#### Test 4: Advanced Audio Formats
**Purpose:** Verify enhanced audio detection  
**Test Files:**
- DTS-HD Master Audio
- TrueHD with Atmos
- E-AC-3 with Atmos
- DTS:X (if available)

**Verification:**
- Full codec name detected (including HD/Atmos/X)
- Channel configuration correct (e.g., "5.1", "7.1")
- Bitrate information present

#### Test 5: 3D/Stereo Content
**Purpose:** Verify 3D detection  
**Test Files:**
- Side-by-side 3D file
- Top-bottom 3D file

**Verification:**
- MultiViewCount > 1
- StereoMode correctly identified
- 3D flag/icon displayed (if applicable)

#### Test 6: DVD/Blu-ray Folders
**Purpose:** Verify disc folder scanning  
**Test Files:**
- VIDEO_TS folder structure
- BDMV folder structure

**Verification:**
- IFO files scanned successfully
- Multiple VOB files aggregated
- Total duration calculated
- Audio/subtitle languages detected
- Folder size calculated

#### Test 7: ISO Image Support
**Purpose:** Verify ISO mounting and scanning  
**Prerequisites:** DAEMON Tools or VirtualCloneDrive configured  
**Test Files:**
- DVD ISO image
- Blu-ray ISO image

**Verification:**
- ISO mounts successfully
- Content scanned after mount
- ISO unmounts after scan
- No orphaned mount processes

#### Test 8: Edge Cases
**Purpose:** Verify robustness  
**Test Files:**
- Very large file (>50GB)
- Corrupted/incomplete file
- File with unusual codec combination
- File with no audio track
- File with no metadata

**Verification:**
- No crashes or hangs
- Graceful error handling
- Partial information extracted when possible
- Error messages clear and helpful

### Step 6: Codec Mapping Updates

**When Needed:**
If new codecs return names that don't match existing mappings, you'll need to update the conversion files.

**Mapping Files Location:**
- `EmberAPI\Defaults\DefaultAdvancedSettings - AudioFormatConverts.xml`
- `EmberAPI\Defaults\DefaultAdvancedSettings - VideoFormatConverts.xml`

**Mapping File Structure Example:**

The XML structure contains an `AdvancedSettings` root element with multiple `Setting` child elements. Each Setting has a `Name` element (the raw codec name from MediaInfo) and a `Value` element (the display name to use).

**Common New Mappings:**
- AV1 codec: Add mapping from "av1" or "AV1" to "av1"
- VP9 codec: Add mapping from "vp9" or "VP9" to "vp9"
- Dolby Vision: May need mapping for "dvhe" or "dvh1" profiles

**Process:**
1. Scan a file with the new codec
2. Check what raw value MediaInfo returns
3. Add mapping entry with that raw value as Name
4. Set Value to the desired display name
5. Rebuild and test

**Flag Images:**
If new video sources or codecs need flag images, create PNG images following naming convention like `vcodec_codecname.png` or `acodec_codecname.png`, place them in the `Images\Flags` folder. Image size should match existing flags (typically 48x48 or similar).

### Step 7: Performance Testing

**Large Library Scan:**
1. Select a set of 100+ movies
2. Initiate bulk metadata refresh
3. Monitor for:
   - Memory leaks (check Task Manager)
   - Scan completion time (compare to baseline)
   - Any timeouts or hangs

**Concurrent Scanning:**
1. Start multiple file info queries simultaneously
2. Verify no deadlocks or race conditions
3. Check that handles are properly cleaned up

---

## Testing Checklist

Use this checklist to ensure comprehensive testing after updating MediaInfo.dll:

**Build & Deployment:**
- [ ] Solution builds without errors or warnings
- [ ] PostBuildEvent executes successfully
- [ ] MediaInfo.DLL present in output Bin folder
- [ ] Application launches without DLL load errors

**Basic Functionality:**
- [ ] Standard MP4 file scans correctly
- [ ] Standard MKV file scans correctly
- [ ] Duration displayed accurately
- [ ] File size shown correctly
- [ ] No crashes during normal scanning

**Video Detection:**
- [ ] H.264 codec recognized
- [ ] H.265/HEVC codec recognized
- [ ] Resolution (width/height) accurate
- [ ] Aspect ratio calculated correctly
- [ ] Framerate detected (if shown)
- [ ] Scan type (progressive/interlaced) correct

**Audio Detection:**
- [ ] AC3 codec recognized
- [ ] DTS codec recognized
- [ ] AAC codec recognized
- [ ] Multi-channel audio (5.1, 7.1) detected
- [ ] Audio bitrate shown
- [ ] Multiple audio tracks listed

**Advanced Audio:**
- [ ] DTS-HD MA/HR detected correctly
- [ ] TrueHD with Atmos identified
- [ ] E-AC-3 with Atmos identified
- [ ] Audio channel configuration accurate

**Subtitle Detection:**
- [ ] Embedded subtitles detected
- [ ] Multiple subtitle tracks listed
- [ ] Forced subtitle flag accurate
- [ ] Subtitle languages correct

**Language Support:**
- [ ] Audio languages detected
- [ ] Subtitle languages detected
- [ ] ISO 639-2 codes correct
- [ ] Long language names displayed

**3D/Stereo:**
- [ ] 3D files detected (MultiViewCount > 1)
- [ ] Stereo mode identified correctly
- [ ] Side-by-side format recognized
- [ ] Top-bottom format recognized

**Special Formats:**
- [ ] DVD folders (VIDEO_TS) scan correctly
- [ ] Blu-ray folders (BDMV) scan correctly
- [ ] IFO files parsed successfully
- [ ] ISO images mount and scan (if configured)
- [ ] Total disc size calculated accurately

**Error Handling:**
- [ ] Corrupted files don't crash application
- [ ] Missing files handled gracefully
- [ ] Invalid paths show appropriate error
- [ ] Timeout scenarios handled properly

**Performance:**
- [ ] Single file scan completes quickly (<2 seconds typical)
- [ ] Bulk scanning doesn't hang or timeout
- [ ] Memory usage stable (no leaks)
- [ ] No handle leaks (verify with handle monitoring tool)

**User Interface:**
- [ ] File Info tab displays all data correctly
- [ ] Media flags/icons display correctly
- [ ] Codec names readable and accurate
- [ ] Duration formatted properly (hours/minutes/seconds)

---

## Rollback Procedure

If you encounter issues after updating MediaInfo.dll, follow this rollback procedure:

### Immediate Rollback

Navigate to the appropriate folder (x64 or x86) and copy the backup file back to `MediaInfo.DLL`, overwriting the new version.

For x86, repeat the same process in the x86 folder.

### Rebuild After Rollback

1. In Visual Studio: Build > Clean Solution
2. Build > Rebuild Solution
3. Verify backup DLL copied to output Bin folder
4. Test basic functionality to confirm rollback successful

### Alternative: Manual Replacement

If backup file missing or corrupted:
1. Download the previous working version from MediaInfo archives
2. Extract and replace the DLL in both x64 and x86 folders
3. Rebuild solution

### Post-Rollback Actions

1. Document the specific issue encountered
2. Check MediaInfo GitHub issues or forums for known problems
3. Consider reporting the issue if it's a regression
4. Wait for a subsequent MediaInfo update that fixes the issue

---

## Additional Notes

### Platform Considerations

**Windows (Unicode Mode):**
- Uses wide-character (Unicode) API functions
- `MediaInfo_Open` and `MediaInfo_Get` called directly
- String marshaling handled automatically by .NET

**Linux/Mono (ANSI Mode):**
- Uses ANSI API functions
- `MediaInfoA_Open` and `MediaInfoA_Get` used instead
- Manual string marshaling required (StringToHGlobalAnsi)

**Detection Logic:**

The code checks if `Master.isWindows` is true. If yes, `UseAnsi` is set to False. Otherwise, `UseAnsi` is set to True.

### Thread Safety

**Current Implementation:**
- Each scan creates a new MediaInfo handle
- Handle is local to the scanning operation
- No shared state between concurrent scans

**Implications:**
- Multiple files can be scanned in parallel safely
- Each thread has its own MediaInfo instance
- No locking required for MediaInfo operations

**Best Practice:**
Always ensure `MediaInfo_Close` and `MediaInfo_Delete` are called, even if exceptions occur. Current code should be reviewed for try-finally blocks.

### ISO Scanning Configuration

**Requirements:**
- DAEMON Tools Lite OR VirtualCloneDrive installed
- Drive letter configured in settings
- Tool path configured correctly

**Settings Location:**
- Drive Letter: `Master.eSettings.GeneralDaemonDrive`
- Tool Path: `Master.eSettings.GeneralDaemonPath`
- Timeout: Advanced setting "GeneralDaemonTimeout" (milliseconds)

**Mount/Unmount Commands:**

VirtualCloneDrive uses `VCDMount.exe /u` to unmount and `VCDMount.exe "path\to\image.iso"` to mount.

DAEMON Tools uses `DTAgent.exe -unmount dt, E` to unmount and `DTAgent.exe -mount dt, E, "path\to\image.iso"` to mount.

### Version History Notes

**TestMediaInfoDLL Function:**
Located in `clsAPICommon.vb` line 1729. This function previously checked MediaInfo version to determine ISO support. Current implementation always sets `Master.CanScanDiscImage = True` and assumes modern MediaInfo versions support ISO. Legacy version check code is commented out.

**Implication:**
The application expects MediaInfo version 0.7.11 or newer, which includes ISO support. All modern versions meet this requirement.

### Performance Optimization

**Caching Strategy:**
- MediaInfo results are stored in `DBElement.Movie.FileInfo`
- Subsequent accesses use cached data unless refresh triggered
- Lock settings prevent accidental overwriting of manually edited data

**Bulk Operations:**
- File info can be refreshed in bulk
- Each file scanned sequentially (not parallelized at MediaInfo level)
- Consider parallel processing at application level if performance critical

### Known Limitations

**Not Supported:**
- Remote files (network paths may work but not officially supported)
- Streaming URLs
- Encrypted content (without proper decryption)

**Special Cases:**
- Stacked files (e.g., CD1/CD2) require special handling
- Multi-part archives (RAR, etc.) may not scan correctly
- Some proprietary formats may have limited support

### Debugging Tips

**Enable Logging:**
The application uses NLog for logging. Check logs for MediaInfo-related errors.

**Common Issues:**
- "DLL not found": Check that MediaInfo.DLL is in the Bin folder
- "Invalid handle": Ensure MediaInfo_New succeeded before calling other functions
- "Timeout": For ISO mounting, increase GeneralDaemonTimeout setting
- "Access denied": Check file permissions, especially for network shares

**Testing MediaInfo Directly:**
Use the MediaInfo GUI application to verify that a file can be scanned outside of Ember MM. If MediaInfo GUI works but Ember doesn't, the issue is in the integration code.

### Maintenance Recommendations

**Regular Updates:**
- Check for MediaInfo updates quarterly
- Review changelog for security fixes or critical bug fixes
- Test updates in development environment before production

**Backup Strategy:**
- Keep previous working version backed up
- Document the version number in use
- Archive release notes with the DLL

**Monitoring:**
- Watch for user reports of scanning issues
- Monitor MediaInfo project for breaking changes
- Subscribe to MediaInfo mailing list or GitHub notifications

---

## Conclusion

Updating MediaInfo.dll in Ember Media Manager is a **low-risk operation** due to the stable C API used by the library. The main considerations are:

1. **API Stability:** The core functions have remained stable for many years
2. **Codec Mapping:** New codec names may require mapping updates
3. **Testing Focus:** Comprehensive testing ensures new formats are recognized
4. **Rollback Ready:** Always maintain backups for quick rollback if needed

By following this guide, you can safely update to the latest MediaInfo version and take advantage of:
- New codec support (AV1, VP9, enhanced HDR detection)
- Improved format detection
- Bug fixes and performance improvements
- Better handling of modern media files

**Recommended Update Frequency:**
Update MediaInfo when:
- A new major codec becomes common (e.g., AV1 adoption)
- Security vulnerabilities are reported
- Significant bug fixes are released
- Annually as part of routine maintenance

Keep this document updated with your specific experiences and any project-specific customizations to the MediaInfo integration.