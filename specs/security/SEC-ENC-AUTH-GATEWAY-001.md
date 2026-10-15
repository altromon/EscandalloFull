---
id: "SEC-ENC-AUTH-GATEWAY-001"
type: "security-enclave"
title: "Gateway Perimetral de Autenticación y Control de Acceso"
status: "draft"
version: "1.0.0"
trust-zone: "dmz"
perimeter-rules:
  - "Obligatoriedad de canal cifrado HTTPS (TLS 1.3)"
  - "Estrangulamiento y Rate-limiting estricto en login (máximo 5 intentos fallidos por IP/cuenta antes de bloqueo)"
  - "Emisión y verificación de tokens de sesión con firma criptográfica HMAC-SHA256 y expiración corta"
allowed-inbound: []
allowed-outbound:
  - "SEC-ENC-SERVICE-CORE-001"
enforced-by:
  - "SEC-REQ-AUTH-MEMBER-001"
authentication-mechanism: "jwt"
---

# Security Enclave: Gateway Perimetral de Autenticación (`SEC-ENC-AUTH-GATEWAY-001`)

## 1. Boundary Definition and Architecture
Punto de entrada público a la plataforma web. Recibe las conexiones entrantes de clientes web, valida la autenticidad de credenciales, emite y renueva tokens de sesión y canaliza las peticiones autenticadas hacia el núcleo de servicios.

## 2. Inbound & Outbound Policies
- **Inbound**: Tráfico público de Internet sobre HTTPS (puerto 443).
- **Outbound**: Comunicación interna y autorizada hacia `SEC-ENC-SERVICE-CORE-001` propagando el contexto de identidad verificado (`UserId`, `TenantId`, `ActiveCenterId`, `Role`).
