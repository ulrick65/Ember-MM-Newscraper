# [↑](#table-of-contents) TVDB API v4 — Full Endpoint Catalog

TVDB API v4 is a redesigned, schema-driven API that covers series, episodes, movies, people, artwork, companies, lists, bulk updates, and more.  
This file lists the main v4 endpoints grouped by category, based on the official v4 documentation and your mapping notes.

---

## Table of Contents

- 🔐 [Authentication](#-authentication)
- 🔍 [Search](#-search)
- 📺 [Series](#-series)
- 🎞 [Episodes](#-episodes)
- 🎬 [Movies](#-movies)
- 👤 [People](#-people)
- 🖼 [Artwork](#-artwork)
- 🏢 [Companies](#-companies)
- 🌐 [Languages & Static Data](#-languages--static-data)
- 🔄 [Updates](#-updates)
- 📦 [Bulk & Sync](#-bulk--sync)
- 📋 [User & Favorites](#-user--favorites)

---

# [↑](#table-of-contents) 🔐 Authentication

- `POST /login`
- `POST /refresh_token`

---

# [↑](#table-of-contents) 🔍 Search

- `GET /search`

---

# [↑](#table-of-contents) 📺 Series

- `GET /series/{id}`
- `GET /series` (filtered listing, where applicable)
- `GET /series/statuses`

> **Notes:**  
> - v4 embeds episodes in the series payload via `includedEpisodes` instead of v3’s `/series/{id}/episodes` pattern.  
> - Series statuses are exposed via dedicated status endpoints.

---

# [↑](#table-of-contents) 🎞 Episodes

- `GET /episodes/{id}`
- `GET /episodes` (query / filter-based)

> **Notes:**  
> - v3’s `/series/{id}/episodes/query` and related endpoints are replaced by global `/episodes` queries with filters.

---

# [↑](#table-of-contents) 🎬 Movies

- `GET /movies`
- `GET /movies/{id}`
- `GET /movie/statuses`

> **Notes:**  
> - Movies are a major v4 addition and have no direct v3 equivalent.

---

# [↑](#table-of-contents) 👤 People

- `GET /people`
- `GET /people/{id}`

> **Notes:**  
> - v3’s actor endpoints are replaced by the more general `people` model.

---

# [↑](#table-of-contents) 🖼 Artwork

- `GET /artwork`
- `GET /artwork/{id}`
- `GET /artwork/types`

> **Notes:**  
> - v3’s image query endpoints are replaced by a unified, typed artwork system.

---

# [↑](#table-of-contents) 🏢 Companies

- `GET /companies`
- `GET /companies/{id}`

> **Notes:**  
> - Companies are a v4 addition, not present in v3.

---

# [↑](#table-of-contents) 🌐 Languages & Static Data

- `GET /languages`
- `GET /genres`
- `GET /genders`
- `GET /inspiration/types`
- `GET /entity/types`
- `GET /source/types`

> **Notes:**  
> - Static, enumerated types for normalizing content and driving UI logic.

---

# [↑](#table-of-contents) 🔄 Updates

- `GET /updates`

> **Notes:**  
> - Intended for incremental sync so clients can maintain a local copy of the database efficiently.

---

# [↑](#table-of-contents) 📦 Bulk & Sync

- `GET /bulk/series`
- `GET /bulk/movies`
- `GET /bulk/people`
- `GET /bulk/artwork`
- (Additional bulk endpoints as exposed by the official schema)

> **Notes:**  
> - Used for mirroring large slices of the database into your own storage.

---

# [↑](#table-of-contents) 📋 User & Favorites

- `GET /user`
- `GET /user/favorites`
- `POST /user/favorites`
- `DELETE /user/favorites/{id}`
- (Additional user-scoped endpoints as needed)

---

# [↑](#table-of-contents) Navigation

- [Overview](overview.md)
- [TVDB v3 Endpoints (Placeholder)](v3-endpoints.md)
- [v3 → v4 Mapping](mapping.md)
- [Notes](notes.md)