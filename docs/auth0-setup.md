---
post_title: Auth0 Setup Guide for IssueManager
author1: Gandalf
post_slug: auth0-setup-guide
microsoft_alias: N/A
featured_image: N/A
categories: Security
tags: auth0, authentication, setup
ai_note: AI-generated setup guide
summary: Step-by-step Auth0 configuration guide for the IssueManager application.
post_date: 2026-02-27
---

## Prerequisites

- An Auth0 account (free tier is sufficient for development)
- Access to the IssueManager GitHub repository secrets

## Auth0 Tenant Setup

### Create an Application (for the Blazor Web UI)

1. Log into [Auth0 Dashboard](https://manage.auth0.com/)
2. Go to **Applications → Applications → Create Application**
3. Name: `IssueManager Web`
4. Type: **Regular Web Application**
5. Configure allowed callbacks:
   - **Allowed Callback URLs:** `https://localhost:7001/callback,https://your-production-domain.com/callback`
   - **Allowed Logout URLs:** `https://localhost:7001,https://your-production-domain.com`
   - **Allowed Web Origins:** `https://localhost:7001,https://your-production-domain.com`
6. Note the **Domain**, **Client ID**, and **Client Secret**

### Create an API (for JWT validation in the API project)

1. Go to **Applications → APIs → Create API**
2. Name: `IssueManager API`
3. Identifier (Audience): `https://api.issuemanager.com` (or your chosen identifier)
4. Signing Algorithm: `RS256`

## Local Development Configuration

Add to user secrets for the **Api** project:

```bash
cd src/Api
dotnet user-secrets set "Auth0:Domain" "your-tenant.auth0.com"
dotnet user-secrets set "Auth0:Audience" "https://api.issuemanager.com"
```

Add to user secrets for the **Web** project:

```bash
cd src/Web
dotnet user-secrets set "Auth0:Domain" "your-tenant.auth0.com"
dotnet user-secrets set "Auth0:ClientId" "your-client-id"
dotnet user-secrets set "Auth0:ClientSecret" "your-client-secret"
dotnet user-secrets set "Auth0:Audience" "https://api.issuemanager.com"
```

## GitHub Actions Secrets

Add these secrets to the repository at **Settings → Secrets and variables → Actions**:

| Secret Name | Value |
|-------------|-------|
| `AUTH0_DOMAIN` | `your-tenant.auth0.com` |
| `AUTH0_CLIENT_ID` | Your Auth0 Web application Client ID |
| `AUTH0_CLIENT_SECRET` | Your Auth0 Web application Client Secret |
| `AUTH0_AUDIENCE` | `https://api.issuemanager.com` |

## Verification

Once configured, the application will:
- Show a Login button in the navigation (once Legolas wires the UI)
- Protect API endpoints with JWT validation (once Sam adds `RequireAuthorization()` to endpoints)
- Redirect unauthenticated users to Auth0 Universal Login

Without configuration, the application runs in **open mode** (no authentication enforced).
