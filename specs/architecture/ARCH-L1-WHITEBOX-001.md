---
id: ARCH-L1-WHITEBOX-001
type: building-blocks-level-1
title: "05. Level 1 Building Blocks: Overall Whitebox - Escandallo Estética"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 5
naf-perspective: "Services & Systems (Level 1)"
bounded-contexts:
  - "Identidad y Acceso Multicentro"
  - "Cálculo de Costes y Escandallo"
  - "Facturación y Suscripciones"
  - "Exportación Documental"
  - "Gestión de Entorno y Datos de Prueba"
  - "Infraestructura de Datos Multi-inquilino"
  - "Presentación Web"
contains-components:
  - CMP-AUTH-GATEWAY-001
  - CMP-COST-ENGINE-001
  - CMP-MARGIN-CALCULATOR-001
  - CMP-SUBSCRIPTION-MGR-001
  - CMP-EXPORT-DOC-001
  - CMP-DEMO-MGR-001
  - CMP-DATA-ACCESS-001
  - CMP-WEB-UI-001
supersedes: null
superseded-by: null
---

# 05. Building Blocks View: Level 1 Whitebox (arc42 Sec. 5 / NAF Services & Systems)

## 1. Overall System Decomposition (Level 1)
Descompone el sistema **Escandallo Estética** en subsistemas modulares, contenedores de ejecución y contextos delimitados (Bounded Contexts) bajo el modelo de arquitectura limpia y enclaves seguros.

### 1.1 Diagrama Whitebox Nivel 1
```mermaid
graph TD
    subgraph UI_Presentation["Presentación Web (Cliente)"]
        CMP_WEB_UI["CMP-WEB-UI-001: Web SPA / UI"]
    end

    subgraph Enclave_DMZ["Enclave DMZ: SEC-ENC-AUTH-GATEWAY-001"]
        CMP_AUTH["CMP-AUTH-GATEWAY-001: Gateway de Autenticación"]
    end

    subgraph Enclave_Core["Enclave Núcleo de Servicios: SEC-ENC-SERVICE-CORE-001"]
        CMP_COST["CMP-COST-ENGINE-001: Motor de Costes y Recálculo"]
        CMP_MARGIN["CMP-MARGIN-CALCULATOR-001: Matriz de Rentabilidad"]
        CMP_DEMO["CMP-DEMO-MGR-001: Gestor de Modo Demo"]
    end

    subgraph Enclave_Billing["Enclave Pagos: SEC-ENC-PAYMENT-INTEGRATION-001"]
        CMP_SUB["CMP-SUBSCRIPTION-MGR-001: Gestor de Suscripciones y Stripe"]
    end

    subgraph Enclave_Sandbox["Enclave Sandbox: SEC-ENC-EXPORT-SANDBOX-001"]
        CMP_EXP["CMP-EXPORT-DOC-001: Motor Documental PDF/XLSX"]
    end

    subgraph Enclave_Storage["Enclave Persistencia: SEC-ENC-DATA-ACCESS-001"]
        CMP_DATA["CMP-DATA-ACCESS-001: Acceso a Datos EF Core + PostgreSQL"]
    end

    ActorUser[ACT-USER-001 / ACT-GUEST-001] -->|HTTPS| CMP_WEB_UI
    CMP_WEB_UI -->|REST API Calls| CMP_AUTH
    CMP_AUTH -->|Claims Verificados & TenantContext| CMP_COST
    CMP_AUTH -->|Claims Verificados & TenantContext| CMP_MARGIN
    CMP_AUTH -->|Claims Verificados & TenantContext| CMP_DEMO
    CMP_AUTH -->|Solicitud Checkout / Portal| CMP_SUB
    CMP_MARGIN -->|DTOs Sanitizados| CMP_EXP

    StripeHook[Stripe Webhooks] -->|POST HMAC-SHA256| CMP_SUB

    CMP_COST -->|In-Process Channel Event| CMP_MARGIN
    CMP_COST -->|Consultas y Mutaciones| CMP_DATA
    CMP_MARGIN -->|Consultas| CMP_DATA
    CMP_DEMO -->|Carga / Purga Transaccional| CMP_DATA
    CMP_SUB -->|Actualización Cuota Freemium/Premium| CMP_DATA
    CMP_AUTH -->|Verificación Membresía| CMP_DATA
```

---

## 2. Bounded Contexts and Root Components Catalog

| Contexto Delimitado | Componente ID | Tipo Implementación | Enclave Asociado | Responsabilidad Primaria |
| :--- | :--- | :---: | :--- | :--- |
| **Identidad y Acceso** | `CMP-AUTH-GATEWAY-001` | `service` | `SEC-ENC-AUTH-GATEWAY-001` | Validación de credenciales, emisión de JWT, limitación de tasa perimetral y comprobación de membresía multicentro. |
| **Cálculo de Costes** | `CMP-COST-ENGINE-001` | `service` | `SEC-ENC-SERVICE-CORE-001` | CRUD de factores de coste (personal, local, consumibles) y motor asíncrono de recálculo en cascada con *debouncing*. |
| **Cálculo de Costes** | `CMP-MARGIN-CALCULATOR-001` | `service` | `SEC-ENC-SERVICE-CORE-001` | Evaluación de fórmulas de escandallo, cálculo de márgenes con IVA y asignación de código semáforo visual (rojo/amarillo/verde). |
| **Facturación** | `CMP-SUBSCRIPTION-MGR-001` | `service` | `SEC-ENC-PAYMENT-INTEGRATION-001` | Control atómico de cuota freemium (máx 2 escandallos) y verificación criptográfica de webhooks Stripe con deduplicación. |
| **Exportación** | `CMP-EXPORT-DOC-001` | `service` | `SEC-ENC-EXPORT-SANDBOX-001` | Generación de informes PDF y XLSX en sandbox estricto sin red y sanitización anti-formula injection. |
| **Gestión de Entorno** | `CMP-DEMO-MGR-001` | `service` | `SEC-ENC-SERVICE-CORE-001` | Inicialización limpia de centros y carga/purga transaccional de datos de demostración con discriminador `is_demo`. |
| **Persistencia** | `CMP-DATA-ACCESS-001` | `composite` | `SEC-ENC-DATA-ACCESS-001` | Aislamiento lógico multi-inquilino mediante EF Core Global Query Filters y registro inmutable append-only de auditoría. |
| **Presentación** | `CMP-WEB-UI-001` | `function` | `SEC-ENC-AUTH-GATEWAY-001` | Interfaz gráfica web responsiva con selector de centros, semáforos visuales interactivos y soporte de censura para invitados. |

---

## 3. Recursive Decomposition into Sub-Levels
Cada subsistema catalogado dispone de su especificación individual Nivel 2 (`CMP-*.md`) alineada con las plantillas oficiales del marco AI-SDLC.

---

## 4. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición de vista de bloques funcionales Nivel 1 Whitebox arc42 / NAF | CHG-ARCH-001 |
