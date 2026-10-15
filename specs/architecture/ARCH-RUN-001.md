---
id: ARCH-RUN-001
type: runtime-view
title: "06. Runtime View and Dynamic Behavior - Escandallo Estética"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 6
naf-perspective: "Behaviour & Sequences"
sequences:
  - SEQ-NOMINAL-001
  - SEQ-RECALC-002
  - SEQ-GUEST-CENSOR-003
  - SEQ-STRIPE-WEBHOOK-004
  - SEQ-EXPORT-SANDBOX-005
  - SEQ-MOCK-SUBSCRIPTION-006
flows:
  - FLW-LIFECYCLE-COSTS-001
satisfies-requirements:
  - FR-AUTH-MULTI-001
  - FR-COST-CONFIG-001
  - FR-SERVICE-MARGIN-001
  - FR-HISTORY-RECALC-001
  - FR-SUBSCRIPTION-GATE-001
  - FR-EXPORT-EXEC-001
  - SEC-REQ-AUTH-MEMBER-001
  - SEC-REQ-DATA-CENSOR-GUEST-001
  - SEC-REQ-RATE-RECALC-THROTTLE-001
  - SEC-REQ-STRIPE-SIGNATURE-001
  - SEC-REQ-EXPORT-SANITIZATION-001
supersedes: null
superseded-by: null
---

# 06. Runtime View (arc42 Sec. 6 / NAF Behaviour)

## 1. Escenario Nominal: Consulta y Cálculo de Escandallos (`SEQ-NOMINAL-001`)
Orquestación estándar de una petición de consulta de escandallos por un usuario gestor (`ACT-USER-001`):

```mermaid
sequenceDiagram
    autonumber
    actor User as Gestor (ACT-USER-001)
    participant UI as Web SPA (CMP-WEB-UI-001)
    participant Gateway as Auth Gateway (CMP-AUTH-GATEWAY-001)
    participant Core as Margin Engine (CMP-MARGIN-CALCULATOR-001)
    participant DB as Persistence (CMP-DATA-ACCESS-001)

    User->>UI: Navega a matriz de servicios del centro activo
    UI->>Gateway: GET /api/v1/escandallos (Bearer JWT, X-Center-Id)
    Gateway->>Gateway: Valida firma JWT y pertenencia de CenterId a TenantId
    Gateway->>Core: Invocación con TenantContext(tenant_id, center_id, role)
    Core->>DB: Consulta servicios con EF Core Global Query Filters
    DB-->>Core: Lista de tratamientos y factores de coste asociados
    Core->>Core: Evalúa costes, IVA y margen neto; asigna semáforo verde/amarillo/rojo
    Core-->>Gateway: CompleteEscandalloDto[] (sin censura para Gestor)
    Gateway-->>UI: 200 OK con payload JSON completo
    UI-->>User: Renderiza tabla con semáforos de rentabilidad
```

---

## 2. Escenario Asíncrono: Variación de Precio y Recálculo en Cascada (`SEQ-RECALC-002`)
Modificación de un precio de compra cosmético con auditoría criptográfica y propagación desacoplada con debouncing (`FR-HISTORY-RECALC-001`, `SEC-REQ-RATE-RECALC-THROTTLE-001`):

```mermaid
sequenceDiagram
    autonumber
    actor User as Gestor (ACT-USER-001)
    participant CostEngine as Cost Engine (CMP-COST-ENGINE-001)
    participant DB as Persistence (CMP-DATA-ACCESS-001)
    participant Channel as In-Process Channel (System.Threading.Channels)
    participant Worker as BackgroundRecalcWorker
    participant MarginEngine as Margin Engine (CMP-MARGIN-CALCULATOR-001)

    User->>CostEngine: PUT /api/v1/costs/products/cos-123 (Nuevo precio: 45.00 EUR)
    CostEngine->>CostEngine: Valida rate limit (< 10 req/min por usuario)
    CostEngine->>DB: Transacción: Actualiza precio producto + INSERT cost_audit_log (hash SHA-256)
    DB-->>CostEngine: Transacción confirmada (ACK)
    CostEngine->>Channel: Publica CostVariationEvent(cos-123, center_id)
    CostEngine-->>User: 202 Accepted (Respuesta inmediata < 30ms)

    Note over Channel,Worker: Debouncing window de 500ms para agrupar ráfagas
    Channel->>Worker: Consume evento(s) acumulados
    Worker->>DB: Consulta escandallos dependientes de cos-123
    DB-->>Worker: 3 servicios identificados
    loop Para cada servicio dependiente
        Worker->>MarginEngine: Recalcula coste total y margen unitario
        MarginEngine->>DB: Actualiza escandallo_cache con nuevos valores
    end
    Worker->>Worker: Finaliza lote sin saturar CPU ni exceder 150MB RAM
```

