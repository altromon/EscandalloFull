---
id: ADR-003-CASCADE-RECALCULATION-INPROCESS
type: architecture-decision-record
title: "Motor Asíncrono de Recálculo en Cascada In-Process con System.Threading.Channels y Debouncing"
status: accepted
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
decision-date: "2026-10-15"
affects-components:
  - CMP-COST-ENGINE-001
  - CMP-MARGIN-CALCULATOR-001
supersedes: null
superseded-by: null
---

# ADR-003: Motor Asíncrono de Recálculo en Cascada In-Process con System.Threading.Channels y Debouncing

## 1. Context and Problem Statement
La modificación de un precio de compra de un cosmético base o el cambio en las tarifas salariales horarias obliga a recalcular automáticamente todos los escandallos y márgenes asociados (`FR-HISTORY-RECALC-001`). Si un producto se utiliza en decenas de servicios o si un usuario emite una ráfaga rápida de modificaciones, el cálculo síncrono bloquearía el hilo de la petición HTTP, dispararía el uso de CPU y amenazaría con desbordar la memoria RAM disponible (< 150 MB, `QR-DEPLOY-COST-001`, `SEC-REQ-RATE-RECALC-THROTTLE-001`).

## 2. Considered Technology Options
1. **Opción A: Recálculo síncrono en el mismo hilo HTTP**:
   - *Pros*: Máxima simplicidad inicial.
   - *Contras*: Tiempos de respuesta lentos (p95 > 2 s ante muchos servicios dependientes); vulnerabilidad directa a ataques DoS de agotamiento de hilos.
2. **Opción B: Broker de mensajería externo (RabbitMQ / Redis / Kafka)**:
   - *Pros*: Escalabilidad horizontal entre múltiples servidores de trabajadores.
   - *Contras*: Introduce servicios adicionales en la infraestructura que consumen entre 200 MB y 1 GB de RAM, violando el presupuesto de costes de `QR-DEPLOY-COST-001` y complicando el despliegue en un único VPS.
3. **Opción C: Canales nativos en memoria `System.Threading.Channels` con Debouncing de 500 ms** (Elegida):
   - *Pros*: Desacoplamiento asíncrono con cero dependencias externas; consumo de memoria despreciable (< 1 MB); canal acotado (BoundedChannel de 1.000 slots) con control de contrapresión; permite agrupar ráfagas de cambios en una ventana de 500 ms de *debouncing*.
   - *Contras*: Si el proceso del contenedor se reinicia abruptamente mientras hay un evento encolado, se requiere reconciliación al inicio (cubierta por una verificación ligera al arrancar).

## 3. Decision Outcome
Se adopta la **Opción C: `System.Threading.Channels` in-process**.
La API HTTP confirma inmediatamente la recepción del cambio con `202 Accepted` y encola el identificador del producto modificado. Un servicio en segundo plano (`IHostedService`) consume los eventos, aplica *debouncing* de 500 ms para acumular cambios secuenciales sobre el mismo cosmético, y ejecuta el recálculo en bloque de los servicios dependientes de manera secuencial y no invasiva.

## 4. Consequences
- **Positive**: Respuesta instantánea al usuario (< 30 ms); aislamiento de carga de CPU; cumplimiento estricto del techo de memoria de 150 MB; mitigación de ataques DoS de recálculo.
- **Negative / Trade-offs**: La cola en memoria es local al proceso; si en el futuro se requiriese escalar horizontalmente a múltiples instancias, la interfaz `IEventChannel` permite sustituir la implementación por un broker externo sin modificar la lógica de dominio.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Decisión arquitectural aceptada | CHG-ARCH-001 |
