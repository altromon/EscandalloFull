---
id: ADR-004-CRYPTO-AUDIT-IMMUTABILITY
type: architecture-decision-record
title: "Registro de Auditoría Inmutable Criptográficamente Sellado con Cadena SHA-256"
status: accepted
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
decision-date: "2026-10-15"
affects-components:
  - CMP-DATA-ACCESS-001
  - CMP-COST-ENGINE-001
supersedes: null
superseded-by: null
---

# ADR-004: Registro de Auditoría Inmutable Criptográficamente Sellado con Cadena SHA-256

## 1. Context and Problem Statement
Para garantizar el no repudio, la trazabilidad financiera y neutralizar la alteración maliciosa o fraudulenta de costes históricos (`SEC-REQ-AUDIT-IMMUTABLE-001`, `ABUSE-HISTORY-REPUD-001`), el sistema debe almacenar un registro fidedigno de cada cambio de precio (salarios, gastos generales, consumibles cosméticos) que demuestre que no ha sido adulterado retroactivamente.

## 2. Considered Technology Options
1. **Opción A: Log de aplicación plano en archivos de texto**:
   - *Pros*: Fácil de implementar.
   - *Contras*: Difícil de consultar relacionalmente; vulnerable a manipulación directa en el sistema de archivos del servidor.
2. **Opción B: Base de datos Ledger especializada (ej. Amazon QLDB o Immudb)**:
   - *Pros*: Inmutabilidad criptográfica nativa garantizada.
   - *Contras*: Servicio adicional externo o SaaS propietario con coste añadido; rompe la autonomía de despliegue en un único host PostgreSQL modesto.
3. **Opción C: Tabla relacional Append-Only en PostgreSQL con Encadenamiento Criptográfico SHA-256** (Elegida):
   - *Pros*: Sin dependencias adicionales; cada registro calcula un hash SHA-256 que incorpora el hash del registro anterior del centro (`prev_hash`), formando una cadena criptográfica verificable; se revoca el permiso de `UPDATE` y `DELETE` para el rol de usuario de la aplicación en PostgreSQL mediante sentencias `REVOKE`.
   - *Contras*: La verificación de integridad completa requiere recorrer secuencialmente la cadena del centro (computacionalmente ligera para auditorías periódicas).

## 3. Decision Outcome
Se adopta la **Opción C**.
Se define la tabla `cost_audit_log` con las columnas obligatorias: `id`, `tenant_id`, `center_id`, `actor_id`, `timestamp_utc`, `entity_type`, `entity_id`, `field_name`, `old_value`, `new_value`, `prev_hash` y `entry_hash`. Un interceptor de EF Core calcula el hash SHA-256 en memoria antes de persistir y deniega cualquier intento de modificación o borrado a nivel de ORM y permisos de base de datos.

## 4. Consequences
- **Positive**: No repudio demostrable criptográficamente; trazabilidad histórica transparente para los gestores; cumplimiento estricto de `SEC-REQ-AUDIT-IMMUTABLE-001` sin sobrecoste de infraestructura.
- **Negative / Trade-offs**: Los registros históricos no se pueden compactar ni purgar manualmente; el crecimiento de almacenamiento es lineal pero muy moderado (unos pocos kilobytes por salón al año).

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Decisión arquitectural aceptada | CHG-ARCH-001 |
