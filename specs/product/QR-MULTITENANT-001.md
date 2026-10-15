---
id: "QR-MULTITENANT-001"
type: "quality-requirement"
title: "Aislamiento Lógico Multi-inquilino y Multicentro"
status: "draft"
version: "1.0.0"
category: "security-confidentiality"
---

# Quality Requirement: Aislamiento Lógico Multi-inquilino y Multicentro (`QR-MULTITENANT-001`)

## 1. Quality Attribute and Target
- **ISO 25010 Attribute**: Security / Confidentiality / Multi-tenancy.
- **Target**: Cero fugas de información (*zero data cross-contamination*) entre clientes/organizaciones o entre centros de estética no autorizados.

## 2. Architectural Strategy
- Esquema de base de datos multi-inquilino compartido con discriminador obligatorio (`TenantId` y `CenterId`) en cada entidad y tabla relacional.
- Aplicación de filtros de consulta globales automáticos a nivel de ORM (Entity Framework Core Global Query Filters) inyectados desde el contexto de la petición autenticada.
- Bloqueo en pipeline de middleware para verificar que el usuario autenticado tiene membresía explícita en el `CenterId` activo.
