---
id: "UC-AUTH-MULTI-001"
type: "use-case"
title: "Autenticación Multi-inquilino y Selección de Centro"
status: "draft"
version: "1.0.0"
primary-actor: "ACT-USER-001"
supporting-actors:
  - "ACT-GUEST-001"
---

# Use Case: Autenticación Multi-inquilino y Selección de Centro (`UC-AUTH-MULTI-001`)

## 1. Intent and Outcome
As a `ACT-USER-001`, I want to log in securely and choose an aesthetic center from my managed centers portfolio, to operate and review escandallos exclusively within the selected center's tenant boundary.

## 2. Preconditions
- The user account exists in the authentication store with assigned organization and aesthetic center memberships.
- The web application is accessible over HTTPS.

## 3. Main Success Scenario
1. The user navigates to the login screen and provides authentication credentials.
2. The system verifies credentials and identifies the tenant and associated aesthetic centers.
3. If the user manages multiple centers, the system displays a center selector.
4. The user selects the active aesthetic center for the session.
5. The system establishes a secure session scope locked to the selected `CenterId` and `TenantId`.
6. The user is redirected to the center's dashboard with active escandallos and margins.

## 4. Alternative and Error Flows
- **Single Center User**: If the user has exactly one center, the system automatically selects it and redirects immediately to the dashboard.
- **Guest Access**: When `ACT-GUEST-001` logs in via an invitation link or credentials, the system establishes a restricted session with access limited to public service breakdowns.
- **Invalid Credentials**: System displays an authentication failure message and throttles failed attempts.
