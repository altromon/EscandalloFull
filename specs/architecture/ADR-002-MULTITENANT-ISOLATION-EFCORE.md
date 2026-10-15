---
id: ADR-002-MULTITENANT-ISOLATION-EFCORE
type: architecture-decision-record
title: "Estrategia de Aislamiento Lógico Multi-inquilino en Base de Datos Compartida con EF Core"
status: accepted
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
decision-date: "2026-10-15"
affects-components:
  - CMP-DATA-ACCESS-001
  - CMP-AUTH-GATEWAY-001
supersedes: null
superseded-by: null
---

# ADR-002: Estrategia de Aislamiento Lógico Multi-inquilino en Base de Datos Compartida con EF Core

## 1. Context and Problem Statement
El sistema debe garantizar la estricta segregación de datos entre organizaciones/clientes y entre centros de estética (`QR-MULTITENANT-001`, `SEC-REQ-AUTHZ-TENANT-FILTER-001`), impidiendo el acceso a registros ajenos (anti-BOLA/IDOR). Al mismo tiempo, la solución debe respetar la restricción presupuestaria de infraestructuras ligeras (< 15 €/mes, `QR-DEPLOY-COST-001`).

## 2. Considered Technology Options
1. **Opción A: Base de datos independiente por inquilino (Database-per-Tenant)**:
   - *Pros*: Máximo aislamiento físico; fácil restauración selectiva.
   - *Contras*: Prohibitiva en entornos de bajo coste (cientos de conexiones abiertas, sobrecarga de RAM inasumible en VPS de 4 GB); migraciones lentas y complejas.
2. **Opción B: Esquema independiente por inquilino (Schema-per-Tenant en PostgreSQL)**:
   - *Pros*: Aislamiento a nivel de namespace en el mismo cluster.
   - *Contras*: Dificultad para agregar telemetría global; sobrecarga del catálogo de PostgreSQL cuando el número de centros crece.
3. **Opción C: Base de datos y esquema compartidos con discriminadores y EF Core Global Query Filters** (Elegida):
   - *Pros*: Huella de memoria mínima (un único pool de conexiones compartido < 20 conexiones); consultas optimizadas con índices compuestos `(tenant_id, center_id)`; aplicación automática en todas las consultas de LINQ en el ORM; protección estructural contra olvidos manuales del desarrollador.
   - *Contras*: Requiere disciplina rigurosa para que toda consulta pase por el ORM con el filtro activo.

## 3. Decision Outcome
Se adopta la **Opción C**.
Se implementa una interfaz `ITenantScopedEntity` en todas las entidades de dominio y se configuran *Global Query Filters* a nivel de `DbContext` que inyectan de forma obligatoria las cláusulas `e.TenantId == _tenantContext.TenantId && e.CenterId == _tenantContext.ActiveCenterId`. Adicionalmente, un interceptor `SaveChangesInterceptor` asigna y verifica automáticamente las claves foráneas antes de confirmar transacciones.

## 4. Consequences
- **Positive**: Cero riesgo de fuga de datos o salto entre inquilinos (Tenant Hopping); excelente rendimiento y densidad de centros por servidor en hardware modesto.
- **Negative / Trade-offs**: Las consultas directas de mantenimiento o migraciones globales requieren un usuario administrativo con privilegios de bypass explícito debidamente auditado.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Decisión arquitectural aceptada | CHG-ARCH-001 |
