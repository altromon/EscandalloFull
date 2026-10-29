# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/FR-DEMO-MODE-001.md
# ID Requerimiento: FR-DEMO-MODE-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-DEMO-MODE-001 @automated @regression
Feature: Gestión de Datos de Prueba y Aislamiento de Modo Demo
  Como usuario gestor de un centro de estética
  Quiero que mi centro de producción inicie limpio y poder activar o purgar datos de demo bajo demanda
  Para no mezclar información ficticia con la operativa contable real de mi salón

  Scenario: Creación de nuevo centro en producción con estado limpio por defecto
    Given un usuario gestor autenticado en la plataforma
    When crea un nuevo centro de estética denominado "Clínica Real Estética"
    Then el centro se inicializa con 0 empleados, 0 gastos de local y 0 cosméticos
    And no contiene servicios ni escandallos precargados

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