---

## 3. Escenario de Seguridad: Censura en Origen para Invitados (`SEQ-GUEST-CENSOR-003`)
Mitigación de fuga de salarios y alquileres frente a colaboradores invitados (`ACT-GUEST-001`, `BR-GUEST-CENSOR-001`, `SEC-REQ-DATA-CENSOR-GUEST-001`):

```mermaid
sequenceDiagram
    autonumber
    actor Guest as Colaborador Invitado (ACT-GUEST-001)
    participant Gateway as Auth Gateway (CMP-AUTH-GATEWAY-001)
    participant Core as Margin Engine (CMP-MARGIN-CALCULATOR-001)
    participant DB as Persistence (CMP-DATA-ACCESS-001)

    Guest->>Gateway: GET /api/v1/escandallos/srv-456 (Bearer Token Rol: ACT-GUEST-001)
    Gateway->>Core: Delegación con Rol = Guest
    Core->>DB: Obtiene datos brutos del escandallo
    DB-->>Core: Entidad completa (incluye salarios y alquileres)
    Core->>Core: Detecta Rol Invitado: Proyecta a GuestEscandalloDto
    Note over Core: Campos worker_hourly_wage y facility_rent omitidos del DTO
    Core-->>Gateway: GuestEscandalloDto (Sólo nombres y consumos de cosméticos)
    Gateway-->>Guest: 200 OK con payload sanitizado en origen
```

---

## 4. Escenario de Integración: Webhook de Stripe e Idempotencia (`SEQ-STRIPE-WEBHOOK-004`)
Procesamiento seguro de eventos de suscripción procedentes de Stripe (`FR-SUBSCRIPTION-GATE-001`, `SEC-REQ-STRIPE-SIGNATURE-001`):

```mermaid
sequenceDiagram
    autonumber
    actor Stripe as Servidores Stripe
    participant WebhookEndpoint as Stripe Webhook (CMP-SUBSCRIPTION-MGR-001)
    participant DB as Persistence (CMP-DATA-ACCESS-001)

    Stripe->>WebhookEndpoint: POST /api/v1/billing/stripe-webhook (Cabecera Stripe-Signature)
    WebhookEndpoint->>WebhookEndpoint: Valida firma HMAC-SHA256 con Secreto de Webhook
    alt Firma Inválida o Marca Temporal > 300s
        WebhookEndpoint-->>Stripe: 400 Bad Request / Alerta de Seguridad
    else Firma Válida
        WebhookEndpoint->>DB: Consulta si event_id existe en stripe_processed_events
        alt Evento ya procesado previamente
            DB-->>WebhookEndpoint: Evento duplicado
            WebhookEndpoint-->>Stripe: 200 OK (Descartado por idempotencia)
        else Evento nuevo
            WebhookEndpoint->>DB: Transacción: INSERT event_id + UPDATE center plan='Premium'
            DB-->>WebhookEndpoint: Confirmación transaccional
            WebhookEndpoint-->>Stripe: 200 OK
        end
    end
```

---

## 5. Escenario de Aislamiento Documental: Sandbox PDF y XLSX (`SEQ-EXPORT-SANDBOX-005`)
Prevención de Formula Injection en hojas de cálculo y SSRF en generación de PDFs (`FR-EXPORT-EXEC-001`, `SEC-REQ-EXPORT-SANITIZATION-001`):

