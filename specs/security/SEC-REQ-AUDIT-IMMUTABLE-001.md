---
id: "SEC-REQ-AUDIT-IMMUTABLE-001"
type: "security-requirement"
title: "Registro Inmutable de Auditoría Criptográficamente Sellado para Variaciones de Costes"
status: "draft"
version: "1.0.0"
security-domain: "audit-logging"
mitigates-abuse-case:
  - "ABUSE-HISTORY-REPUD-001"
enforced-in-enclave: "SEC-ENC-DATA-ACCESS-001"
compliance-references:
  - "OWASP-ASVS-4.0.3-V10.1"
  - "OWASP-ASVS-4.0.3-V10.3"
acceptance-format: "gherkin"
cucumber-tags:
  - "@SEC-REQ-AUDIT-IMMUTABLE-001"
  - "@automated"
  - "@regression"
---

# Security Requirement: Registro Inmutable de Auditoría (`SEC-REQ-AUDIT-IMMUTABLE-001`)

El sistema debe registrar cada modificación de parámetros de coste (salarios, alquileres, suministros, productos cosméticos) en una tabla de auditoría histórica inmutable (*append-only*), capturando de forma obligatoria el identificador del usuario, dirección IP, marca temporal UTC, valor numérico anterior, valor numérico nuevo y hash SHA-256 de integridad del registro, prohibiendo a nivel de base de datos operaciones de actualización (`UPDATE`) o borrado (`DELETE`) sobre dicha tabla.

```gherkin
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
```
