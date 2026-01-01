# TV Show Scraping Performance Data

| Document Info | |
|---------------|---|
| **Created** | December 31, 2025 |
| **Updated** | December 31, 2025 |
| **Author** | Eric H. Anderson |
| **Purpose** | Track TV show scraping performance metrics over time |

---

## Table of Contents

- [Overview](#overview)
- [Test Configuration](#test-configuration)
- [New Baseline: December 31, 2025](#new-baseline-december-31-2025)

---

## Overview

This document captures TV show scraping performance metrics to track improvements and identify bottlenecks. Data is collected using the `PerformanceTracker` instrumentation across IMDB, TMDB, and TVDB scrapers.

---

## Test Configuration

### Standard Test Set

| # | TV Show | Seasons | Episodes | Notes |
|---|---------|---------|----------|-------|
| 1 | Show 1 | - | - | - |
| 2 | Show 2 | - | - | - |
| 3 | Show 3 | - | - | - |
| 4 | Show 4 | - | - | - |
| 5 | Show 5 | - | - | - |
| 6 | Show 6 | - | - | - |

**Totals:** 6 TV shows, 23 seasons, 209 episodes

### Scraper Configuration

- **Data Scrapers:** TMDB, TVDB, IMDB (all enabled)
- **Image Scrapers:** All enabled
- **Test Method:** Clean scrape of all items

---

## New Baseline: December 31, 2025

### Test Environment

- **Branch:** feature/performance-improvements-phase4
- **Scrapers:** IMDB + TMDB + TVDB (multi-scraper mode)
- **Content:** 6 TV shows, 23 seasons, 209 episodes

### Raw Metrics

| Operation | Count | Min (ms) | Avg (ms) | Max (ms) | Total (ms) |
|-----------|-------|----------|----------|----------|------------|
| Database.Add_Actor | 6,689 | 0.01 | 0.21 | 2.70 | 1,402.84 |
| Image.LoadFromWebAsync | 307 | 36.06 | 93.37 | 1,296.99 | 28,664.20 |
| Image.LoadFromWebAsync.Download | 307 | 36.04 | 93.29 | 1,294.41 | 28,639.54 |
| ImagesContainer.SaveAllImagesAsync.TVEpisode | 209 | 56.27 | 65.39 | 105.43 | 13,667.38 |
| ImagesContainer.SaveAllImagesAsync.TVEpisode.ParallelDownload | 209 | 49.50 | 59.74 | 99.21 | 12,486.68 |
| ImagesContainer.SaveAllImagesAsync.TVEpisode.SaveToDisk | 209 | 3.63 | 5.01 | 8.28 | 1,046.14 |
| ImagesContainer.SaveAllImagesAsync.TVSeason | 32 | 17.91 | 95.39 | 241.05 | 3,052.36 |
| ImagesContainer.SaveAllImagesAsync.TVSeason.ParallelDownload | 26 | 46.60 | 82.08 | 216.41 | 2,134.15 |
| ImagesContainer.SaveAllImagesAsync.TVSeason.SaveToDisk | 32 | 17.18 | 27.86 | 47.08 | 891.38 |
| ImagesContainer.SaveAllImagesAsync.TVShow | 6 | 295.88 | 529.86 | 1,511.01 | 3,179.14 |
| ImagesContainer.SaveAllImagesAsync.TVShow.ParallelDownload | 6 | 269.93 | 498.14 | 1,475.41 | 2,988.82 |
| ImagesContainer.SaveAllImagesAsync.TVShow.SaveToDisk | 6 | 25.32 | 31.00 | 34.52 | 185.98 |
| IMDB.GetTVEpisodeInfo | 210 | 1,251.41 | 2,563.72 | 5,387.84 | 538,382.07 |
| IMDB.GetTVEpisodeInfo.HttpRequest | 210 | 1,243.95 | 2,547.43 | 5,382.93 | 534,959.88 |
| IMDB.GetTVSeasonInfo | 23 | 3,036.95 | 24,538.40 | 41,463.70 | 564,383.21 |
| IMDB.GetTVSeasonInfo.Episodes | 23 | 1,323.67 | 23,408.09 | 39,217.14 | 538,386.18 |
| IMDB.GetTVSeasonInfo.HttpRequest.SeasonPage | 23 | 377.16 | 1,123.33 | 2,235.98 | 25,836.58 |
| IMDB.GetTVShowInfo | 6 | 59,445.96 | 97,600.47 | 143,183.07 | 585,602.85 |
| IMDB.GetTVShowInfo.HttpRequest | 6 | 1,174.41 | 1,823.89 | 2,580.13 | 10,943.33 |
| SaveAllImages.TVSeason.Total | 28 | 0.11 | 0.63 | 1.96 | 17.65 |
| TMDB.GetInfo_TVShow | 6 | 807.30 | 2,012.89 | 3,784.64 | 12,077.34 |
| TMDB.GetInfo_TVShow.APICall | 6 | 22.30 | 65.77 | 119.96 | 394.64 |
| TMDB.SearchTVShow | 6 | 55.74 | 111.19 | 214.28 | 667.14 |
| TMDB.SearchTVShow.APICall | 6 | 27.52 | 68.48 | 157.00 | 410.86 |
| TVDB.GetTVShowInfo | 6 | 65.41 | 99.94 | 204.97 | 599.66 |
| TVDB.GetTVShowInfo.APICall | 6 | 65.05 | 97.32 | 193.70 | 583.94 |

### Analysis

#### Scraper Performance Comparison

| Scraper | Total Time | Avg per Show | Architecture |
|---------|------------|--------------|--------------|
| **IMDB** | 585,603 ms (~9.7 min) | 97,600 ms | 1 HTTP request per episode |
| **TMDB** | 12,077 ms (~12 sec) | 2,013 ms | Bulk API call |
| **TVDB** | 600 ms (~0.6 sec) | 100 ms | Bulk API call |

#### Key Findings

1. **IMDB is 97% of total scrape time** due to per-episode HTTP requests
2. **210 IMDB HTTP requests** averaging 2.5 seconds each = 8.9 minutes just for episodes
3. **TMDB and TVDB use bulk APIs** - fetch all episodes in one call per show
4. **HTTP requests are 99.4% of IMDB episode time** - no optimization possible without parallelization
5. **Image downloads are efficient** - parallel download working well

#### Bottleneck Breakdown (IMDB)

| Component | Time (ms) | % of IMDB Total |
|-----------|-----------|-----------------|
| Episode HTTP requests | 534,960 | 91.4% |
| Season page requests | 25,837 | 4.4% |
| Show HTTP requests | 10,943 | 1.9% |
| Processing overhead | ~13,863 | 2.4% |

#### Recommendations

1. **Use multi-scraper mode** - TMDB/TVDB handle bulk episode data efficiently
2. **IMDB best for supplemental data** - ratings, certifications after primary scrape
3. **Avoid IMDB-only mode for TV** - 10+ minutes for 6 shows is impractical for large libraries

---

*Document maintained for tracking TV scraping performance improvements.*