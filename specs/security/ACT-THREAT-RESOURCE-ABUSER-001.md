---
id: "ACT-THREAT-RESOURCE-ABUSER-001"
type: "threat-actor"
title: "Abusador de Cuota y Recursos Computacionales"
status: "draft"
version: "1.0.0"
threat-capability: "automated-botnet"
motivation: "sabotage"
attack-surfaces:
  - "cascade-recalculation-engine"
  - "export-document-generator"
  - "subscription-quota-service"
  - "demo-mode-lifecycle-service"
---

# Threat Actor: Abusador de Cuota y Recursos Computacionales (`ACT-THREAT-RESOURCE-ABUSER-001`)

## 1. Profile and Characteristics
- **Actor Class**: Usuario freemium o atacante automatizado que busca evadir los límites comerciales o colapsar la infraestructura económica del servicio (.NET 10 en VPS económico).
- **Capability Level**: `automated-botnet` / scripts concurrentes multitarea, ataques de colisión y saturación asíncrona.
- **Primary Motivation**: `sabotage` (denegación de servicio a otros usuarios del VPS) y `financial-gain` (explotar indefinidamente el plan Free creando decenas de escandallos concurrentes sin pagar suscripción a Stripe).

## 2. Attack Vectors and Capabilities
- Ejecución de ráfagas concurrentes de peticiones `POST /services` en el milisegundo exacto para provocar condiciones de carrera (Race Condition / TOCTOU) y crear más de 2 escandallos antes de que el contador se actualice.
- Disparo simultáneo de recálculos masivos en cascada y peticiones concurrentes de exportación PDF de alta resolución para agotar la memoria RAM (<150 MB límite) y colapsar el contenedor Docker.
- Llamadas continuas a alternancia de Modo Demo para generar fragmentación en la base de datos relacional.
