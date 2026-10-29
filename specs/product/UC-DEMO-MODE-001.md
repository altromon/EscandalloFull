---
id: "UC-DEMO-MODE-001"
type: "use-case"
title: "Activación, Exploración y Limpieza de Datos en Modo Demo"
status: "draft"
version: "1.0.0"
primary-actor: "ACT-USER-001"
supporting-actors: []
---

# Use Case: Activación, Exploración y Limpieza de Datos en Modo Demo (`UC-DEMO-MODE-001`)

## 1. Intent and Outcome
As a `ACT-USER-001`, I want to optionally seed a realistic sample dataset into an aesthetic center and reset it on demand, to explore the application's calculations and dashboards without permanently polluting my real production accounting data.

## 2. Preconditions
- `ACT-USER-001` is authenticated and has selected an active aesthetic center.

## 3. Main Success Scenario
1. The user creates a new aesthetic center, which is provisioned completely empty (clean slate).
2. The user sees an onboarding banner: *"¿Quieres probar la aplicación con datos de ejemplo?"*
3. The user confirms the action *"Cargar datos de demo"*.
4. The system populates the center with the isolated sample dataset:
   - Typical aesthetic clinic overhead costs (premises, utilities, disposables).
   - Standard staffing categories (technicians, supervisors, nurses).
   - Sample cosmetic products (cleansing milks, tonics, serums, vials).
   - Sample services with precalculated escandallos (e.g. `Mesodermolifting`).
5. The center is visually flagged with a prominent badge: **"Modo Demo Activo"**.
6. When the user finishes exploring, they click *"Eliminar datos de prueba e iniciar centro en blanco"*.
7. The system purges all demo records and returns the center to its clean operational state.

## 4. Alternative and Error Flows
- **Production Guard**: If the center already contains real custom services or real costs, the system warns the user that loading demo data may conflict with existing items and requires explicit confirmation.
- **Decline Demo Data**: If the user declines sample data, the banner is dismissed and the center remains completely blank for real manual data entry.
