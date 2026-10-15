---
id: "ACT-THREAT-INSIDER-GUEST-001"
type: "threat-actor"
title: "Colaborador Interno No Confiable o Curioso"
status: "draft"
version: "1.0.0"
threat-capability: "insider-threat"
motivation: "data-theft"
attack-surfaces:
  - "service-margin-api"
  - "audit-trail-endpoints"
  - "export-generation-api"
---

# Threat Actor: Colaborador Interno No Confiable o Curioso (`ACT-THREAT-INSIDER-GUEST-001`)

## 1. Profile and Characteristics
- **Actor Class**: Empleado, recepcionista o esteticista invitado (`ACT-GUEST-001`) con credenciales activas y autorizadas para consultar escandallos y tratamientos, pero sin autorización para ver datos financieros confidenciales.
- **Capability Level**: `insider-threat` / usuario interno con acceso a la interfaz web, herramientas de desarrollador del navegador (DevTools / F12), intercepción de peticiones HTTP locales y extracción de informes.
- **Primary Motivation**: `data-theft` (curiosidad salarial sobre las nóminas de compañeros, descubrimiento de márgenes netos del propietario o coste real de alquiler del local) y `reputational-damage` (filtración de salarios dentro del equipo).

## 2. Attack Vectors and Capabilities
- Inspección de respuestas JSON sin procesar en la pestaña Network del navegador para encontrar campos salariales omitidos solo visualmente en la vista UI (elusión de censura solo por CSS).
- Peticiones directas a endpoints de historial `/api/v1/costs/history` o exportaciones ejecutivas para obtener cifras agregadas y desgloses confidenciales reservados al propietario.
