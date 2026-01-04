# [↑](#table-of-contents) TVDB API Overview

The TVDB API provides metadata for TV series, episodes, movies, people, artwork, and more.  
The current API is **v4**, a redesigned, schema-driven API that replaces the older v3 and v2 APIs for most use cases.

This documentation set focuses on **TVDB v4**, with stubs and mapping notes for **v3** so you can bridge older integrations as needed.

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
- 🌐 [Languages](#-languages)
- 🔄 [Updates](#-updates)
- 📦 [Bulk & Sync](#-bulk--sync)
- 📋 [Lists](#-lists)
- 🧩 [v3 → v4 Mapping](#-v3--v4-mapping)
- 📝 [Notes](#-notes)

---

# [↑](#table-of-contents) 🔐 Authentication

High-level overview of TVDB v4 authentication, including login, token refresh, and API key + PIN requirements.

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 🔍 Search

Unified search across series, episodes, movies, people, and other entity types.

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 📺 Series

Endpoints for series details, embedded episodes, artwork, and related entities.

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 🎞 Episodes

Endpoints for episode details and queries using global filters.

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 🎬 Movies

Endpoints for movie metadata and related entities (a major addition in v4).

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 👤 People

Endpoints for people (replacing actor-specific v3 endpoints) and their appearances.

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 🖼 Artwork

Typed artwork endpoints covering posters, banners, backgrounds, and more, with query support by entity and type.

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 🏢 Companies

Endpoints for companies and related metadata (new in v4).

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 🌐 Languages

Language list endpoints for normalizing and filtering localized data.

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 🔄 Updates

Update endpoints to track changed records and keep a local cache in sync — a recommended integration pattern.

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 📦 Bulk & Sync

Bulk-style endpoints that allow mirroring large portions of the database for offline or cached use.

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 📋 Lists

Endpoints for user lists and favorites, where applicable.

See full endpoint list:  
**[`v4-endpoints.md`](./v4-endpoints.md)**

---

# [↑](#table-of-contents) 🧩 v3 → v4 Mapping

Mapping of legacy v3 endpoints to their v4 equivalents, including removed and added functionality.

See mapping details:  
**[`mapping.md`](./mapping.md)**

---

# [↑](#table-of-contents) 📝 Notes

Implementation notes, SDK recommendations, and behavior differences between v3 and v4.

See:  
**[`notes.md`](./notes.md)**

---

# [↑](#table-of-contents) Navigation

- [TVDB v4 Endpoints](v4-endpoints.md)
- [TVDB v3 Endpoints (Placeholder)](v3-endpoints.md)
- [v3 → v4 Mapping](mapping.md)
- [Notes](notes.md)