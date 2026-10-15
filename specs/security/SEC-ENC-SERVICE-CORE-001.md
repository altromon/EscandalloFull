---
id: "SEC-ENC-SERVICE-CORE-001"
type: "security-enclave"
title: "Núcleo de Servicios de Dominio y Cálculo de Escandallos"
status: "draft"
version: "1.0.0"
trust-zone: "trusted"
perimeter-rules:
  - "Inyección obligatoria de TenantContext verificado en cada llamada de servicio"
  - "Control de acceso basado en roles (RBAC) y filtrado de datos en origen para invitados (BR-GUEST-CENSOR-001)"
  - "Limitación de concurrencia y rate-limiting en recálculos masivos para protección de memoria RAM"
allowed-inbound:
  - "SEC-ENC-AUTH-GATEWAY-001"
allowed-outbound:
  - "SEC-ENC-DATA-ACCESS-001"
  - "SEC-ENC-EXPORT-SANDBOX-001"
enforced-by:
  - "SEC-REQ-DATA-CENSOR-GUEST-001"
  - "SEC-REQ-RATE-RECALC-THROTTLE-001"
  - "SEC-REQ-QUOTA-BACKEND-ENFORCE-001"
authentication-mechanism: "jwt"
---

# Security Enclave: Núcleo de Servicios de Dominio (`SEC-ENC-SERVICE-CORE-001`)

## 1. Boundary Definition and Architecture
Capa de lógica de negocio y aplicación en ASP.NET Core (.NET 10). Ejecuta las reglas de cálculo financiero (`BR-COST-FORMULA-001`), gestión de umbrales visuales (`BR-COLOR-THRESHOLDS-001`), control de cuota freemium (`BR-FREEMIUM-LIMIT-001`) y censura en origen para invitados (`BR-GUEST-CENSOR-001`).

## 2. Inbound & Outbound Policies
- **Inbound**: Solicitudes preautenticadas y validadas procedentes de `SEC-ENC-AUTH-GATEWAY-001`.
- **Outbound**: Consultas transaccionales hacia `SEC-ENC-DATA-ACCESS-001` y llamadas a renderizado en `SEC-ENC-EXPORT-SANDBOX-001`.
