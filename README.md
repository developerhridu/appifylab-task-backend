# Buddy Script — Backend API

Social feed API for the Buddy Script task. **.NET 10 Minimal API** in **Clean Architecture** with **CQRS (MediatR)**, **EF Core + PostgreSQL**, cookie-based **JWT** auth, and **Cloudinary** image storage.

## Architecture

```
src/
├─ BuddyScript.Domain          entities, enums, base types — zero dependencies
├─ BuddyScript.Application     CQRS commands/queries + handlers, validators, interfaces, DTOs
├─ BuddyScript.Infrastructure  EF Core DbContext + configs, JWT/hashing, Cloudinary
└─ BuddyScript.Api             Minimal API endpoint groups, middleware, DI, Program.cs
```

Dependency rule points inward: `Api → Application → Domain`, `Infrastructure → Application/Domain`, `Domain → nothing`. Endpoints hold no logic — they delegate to `ISender.Send(...)`.

## Features

- **Auth** — register / login / logout / me. JWT delivered in an **httpOnly + Secure + SameSite** cookie; PBKDF2 password hashing; login failures don't reveal whether an email exists.
- **Posts** — create (text + image), keyset-paginated feed (newest first), **Public / Private** visibility.
- **Likes** — idempotent like/unlike for posts and comments + "who liked" lists.
- **Comments** — comments, nested replies, per-item like state; comment tree built in a single query (no N+1).
- **Cross-cutting** — FluentValidation pipeline, RFC7807 error responses, CORS, auth rate limiting.

## Prerequisites

- .NET SDK 10
- A PostgreSQL database (this project uses **Neon**)
- A Cloudinary account (free tier) for image uploads

## Configuration

Settings live in `src/BuddyScript.Api/appsettings.json`:

| Key | Purpose |
|---|---|
| `ConnectionStrings:Default` | PostgreSQL connection string |
| `Jwt:Key / Issuer / Audience / ExpiryMinutes` | JWT signing + lifetime |
| `Cloudinary:CloudName / ApiKey / ApiSecret` | image storage |
| `Cors:AllowedOrigins` | frontend origin(s), e.g. `http://localhost:3000` |

> Secret values in `appsettings.json` are intentionally blank — never commit real secrets. Supply them via **user-secrets** for local dev and **environment variables** for deployment (.NET maps `:` → `__`), e.g. `ConnectionStrings__Default`, `Cloudinary__ApiSecret`, `Jwt__Key`.

Local dev secrets (the API project already has a `UserSecretsId`):

```bash
cd src/BuddyScript.Api
dotnet user-secrets set "ConnectionStrings:Default" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "Jwt:Key" "<min-32-byte-random-key>"
dotnet user-secrets set "Cloudinary:CloudName" "<cloud-name>"
dotnet user-secrets set "Cloudinary:ApiKey" "<api-key>"
dotnet user-secrets set "Cloudinary:ApiSecret" "<api-secret>"
dotnet user-secrets set "Cors:AllowedOrigins:0" "http://localhost:3000"
```

## Run

```bash
# from repo root
dotnet restore
dotnet ef database update \
  --project src/BuddyScript.Infrastructure \
  --startup-project src/BuddyScript.Api        # apply migrations
dotnet run --project src/BuddyScript.Api        # → http://localhost:5261 (Swagger at /swagger)
```

## Seed demo data

With the API running:

```bash
node tools/seed.mjs                 # local (http://localhost:5261)
API=https://your-api node tools/seed.mjs
```

Creates demo users **alice@demo.com** / **bob@demo.com** (password `Passw0rd!`) with sample posts, comments, replies and likes.

## API summary

```
POST   /api/auth/register | login | logout        GET /api/auth/me
GET    /api/posts?cursor=&limit=                   POST /api/posts (multipart: content, visibility, image?)
POST   /api/posts/{id}/like   DELETE .../like      GET /api/posts/{id}/likes
GET    /api/posts/{id}/comments                    POST /api/posts/{id}/comments
POST   /api/comments/{id}/like DELETE .../like     GET /api/comments/{id}/likes
```

## Deployment (Render, Docker)

CI/CD is wired for [Render](https://render.com) using the repo `Dockerfile`.

**Pipeline:**

1. Open a PR → **GitHub Actions** (`.github/workflows/ci.yml`) restores + builds in Release. Merge is blocked if the build fails.
2. Merge to `main` → **Render** detects the push, builds the `Dockerfile`, and deploys.
3. Container boots → `db.Database.Migrate()` applies pending EF migrations → Render probes `/health` → traffic cuts over on success. A failed migration or boot fails the deploy and keeps the previous version live.

**One-time Render setup:**

- New **Web Service** → connect this repo. `render.yaml` (Blueprint) sets runtime = `docker`, `dockerfilePath: ./Dockerfile`, `healthCheckPath: /health`, `autoDeploy: true`.
- The container listens on **8080** (`ASPNETCORE_URLS=http://+:8080`); Render forwards to it automatically.
- Set these environment variables in the Render dashboard (marked `sync: false` in `render.yaml`):

| Env var | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` (preset in `render.yaml`) |
| `ConnectionStrings__Default` | PostgreSQL connection string |
| `Jwt__Key` | JWT signing key (min 32 bytes) |
| `Cloudinary__CloudName` | Cloudinary cloud name |
| `Cloudinary__ApiKey` | Cloudinary API key |
| `Cloudinary__ApiSecret` | Cloudinary API secret |
| `Cors__AllowedOrigins__0` | deployed frontend origin |

> **⚠ Rotate leaked credentials.** Earlier commits contained real Neon, JWT, and Cloudinary secrets. They remain in git history — rotate all of them (new Neon password, new JWT key, new Cloudinary secret) before or immediately after going live. History scrubbing is optional but recommended.

## Scale notes

- Feed uses **keyset pagination** (`(CreatedAt, Id)` cursor) with index `ix_posts_feed` — stable and fast across millions of rows (no OFFSET).
- Like/comment counts and `likedByMe` are computed in the SQL projection — no N+1.
- Visibility is always filtered **server-side**; clients cannot request others' private posts.
