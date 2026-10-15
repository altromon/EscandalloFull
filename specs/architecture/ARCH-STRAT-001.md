---
id: ARCH-STRAT-001
type: solution-strategy
title: "04. Architecture Solution Strategy - Escandallo Estética"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 4
naf-perspective: "Service & Resource Strategy"
strategies:
  - STRAT-ARCH-CLEAN-DOTNET
  - STRAT-DATA-MULTITENANT-FILTERS
  - STRAT-ASYNC-CHANNELS-DEBOUNCE
  - STRAT-SEC-ZERO-TRUST-ENCLAVES
  - STRAT-AUDIT-CRYPTO-CHAIN
supersedes: null
superseded-by: null
---

# 04. Solution Strategy (arc42 Sec. 4 / NAF Strategy)

## 1. Fundamental Structural Decisions (`STRAT-*`)
Estrategias globales de arquitectura para satisfacer los requisitos funcionales, objetivos de calidad y restricciones operativas:

| Strategy ID | Fundamental Pattern / Decision | Technical Rationale & Trade-offs |
| :--- | :--- | :--- |
| `STRAT-ARCH-CLEAN-DOTNET` | Arquitectura Limpia Modular en .NET 10 | Desacoplamiento estricto del dominio financiero respecto a frameworks web y de persistencia. Consumo en reposo < 150 MB de RAM (`QR-DEPLOY-COST-001`). |
| `STRAT-DATA-MULTITENANT-FILTERS` | Multi-inquilino Compartido con Filtros Globales en ORM | Base de datos PostgreSQL compartida con columnas obligatorias `tenant_id` y `center_id` en todas las tablas operativas, blindadas mediante EF Core Global Query Filters (`QR-MULTITENANT-001`, `SEC-REQ-AUTHZ-TENANT-FILTER-001`). |
| `STRAT-ASYNC-CHANNELS-DEBOUNCE` | Recálculo Asíncrono Ligero con `System.Threading.Channels` | Desacoplamiento in-process con *debouncing* de 500 ms para recálculos en cascada (`FR-HISTORY-RECALC-001`, `SEC-REQ-RATE-RECALC-THROTTLE-001`). Evita brokers pesados como Kafka/RabbitMQ preservando memoria en VPS económico. |
| `STRAT-SEC-ZERO-TRUST-ENCLAVES` | Segregación por Enclaves de Seguridad | Zonificación estricta (`SEC-ENC-*`) con censura en origen para invitados (`BR-GUEST-CENSOR-001`) y sandbox documental aislado sin acceso a red externa (`SEC-ENC-EXPORT-SANDBOX-001`). |
| `STRAT-AUDIT-CRYPTO-CHAIN` | Cadena Criptográfica Inmutable de Auditoría | Registro append-only de modificaciones de costes con hashing encadenado SHA-256 e inhabilitación de `UPDATE` / `DELETE` (`SEC-REQ-AUDIT-IMMUTABLE-001`). |

---

## 2. Decomposition and Design Principles
- **Domain-Driven Design (DDD)**: Modelo de dominio rico con entidades puras de cálculo de escandallos (`Recipe`, `CostFactor`, `WorkerCost`, `OverheadCost`, `CosmeticProduct`, `PackSession`) libres de dependencias de I/O.
- **Contract-First Full-Stack**: Especificaciones OpenAPI / DTOs tipados compartidos para frontend y backend. Proyección de DTOs sanitizados en backend para rol invitado (`ACT-GUEST-001`).
- **Resiliencia y Monitoreo**: Health checks nativos (`/healthz` para liveness, `/readyz` para readiness de BD PostgreSQL) y reinicio supervisado del contenedor (`QR-AVAILABILITY-001`).
- **Protección de Datos y Recuperación**: Copias de seguridad periódicas en frío/caliente con volcado lógico cifrado y sincronización de registros WAL hacia almacenamiento S3 de ultrabajo coste (`QR-DR-BACKUP-001`).

---

## 3. Quality Goals Fulfillment Matrix

| Quality Goal | Adopted Decision / Strategy | Guarantee Mechanism |
| :--- | :--- | :--- |
| `QR-DEPLOY-COST-001` (Coste < 15 €/mes, RAM < 150 MB) | .NET 10 Web API Controllers + In-process Channels + PostgreSQL en VPS | Contenedor Chiseled Linux sin runtime pesado ni middleware redundante |
| `QR-MULTITENANT-001` (Aislamiento Multi-inquilino) | EF Core Global Query Filters automáticos + Claims en JWT | Imposibilidad estructural de consultar datos entre centros ajenos |
| `QR-AVAILABILITY-001` (99.9% Disponibilidad) | Docker Restart Policy + Health Checks + Desacoplamiento Webhooks | Reinicio automático en caso de fallo y degradación controlada |
| `QR-DR-BACKUP-001` (RPO $\le$ 1h, RTO $\le$ 2h) | WAL Archiving continuo + `pg_dump` cifrado AES-256 hacia S3 | Procedimiento reproducible como código para restauración |

---

## 4. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición inicial de estrategias de solución arquitectural arc42 / NAF | CHG-ARCH-001 |
