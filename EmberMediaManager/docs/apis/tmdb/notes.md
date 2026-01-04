# [↑](#table-of-contents) TMDB API Notes & Implementation Details

This file collects practical notes, quirks, behaviors, and implementation details discovered while working with TMDB API v3 and v4.  
It is intended as a living reference for development, debugging, and workflow optimization.

---

## Table of Contents

- 🧭 [General Notes](#-general-notes)
- 🔐 [Authentication](#-authentication)
- 🎬 [Movies](#-movies)
- 📺 [TV](#-tv)
- 👤 [People](#-people)
- 🔍 [Search](#-search)
- 📈 [Trending](#-trending)
- 🔎 [Discover](#-discover)
- 🖼 [Images & Configuration](#-images--configuration)
- 📋 [Lists](#-lists)
- ⭐ [Ratings](#-ratings)
- 👥 [Guest Sessions](#-guest-sessions)
- 🧩 [v3 vs v4 Behavior Differences](#-v3-vs-v4-behavior-differences)
- 🧪 [Testing Notes](#-testing-notes)
- 🔗 [Navigation](#-navigation)

---

# [↑](#table-of-contents) 🧭 General Notes

- TMDB v3 is the primary metadata API (movies, TV, people, search, images, etc.).
- TMDB v4 is an OAuth-based account API (lists, ratings, permissions, organizations).
- v3 and v4 are complementary — neither replaces the other.
- v3 uses API keys; v4 uses OAuth tokens.
- Rate limiting applies across both APIs.
- Some endpoints return partial or empty data depending on region or availability.

---

# [↑](#table-of-contents) 🔐 Authentication

### v3
- Supports API key via query string or header.
- Supports session-based authentication for account actions.
- Guest sessions allow ratings without login.

### v4
- Requires OAuth 2.0.
- Tokens must be refreshed periodically.
- No guest session equivalent.

---

# [↑](#table-of-contents) 🎬 Movies

- Some movies may have incomplete metadata depending on region.
- `release_dates` often contains multiple certification entries per country.
- `watch/providers` availability varies widely by region.
- `alternative_titles` may include festival or working titles.

---

# [↑](#table-of-contents) 📺 TV

- TV endpoints include season and episode metadata not present in movie endpoints.
- `aggregate_credits` merges cast/crew across episodes.
- `content_ratings` differ significantly by region.
- Episode groups can be non-intuitive (e.g., Netflix order vs broadcast order).

---

# [↑](#table-of-contents) 👤 People

- `combined_credits` merges movie and TV credits.
- Some people have incomplete or missing biography data.
- Image availability varies widely.

---

# [↑](#table-of-contents) 🔍 Search

- `multi` search returns mixed media types.
- Search results may include adult content depending on settings.
- Company and keyword search results are often sparse.

---

# [↑](#table-of-contents) 📈 Trending

- Trending windows: `day` and `week`.
- Trending results may include media types not requested in other endpoints.

---

# [↑](#table-of-contents) 🔎 Discover

- Discover supports dozens of filters.
- Some filters only work when combined with others.
- Region and language filters significantly affect results.

---

# [↑](#table-of-contents) 🖼 Images & Configuration

- `configuration` endpoint provides base URLs and size options.
- Always use configuration data to construct image URLs.
- Image sizes differ between posters, backdrops, logos, and profiles.
- Languages and countries lists are static but occasionally updated.

---

# [↑](#table-of-contents) 📋 Lists

### v3
- Lists are simple and session-based.
- Adding/removing items is one item at a time.

### v4
- Lists are OAuth-based.
- Supports batch add/remove operations.
- Lists can contain mixed media types.

---

# [↑](#table-of-contents) ⭐ Ratings

### v3
- Ratings exist for movies, TV, and episodes.
- Guest sessions allow anonymous ratings.

### v4
- Unified rating endpoint.
- Requires OAuth.
- No guest session support.

---

# [↑](#table-of-contents) 👥 Guest Sessions

- Only available in v3.
- Useful for temporary or anonymous rating workflows.
- Expire after a limited time.

---

# [↑](#table-of-contents) 🧩 v3 vs v4 Behavior Differences

- v3 = metadata; v4 = account operations.
- v4 consolidates ratings under a single endpoint.
- v4 lists are more powerful and flexible.
- v4 introduces organizations, roles, and permissions.
- v3 includes search, trending, discover — v4 does not.

---

# [↑](#table-of-contents) 🧪 Testing Notes

- Always test with both valid and invalid IDs.
- Some endpoints return HTTP 200 with empty payloads instead of 404.
- Rate limiting may not always return explicit error messages.
- Region-specific data can cause inconsistent results across environments.

---

# [↑](#table-of-contents) 🔗 Navigation

- [Overview](overview.md)
- [TMDB v3 Endpoints](v3-endpoints.md)
- [TMDB v4 Endpoints](v4-endpoints.md)
- [v3 → v4 Mapping](mapping.md)