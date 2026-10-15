---
id: "SEC-ENC-DATA-ACCESS-001"
type: "security-enclave"
title: "Capa de Persistencia y Aislamiento Multi-inquilino"
status: "draft"
version: "1.0.0"
trust-zone: "highly-trusted"
perimeter-rules:
  - "Filtros globales obligatorios a nivel de ORM (EF Core Global Query Filters por TenantId y CenterId)"
  - "Inmutabilidad forzada y registro solo de inserción (append-only) en tablas de auditoría histórica"
  - "Cadena de conexión a base de datos PostgreSQL protegida mediante variables de entorno seguras"
allowed-inbound:
  - "SEC-ENC-SERVICE-CORE-001"
  - "SEC-ENC-PAYMENT-INTEGRATION-001"
allowed-outbound: []
enforced-by:
  - "SEC-REQ-AUTHZ-TENANT-FILTER-001"
  - "SEC-REQ-AUDIT-IMMUTABLE-001"
  - "SEC-REQ-DEMO-ISOLATION-001"
authentication-mechanism: "none"
---

# Security Enclave: Capa de Persistencia (`SEC-ENC-DATA-ACCESS-001`)

## 1. Boundary Definition and Architecture
Contenedor de base de datos relacional PostgreSQL y capa de acceso a datos mediante Entity Framework Core. Custodia los factores de coste, catálogos de cosméticos, fórmulas de escandallo, histórico de variaciones y suscripciones.

## 2. Inbound & Outbound Policies
- **Inbound**: Conexiones internas desde `SEC-ENC-SERVICE-CORE-001` y `SEC-ENC-PAYMENT-INTEGRATION-001`. Acceso externo directo prohibido.
- **Outbound**: Ninguna conexión saliente hacia Internet o servicios externos.
