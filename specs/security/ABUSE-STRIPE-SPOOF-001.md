---
id: "ABUSE-STRIPE-SPOOF-001"
type: "abuse-case"
title: "Falsificación de Webhooks de Pago Stripe para Activación Fraudulenta de Cuota"
status: "draft"
version: "1.0.0"
primary-threat-actor: "ACT-THREAT-EXTERNAL-001"
targets-use-case: "UC-SUBSCRIPTION-GATE-001"
stride-category: "spoofing"
mitigated-by:
  - "SEC-REQ-STRIPE-SIGNATURE-001"
asvs-controls:
  - "V3.5.1"
  - "V13.4.1"
---

# Abuse Case: Falsificación de Webhooks de Pago Stripe (`ABUSE-STRIPE-SPOOF-001`)

## 1. Description and Adversary Objective
El adversario `ACT-THREAT-EXTERNAL-001` envía peticiones HTTP falsificadas directamente al endpoint de recepción de eventos de Stripe (`POST /api/v1/billing/stripe-webhook`) emulando el evento `checkout.session.completed` o `customer.subscription.created` para activar el plan Premium de un centro sin haber pagado.

## 2. Attack Execution Scenario
1. El atacante analiza el tráfico de red de la aplicación durante un intento de checkout o deduce la estructura de eventos estándar de Stripe.
2. Construye un payload JSON con un evento simulado de Stripe conteniendo `metadata.centerId` y estado `paid`.
3. Envía la petición al endpoint `/api/v1/billing/stripe-webhook` omitiendo la cabecera `Stripe-Signature` o enviando un valor arbitrario.
4. Si el servidor no valida la firma HMAC-SHA256 con el webhook secret configurado, procesa el evento como legítimo.
5. El sistema eleva el plan del centro a Premium, desbloqueando el límite freemium de 2 escandallos sin abonar tarifa.

## 3. Impact Assessment
- **Confidencialidad**: Baja.
- **Integridad**: Alta. Subversión del modelo de monetización y evasión de pago comercial.
- **Disponibilidad**: Baja.
