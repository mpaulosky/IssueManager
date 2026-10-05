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

# Auth0 Setup Guide for IssueManager

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

1. Click **Deploy**
2. Go to **Actions → Flows → Login**
3. Drag the `Add Roles to Tokens` action into the flow between **Start** and **Complete**
4. Click **Apply**

> **Note:** The namespace `https://articlesite.com/roles` must match the constant defined in `Auth0AuthenticationStateProvider.cs`.
> You can customize this namespace, but ensure both the Auth0 Action and the code use the same value.

## Local Development Configuration

The Api, Web and `AppHost.Tests.E2E` projects share one user-secrets store (`UserSecretsId`
`94491f6e-auth0-values-3ff40da38702`), so each value is set once:

```bash
dotnet user-secrets set "Auth0:Domain" "your-tenant.auth0.com" --project src/Api
dotnet user-secrets set "Auth0:Audience" "https://api.issuemanager.com" --project src/Api
dotnet user-secrets set "Auth0:ClientId" "<Web application Client ID>" --project src/Api
dotnet user-secrets set "Auth0:ClientSecret" "<Web application Client Secret>" --project src/Api
```

The E2E tests log in as one test user per role:

```bash
dotnet user-secrets set "Auth0:Admin:Username" "<admin email>" --project src/Api
dotnet user-secrets set "Auth0:Admin:Password" "<admin password>" --project src/Api
# ...and the same for Auth0:Author and Auth0:User
```

The E2E host runs the Web app on `https://localhost:7176`, so the Auth0 application needs
`https://localhost:7176/callback` in **Allowed Callback URLs** and `https://localhost:7176/` in **Allowed Logout URLs**.

> **Moving from per-project secrets:** the Api and Web projects used to have their own stores
> (`37795a0b-dd55-4ca7-a0ac-d5b6effd8208` and `9ea929cd-8b58-4052-a68a-b79a1b47c36c`). Values left there
> are no longer read. Copy them into the shared store with the commands above; `dotnet user-secrets list --id <old id>` shows what you had.

## CI Secrets

CI reads one repository secret, `TEST_ENV` (**Settings → Secrets and variables → Actions**), holding `NAME=value` lines
that are exported to the test jobs:

```text
Auth0__Domain=your-tenant.auth0.com
Auth0__ClientId=<Web application Client ID>
Auth0__ClientSecret=<Web application Client Secret>
Auth0__Audience=https://api.issuemanager.com
Auth0__Admin__Username=<admin email>
Auth0__Admin__Password=<admin password>
Auth0__Author__Username=<author email>
Auth0__Author__Password=<author password>
Auth0__User__Username=<user email>
Auth0__User__Password=<user password>
```

Without the Auth0 values the E2E host can't start, and the E2E job fails.

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
