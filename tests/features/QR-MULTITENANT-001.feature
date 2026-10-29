@QR-MULTITENANT-001 @automated @regression
Feature: Aislamiento Lógico Multi-inquilino y Multicentro
  Como subsistema de seguridad y persistencia de Escandallo Estética
  Quiero garantizar el aislamiento absoluto de datos entre inquilinos y centros
  Para neutralizar cualquier riesgo de fuga de información contable o contaminación cruzada

  Scenario: Aislamiento estricto de consultas por discriminador de centro e inquilino
    Given un usuario gestor autenticado con contexto activo en "ctr-madrid-1"
    When consulta el catálogo de escandallos y factores de coste
    Then la consulta aplica el filtro global de aislamiento por centro
    And únicamente se retornan entidades pertenecientes a "ctr-madrid-1"
    And ningún registro de otros centros de la plataforma es visible en la respuesta

  Scenario Outline: Prevención de acceso y mutación cruzada entre centros no autorizados
    Given un usuario autenticado sin membresía en el centro <centro_destino>
    When intenta ejecutar una acción de <operacion> sobre un recurso del centro <centro_destino>
    Then la capa de persistencia o el middleware bloquea la petición
    And responde con código de acceso denegado <codigo_error>
    And el estado del centro <centro_destino> no sufre modificaciones

    Examples:
      | centro_destino    | operacion   | codigo_error |
      | "ctr-valencia-2"  | "lectura"   | 403          |
      | "ctr-barcelona-9" | "mutacion"  | 403          |
