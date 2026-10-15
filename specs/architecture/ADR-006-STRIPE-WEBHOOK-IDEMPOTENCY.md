---
id: ADR-006-STRIPE-WEBHOOK-IDEMPOTENCY
type: architecture-decision-record
title: "Integración de Stripe con Verificación Criptográfica HMAC-SHA256 y Ledger de Idempotencia"
status: accepted
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
decision-date: "2026-10-15"
affects-components:
  - CMP-SUBSCRIPTION-MGR-001
supersedes: null
superseded-by: null
---

# ADR-006: Integración de Stripe con Verificación Criptográfica HMAC-SHA256 y Ledger de Idempotencia

## 1. Context and Problem Statement
El modelo freemium restringe a un máximo de 2 escandallos activos por centro en el plan gratuito (`BR-FREEMIUM-LIMIT-001`), requiriendo suscripción de pago mensual o anual procesada por Stripe para desbloquear escandallos ilimitados (`FR-SUBSCRIPTION-GATE-001`). Los eventos asíncronos emitidos por Stripe hacia la aplicación web (`checkout.session.completed`, `customer.subscription.deleted`, `invoice.payment_succeeded`) están expuestos a suplantación de eventos falsos (`ABUSE-STRIPE-SPOOF-001`) y ataques de repetición (Replay Attacks) si no se implementa una verificación exhaustiva (`SEC-REQ-STRIPE-SIGNATURE-001`).

## 2. Considered Technology Options
1. **Opción A: Validación básica por token secreto en query param o cabecera simple**:
   - *Pros*: Fácil configuración.
   - *Contras*: Extremadamente vulnerable a intercepción, ataques de intermediario y repetición indefinida de eventos antiguos capturados.
2. **Opción B: Polling periódico a la API de Stripe**:
   - *Pros*: Comunicación iniciada desde el servidor.
   - *Contras*: Latencia de sincronización (minutos de espera tras el pago para el usuario); consumo innecesario de cuota API de Stripe y recursos de red.
3. **Opción C: Endpoint Webhook con Verificación Criptográfica HMAC-SHA256, Ventana Temporal de 300 s y Tabla de Deduplicación Idempotente** (Elegida):
   - *Pros*: Verificación estricta mediante el SDK oficial `Stripe.net` (licencia MIT); comprueba la firma matemática con el secreto del webhook (`EndpointSecret`); descarta marcas temporales desfasadas en más de 300 segundos; registra transaccionalmente el identificador único `event.Id` en la tabla `stripe_processed_events` de PostgreSQL; garantiza que los reintentos automáticos de Stripe sean totalmente inocuos.
   - *Contras*: Requiere configurar y rotar adecuadamente el secreto de webhook por entorno.

## 3. Decision Outcome
Se adopta la **Opción C**.
El componente `CMP-SUBSCRIPTION-MGR-001` expone el controlador perimetral en `SEC-ENC-PAYMENT-INTEGRATION-001`. El flujo de procesamiento:
1. Lee el cuerpo binario de la solicitud cruda (*raw request body*) para no alterar el cálculo del digest.
2. Invoca `EventUtility.ConstructEvent(json, stripeSignatureHeader, webhookSecret, tolerance: 300)`.
3. Inicia una transacción en PostgreSQL; si `event.Id` ya existe, confirma con HTTP 200 inmediatamente. Si no existe, inserta el `event.Id` y actualiza atómicamente el estado del centro a `Premium` o `Free`.

## 4. Consequences
- **Positive**: Neutralización total de suplantación de pagos y ataques de repetición; consistencia transaccional absoluta entre el estado de facturación de Stripe y los permisos de la aplicación.
- **Negative / Trade-offs**: La tabla `stripe_processed_events` acumula registros de eventos que deben conservarse para garantizar la idempotencia histórica (volumen insignificante, < 1.000 filas al año por centro).

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Decisión arquitectural aceptada | CHG-ARCH-001 |
