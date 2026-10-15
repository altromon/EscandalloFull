---
id: ARCH-RISK-001
type: risks-and-technical-debt
title: "11. Architecture Risks and Technical Debt - Escandallo Estética"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 11
naf-perspective: "Risk & Technical Debt"
risks:
  - RSK-BURST-RECALC-001
  - RSK-DATA-LEAK-GUEST-002
  - RSK-STRIPE-REPLAY-003
  - RSK-FORMULA-INJECT-004
supersedes: null
superseded-by: null
---

# 11. Risks and Technical Debt (arc42 Sec. 11 / NAF Risk & Debt)

## 1. Matriz de Riesgos Arquitecturales (`RSK-*`)
Identificación, evaluación de impacto y estrategias preventivas de mitigación:

| Identificador | Descripción del Riesgo | Probabilidad | Impacto | Estrategia de Mitigación |
| :--- | :--- | :---: | :---: | :--- |
| `RSK-BURST-RECALC-001` | Ráfagas de recálculos concurrentes que saturen la CPU del VPS o excedan el techo de 150 MB RAM | Media | Alto | Canal en memoria acotado (`System.Threading.Channels`), *debouncing* de 500 ms y limitación de tasa a máx 10 mutaciones/min por usuario (`SEC-REQ-RATE-RECALC-THROTTLE-001`). |
| `RSK-DATA-LEAK-GUEST-002` | Divulgación accidental de salarios de empleados y costes fijos de local a colaboradores invitados | Media | Crítico | Proyección y censura forzosa en origen (backend) mediante DTOs específicos (`GuestEscandalloDto`); prohibición de delegar el filtrado a la interfaz de usuario (`SEC-REQ-DATA-CENSOR-GUEST-001`). |
| `RSK-STRIPE-REPLAY-003` | Inyección de webhooks fraudulentos o repetición de eventos de pago para evadir cuota freemium | Baja | Crítico | Validación estricta de firma HMAC-SHA256 con ventana máxima de 300 segundos y deduplicación atómica en tabla `stripe_processed_events` (`SEC-REQ-STRIPE-SIGNATURE-001`). |
| `RSK-FORMULA-INJECT-004` | Inyección de código malicioso o ejecución remota al abrir archivos Excel generados en dirección | Media | Alto | Sanitización preventiva sistemática anteponiendo comilla simple (`'`) en toda celda que inicie por `=`, `+`, `-`, `@`, `\t`, `\r` (`SEC-REQ-EXPORT-SANITIZATION-001`). |

---

## 2. Registro de Deuda Técnica y Compromisos Asumidos (Technical Debt Log)
Decisiones conscientes tomadas para optimizar el coste de infraestructura y acelerar el MVP:

| Elemento de Deuda | Componente Afectado | Justificación del Compromiso | Plan de Evolución / Pago de Deuda |
| :--- | :--- | :--- | :--- |
| Canal de eventos en memoria local en lugar de broker distribuido | `CMP-COST-ENGINE-001` | Permite operar en un único VPS con consumo inferior a 150 MB de RAM y coste $< 15$ €/mes (`QR-DEPLOY-COST-001`). | Desacoplado tras la interfaz `IEventChannel`. Si se escala horizontalmente a múltiples servidores, se sustituirá por Redis Streams o RabbitMQ sin alterar el dominio. |
| Despliegue en contenedor único Docker Compose en VPS | `ARCH-DEP-001` | Mínima complejidad operativa y máxima rentabilidad económica para los primeros cientos de centros de estética. | Migración planificada hacia clúster Kubernetes o Azure Container Apps con base de datos gestionada cuando el volumen supere los 1.000 salones activos. |

---

## 3. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición de riesgos técnicos y registro de compromisos arc42 / NAF | CHG-ARCH-001 |
