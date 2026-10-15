@FR-AUTH-MULTI-001 @automated @regression
Feature: Autenticación Multi-inquilino y Selección de Centro
  Como usuario gestor de uno o varios centros de estética
  Quiero autenticarme y seleccionar el centro de trabajo activo
  Para gestionar exclusivamente los escandallos y costes de dicho centro con aislamiento estricto

  # 1. NOMINAL CASE (Happy Path)
  Scenario: Inicio de sesión exitoso y selección de centro único
    Given existe un usuario "gestor@estetica.com" con un centro asignado "Centro Belleza Madrid"
    When el usuario inicia sesión con credenciales válidas
    Then el sistema autentica al usuario exitosamente
    And establece "Centro Belleza Madrid" como el centro activo en la sesión
    And emite un token de sesión con rol "ACT-USER-001"

  # 2. BOUNDARY CASES (Scenario Outline + Examples)
  Scenario Outline: Acceso y cambio entre múltiples centros asignados
    Given el usuario autenticado tiene acceso a los centros <centros_asignados>
    When intenta seleccionar el centro <centro_solicitado>
    Then el resultado de la selección es <resultado>
    And el centro activo resultante es <centro_activo>

    Examples:
      | centros_asignados                     | centro_solicitado      | resultado   | centro_activo      |
      | "Centro Centro, Centro Norte"        | "Centro Centro"        | "permitido" | "Centro Centro"    |
      | "Centro Centro, Centro Norte"        | "Centro Norte"         | "permitido" | "Centro Norte"     |
      | "Centro Unico"                       | "Centro Unico"         | "permitido" | "Centro Unico"     |

  # 3. OUT-OF-RANGE / INVALID CASES (Scenario Outline + Examples)
  Scenario Outline: Rechazo de acceso a centros ajenos o credenciales no válidas
    Given el usuario autenticado tiene asignado únicamente "Centro Centro"
    When intenta acceder con <parametro_invalido> y valor <valor_invalido>
    Then el sistema deniega el acceso con código <codigo_error>

    Examples:
      | parametro_invalido       | valor_invalido          | codigo_error                  |
      | "centro_no_asignado"     | "Centro Sur Ajeno"      | "TENANT_MEMBERSHIP_VIOLATION" |
      | "centro_inexistente"     | "Centro Inexistente"    | "CENTER_NOT_FOUND"            |
      | "credencial_vacia"       | ""                      | "INVALID_CREDENTIALS"         |
      | "password_incorrecta"    | "wrong_pass_123"        | "INVALID_CREDENTIALS"         |
