---
id: "ABUSE-DEMO-POLLUTION-001"
type: "abuse-case"
title: "Contaminación de Datos Demo en Contabilidad Real y Sobrescritura de Escandallos"
status: "draft"
version: "1.0.0"
primary-threat-actor: "ACT-THREAT-RESOURCE-ABUSER-001"
targets-use-case: "UC-DEMO-MODE-001"
stride-category: "tampering"
mitigated-by:
  - "SEC-REQ-DEMO-ISOLATION-001"
asvs-controls:
  - "V8.1.1"
  - "V8.2.1"
---

# Abuse Case: Contaminación de Datos Demo en Contabilidad Real (`ABUSE-DEMO-POLLUTION-001`)

## 1. Description and Adversary Objective
El adversario o un usuario descuidado desencadena la carga o purga de datos de ejemplo (`Cleasing Milk`, `Mesodermolifting`) en un centro de producción activo con datos contables reales, o aprovecha un fallo de aislamiento para filtrar datos de demostración o sobrescribir factores de coste reales.

## 2. Attack Execution Scenario
1. Un centro con 50 escandallos reales configurados invoca el endpoint `POST /api/v1/demo/load`.
2. Si el sistema no comprueba el estado de producción ni aisla los registros con flags inmutables de entorno (`is_demo = true`), los datos de ejemplo sobrescriben tarifas salariales o crean colisiones de claves primarias.
3. Posteriormente, el usuario invoca `DELETE /api/v1/demo/purge`: si la consulta de eliminación borra registros por coincidencia de nombre o sin discriminar `is_demo`, se eliminan accidentalmente productos reales del centro.
4. Se pierde la integridad histórica y contable de la clínica estética.

## 3. Impact Assessment
- **Confidencialidad**: Baja.
- **Integridad**: Crítica. Pérdida o corrupción de datos financieros y contables de centros reales.
- **Disponibilidad**: Media. Inutilización temporal del catálogo de servicios.
