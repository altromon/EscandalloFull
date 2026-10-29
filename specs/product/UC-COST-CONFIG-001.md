---
id: "UC-COST-CONFIG-001"
type: "use-case"
title: "Definición Paramétrica de Factores de Coste del Escandallo"
status: "draft"
version: "1.0.0"
primary-actor: "ACT-USER-001"
supporting-actors: []
---

# Use Case: Definición Paramétrica de Factores de Coste del Escandallo (`UC-COST-CONFIG-001`)

## 1. Intent and Outcome
As a `ACT-USER-001`, I want to create, update, and manage the base cost elements (staff wages, premises overhead, cosmetic products, and disposables), to accurately establish the baseline hourly and unit costs for all aesthetic treatments.

## 2. Preconditions
- `ACT-USER-001` is authenticated and has an active aesthetic center selected.

## 3. Main Success Scenario
1. The user navigates to the Cost Configuration section for the active center.
2. The user registers worker profiles specifying gross monthly/annual compensation and effective working hours to obtain the cost-per-hour.
3. The user inputs monthly fixed premises costs (rent, utilities, equipment depreciation) and monthly operational opening hours to compute the facility overhead cost-per-minute.
4. The user catalogs cosmetic products and consumables specifying commercial container volume/weight, purchase cost, waste percentage (merma), and default dosage per treatment.
5. The system validates positive monetary values and operational formulas.
6. The system saves the configuration and records a new revision in the cost audit log.

## 4. Alternative and Error Flows
- **Zero Opening Hours**: If operational hours are zero or negative, the system flags a validation error preventing division by zero.
- **Negative Pricing**: If any cost or wage is negative, the system rejects the entry.
