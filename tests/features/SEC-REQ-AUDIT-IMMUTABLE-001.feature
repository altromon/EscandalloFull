# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/security/SEC-REQ-AUDIT-IMMUTABLE-001.md
# ID Requerimiento: SEC-REQ-AUDIT-IMMUTABLE-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@SEC-REQ-AUDIT-IMMUTABLE-001 @security @mitigation
Feature: Registro Inmutable de Auditoría Criptográficamente Sellado
  Como subsistema de trazabilidad financiera de Escandallo Estética
  Quiero preservar un historial append-only inmutable de todas las variaciones de costes
  Para garantizar el no repudio y neutralizar la manipulación oculta de precios y márgenes

  @security @mitigation
  Scenario: Registro obligatorio y automático de auditoría ante una variación de precio
    Given un producto cosmético con precio de compra registrado de 30.00 EUR
    When el gestor "gestor@estetica.com" actualiza el precio a 45.00 EUR
    Then el sistema almacena el nuevo precio del producto
    And crea inmediatamente una entrada en el log histórico de auditoría conteniendo:
      | campo         | valor                   |
      | actor_id      | "gestor@estetica.com"   |
      | valor_previo  | 30.00                   |
      | valor_nuevo   | 45.00                   |
      | tipo_cambio   | "UPDATE_COSMETIC_PRICE" |
    And calcula un hash criptográfico SHA-256 encadenado que sella el registro

  @security @mitigation
  Scenario: Rechazo de intentos de alteración o borrado de registros de auditoría histórica
    Given existen registros históricos de cambios de precio en el centro activo
    When un usuario intenta ejecutar una operación "DELETE" o "UPDATE" sobre la tabla de histórico
    Then el motor de persistencia deniega la transacción con error de restricción de integridad
    And el historial de costes permanece íntegro y protegido contra alteraciones
