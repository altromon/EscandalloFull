---
id: "QR-AVAILABILITY-001"
type: "quality-requirement"
title: "Alta Disponibilidad 24/7 y Tolerancia a Fallos"
status: "draft"
version: "1.0.0"
category: "reliability"
---

# Quality Requirement: Alta Disponibilidad 24/7 y Tolerancia a Fallos (`QR-AVAILABILITY-001`)

## 1. Quality Attribute and Target
- **ISO 25010 Attribute**: Reliability / Availability.
- **Target**: Disponibilidad de servicio objetivo del 99.9% anual (tiempo de inactividad no planificado inferior a 8.7 horas al año).

## 2. Architectural Strategy
- Endpoints de verificación de estado y vitalidad (`/healthz` y `/readyz`) integrados en ASP.NET Core.
- Política de reinicio automático (`restart: unless-stopped`) en contenedor Docker o orquestador.
- Desacoplamiento de llamadas externas síncronas críticas (ej. procesamiento asíncrono o webhook tolerante a reintentos para eventos de Stripe).
