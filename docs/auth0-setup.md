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

### Create User Roles

1. Go to **User Management → Roles → Create Role**
2. Create the following roles:
   - **Admin** - Full administrative access (manage categories, statuses, sample data)
   - **Author** - Can create and edit issues
   - **User** - Basic read access

### Assign Roles to Users

1. Go to **User Management → Users**
2. Click on a user
3. Go to the **Roles** tab
4. Click **Assign Roles** and select the appropriate role(s)

### Create Post-Login Action (Required for Role-Based Access)

This action adds user roles to the authentication tokens, enabling role-based authorization in the application.

1. Go to **Actions → Library → Create Action**
2. Name: `Add Roles to Tokens`
3. Trigger: **Login / Post Login**
4. Runtime: **Node 22**
5. Add the following code:

```javascript
exports.onExecutePostLogin = async (event, api) => {
  const namespace = 'https://articlesite.com/roles';
  
  // Get roles assigned to the user
  const assignedRoles = event.authorization?.roles || [];
  
  if (assignedRoles.length > 0) {
    // Add roles to both ID token and access token
    api.idToken.setCustomClaim(namespace, assignedRoles);
    api.accessToken.setCustomClaim(namespace, assignedRoles);
  }
};
```

6. Click **Deploy**
7. Go to **Actions → Flows → Login**
8. Drag the `Add Roles to Tokens` action into the flow between **Start** and **Complete**
9. Click **Apply**

> **Note:** The namespace `https://articlesite.com/roles` must match the constant defined in `Auth0AuthenticationStateProvider.cs`. You can customize this namespace, but ensure both the Auth0 Action and the code use the same value.

## Local Development Configuration

Add to user secrets for the **Api** project:

```bash
cd src/Api
dotnet user-secrets set "Auth0:Domain" "dev-63xbriztum2j1765.us.auth0.com"
dotnet user-secrets set "Auth0:Audience" "https://api.issuemanager.com"
```

Add to user secrets for the **Web** project:

```bash
cd src/Web
dotnet user-secrets set "Auth0:Domain" "dev-63xbriztum2j1765.us.auth0.com"
dotnet user-secrets set "Auth0:ClientId" "TCtpPzgZldVYUo1OdhFTrEUMLYwrZibs"
dotnet user-secrets set "Auth0:ClientSecret" "EIB-OTL_mWm87JD61g-jg_-5kwlNPZ6UXJzEB9-1yVTIZ8Nkpp6EeOE5FC86DwaL"
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
- Show a Login button in the navigation
- Display role-specific menu items (Admin users see Categories, Statuses, Admin, Sample Data)
- Protect API endpoints with JWT validation
- Redirect unauthenticated users to Auth0 Universal Login

### Troubleshooting Roles

If admin menu items are not appearing:

1. **Check user roles in Auth0:**
   - Go to **User Management → Users → [Your User] → Roles**
   - Verify the "Admin" role is assigned

2. **Verify the Post-Login Action is active:**
   - Go to **Actions → Flows → Login**
   - Confirm the "Add Roles to Tokens" action is in the flow

3. **Check application logs:**
   - The app logs all claims at debug level when a user authenticates
   - Look for: `Claim: https://articlesite.com/roles = ["Admin"]`
   - If this claim is missing, the Post-Login Action isn't working

4. **Test the token:**
   - Use [jwt.io](https://jwt.io) to decode your ID token
   - Look for the `https://articlesite.com/roles` claim in the payload

Without configuration, the application runs in **open mode** (no authentication enforced).
