---
id: "ABUSE-COST-TAMPER-001"
type: "abuse-case"
title: "Manipulación No Autorizada de Factores de Coste de Otro Centro (IDOR/BOLA)"
status: "draft"
version: "1.0.0"
primary-threat-actor: "ACT-THREAT-COMPETITOR-001"
targets-use-case: "UC-COST-CONFIG-001"
stride-category: "tampering"
mitigated-by:
  - "SEC-REQ-AUTHZ-TENANT-FILTER-001"
asvs-controls:
  - "V4.1.2"
  - "V4.2.1"
---

# Abuse Case: Manipulación de Factores de Coste entre Centros (`ABUSE-COST-TAMPER-001`)

## 1. Description and Adversary Objective
El adversario `ACT-THREAT-COMPETITOR-001` altera factores de coste (salarios de empleados, gastos de local, precios de cosméticos) de un centro ajeno manipulando los identificadores de recurso (`costId`, `centerId`) en peticiones `PUT` / `PATCH` / `DELETE` a la API.

## 2. Attack Execution Scenario
1. El usuario atacante accede a la configuración de costes de su propio centro y captura la petición HTTP `PUT /api/v1/costs/workers/{workerId}`.
2. Modifica el `workerId` en la ruta o incluye un `centerId` ajeno en el cuerpo de la petición.
3. Si la capa de datos no aplica un filtro estricto por inquilino y centro activo (`Global Query Filters` de EF Core vinculados al token de sesión), la actualización se ejecuta sobre el registro de la víctima.
4. Los costes del centro competidor se alteran silenciosamente, descalibrando los escandallos y márgenes del negocio afectado.

## 3. Impact Assessment
- **Confidencialidad**: Media. Posible inferencia de IDs existentes.
- **Integridad**: Crítica. Corrupción contable directa de bases de cálculo financiero.
- **Disponibilidad**: Media. Potencial inducción de errores de división por cero si se introducen horas a cero.
