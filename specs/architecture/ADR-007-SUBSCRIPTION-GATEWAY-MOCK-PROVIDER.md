---
id: ADR-007-SUBSCRIPTION-GATEWAY-MOCK-PROVIDER
type: architecture-decision-record
title: "Abstracción de Pasarela de Suscripciones y Proveedor Fake/Mock para Pruebas Locales"
status: accepted
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
decision-date: "2026-10-15"
affects-components:
  - CMP-SUBSCRIPTION-MGR-001
  - CMP-WEB-UI-001
supersedes: null
superseded-by: null
---

# ADR-007: Abstracción de Pasarela de Suscripciones y Proveedor Fake/Mock para Pruebas Locales

## 1. Context and Problem Statement
El requisito `FR-SUBSCRIPTION-GATE-001` impone el control de cuota freemium (máximo 2 escandallos activos por centro en plan `Free` y desbloqueo ilimitado en plan `Premium`) mediante suscripciones procesadas por Stripe.
Durante el desarrollo local, pruebas automatizadas en CI, pruebas BDD por parte de `agent-qa-engineer` y validaciones manuales de usuario, requerir credenciales reales de Stripe, túneles de red (ngrok) o conexión a Internet dificulta las pruebas de ciclo de vida de suscripción y el desbloqueo de cuota. Se necesita una solución desacoplada que permita probar de forma determinista y configurable la suscripción completa mediante un mock/fake sin comprometer la seguridad perimetral en producción.

## 2. Considered Technology Options
1. **Opción A: Acoplamiento directo exclusivo con Stripe SDK (`StripeClient`) en toda la aplicación**:
   - *Pros*: Sin capas intermedias.
   - *Contras*: Imposible probar la creación y cancelación de suscripciones sin conexión a Stripe y claves API; tests frágiles en CI dependientes de red externa; alta fricción para desarrolladores y evaluadores locales.
2. **Opción B: Servidor contenedor mock de Stripe (Stripe-mock)**:
   - *Pros*: Emula endpoints oficiales de Stripe vía HTTP.
   - *Contras*: Consume memoria RAM adicional en el host; no soporta el ciclo de vida completo de webhooks de checkout ni la simulación interactiva desde la interfaz web sin configurar proxies locales.
3. **Opción C: Patrón Adapter con interfaz `ISubscriptionGateway` y proveedor dual (`Stripe` / `Fake`)** (Elegida):
   - *Pros*:
     - Principio de Inversión de Dependencias (DIP): la lógica de negocio y el control de cuotas freemium interactúan únicamente con `ISubscriptionGateway`.
     - Implementación `StripeSubscriptionGateway`: invoca la API real y valida firmas HMAC-SHA256 en producción.
     - Implementación `FakeSubscriptionGateway`: simula sesiones de checkout instantáneas, emite eventos simulados y expone endpoints de prueba para activar o revocar suscripciones Premium con un solo clic o llamada API.
     - Configuración declarativa en `appsettings.json` (`Billing:Provider = "Fake" | "Stripe"`).
     - En producción, los endpoints de mock se deshabilitan estrictamente a nivel de pipeline de middleware (`404 Not Found`).
   - *Contras*: Requiere mantener el adaptador fake alineado con el contrato de la interfaz (mantenimiento mínimo).

## 3. Decision Outcome
Se adopta la **Opción C**.
Se define el puerto `ISubscriptionGateway` con dos adaptadores:
1. `StripeSubscriptionGateway`: Para despliegues en producción y staging con credenciales oficiales de Stripe.
2. `FakeSubscriptionGateway`: Para entornos de desarrollo local, pipelines de CI y pruebas funcionales de usuario.

### 3.1 Endpoints de Prueba del Proveedor Fake
Cuando `Billing:Provider == "Fake"`:
- `POST /api/v1/billing/checkout`: Devuelve una URL de checkout mock interna (`/mock-checkout?center_id=...&cycle=monthly`).
- `POST /api/v1/billing/mock-complete-checkout`: Simula la finalización exitosa del pago y emite internamente el evento `checkout.session.completed`, actualizando el centro a `Premium` e incrementando la cuota a ilimitada.
- `POST /api/v1/billing/mock-cancel-subscription`: Simula la cancelación de la suscripción (`customer.subscription.deleted`), degradando el centro a `Free` y reestableciendo el límite de 2 escandallos.

## 4. Consequences
- **Positive**: Capacidad inmediata de probar el flujo completo de monetización y límites freemium sin depender de Stripe; tests de integración 100% herméticos y rápidos en CI; conmutación sencilla mediante configuración.
- **Negative / Trade-offs**: Debe verificarse en el arranque de la aplicación que si el entorno es `Production`, el proveedor configurado sea obligatoriamente `Stripe` para evitar el uso accidental del proveedor fake.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición de abstracción de suscripciones y proveedor Fake/Mock | CHG-ARCH-001 |
