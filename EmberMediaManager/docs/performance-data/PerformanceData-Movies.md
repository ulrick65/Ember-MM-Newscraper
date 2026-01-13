# Movie Scraping Performance Data

| Document Info | |
|---------------|---|
| **Created** | December 31, 2025 |
| **Updated** | December 31, 2025 |
| **Author** | ulrick65 |
| **Purpose** | Track movie scraping performance metrics over time |

---

## Table of Contents

- [Overview](#overview)
- [Test Configuration](#test-configuration)
- [New Baseline: December 31, 2025](#new-baseline-december-31-2025)

---

## Overview

This document captures movie scraping performance metrics to track improvements and identify bottlenecks. Data is collected using the `PerformanceTracker` instrumentation across IMDB and TMDB scrapers.

---

## Test Configuration

### Standard Test Set

**Movies:** 15 movies (clean scrape)

### Scraper Configuration

- **Data Scrapers:** TMDB, IMDB (all enabled)
- **Image Scrapers:** All enabled
- **Test Method:** Clean scrape of all items

---

## New Baseline: December 31, 2025

### Test Environment

- **Branch:** feature/performance-improvements-phase4
- **Scrapers:** IMDB + TMDB (multi-scraper mode)
- **Content:** 15 movies

### Raw Metrics

| Operation | Count | Min (ms) | Avg (ms) | Max (ms) | Total (ms) |
|-----------|-------|----------|----------|----------|------------|
| Database.Add_Actor | 913 | 0.01 | 0.63 | 1.05 | 572.28 |
| Database.Save_MovieAsync | 15 | 88.70 | 453.18 | 1,087.84 | 6,797.72 |
| Image.LoadFromWebAsync | 96 | 29.59 | 96.15 | 384.48 | 9,229.98 |
| Image.LoadFromWebAsync.Download | 96 | 29.55 | 96.06 | 384.42 | 9,221.85 |
| ImagesContainer.SaveAllImagesAsync.Movie | 15 | 44.10 | 232.89 | 535.22 | 3,493.28 |
| ImagesContainer.SaveAllImagesAsync.Movie.ParallelDownload | 15 | 30.76 | 190.97 | 491.36 | 2,864.50 |
| ImagesContainer.SaveAllImagesAsync.Movie.SaveToDisk | 15 | 13.04 | 41.33 | 56.33 | 619.90 |
| IMDB.GetMovieInfo | 15 | 1,451.76 | 2,669.15 | 5,353.09 | 40,037.19 |
| IMDB.GetMovieInfo.HttpRequest | 15 | 1,442.66 | 2,652.41 | 5,339.46 | 39,786.13 |
| TMDB.GetInfo_Movie | 15 | 146.45 | 1,186.55 | 2,104.12 | 17,798.30 |
| TMDB.GetInfo_Movie.APICall | 15 | 54.38 | 92.79 | 194.43 | 1,391.90 |
| TMDB.SearchMovie | 15 | 104.45 | 159.51 | 354.51 | 2,392.62 |
| TMDB.SearchMovie.APICall | 15 | 45.48 | 70.26 | 195.75 | 1,053.84 |

### Analysis

#### Scraper Performance Comparison

| Scraper | Total Time | Avg per Movie | Architecture |
|---------|------------|---------------|--------------|
| **IMDB** | 40,037 ms (~40 sec) | 2,669 ms | 1 HTTP request per movie |
| **TMDB** | 20,191 ms (~20 sec) | 1,346 ms | API calls (search + details) |

#### Time Distribution

| Component | Time (ms) | % of Total |
|-----------|-----------|------------|
| IMDB scraping | 40,037 | 55.6% |
| TMDB scraping | 20,191 | 28.0% |
| Image downloads | 9,230 | 12.8% |
| Database saves | 6,798 | 9.4% |
| Image save to disk | 620 | 0.9% |

#### Key Findings

1. **IMDB is ~2x slower than TMDB** per movie (2.67s vs 1.35s average)
2. **HTTP requests are 99.4% of IMDB time** - network latency dominates
3. **TMDB API is efficient** - actual API calls only 93ms average, rest is processing
4. **Image downloads are well optimized** - parallel download working effectively
5. **Database operations are reasonable** - 453ms average per movie save

#### IMDB Breakdown

| Component | Time (ms) | % of IMDB Total |
|-----------|-----------|-----------------|
| HTTP requests | 39,786 | 99.4% |
| Processing overhead | 251 | 0.6% |

#### TMDB Breakdown

| Component | Time (ms) | % of TMDB Total |
|-----------|-----------|-----------------|
| Search API calls | 1,054 | 5.2% |
| GetInfo API calls | 1,392 | 6.9% |
| Processing/other | 17,745 | 87.9% |

### Observations

#### Movie vs TV Comparison

| Content Type | IMDB Avg per Item | TMDB Avg per Item |
|--------------|-------------------|-------------------|
| **Movies** | 2,669 ms | 1,346 ms |
| **TV Shows** | 97,600 ms | 2,013 ms |

Movies are **much faster** than TV shows because:
- Movies require only 1 HTTP request per item
- No per-episode scraping needed
- No season page fetching

#### Recommendations

1. **Current performance is acceptable** for movie scraping
2. **IMDB remains slower** but not prohibitively so for movies
3. **Multi-scraper mode works well** - TMDB provides fast primary data, IMDB supplements

---

*Document maintained for tracking movie scraping performance improvements.*