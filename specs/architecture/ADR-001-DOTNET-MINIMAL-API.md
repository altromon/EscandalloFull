---
id: ADR-001-DOTNET-CONTROLLERS-API
type: architecture-decision-record
title: "Selección de .NET 10 LTS con ASP.NET Core Web API (Controladores Clásicos) y SPA Estático"
status: accepted
version: "1.1.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
decision-date: "2026-10-15"
affects-components:
  - CMP-AUTH-GATEWAY-001
  - CMP-COST-ENGINE-001
  - CMP-MARGIN-CALCULATOR-001
  - CMP-SUBSCRIPTION-MGR-001
  - CMP-EXPORT-DOC-001
  - CMP-DEMO-MGR-001
  - CMP-WEB-UI-001
supersedes: null
superseded-by: null
---

# ADR-001: Selección de .NET 10 LTS con ASP.NET Core Web API (Controladores Clásicos) y SPA Estático

## 1. Context and Problem Statement
El sistema **Escandallo Estética** exige una ejecución de alto rendimiento y robustez de tipos en cálculos financieros, condicionado por el objetivo de calidad `QR-DEPLOY-COST-001`: operar la aplicación completa (backend, frontend y base de datos) en producción con un presupuesto inferior a 10-15 €/mes en un servidor VPS modesto (ej. Hetzner CX22 con 2 vCPU y 4 GB de RAM), manteniendo el consumo en reposo del backend por debajo de 150 MB de RAM.
Se requiere una estructura de código clásica, modular y mantenible para el backend, priorizando la legibilidad, la separación explícita de responsabilidades mediante clases controladoras y la facilidad de inyección de filtros y atributos de seguridad.

## 2. Considered Technology Options
1. **Opción A: Node.js / NestJS + PostgreSQL**:
   - *Pros*: Ecosistema común JavaScript/TypeScript; desarrollo ágil.
   - *Contras*: Mayor consumo de memoria heap en reposo (> 180-220 MB en NestJS); falta de tipos numéricos decimales de alta precisión nativos (riesgo de imprecisión en punto flotante para escandallos financieros).
2. **Opción B: ASP.NET Core Minimal APIs (`app.MapGet(...)`)**:
   - *Pros*: Sintaxis concisa en archivos reducidos.
   - *Contras*: Menor estructura organizativa en proyectos medianos/grandes; configuración más dispersa de filtros y decoradores de autorización en comparación con la convención clásica.
3. **Opción C: .NET 10 LTS (C# 13) con Controladores Clásicos (`[ApiController]` / `ControllerBase`) + SPA Estático** (Elegida):
   - *Pros*: Estructura canónica y probada en la industria; enrutamiento declarativo por atributos (`[Route]`, `[HttpGet]`); agrupación natural de endpoints en clases desacopladas por Bounded Context (`AuthController`, `CostsController`, `EscandallosController`, `BillingController`, `DemoController`, `ExportController`); integración nativa con filtros de acción y validación de modelos ModelState; tipo nativo `decimal` de 128 bits para cálculos financieros exactos; consumo de memoria en reposo perfectamente contenido (< 90-130 MB de RAM en contenedores Chiseled Linux); licencia MIT permisiva.
   - *Contras*: Ligeramente más verboso que Minimal APIs, ampliamente compensado por su claridad estructural y mantenibilidad a largo plazo.

## 3. Decision Outcome
Se elige la **Opción C: .NET 10 LTS con Controladores Clásicos (`[ApiController]`)**.
El backend se organiza en controladores dedicados según la arquitectura limpia de ASP.NET Core. El mismo contenedor Linux optimizado sirve los activos estáticos del frontend SPA (compilados con Vite), eliminando servidores web secundarios y garantizando el cumplimiento estricto de las restricciones presupuestarias y de memoria.

## 4. Consequences
- **Positive**: Cumplimiento holgado de `QR-DEPLOY-COST-001` (< 150 MB RAM en reposo); organización clara y modular del código sin dispersión de lambdas; filtros de seguridad y autorización (`AuthorizeAttribute`, validación de membresía) aplicables limpiamente a nivel de clase o acción; licencia MIT permisiva.
- **Negative / Trade-offs**: Requiere clases controladoras explícitas (`*Controller.cs`), lo cual es el estándar preferido por el equipo para el mantenimiento del sistema.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Propuesta inicial con Minimal APIs | CHG-ARCH-001 |
| **1.1.0** | 2026-10-15 | `agent-system-architect` & PO | Cambio a controladores clásicos `[ApiController]` | CHG-ARCH-001 |
