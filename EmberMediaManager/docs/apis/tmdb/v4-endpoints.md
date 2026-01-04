# [↑](#table-of-contents) TMDB API v4 — Full Endpoint Catalog

TMDB API v4 is a modern, OAuth-based API focused on account-level operations, lists, ratings, permissions, and organizational roles.  
It complements v3 rather than replacing it.

---

## Table of Contents

- 🔐 [Authentication](#-authentication)
- 👤 [Account](#-account)
- 📋 [Lists](#-lists)
- 📦 [Items](#-items)
- ⭐ [Ratings](#-ratings)
- 🔑 [Permissions](#-permissions)
- 🏢 [Organizations](#-organizations)
- 🧩 [Roles](#-roles)
- 🔑 [Tokens](#-tokens)

---

# [↑](#table-of-contents) 🔐 Authentication (OAuth 2.0)

- `POST /auth/request_token`
- `POST /auth/access_token`
- `DELETE /auth/access_token`

---

# [↑](#table-of-contents) 👤 Account

- `GET /account`
- `GET /account/lists`
- `GET /account/favorites`
- `GET /account/recommendations`
- `GET /account/watchlist`

---

# [↑](#table-of-contents) 📋 Lists

- `GET /list/{list_id}`
- `POST /list`
- `PUT /list/{list_id}`
- `DELETE /list/{list_id}`
- `POST /list/{list_id}/items`
- `DELETE /list/{list_id}/items`

---

# [↑](#table-of-contents) 📦 Items

- `GET /item/{item_id}`
- `GET /item/{item_id}/changes`

---

# [↑](#table-of-contents) ⭐ Ratings

- `POST /account/{account_id}/rating`
- `DELETE /account/{account_id}/rating`

---

# [↑](#table-of-contents) 🔑 Permissions

- `GET /account/permissions`

---

# [↑](#table-of-contents) 🏢 Organizations

- `GET /organization/{organization_id}`
- `GET /organization/{organization_id}/roles`

---

# [↑](#table-of-contents) 🧩 Roles

- `GET /role/{role_id}`

---

# [↑](#table-of-contents) 🔑 Tokens

- `POST /auth/request_token`
- `POST /auth/access_token`
- `DELETE /auth/access_token`

---

# [↑](#table-of-contents) Navigation

- [Overview](overview.md)
- [TMDB v3 Endpoints](v3-endpoints.md)
- [v3 → v4 Mapping](mapping.md)
- [Notes](notes.md)