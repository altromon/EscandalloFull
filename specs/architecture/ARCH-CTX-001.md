---
id: ARCH-CTX-001
type: context-and-scope
title: "03. System Context and Scope - Escandallo Estética"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 3
naf-perspective: "Operational Perspective"
operational-exchanges:
  - OIE-WEB-CLIENT-001
  - OIE-STRIPE-WEBHOOK-002
  - OIE-S3-BACKUP-003
context-boundaries:
  - CTX-DMZ-PERIMETER-001
  - CTX-TRUSTED-CORE-002
  - CTX-STORAGE-BACKUP-003
supersedes: null
superseded-by: null
---

# 03. Context and Scope (arc42 Sec. 3 / NAF Operational)

## 1. Business Context
Define las fronteras conceptuales y operativas del sistema **Escandallo Estética** con respecto a sus actores humanos y servicios externos integrados.

### 1.1 Diagrama de Contexto de Negocio
```mermaid
graph LR
    User[ACT-USER-001: Gestor / Propietario] -->|Gestión Costes, Escandallos, Suscripción| System([Escandallo Estética Platform])
    Guest[ACT-GUEST-001: Colaborador / Invitado] -->|Consulta Escandallos con Censura Salarial| System
    Maintainer[ACT-MAINTAINER-001: Operador de Sistemas] -->|Métricas de Salud y Telemetría| System

    System -->|Checkout y Gestión de Suscripciones| StripeAPI[Pasarela Stripe API]
    StripeAPI -->|Notificaciones Webhook checkout.session| System
    System -->|Replicación Cifrada WAL / Snapshots| S3Storage[Almacenamiento S3 / R2]
```

### 1.2 Intercambios de Información Operativa (`OIE-*`)
| Exchange ID | Origen / Destino | Carga Útil / Mensaje | Protocolo / Canal | Formato |
| :--- | :--- | :--- | :--- | :--- |
| `OIE-WEB-CLIENT-001` | Cliente Web ➔ Sistema | Peticiones CRUD costes, escandallos y reportes | HTTPS (TLS 1.3) / REST | JSON (OpenAPI v3.1) |
| `OIE-STRIPE-WEBHOOK-002` | Stripe ➔ Sistema | Eventos de pago (`checkout.session.completed`) | HTTPS / POST | JSON firmado HMAC-SHA256 |
| `OIE-S3-BACKUP-003` | Sistema ➔ S3 Storage | Volcados lógicos de BD y fragmentos WAL | HTTPS / S3 API | Blobs binarios cifrados AES-256 |
| `OIE-DOC-EXPORT-004` | Sistema ➔ Gestor | Informes ejecutivos consolidados de escandallos | HTTPS / GET Stream | Binario (PDF / XLSX) |

---

## 2. Technical Context and Perimeter Infrastructure
Define los límites físicos y lógicos de la infraestructura de red, zonas de seguridad y componentes de enrutamiento.

### 2.1 Diagrama de Contexto Técnico
```mermaid
graph TD
    subgraph PublicInternet["Red Pública (Internet)"]
        Browser[Navegadores de Usuarios ACT-USER / ACT-GUEST]
        StripeCloud[Servidores de Notificación Stripe]
    end

    subgraph EdgePerimeter["Perímetro / DMZ (SEC-ENC-AUTH-GATEWAY-001)"]
        ReverseProxy[Reverse Proxy Nginx / Caddy + TLS 1.3]
        AppGateway[ASP.NET Core Web API Controllers Gateway]
    end

    subgraph PrivateEnclaves["Red Interna Aislada"]
        ServiceCore["Núcleo de Servicios (SEC-ENC-SERVICE-CORE-001)"]
        SandboxExport["Sandbox Documental (SEC-ENC-EXPORT-SANDBOX-001)"]
        PaymentWebhook["Enclave Stripe Webhook (SEC-ENC-PAYMENT-INTEGRATION-001)"]
        Database[(PostgreSQL 16: SEC-ENC-DATA-ACCESS-001)]
    end

    subgraph BackupCloud["Almacenamiento Externo de Respaldo"]
        S3Bucket[(S3 Compatible / Cloudflare R2)]
    end

    Browser -->|Puerto 443 / HTTPS| ReverseProxy
    StripeCloud -->|Webhook POST /api/v1/billing/stripe-webhook| ReverseProxy
    ReverseProxy -->|Proxy Pass HTTP Interno| AppGateway

    AppGateway -->|Contexto Autenticado| ServiceCore
    ServiceCore -->|Generación Segura| SandboxExport
    AppGateway -->|Eventos de Pago| PaymentWebhook
    ServiceCore -->|EF Core Npgsql / Transacciones| Database
    PaymentWebhook -->|Actualización Cuota| Database
    Database -.->|Litestream / WAL Archiving| S3Bucket
```

---

## 3. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición de contexto de negocio y contexto técnico arc42 / NAF | CHG-ARCH-001 |
