---
id: "ABUSE-EXPORT-INJECT-001"
type: "abuse-case"
title: "Inyección de Fórmulas Maliciosas en XLSX y SSRF/Inundación de Memoria en Exportación PDF"
status: "draft"
version: "1.0.0"
primary-threat-actor: "ACT-THREAT-INSIDER-GUEST-001"
targets-use-case: "UC-EXPORT-EXEC-001"
stride-category: "tampering"
mitigated-by:
  - "SEC-REQ-EXPORT-SANITIZATION-001"
asvs-controls:
  - "V5.1.3"
  - "V12.5.2"
---

# Abuse Case: Inyección en Exportaciones XLSX/PDF (`ABUSE-EXPORT-INJECT-001`)

## 1. Description and Adversary Objective
El adversario `ACT-THREAT-INSIDER-GUEST-001` o un usuario gestor malicioso introduce fórmulas ejecutables de hoja de cálculo (`=CMD|' /C ...'!A0`, `@SUM(...)`, `-2+3+cmd|...`) o etiquetas HTML/CSS maliciosas en nombres de tratamientos, notas de escandallo o descripciones de cosméticos, para atacar a directivos al abrir los archivos exportados o comprometer el motor de renderizado PDF en el servidor.

## 2. Attack Execution Scenario
1. El atacante registra un nuevo producto cosmético con el nombre `=cmd|'/C calc'!A0` o una carga HTML `<img src="http://169.254.169.254/latest/meta-data/">`.
2. Genera o induce al propietario a descargar la exportación ejecutiva en formato XLSX o PDF (`UC-EXPORT-EXEC-001`).
3. En la exportación XLSX: la aplicación de hoja de cálculo del ejecutivo ejecuta la fórmula o extrae datos confidenciales de la máquina local.
4. En la exportación PDF: el motor de renderizado del backend (.NET) interpreta las etiquetas HTML/CSS, intentando peticiones SSRF a redes internas o saturando la memoria del contenedor mediante entidades externas (XXE / DoS por recursión).

## 3. Impact Assessment
- **Confidencialidad**: Alta (riesgo de exfiltración de metadatos o datos de la máquina del usuario).
- **Integridad**: Alta (ejecución no autorizada de código en el cliente del ejecutivo).
- **Disponibilidad**: Alta (colapso de memoria RAM por PDF complejo en VPS ligero).
