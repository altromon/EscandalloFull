---
id: "ABUSE-FREEMIUM-BYPASS-001"
type: "abuse-case"
title: "Evasión del Límite de Cuota Freemium Mediante Carrera Concurrente (TOCTOU)"
status: "draft"
version: "1.0.0"
primary-threat-actor: "ACT-THREAT-RESOURCE-ABUSER-001"
targets-use-case: "UC-SUBSCRIPTION-GATE-001"
stride-category: "elevation-of-privilege"
mitigated-by:
  - "SEC-REQ-QUOTA-BACKEND-ENFORCE-001"
asvs-controls:
  - "V4.1.3"
  - "V13.3.1"
---

# Abuse Case: Evasión del Límite Freemium por Condición de Carrera (`ABUSE-FREEMIUM-BYPASS-001`)

## 1. Description and Adversary Objective
El adversario `ACT-THREAT-RESOURCE-ABUSER-001` busca crear más de 2 escandallos en un centro con plan Free (`BR-FREEMIUM-LIMIT-001`) eludiendo la pasarela de pago Stripe (`UC-SUBSCRIPTION-GATE-001`) mediante ataques de condición de carrera Time-of-Check to Time-of-Use (TOCTOU) o elusión de validaciones exclusivas en frontend.

## 2. Attack Execution Scenario
1. El atacante tiene un centro en plan gratuito con 1 escandallo activo (límite máximo permitido: 2).
2. Prepara 20 peticiones concurrentes simultáneas `POST /api/v1/centers/{centerId}/services` para crear nuevos tratamientos.
3. Envía todas las peticiones con sincronización milimétrica (TCP keep-alive / HTTP/2 multiplexing).
4. Si el backend realiza la comprobación de cuota en memoria de forma no transaccional (ej: `var count = await db.Services.CountAsync(); if (count >= 2) throw ...; await db.Services.AddAsync(...)`), todas las peticiones leen `count = 1` simultáneamente antes de que cualquiera inserte su fila.
5. Las 20 peticiones se insertan con éxito en la base de datos, resultando en un centro Free con 21 escandallos activos sin abonar cuota Premium.

## 3. Impact Assessment
- **Confidencialidad**: Nula.
- **Integridad**: Alta. Violación de las políticas de suscripción y monetización comercial.
- **Elevación de Privilegios**: Crítica. Acceso ilegítimo permanente a funciones Premium.
