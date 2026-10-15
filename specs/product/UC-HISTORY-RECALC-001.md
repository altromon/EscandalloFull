---
id: "UC-HISTORY-RECALC-001"
type: "use-case"
title: "Histórico de Modificaciones y Recálculo Automático"
status: "draft"
version: "1.0.0"
primary-actor: "ACT-USER-001"
secondary-actors: []
---

# Use Case: Histórico de Modificaciones y Recálculo Automático (`UC-HISTORY-RECALC-001`)

## 1. Intent and Outcome
As a `ACT-USER-001`, I want the system to preserve an immutable audit trail of past cost changes and automatically recompute all dependent service escandallos, to ensure financial calculations always reflect real updated costs without manual re-entry.

## 2. Preconditions
- The aesthetic center contains established escandallos referencing cost parameters (products, wages, rent).

## 3. Main Success Scenario
1. The user modifies a cost parameter (e.g. increase in cosmetic supply purchase price or staff wage update).
2. The system registers a snapshot in the historical values table including timestamp, previous value, new value, and responsible actor.
3. The system detects all service escandallos that use the updated parameter.
4. The system executes an automatic batch recalculation of total costs, profit margins, and traffic light color indicators.
5. The system notifies the user of the number of affected services updated.
6. The user can view the historical timeline of changes for any specific cost factor or service.

## 4. Alternative and Error Flows
- **Cascade Error Guard**: If recalculation encounters invalid numerical data, the transaction aborts and preserves the previous stable state.
