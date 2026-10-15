---
id: "UC-SERVICE-MARGIN-001"
type: "use-case"
title: "Revisión Visual de Rentabilidad y Semáforo de Colores"
status: "draft"
version: "1.0.0"
primary-actor: "ACT-USER-001"
secondary-actors:
  - "ACT-GUEST-001"
---

# Use Case: Revisión Visual de Rentabilidad y Semáforo de Colores (`UC-SERVICE-MARGIN-001`)

## 1. Intent and Outcome
As a `ACT-USER-001`, I want to inspect my center's services matrix with clear color-coded profitability indicators, to identify high-margin treatments and immediately spot underpriced or loss-making services.

## 2. Preconditions
- The active aesthetic center has defined cost factors and at least one service escandallo configured.

## 3. Main Success Scenario
1. The user navigates to the Services & Profitability view.
2. The user sets the applicable VAT regime for the service (21% general, 10% reduced, or 0% exempt medical).
3. The system computes for each service:
   - Total direct cost per session (products with merma + labor + facility overhead).
   - Absolute profit margin = (PVP without VAT - Total Cost).
   - Profit margin percentage = (Profit / PVP without VAT) * 100.
4. If the service configures Session Packs / Bonos (e.g. 5 or 10 sessions pack), the system computes effective per-session revenue and margin for each pack.
5. The system maps each service and its associated packs to a traffic light color code:
   - 🟢 **Green (High Profit)**: Margin percentage $\ge 40\%$.
   - 🟡 **Yellow (Acceptable Profit)**: Margin percentage between $20\%$ and $39.99\%$.
   - 🔴 **Red (Low / Critical Profit or Loss)**: Margin percentage $< 20\%$.
6. The system presents the visual dashboard with sorting and filtering by margin band. All configurable and computed fields display explicit units of measurement (`€`, `min`, `ml`, `ud`, `g`, `%`), and all percentages are displayed strictly in tanto por 100 ($0\%-100\%$).
7. If accessed by `ACT-GUEST-001`, the system renders the service margin status, pack options, and product quantities while suppressing confidential wage and rental breakdown lines.
8. **Service Duplication**: The user can duplicate any existing service ("Duplicar"). The system creates an exact clone (times, professional category, products, and bonus packs) prefixed with *"Copia de..."*.
9. **Active/Inactive Status Toggle**: The user can toggle a service between active (`is_active = true`) and inactive (`is_active = false`), allowing management of active portfolio within plan quotas.

## 4. Alternative and Error Flows
- **Negative Margin (Losses)**: If total cost exceeds PVP, the system highlights the row in vivid red and displays an explicit loss warning message: *"Atención: Estás perdiendo X,XX € por cada sesión"*.
- **Freemium Quota Exceeded on Duplication or Activation**: If a user on the Free tier attempts to duplicate an active service or activate an inactive service when 2 active escandallos already exist, the action is blocked with error `QUOTA_EXCEEDED` and triggers the Premium upgrade modal (`BR-FREEMIUM-LIMIT-001`).
