# [↑](#table-of-contents) TMDB API v3 — Full Endpoint Catalog

TMDB API v3 is the primary metadata API. It includes all movie, TV, people, search, trending, and discovery endpoints.

This file lists all known v3 endpoints grouped by category.

---

## Table of Contents

- 🎬 [Movies](#-movies)
- 📺 [TV](#-tv)
- 👤 [People](#-people)
- 🔍 [Search](#-search)
- 📈 [Trending](#-trending)
- 🔎 [Discover](#-discover)
- 🎞 [Collections](#-collections)
- 🏢 [Companies](#-companies)
- 📡 [Networks](#-networks)
- 🏷 [Keywords](#-keywords)
- 🎭 [Genres](#-genres)
- 🏅 [Certifications](#-certifications)
- 🎥 [Watch Providers](#-watch-providers)
- 📝 [Reviews](#-reviews)
- 🖼 [Images](#-images)
- 🌐 [Translations](#-translations)
- 🎭 [Credits](#-credits)
- 🔎 [Find](#-find)
- 👤 [Account](#-account)
- 🔐 [Authentication](#-authentication)
- 📋 [Lists](#-lists)
- ⭐ [Ratings](#-ratings)
- 👥 [Guest Sessions](#-guest-sessions)
- 🔄 [Changes](#-changes)

---

# [↑](#table-of-contents) 🎬 Movies

- `GET /movie/{movie_id}`
- `GET /movie/{movie_id}/account_states`
- `GET /movie/{movie_id}/alternative_titles`
- `GET /movie/{movie_id}/changes`
- `GET /movie/{movie_id}/credits`
- `GET /movie/{movie_id}/external_ids`
- `GET /movie/{movie_id}/images`
- `GET /movie/{movie_id}/keywords`
- `GET /movie/{movie_id}/lists`
- `GET /movie/{movie_id}/recommendations`
- `GET /movie/{movie_id}/release_dates`
- `GET /movie/{movie_id}/reviews`
- `GET /movie/{movie_id}/similar`
- `GET /movie/{movie_id}/translations`
- `GET /movie/{movie_id}/videos`
- `GET /movie/{movie_id}/watch/providers`
- `GET /movie/latest`
- `GET /movie/now_playing`
- `GET /movie/popular`
- `GET /movie/top_rated`
- `GET /movie/upcoming`

---

# [↑](#table-of-contents) 📺 TV

- `GET /tv/{tv_id}`
- `GET /tv/{tv_id}/account_states`
- `GET /tv/{tv_id}/aggregate_credits`
- `GET /tv/{tv_id}/alternative_titles`
- `GET /tv/{tv_id}/changes`
- `GET /tv/{tv_id}/content_ratings`
- `GET /tv/{tv_id}/credits`
- `GET /tv/{tv_id}/episode_groups`
- `GET /tv/{tv_id}/external_ids`
- `GET /tv/{tv_id}/images`
- `GET /tv/{tv_id}/keywords`
- `GET /tv/{tv_id}/recommendations`
- `GET /tv/{tv_id}/reviews`
- `GET /tv/{tv_id}/screened_theatrically`
- `GET /tv/{tv_id}/similar`
- `GET /tv/{tv_id}/translations`
- `GET /tv/{tv_id}/videos`
- `GET /tv/{tv_id}/watch/providers`
- `GET /tv/latest`
- `GET /tv/airing_today`
- `GET /tv/on_the_air`
- `GET /tv/popular`
- `GET /tv/top_rated`

---

# [↑](#table-of-contents) 👤 People

- `GET /person/{person_id}`
- `GET /person/{person_id}/changes`
- `GET /person/{person_id}/combined_credits`
- `GET /person/{person_id}/external_ids`
- `GET /person/{person_id}/images`
- `GET /person/{person_id}/movie_credits`
- `GET /person/{person_id}/tv_credits`
- `GET /person/{person_id}/translations`
- `GET /person/latest`
- `GET /person/popular`

---

# [↑](#table-of-contents) 🔍 Search

- `GET /search/company`
- `GET /search/collection`
- `GET /search/keyword`
- `GET /search/movie`
- `GET /search/multi`
- `GET /search/person`
- `GET /search/tv`

---

# [↑](#table-of-contents) 📈 Trending

- `GET /trending/{media_type}/{time_window}`

---

# [↑](#table-of-contents) 🔎 Discover

- `GET /discover/movie`
- `GET /discover/tv`

---

# [↑](#table-of-contents) 🎞 Collections

- `GET /collection/{collection_id}`
- `GET /collection/{collection_id}/images`

---

# [↑](#table-of-contents) 🏢 Companies

- `GET /company/{company_id}`
- `GET /company/{company_id}/alternative_names`
- `GET /company/{company_id}/images`

---

# [↑](#table-of-contents) 📡 Networks

- `GET /network/{network_id}`
- `GET /network/{network_id}/alternative_names`
- `GET /network/{network_id}/images`

---

# [↑](#table-of-contents) 🏷 Keywords

- `GET /keyword/{keyword_id}`
- `GET /keyword/{keyword_id}/movies`

---

# [↑](#table-of-contents) 🎭 Genres

- `GET /genre/movie/list`
- `GET /genre/tv/list`

---

# [↑](#table-of-contents) 🏅 Certifications

- `GET /certification/movie/list`
- `GET /certification/tv/list`

---

# [↑](#table-of-contents) 🎥 Watch Providers

- `GET /watch/providers/movie`
- `GET /watch/providers/tv`

---

# [↑](#table-of-contents) 📝 Reviews

- `GET /review/{review_id}`

---

# [↑](#table-of-contents) 🖼 Images

- `GET /configuration`
- `GET /configuration/languages`
- `GET /configuration/countries`
- `GET /configuration/jobs`
- `GET /configuration/timezones`

---

# [↑](#table-of-contents) 🌐 Translations

- `GET /movie/{movie_id}/translations`
- `GET /tv/{tv_id}/translations`
- `GET /person/{person_id}/translations`

---

# [↑](#table-of-contents) 🎭 Credits

- `GET /credit/{credit_id}`

---

# [↑](#table-of-contents) 🔎 Find

- `GET /find/{external_id}`

---

# [↑](#table-of-contents) 👤 Account

- `GET /account`
- `GET /account/{account_id}/lists`
- `GET /account/{account_id}/favorite/movies`
- `GET /account/{account_id}/favorite/tv`
- `GET /account/{account_id}/rated/movies`
- `GET /account/{account_id}/rated/tv`
- `GET /account/{account_id}/rated/tv/episodes`
- `GET /account/{account_id}/watchlist/movies`
- `GET /account/{account_id}/watchlist/tv`

---

# [↑](#table-of-contents) 🔐 Authentication

- `GET /authentication/token/new`
- `GET /authentication/token/validate_with_login`
- `GET /authentication/session/new`
- `GET /authentication/guest_session/new`

---

# [↑](#table-of-contents) 📋 Lists

- `GET /list/{list_id}`
- `POST /list`
- `POST /list/{list_id}/add_item`
- `POST /list/{list_id}/remove_item`
- `DELETE /list/{list_id}`

---

# [↑](#table-of-contents) ⭐ Ratings

- `POST /movie/{movie_id}/rating`
- `POST /tv/{tv_id}/rating`
- `POST /tv/{tv_id}/episode/{episode_number}/rating`
- `DELETE /movie/{movie_id}/rating`
- `DELETE /tv/{tv_id}/rating`
- `DELETE /tv/{tv_id}/episode/{episode_number}/rating`

---

# [↑](#table-of-contents) 👥 Guest Sessions

- `GET /guest_session/{guest_session_id}/rated/movies`
- `GET /guest_session/{guest_session_id}/rated/tv`
- `GET /guest_session/{guest_session_id}/rated/tv/episodes`

---

# [↑](#table-of-contents) 🔄 Changes

- `GET /movie/changes`
- `GET /tv/changes`
- `GET /person/changes`

---

# [↑](#table-of-contents) Navigation

- [Overview](overview.md)
- [TMDB v4 Endpoints](v4-endpoints.md)
- [v3 → v4 Mapping](mapping.md)
- [Notes](notes.md)