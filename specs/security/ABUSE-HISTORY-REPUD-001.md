---
id: "ABUSE-HISTORY-REPUD-001"
type: "abuse-case"
title: "Repudio y Manipulación Oculta del Historial de Precios y Costes"
status: "draft"
version: "1.0.0"
primary-threat-actor: "ACT-THREAT-INSIDER-GUEST-001"
targets-use-case: "UC-HISTORY-RECALC-001"
stride-category: "repudiation"
mitigated-by:
  - "SEC-REQ-AUDIT-IMMUTABLE-001"
asvs-controls:
  - "V10.1.1"
  - "V10.3.1"
---

# Abuse Case: Repudio y Manipulación del Historial de Costes (`ABUSE-HISTORY-REPUD-001`)

## 1. Description and Adversary Objective
Un usuario interno deshonesto o administrador (`ACT-USER-001` o `ACT-THREAT-INSIDER-GUEST-001`) altera fraudulentamente el coste de adquisición de cosméticos o la retribución de trabajadores en el pasado, alterando márgenes para encubrir desvíos contables, y posteriormente niega la autoría del cambio debido a la falta de trazabilidad forense no repudiable.

## 2. Attack Execution Scenario
1. El usuario modifica el precio de compra de un principio activo costoso de 80 € a 20 € para fingir una rentabilidad ficticia ante sus socios.
2. El sistema actualiza el registro en la tabla de productos, pero si el registro histórico no captura la identidad criptográfica del actor, la dirección IP y el valor anterior de forma inmutable (append-only), el autor afirma que fue un "error del sistema" o un cambio no autorizado de otro usuario.
3. Si los registros históricos admiten operaciones `UPDATE` o `DELETE`, el atacante borra la traza del cambio después de la auditoría.

## 3. Impact Assessment
- **Confidencialidad**: Baja.
- **Integridad**: Alta. Destrucción de la fidelidad contable y pérdida de veracidad histórica.
- **No Repudio**: Crítica. Imposibilidad legal o técnica de determinar responsabilidades económicas.
