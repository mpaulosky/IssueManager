# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in **IssueManager**, please report it responsibly by sending an email to the maintainers rather than using the public issue tracker.

**Email:** [security contact to be added by maintainers]

Include:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Any suggested fixes (if available)

## Security Response

- **Acknowledgment:** We will acknowledge your report within 48 hours
- **Assessment:** We will investigate and determine severity
- **Fix & Patch:** We will work to fix the issue and release a patch
- **Disclosure:** We follow coordinated disclosure practices

## Supported Versions

| Version | Supported |
| --- | --- |
| Latest | ✅ Yes |
| Previous major | ✅ Limited |
| Older | ❌ No |

## Security Best Practices

- Use **HTTPS** for all connections
- Enable **authentication** and **authorization** on all endpoints
- Use **strong secrets** in production (Key Vault, environment variables)
- Keep **.NET and MongoDB** up to date
- Enable **audit logging** for sensitive operations
- Review security headers in middleware configuration

## Dependencies

We regularly update NuGet packages to include security patches. Monitor `.github/dependabot` alerts and `.github/workflows` for automated security scanning.

---

**Last Updated:** 2026-02-17  
**Maintainers:** IssueManager Squad
