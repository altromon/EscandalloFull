---
tasks:
  - id: "TSK-REM2-001"
    title: "Fase 1: CRUD Completo de Costes (Personal, Cosméticos, Salón) y Eliminación de Escandallos"
    complexity: "MEDIUM"
    risk-level: "LOW"
    autonomy-mode: "AUTONOMOUS"
    assigned-to: "agent-developer"
    status: "COMPLETED"
    verification:
      method: "COMMAND_VERIFICATION"
      command-or-criteria: "dotnet test tests/unit --filter Category=FR-COST-CONFIG-001"

  - id: "TSK-REM2-002"
    title: "Fase 2: Purga Real de Datos de Demo y Contratación/Cancelación de Plan Pro"
    complexity: "MEDIUM"
    risk-level: "LOW"
    autonomy-mode: "AUTONOMOUS"
    assigned-to: "agent-developer"
    status: "COMPLETED"
    verification:
      method: "COMMAND_VERIFICATION"
      command-or-criteria: "dotnet test tests/unit --filter Category=FR-SUBSCRIPTION-GATE-001"

  - id: "TSK-REM2-003"
    title: "Fase 3: Multi-sede Dinámico (Crear Centros) y Registro Dinámico de Usuarios"
    complexity: "MEDIUM"
    risk-level: "LOW"
    autonomy-mode: "AUTONOMOUS"
    assigned-to: "agent-developer"
    status: "COMPLETED"
    verification:
      method: "COMMAND_VERIFICATION"
      command-or-criteria: "dotnet test tests/Escandallo.Tests.UI --filter Category=AuthSecurity"

  - id: "TSK-REM2-004"
    title: "Fase 4: Histórico Criptográfico SHA-256 y Visor de Observabilidad de Logs y Trazas"
    complexity: "MEDIUM"
    risk-level: "LOW"
    autonomy-mode: "AUTONOMOUS"
    assigned-to: "agent-developer"
    status: "COMPLETED"
    verification:
      method: "COMMAND_VERIFICATION"
      command-or-criteria: "dotnet test"
---

# Plan de Tareas: CHG-REMEDIATION-002

Desglose de tareas por fases independientes para su implementación y verificación progresiva.
