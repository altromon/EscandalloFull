@FR-DEMO-MODE-001 @automated @regression
Feature: Gestión de Datos de Prueba y Aislamiento de Modo Demo
  Como usuario gestor de un centro de estética
  Quiero que mi centro de producción inicie limpio y poder activar o purgar datos de demo bajo demanda
  Para no mezclar información ficticia con la operativa contable real de mi salón

  # 1. NOMINAL CASE (Happy Path)
  Scenario: Creación de nuevo centro en producción con estado limpio por defecto
    Given un usuario gestor autenticado en la plataforma
    When crea un nuevo centro de estética denominado "Clínica Real Estética"
    Then el centro se inicializa con 0 empleados, 0 gastos de local y 0 cosméticos
    And no contiene servicios ni escandallos precargados

  # 2. BOUNDARY CASES (Scenario Outline + Examples)
  Scenario Outline: Ciclo de vida de datos de demostración en un centro
    Given un centro de estética en estado <estado_inicial>
    When el usuario gestor ejecuta la acción <accion_usuario>
    Then el estado resultante del centro es <estado_final>
    And el conteo de servicios de prueba en catálogo es <servicios_demo>

    Examples:
      | estado_inicial | accion_usuario         | estado_final     | servicios_demo |
      | "limpio"       | "cargar_datos_demo"    | "modo_demo"      | 2              |
      | "modo_demo"    | "limpiar_datos_demo"   | "limpio"         | 0              |
      | "limpio"       | "rechazar_datos_demo"  | "limpio"         | 0              |

  # 3. OUT-OF-RANGE / INVALID CASES (Scenario Outline + Examples)
  Scenario: Bloqueo de carga de datos demo si el centro ya contiene registros reales sin confirmación
    Given un centro que contiene 3 cosméticos reales con "is_demo = false"
    When el usuario solicita cargar datos demo con "overrideIfRealExists = false"
    Then el sistema rechaza la carga con código HTTP 409 Conflict
    And no se altera la base de datos del centro

  Scenario: Purga de datos demo sobre un centro que ya está limpio
    Given un centro limpio que no contiene ningún dato de demo
    When el usuario solicita purgar datos demo
    Then el sistema responde con éxito de forma idempotente reportando 0 registros eliminados
