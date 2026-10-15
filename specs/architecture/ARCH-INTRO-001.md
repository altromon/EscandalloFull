---
id: ARCH-INTRO-001
type: architecture-introduction
title: "01. Introduction and System Goals - Escandallo Estética"
status: draft
version: "1.0.0"
schema-version: "1.0"
arc42-section: 1
naf-perspective: "Enterprise & Capability"
cites-product-actors:
  - ACT-USER-001
  - ACT-GUEST-001
  - ACT-MAINTAINER-001
cites-quality-goals:
  - QR-DEPLOY-COST-001
  - QR-AVAILABILITY-001
  - QR-MULTITENANT-001
  - QR-DR-BACKUP-001
cites-use-cases:
  - UC-AUTH-MULTI-001
  - UC-COST-CONFIG-001
  - UC-SERVICE-MARGIN-001
  - UC-HISTORY-RECALC-001
  - UC-SUBSCRIPTION-GATE-001
  - UC-EXPORT-EXEC-001
  - UC-DEMO-MODE-001
supersedes: null
superseded-by: null
---

# 01. Introduction and System Goals (arc42 Sec. 1 / NAF Enterprise)

## 1. System Vision and Executive Summary
Escandallo Estética es una solución web multi-inquilino para la formulación, cálculo de costes (escandallo) y control de rentabilidad de servicios y tratamientos en centros de estética y belleza. Resuelve la falta de precisión financiera y opacidad de costes operativos en salones y clínicas estéticas, calculando con exactitud el coste unitario por sesión a partir de los factores clave: coste horario de mano de obra/trabajadores, coste horario imputable del local/aparatología/suministros, y coste neto de productos cosméticos y consumibles por aplicación.

### 1.1 Mission Statement
> *El sistema **Escandallo Estética** proporciona a los propietarios y gestores de centros de estética una plataforma web ágil y fiable para modelar, auditar y recalcular automáticamente el escandallo y margen de rentabilidad de sus servicios por centro, garantizando aislamiento estricto multi-inquilino, control de visibilidad para colaboradores invitados, alta disponibilidad 24/7 y la mínima huella económica de infraestructura en .NET 10.*

---

## 2. Priority Quality Goals (Canonical ProductShape Citations)

| Priority | Quality Goal | Requirement ID | Architectural Rationale |
| :---: | :--- | :--- | :--- |
| **1** | Mínimo Coste de Despliegue e Infraestructura | `QR-DEPLOY-COST-001` | Arquitectura ligera en .NET 10 (Linux Container) optimizada para despliegue en VPS económico / Container Apps con consumo mínimo de recursos. |
| **2** | Aislamiento y Seguridad Multi-inquilino | `QR-MULTITENANT-001` | Particionamiento lógico estricto por cliente/inquilino y centro de estética; segregación de permisos y ocultación de datos sensibles de costes a invitados. |
| **3** | Alta Disponibilidad y Resiliencia 24/7 | `QR-AVAILABILITY-001` | Servicio web continuo 24/7 con reinicio automático de contenedores, monitoreo de salud (*health checks*) y degradación grácil. |
| **4** | Continuidad de Negocio y Recuperación ante Desastres (DR) | `QR-DR-BACKUP-001` | Persistencia relacional con copias de seguridad automáticas periódicas/WAL y snapshots cifrados replicados en almacenamiento de objetos S3 económico. |

---

## 3. Stakeholder and Primary Actors Matrix

| Role / Stakeholder | Actor ID | Architectural Expectations |
| :--- | :--- | :--- |
| **Usuario Gestor / Propietario** | `ACT-USER-001` | Control CRUD integral de centros, factores de costes (personal, local, consumibles), fijación de PVP, visualización de márgenes y suscripciones. |
| **Colaborador / Invitado** | `ACT-GUEST-001` | Consulta de escandallos y desglose de costes por servicio con censura estricta de salarios de personal y costes fijos de local. |
| **Mantenedor / Operador de Sistemas** | `ACT-MAINTAINER-001` | Extracción y consulta de telemetría, métricas de rendimiento y logs estructurados sin acceso a datos comerciales sensibles. |

---

## 4. Key Strategic Capabilities (MVP Scope)

1. **Gestión Multicentro y Autenticación Multi-inquilino (`UC-AUTH-MULTI-001`)**: Acceso seguro con selector de centros de estética asignados a la cuenta de usuario.
2. **Definición Paramétrica de Factores de Coste (`UC-COST-CONFIG-001`)**: Modelado granular de trabajadores (coste/h), local y suministros (coste/h de apertura), y consumibles cosméticos (coste por dosis/ml/gramo).
3. **Semáforo Visual de Rentabilidad (`UC-SERVICE-MARGIN-001`)**: Inspección ágil de márgenes brutos y netos mediante código de colores configurables (verde, amarillo, rojo).
4. **Histórico de Costes y Recálculo Automático (`UC-HISTORY-RECALC-001`)**: Registro histórico inmutable de variaciones de precios con propagación instantánea a los escandallos dependientes.
5. **Monetización Freemium y Puerta de Pago Stripe (`UC-SUBSCRIPTION-GATE-001`)**: Límite de 2 escandallos por centro en modo gratuito y checkout de suscripción mensual/anual vía Stripe.
6. **Exportación Ejecutiva PDF/XLSX (`UC-EXPORT-EXEC-001`)**: Generación de informes profesionales con diseño estilizado y paleta ejecutiva para comités y dirección.
7. **Aislamiento de Entorno y Modo Demo (`UC-DEMO-MODE-001`)**: Inicialización limpia de centros en producción por defecto, con capacidad opt-in para cargar y purgar datos de prueba realistas sin contaminar la contabilidad real.

---

## 5. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **0.1.0** | 2026-10-15 | `agent-product-analyst` | Borrador inicial de visión de producto y metas NAF v4 / arc42 | CHG-INIT-001 |
