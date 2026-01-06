# BL-CQ-004: Fix Fragile Multi-Episode Regex Pattern

| Field | Value |
|-------|-------|
| **ID** | BL-CQ-004 |
| **Category** | Code Quality (CQ) |
| **Priority** | Low |
| **Effort** | 2-3 hrs |
| **Created** | January 7, 2026 |
| **Status** | Open |
| **Related Files** | `clsAPINFO.vb` |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Overview

The regex pattern used to extract multiple episode details from a single NFO file in `LoadFromNFO_TVEpisode` could fail with malformed XML or edge cases.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Current Behavior](#current-behavior)
- [Proposed Solutions](#proposed-solutions)
  - [Option 1: XmlReader Approach](#option-1-xmlreader-approach)
  - [Option 2: XDocument Approach](#option-2-xdocument-approach)
- [Affected Methods](#affected-methods)
- [Related Files](#related-files)
- [Benefits](#benefits)
- [Risks](#risks)
- [Source Reference](#source-reference)

---

## [↑](#table-of-contents) Problem Description

The current regex-based approach for splitting multi-episode NFO files can fail with:

- CDATA sections containing angle brackets
- XML comments between episode blocks
- Attributes with angle brackets in values
- Malformed but parseable XML

---

## [↑](#table-of-contents) Current Behavior

The method uses regex to split multi-episode NFO files:

    Dim sPattern As String = "<episodedetails.*?>.*?</episodedetails>"
    Dim Matches As MatchCollection = Regex.Matches(sXML, sPattern, 
        RegexOptions.IgnoreCase Or RegexOptions.Singleline)

---

## [↑](#table-of-contents) Proposed Solutions

### [↑](#table-of-contents) Option 1: XmlReader Approach

Use XmlReader with fragment parsing capability:

    Using reader As XmlReader = XmlReader.Create(New StringReader(sXML))
        While reader.Read()
            If reader.NodeType = XmlNodeType.Element AndAlso 
               reader.Name = "episodedetails" Then
                Dim episodeXml As String = reader.ReadOuterXml()
                ' Process each episode
            End If
        End While
    End Using

**Pros:**
- Proper XML parsing
- Handles all XML constructs correctly
- Memory efficient (streaming)

**Cons:**
- More complex implementation
- Requires careful error handling

---

### [↑](#table-of-contents) Option 2: XDocument Approach

Use LINQ to XML for parsing:

    ' Wrap in root element to make valid XML
    Dim doc As XDocument = XDocument.Parse("<root>" & sXML & "</root>")
    For Each episode In doc.Descendants("episodedetails")
        Dim episodeXml As String = episode.ToString()
        ' Process each episode
    Next

**Pros:**
- Clean, readable code
- Full XML support
- Easy to debug

**Cons:**
- Loads entire document into memory
- Requires wrapping in root element

---

## [↑](#table-of-contents) Affected Methods

| Method | File |
|--------|------|
| `LoadFromNFO_TVEpisode` | [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) |

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) | NFO loading with multi-episode support |

---

## [↑](#table-of-contents) Benefits

- More robust parsing of multi-episode NFO files
- Better handling of edge cases
- Proper XML handling instead of text manipulation

---

## [↑](#table-of-contents) Risks

- May change behavior for some existing NFO files
- Need backward compatibility testing with various NFO formats

---

## [↑](#table-of-contents) Source Reference

[NfoFileImprovements.md](../NfoFileImprovements.md) - Item 3.2

---

*Created: January 7, 2026*