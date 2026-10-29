# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/FR-AUTH-MULTI-001.md
# ID Requerimiento: FR-AUTH-MULTI-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-AUTH-MULTI-001 @automated @regression
Feature: Autenticación Multi-inquilino y Selección de Centro
  Como usuario gestor de uno o varios centros de estética
  Quiero autenticarme y seleccionar el centro de trabajo activo
  Para gestionar exclusivamente los escandallos y costes de dicho centro con aislamiento estricto

  Scenario: Inicio de sesión exitoso y selección de centro único
    Given existe un usuario "gestor@estetica.com" con un centro asignado "Centro Belleza Madrid"
    When el usuario inicia sesión con credenciales válidas
    Then el sistema autentica al usuario exitosamente
    And establece "Centro Belleza Madrid" como el centro activo en la sesión

  Scenario Outline: Acceso a centros según permisos del usuario
    Given el usuario autenticado tiene acceso a <centros_asignados>
    When intenta seleccionar el centro <centro_solicitado>
    Then el resultado de la selección es <resultado>

    Examples:
      | centros_asignados                     | centro_solicitado      | resultado   |
      | "Centro Centro, Centro Norte"        | "Centro Centro"        | "permitido" |
      | "Centro Centro, Centro Norte"        | "Centro Norte"         | "permitido" |
      | "Centro Centro"                      | "Centro Sur Ajeno"     | "denegado"  |
