---
id: "ACT-THREAT-COMPETITOR-001"
type: "threat-actor"
title: "Inquilino Competidor Malicioso (Tenant Hopping Attacker)"
status: "draft"
version: "1.0.0"
threat-capability: "advanced-persistent-threat"
motivation: "espionage"
attack-surfaces:
  - "center-selection-api"
  - "cost-configuration-endpoints"
  - "multi-tenant-data-layer"
---

# Threat Actor: Inquilino Competidor Malicioso (`ACT-THREAT-COMPETITOR-001`)

## 1. Profile and Characteristics
- **Actor Class**: Usuario legítimamente autenticado como gestor (`ACT-USER-001`) de un centro o inquilino propio, pero con intenciones hostiles o de espionaje comercial contra otros centros de estética competidores en la misma plataforma multi-inquilino.
- **Capability Level**: `advanced-persistent-threat` / profesional técnico con conocimiento profundo de arquitectura web multi-inquilino y explotación de APIs REST.
- **Primary Motivation**: `espionage` y `data-theft` (extraer fórmulas de escandallo, costes de adquisición de cosméticos, acuerdos de alquiler y nóminas de salones rivales) y `sabotage` (modificar tarifas de la competencia).

## 2. Attack Vectors and Capabilities
- Salto de inquilino (*tenant hopping*) mediante manipulación de parámetros de ruta (`/centers/{centerId}/costs`), inyección de cabeceras (`X-Center-Id`, `X-Tenant-Id`) o manipulación de identificadores (Broken Object Level Authorization - BOLA / IDOR).
- Alteración de factores de coste de centros ajenos para distorsionar su contabilidad o forzarlos a operar con pérdidas.