```mermaid
sequenceDiagram
    autonumber
    actor User as Gestor (ACT-USER-001)
    participant ExportService as Document Engine (CMP-EXPORT-DOC-001)
    participant Sandbox as Export Sandbox (SEC-ENC-EXPORT-SANDBOX-001)

    User->>ExportService: GET /api/v1/export/xlsx
    ExportService->>Sandbox: Envía datos crudos del centro
    Note over Sandbox: Prefijado de comilla simple (') en celdas con =, +, -, @
    Sandbox->>Sandbox: Compila binario XLSX sanitizado
    Sandbox-->>ExportService: Stream binario XLSX
    ExportService-->>User: 200 OK (application/vnd.openxmlformats...)

    User->>ExportService: GET /api/v1/export/pdf
    ExportService->>Sandbox: Renderiza informe ejecutivo PDF
    Note over Sandbox: Sin acceso a sockets de red ni resolución DNS externa
    Sandbox->>Sandbox: Compila PDF en memoria con límite duro de 50MB RAM
    Sandbox-->>ExportService: Stream binario PDF
    ExportService-->>User: 200 OK (application/pdf)
```

---

## 6. Escenario de Pruebas: Ciclo de Vida de Suscripción con Proveedor Fake/Mock (`SEQ-MOCK-SUBSCRIPTION-006`)
Flujo hermético para desarrolladores, evaluadores y tests automatizados (CI) para validar el desbloqueo de la cuota freemium sin depender de cuentas ni servidores de Stripe (`ADR-007`):

```mermaid
sequenceDiagram
    autonumber
    actor Tester as Evaluador / Test Automatizado
    participant UI as Web SPA (CMP-WEB-UI-001)
    participant BillingController as BillingController (CMP-SUBSCRIPTION-MGR-001)
    participant FakeGateway as FakeSubscriptionGateway
    participant DB as Persistence (CMP-DATA-ACCESS-001)
    participant CostEngine as Cost Engine (CMP-COST-ENGINE-001)

    Note over Tester,FakeGateway: Entorno con Billing:Provider = "Fake"
    Tester->>BillingController: GET /api/v1/billing/status
    BillingController->>DB: Consulta plan y escandallos activos
    DB-->>BillingController: Plan="Free", Escandallos=2, canCreateMore=false, isMockProvider=true
    BillingController-->>Tester: 200 OK con SubscriptionStatusDto

    Tester->>CostEngine: POST /api/v1/escandallos (Intento 3er escandallo)
    CostEngine->>BillingController: Verifica cuota
    BillingController-->>CostEngine: Cuota excedida (2/2)
    CostEngine-->>Tester: 403 Forbidden (QUOTA_EXCEEDED)

    Note over Tester,BillingController: Activación de prueba mediante mock
    Tester->>BillingController: POST /api/v1/billing/mock-complete-checkout
    BillingController->>FakeGateway: Simula webhook checkout.session.completed
    FakeGateway->>DB: Transacción: UPDATE centers SET plan='Premium' WHERE id=ActiveCenterId
    DB-->>FakeGateway: Confirmación (ACK)
    FakeGateway-->>BillingController: Estado Premium actualizado
    BillingController-->>Tester: 200 OK (Plan="Premium", canCreateMore=true)

    Tester->>CostEngine: POST /api/v1/escandallos (Creación 3er escandallo)
    CostEngine->>BillingController: Verifica cuota
    BillingController-->>CostEngine: Autorizado (Plan Premium ilimitado)
    CostEngine->>DB: INSERT nuevo escandallo
    CostEngine-->>Tester: 201 Created (Escandallo creado exitosamente)

    Note over Tester,BillingController: Simulación de cancelación de suscripción
    Tester->>BillingController: POST /api/v1/billing/mock-cancel-subscription
    BillingController->>FakeGateway: Simula customer.subscription.deleted
    FakeGateway->>DB: UPDATE centers SET plan='Free'
    BillingController-->>Tester: 200 OK (Plan="Free", canCreateMore=false)
```

---

## 7. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición de vista dinámica y escenarios nominales y de seguridad arc42 | CHG-ARCH-001 |
| **1.1.0** | 2026-10-15 | `agent-system-architect` | Adición de escenario SEQ-MOCK-SUBSCRIPTION-006 con proveedor Fake/Mock | CHG-ARCH-001 |
