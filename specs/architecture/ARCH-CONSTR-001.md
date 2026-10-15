---
id: ARCH-CONSTR-001
type: architecture-constraints
title: "02. Architecture Constraints - Escandallo Estética"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 2
naf-perspective: "Architecture Constraints"
constraints:
  - CON-TECH-DOTNET10
  - CON-TECH-MEMORY-150MB
  - CON-TECH-POSTGRES
  - CON-TECH-LINUX-CONTAINER
  - ACON-ORG-AISDLC
  - ACON-ORG-HUMAN-SOVEREIGNTY
  - LIC-POL-PERMISSIVE-ONLY
license-policy: "license-policy.yaml"
supersedes: null
superseded-by: null
---

# 02. Architecture Constraints (arc42 Sec. 2 / NAF Constraints)

## 1. Non-Negotiable Technical Constraints (`CON-*`)
Limitaciones técnicas de plataforma, hardware, runtime y compatibilidad binaria:

| Constraint ID | Constraint Name | Description and Technical Rationale |
| :--- | :--- | :--- |
| `CON-TECH-DOTNET10` | Runtime .NET 10 LTS | El backend debe compilar y ejecutarse en .NET 10 (C# 13) utilizando ASP.NET Core Web API con controladores clásicos (`[ApiController]`) para garantizar estructura modular, separación clara y mínimo consumo de recursos. |
| `CON-TECH-MEMORY-150MB` | Techo de Memoria RAM (< 150 MB) | Conforme a `QR-DEPLOY-COST-001`, la huella de memoria en reposo del backend no debe superar 150 MB para permitir despliegues en VPS ultrabaratos (< 10-15 €/mes, ej. Hetzner CX22 o Fly.io). |
| `CON-TECH-POSTGRES` | Motor Relacional PostgreSQL 16+ | Persistencia unificada en PostgreSQL con capacidades transaccionales ACID, nivel de aislamiento `Repeatable Read` / `Serializable` y soporte de tipos JSONB para histórico e indexación. |
| `CON-TECH-LINUX-CONTAINER` | Contenedores Minimalistas Linux | Empaquetado de producción en imágenes Docker ligeras basadas en Ubuntu Chiseled o Alpine Linux sin utilidades innecesarias (superficie de ataque reducida). |

---

## 2. Organizational and Process Constraints (`ACON-*`)
Reglas de gobernanza, estándares organizativos y flujos de trabajo del marco AI-SDLC:

| Constraint ID | Name | Binding Directive |
| :--- | :--- | :--- |
| `ACON-ORG-AISDLC` | Flujo de Git de 4 Niveles | Jerarquía estricta: `task/*` ➔ `feat/*` / `bug/*` ➔ `release/*` ➔ `main` con trazabilidad 360° en commits (Trailers `Author-Type`, `Task-ID`). |
| `ACON-ORG-HUMAN-SOVEREIGNTY` | Soberanía Humana Inalienable | Prohibición absoluta de auto-aprobación o auto-fusión de Pull Requests por parte de agentes autónomos. |
| `ACON-ORG-CONTRACT-FIRST` | Integración Contract-First Full-Stack | Toda interfaz frontend-backend debe definirse previamente mediante especificaciones de contrato tipado antes de la implementación de código de producción. |

---

## 3. Open Source License Compliance (`LIC-POL-*`)
En estricto cumplimiento con [`license-policy.yaml`](../../license-policy.yaml):
- **Permisivas (ALLOW)**: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC.
- **Copyleft Débil (REVIEW_REQUIRED)**: LGPL-2.1+, MPL-2.0 (restringidas a librerías desacopladas dinámicamente).
- **Copyleft Fuerte / Viral (DENY)**: GPLv2, GPLv3, AGPLv3 (terminantemente prohibidas en el árbol de dependencias).
- **Comerciales / Source-Available (COMMERCIAL_APPROVAL_REQUIRED)**: SSPL, BSL (prohibidas para SaaS sin adquisición de licencia corporativa).

---

## 4. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición inicial de restricciones técnicas y organizativas | CHG-ARCH-001 |
