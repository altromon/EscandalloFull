---
id: "SEC-ENC-EXPORT-SANDBOX-001"
type: "security-enclave"
title: "Sandbox Aislado de Renderizado Documental PDF y XLSX"
status: "draft"
version: "1.0.0"
trust-zone: "semi-trusted"
perimeter-rules:
  - "Inyección preventiva de comilla simple (') en campos de texto para neutralizar Formula Injection en XLSX"
  - "Aislamiento de red total durante el renderizado PDF (deshabilitación de carga remota de imágenes / SSRF)"
  - "Límite duro de memoria RAM (máx 50 MB) y timeout de ejecución de 10 segundos por documento generado"
allowed-inbound:
  - "SEC-ENC-SERVICE-CORE-001"
allowed-outbound: []
enforced-by:
  - "SEC-REQ-EXPORT-SANITIZATION-001"
authentication-mechanism: "none"
---

# Security Enclave: Sandbox Documental (`SEC-ENC-EXPORT-SANDBOX-001`)

## 1. Boundary Definition and Architecture
Módulo aislado para la compilación, ensamblado y exportación de informes ejecutivos en formatos PDF y XLSX (`UC-EXPORT-EXEC-001`).

## 2. Inbound & Outbound Policies
- **Inbound**: Objetos de transferencia de datos (DTOs) ya filtrados y autorizados procedentes de `SEC-ENC-SERVICE-CORE-001`.
- **Outbound**: Ninguna conexión saliente hacia Internet o red local durante el renderizado.
