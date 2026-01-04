# [↑](#table-of-contents) TVDB API Notes & Implementation Details

This file collects practical notes, quirks, and implementation details for the TVDB v4 API, plus context for legacy v3 usage.  
It is intended as a living reference for development, debugging, and workflow design.

---

## Table of Contents

- 🧭 [General notes](#-general-notes)
- 🔐 [Authentication](#-authentication)
- 🔍 [Search](#-search)
- 📺 [Series & Episodes](#-series--episodes)
- 🎬 [Movies](#-movies)
- 👤 [People](#-people)
- 🖼 [Artwork](#-artwork)
- 🌐 [Languages & Static Data](#-languages--static-data)
- 🔄 [Updates & Sync](#-updates--sync)
- 📦 [Bulk data strategy](#-bulk-data-strategy)
- 📋 [Lists & Favorites](#-lists--favorites)
- 🧩 [v3 vs v4 behavior differences](#-v3-vs-v4-behavior-differences)
- 📦 [SDK usage notes](#-sdk-usage-notes)
- 🔗 [Navigation](#-navigation)

---

# [↑](#table-of-contents) 🧭 General notes

- TVDB v4 is the current, actively maintained API, documented publicly on GitHub.
- Access is subscription/licensing-based; an API key is required for use.
- The API enforces a minimum of TLS 1.2.
- v3 is considered legacy and should be used only for migration or maintenance scenarios.

---

# [↑](#table-of-contents) 🔐 Authentication

- v4 authentication uses **API key + PIN** via the `POST /login` endpoint.
- Tokens are refreshed via `POST /refresh_token`.
- Plan for token lifecycle handling and error cases (expired/invalid tokens).

---

# [↑](#table-of-contents) 🔍 Search

- `GET /search` is unified across entity types (series, episodes, people, etc.).
- Legacy v3 search endpoints (`/search/series`, `/search/episodes`, `/search/actors`) map into this unified search.

---

# [↑](#table-of-contents) 📺 Series & Episodes

- v4 often embeds episodes within the series payload (`includedEpisodes`), reducing the need for per-series episode endpoints.
- Global `GET /episodes` with filters replaces v3’s numerous query endpoints.
- Expect to lean more on query parameters and filters than on nested REST paths.

---

# [↑](#table-of-contents) 🎬 Movies

- Movies are a major v4 addition with no real v3 equivalent.
- Treat movies as first-class entities, similar to series, with their own statuses and metadata.

---

# [↑](#table-of-contents) 👤 People

- v3 actor endpoints are replaced by the more general `people` model.
- People can be associated with series, episodes, movies, and other entities.

---

# [↑](#table-of-contents) 🖼 Artwork

- v4 uses a unified, typed artwork system (`/artwork`, `/artwork/types`).
- This replaces numerous v3 image-query endpoints with consistent filtering across entity types.
- Artwork types, sizes, and relationships are defined in the static data endpoints.

---

# [↑](#table-of-contents) 🌐 Languages & Static Data

- Languages, genres, genders, inspiration types, entity types, and source types are exposed via dedicated endpoints.
- These are ideal for seeding local reference tables and driving UI (dropdowns, filters, etc.).

---

# [↑](#table-of-contents) 🔄 Updates & Sync

- `GET /updates` is designed for incremental sync: use it to keep a local cache in step with TVDB changes.
- Some deletions include merge information so you can migrate data from deprecated IDs to their replacements.

---

# [↑](#table-of-contents) 📦 Bulk data strategy

- TVDB recommends maintaining your own copy of the database or using a caching proxy for heavy usage scenarios.
- Bulk endpoints (`/bulk/...`) enable full or large-scale ingestion of records into your own storage.
- A typical pattern:
  - Ingest via bulk endpoints.
  - Keep in sync via `/updates`.
  - Serve your applications from your own database or caching layer.

---

# [↑](#table-of-contents) 📋 Lists & Favorites

- v4 retains user favorites but removes user ratings.
- `POST /user/favorites` replaces the v3 `PUT /user/favorites/{id}` pattern.
- Deletion remains `DELETE /user/favorites/{id}`.

---

# [↑](#table-of-contents) 🧩 v3 vs v4 behavior differences

- v4 is not a drop-in replacement; it’s a redesigned model with different patterns.
- v3:
  - Entity-specific, nested endpoints.
  - Actor-centric.
  - More granular series/episode sub-endpoints.
  - Supports user ratings.
- v4:
  - Unified search and artwork.
  - People-based model.
  - Bulk and update endpoints for full-database workflows.
  - No user ratings; focus is on metadata and structure.

---

# [↑](#table-of-contents) 📦 SDK usage notes

- **v4:** `Tvdb.Sdk` — auto-generated from the v4 schema; best choice for new work.
- **v3:** `DustyPig.TVDB` — suitable for legacy v3 usage where needed.
- **v2 (legacy XML):** `TVDBSharp` — generally not recommended for modern projects.

---

# [↑](#table-of-contents) 🔗 Navigation

- [Overview](overview.md)
- [TVDB v4 Endpoints](v4-endpoints.md)
- [TVDB v3 Endpoints (Placeholder)](v3-endpoints.md)
- [v3 → v4 Mapping](mapping.md)