# [↑](#table-of-contents) TMDB API v3 → v4 Mapping

TMDB v3 and v4 serve different purposes.  
v3 is the primary metadata API (movies, TV, people, search, images).  
v4 is an OAuth-based account API (lists, ratings, permissions, organizations).

This file outlines how the two APIs relate, where they overlap, and where they diverge.

---

## Table of Contents

- 🔄 [Overview](#-overview)
- 🎬 [Movies](#-movies)
- 📺 [TV](#-tv)
- 👤 [People](#-people)
- 🔍 [Search](#-search)
- 📈 [Trending](#-trending)
- 🔎 [Discover](#-discover)
- 📋 [Lists](#-lists)
- ⭐ [Ratings](#-ratings)
- 👤 [Account](#-account)
- 🔐 [Authentication](#-authentication)
- 🏢 [Organizations](#-organizations)
- 🧩 [Roles](#-roles)
- 🔑 [Permissions](#-permissions)
- 🧭 [Summary Table](#-summary-table)
- 🔗 [Navigation](#-navigation)

---

# [↑](#table-of-contents) 🔄 Overview

TMDB v3 and v4 are **complementary**, not replacements for one another.

### v3 focuses on:
- Movies  
- TV  
- People  
- Search  
- Trending  
- Discover  
- Images  
- Translations  
- Credits  
- Metadata  

### v4 focuses on:
- OAuth authentication  
- Account details  
- Lists  
- Ratings  
- Permissions  
- Organizations & roles  

### High-level mapping:
- **Most v3 metadata endpoints have no v4 equivalent.**
- **Most v4 account endpoints replace or extend v3 account endpoints.**
- **Lists and ratings exist in both, but v4 is the modern version.**

---

# [↑](#table-of-contents) 🎬 Movies

### v3 → v4 Mapping
| v3 Endpoint | v4 Equivalent | Notes |
|------------|---------------|-------|
| Movie details, credits, images, keywords, recommendations, etc. | ❌ None | v4 does not provide metadata. |
| Movie ratings (`POST /movie/{id}/rating`) | ✔️ `POST /account/{account_id}/rating` | v4 consolidates ratings under account. |
| Movie lists (`GET /movie/{id}/lists`) | ✔️ v4 Lists API | v4 lists are more powerful and OAuth-based. |

---

# [↑](#table-of-contents) 📺 TV

### v3 → v4 Mapping
| v3 Endpoint | v4 Equivalent | Notes |
|------------|---------------|-------|
| TV details, credits, images, keywords, recommendations, etc. | ❌ None | v4 does not provide metadata. |
| TV ratings (`POST /tv/{id}/rating`) | ✔️ `POST /account/{account_id}/rating` | Unified rating system. |

---

# [↑](#table-of-contents) 👤 People

### v3 → v4 Mapping
| v3 Endpoint | v4 Equivalent | Notes |
|------------|---------------|-------|
| Person details, images, credits | ❌ None | v4 does not provide metadata. |

---

# [↑](#table-of-contents) 🔍 Search

### v3 → v4 Mapping
| v3 Search | v4 Equivalent | Notes |
|-----------|---------------|-------|
| All search endpoints | ❌ None | v4 does not include search. |

---

# [↑](#table-of-contents) 📈 Trending

### v3 → v4 Mapping
| v3 Trending | v4 Equivalent | Notes |
|-------------|---------------|-------|
| Trending endpoints | ❌ None | v4 does not include trending. |

---

# [↑](#table-of-contents) 🔎 Discover

### v3 → v4 Mapping
| v3 Discover | v4 Equivalent | Notes |
|-------------|---------------|-------|
| Discover movie / TV | ❌ None | v4 does not include discovery. |

---

# [↑](#table-of-contents) 📋 Lists

### v3 → v4 Mapping
| v3 Lists | v4 Lists | Notes |
|----------|----------|-------|
| `GET /list/{list_id}` | ✔️ `GET /list/{list_id}` | Same concept, different auth. |
| `POST /list` | ✔️ `POST /list` | v4 uses OAuth. |
| `POST /list/{id}/add_item` | ✔️ `POST /list/{id}/items` | v4 supports batch operations. |
| `POST /list/{id}/remove_item` | ✔️ `DELETE /list/{id}/items` | v4 uses DELETE with payload. |
| `DELETE /list/{id}` | ✔️ `DELETE /list/{id}` | Same behavior. |

---

# [↑](#table-of-contents) ⭐ Ratings

### v3 → v4 Mapping
| v3 Ratings | v4 Ratings | Notes |
|------------|------------|-------|
| Movie rating | ✔️ Unified rating endpoint | v4 consolidates all ratings. |
| TV rating | ✔️ Unified rating endpoint | |
| Episode rating | ✔️ Unified rating endpoint | |
| Guest session ratings | ❌ None | v4 requires OAuth. |

---

# [↑](#table-of-contents) 👤 Account

### v3 → v4 Mapping
| v3 Account | v4 Account | Notes |
|------------|------------|-------|
| `GET /account` | ✔️ `GET /account` | v4 is OAuth-based. |
| Favorites | ✔️ `GET /account/favorites` | v4 consolidates. |
| Watchlist | ✔️ `GET /account/watchlist` | |
| Rated movies / TV / episodes | ✔️ `GET /account/{id}/rating` | Unified. |
| Lists | ✔️ `GET /account/lists` | |

---

# [↑](#table-of-contents) 🔐 Authentication

### v3 → v4 Mapping
| v3 Auth | v4 Auth | Notes |
|---------|---------|-------|
| API key auth | ❌ None | v4 requires OAuth. |
| Request token | ✔️ `POST /auth/request_token` | |
| Session creation | ✔️ `POST /auth/access_token` | |
| Guest sessions | ❌ None | v4 does not support guest sessions. |

---

# [↑](#table-of-contents) 🏢 Organizations

### v3 → v4 Mapping
| v3 | v4 | Notes |
|----|----|-------|
| ❌ None | ✔️ Organizations API | New in v4. |

---

# [↑](#table-of-contents) 🧩 Roles

### v3 → v4 Mapping
| v3 | v4 | Notes |
|----|----|-------|
| ❌ None | ✔️ Roles API | New in v4. |

---

# [↑](#table-of-contents) 🔑 Permissions

### v3 → v4 Mapping
| v3 | v4 | Notes |
|----|----|-------|
| ❌ None | ✔️ Permissions API | New in v4. |

---

# [↑](#table-of-contents) 🧭 Summary Table

| Category | v3 | v4 | Notes |
|----------|----|----|-------|
| Metadata | ✔️ Yes | ❌ No | v3 only |
| Search | ✔️ Yes | ❌ No | v3 only |
| Discover | ✔️ Yes | ❌ No | v3 only |
| Trending | ✔️ Yes | ❌ No | v3 only |
| Lists | ✔️ Yes | ✔️ Yes | v4 is modern version |
| Ratings | ✔️ Yes | ✔️ Yes | v4 consolidates |
| Account | ✔️ Yes | ✔️ Yes | v4 is OAuth-based |
| Organizations | ❌ No | ✔️ Yes | v4 only |
| Roles | ❌ No | ✔️ Yes | v4 only |
| Permissions | ❌ No | ✔️ Yes | v4 only |

---

# [↑](#table-of-contents) 🔗 Navigation

- [Overview](overview.md)
- [TMDB v3 Endpoints](v3-endpoints.md)
- [TMDB v4 Endpoints](v4-endpoints.md)
- [Notes](notes.md)