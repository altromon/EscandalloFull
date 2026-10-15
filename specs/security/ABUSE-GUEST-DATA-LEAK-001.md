---
id: "ABUSE-GUEST-DATA-LEAK-001"
type: "abuse-case"
title: "Fuga de Salarios de Personal y Costes Fijos de Local a Invitados"
status: "draft"
version: "1.0.0"
primary-threat-actor: "ACT-THREAT-INSIDER-GUEST-001"
targets-use-case: "UC-SERVICE-MARGIN-001"
stride-category: "information-disclosure"
mitigated-by:
  - "SEC-REQ-DATA-CENSOR-GUEST-001"
asvs-controls:
  - "V4.1.5"
  - "V8.3.1"
---

# Abuse Case: Fuga de Salarios y Costes a Invitados (`ABUSE-GUEST-DATA-LEAK-001`)

## 1. Description and Adversary Objective
El adversario `ACT-THREAT-INSIDER-GUEST-001` (colaborador o esteticista con rol `ACT-GUEST-001`) busca descubrir el salario exacto de sus compañeros de trabajo ($C_{personal}$) y el coste mensual del alquiler y suministros del centro ($C_{local}$), vulnerando la regla de negocio `BR-GUEST-CENSOR-001`.

## 2. Attack Execution Scenario
1. El colaborador invitado inicia sesión en la plataforma y abre la vista de servicios y rentabilidad (`UC-SERVICE-MARGIN-001`).
2. Abre las herramientas de desarrollador del navegador (F12) o utiliza una herramienta de inspección HTTP (Postman/curl).
3. Analiza el payload devuelto por el endpoint `GET /api/v1/centers/{centerId}/services`.
4. Si el backend comete la negligencia de enviar el modelo de dominio completo con las propiedades `workerMonthlyWage`, `facilityOverheadPerMinute` o desglose salarial, esperando que la interfaz gráfica (Blazor/React) oculte los campos mediante CSS (`display: none`), el atacante lee directamente los valores sensibles en el JSON.

## 3. Impact Assessment
- **Confidencialidad**: Crítica. Violación del Reglamento General de Protección de Datos (RGPD) en materia de privacidad salarial y ruptura de confidencialidad comercial del local.
- **Integridad**: Nula.
- **Disponibilidad**: Nula.
