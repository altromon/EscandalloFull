---
id: "QR-DEPLOY-COST-001"
type: "quality-requirement"
title: "Mínimo Coste de Despliegue e Infraestructura"
status: "draft"
version: "1.0.0"
category: "efficiency"
---

# Quality Requirement: Mínimo Coste de Despliegue e Infraestructura (`QR-DEPLOY-COST-001`)

## 1. Quality Attribute and Target
- **ISO 25010 Attribute**: Resource Utilization / Cost Efficiency.
- **Target**: La aplicación completa (.NET 10 Web API + Frontend Blazor/SPA + Base de Datos) debe poder ejecutarse en producción con un coste de infraestructura inferior a 10-15 €/mes para soportar los primeros cientos de centros de estética.

## 2. Architectural Strategy
- Empaquetado en contenedor Docker Linux minimalista (.NET 10 AOT o base Chiseled Ubuntu/Alpine).
- Consumo de memoria RAM en reposo del backend inferior a 150 MB.
- Despliegue recomendado: Servidor VPS económico (ej. Hetzner Cloud CX22 ~4 €/mes o Fly.io / Container Apps con escalado a cero) orquestado con Docker Compose o Dokku/Coolify.
