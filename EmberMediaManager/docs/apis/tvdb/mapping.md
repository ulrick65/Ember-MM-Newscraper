# [↑](#table-of-contents) TVDB API v3 → v4 Mapping

A reference comparing known v3 endpoints to their closest v4 equivalents, plus notes on added and removed features.

---

## Table of Contents

- 🔄 [Side-by-side endpoint mapping](#-side-by-side-endpoint-mapping)
- ➕ [Major additions in v4](#-major-additions-in-v4)
- ➖ [Major v3 features removed in v4](#-major-v3-features-removed-in-v4)
- 🧠 [API philosophy changes](#-api-philosophy-changes)
- 📦 [Recommended NuGet packages](#-recommended-nuget-packages)
- 📝 [Summary](#-summary)
- 🔗 [Navigation](#-navigation)

---

# [↑](#table-of-contents) 🔄 Side-by-side endpoint mapping

| v3 Endpoint | v4 Equivalent | Notes |
|------------|---------------|-------|
| **POST /login** | POST /login | ✓ Same purpose, but v4 requires API key + PIN |
| **GET /refresh_token** | POST /refresh_token | ✓ Same purpose, different method |
| **GET /search/series** | GET /search | ~ v4 search is unified and more powerful |
| **GET /search/episodes** | GET /search | ~ v4 search returns mixed entity types |
| **GET /search/actors** | GET /search | ~ v4 search includes people |
| **GET /series/{id}** | GET /series/{id} | ✓ Direct equivalent |
| **GET /series/{id}/episodes** | GET /series/{id} → includedEpisodes | ~ v4 embeds episodes in series payload |
| **GET /series/{id}/episodes/query** | GET /episodes | ~ v4 uses global episode listing with filters |
| **GET /series/{id}/episodes/query/params** | GET /episodes | ~ v4 uses query parameters instead |
| **GET /series/{id}/actors** | GET /people?series={id} | ~ v4 uses people endpoint with filters |
| **GET /series/{id}/images** | GET /artwork?series={id} | ~ v4 artwork is fully typed |
| **GET /series/{id}/images/query** | GET /artwork | ~ v4 uses query parameters |
| **GET /series/{id}/images/query/params** | GET /artwork | ~ v4 uses query parameters |
| **GET /episodes/{id}** | GET /episodes/{id} | ✓ Direct equivalent |
| **GET /actors/{id}** | GET /people/{id} | ✓ People replace actors in v4 |
| **GET /updated/query** | GET /updates | ~ v4 has multiple update types (series, movies, people) |
| **GET /languages** | GET /languages | ✓ Direct equivalent |
| **GET /languages/{id}** | GET /languages | ~ v4 returns full list only |
| **GET /user** | GET /user | ✓ Direct equivalent |
| **GET /user/favorites** | GET /user/favorites | ✓ Direct equivalent |
| **PUT /user/favorites/{id}** | POST /user/favorites | ~ v4 uses POST instead of PUT |
| **DELETE /user/favorites/{id}** | DELETE /user/favorites/{id} | ✓ Direct equivalent |
| **GET /user/ratings** | ✗ None | v4 removed user ratings |
| **PUT /user/ratings/{type}/{id}** | ✗ None | v4 removed user ratings |
| **DELETE /user/ratings/{type}/{id}** | ✗ None | v4 removed user ratings |

---

# [↑](#table-of-contents) ➕ Major additions in v4

- `/movies`
- `/lists`
- `/companies`
- `/artwork/types`
- `/genres`
- `/genders`
- `/inspiration/types`
- `/entity/types`
- `/source/types`
- `/movie/statuses`
- `/series/statuses`
- `/bulk`-style endpoints for mirroring the entire database

---

# [↑](#table-of-contents) ➖ Major v3 features removed in v4

- User ratings  
- Actor-specific endpoints (replaced by `people`)  
- Image query endpoints (replaced by unified artwork system)  
- Some episode/series query endpoints (replaced by global filters)

---

# [↑](#table-of-contents) 🧠 API philosophy changes

## v3

- REST endpoints grouped by entity  
- Many series-specific sub-endpoints  
- Actor-focused model  
- Limited search  
- No bulk data access  
- User ratings supported  

## v4

- Fully redesigned data model  
- Unified search across all entity types  
- Artwork is typed and queryable  
- People replace actors  
- Bulk endpoints for full database sync  
- No user ratings  
- Authentication requires API key + PIN  

---

# [↑](#table-of-contents) 📦 Recommended NuGet packages

| API Version | Package | Notes |
|------------|---------|-------|
| **v4** | `Tvdb.Sdk` | Auto-generated from v4 schema; best choice |
| **v3** | `DustyPig.TVDB` | Only modern, maintained v3 client |
| **v2 (legacy)** | `TVDBSharp` | Old XML API; not recommended |

---

# [↑](#table-of-contents) 📝 Summary

This document provides a mapping of key v3 endpoints to their closest v4 equivalents, along with notes on removed features, new v4 capabilities, and recommended client libraries for different API versions.

---

# [↑](#table-of-contents) 🔗 Navigation

- [Overview](overview.md)
- [TVDB v4 Endpoints](v4-endpoints.md)
- [TVDB v3 Endpoints (Placeholder)](v3-endpoints.md)
- [Notes](notes.md)