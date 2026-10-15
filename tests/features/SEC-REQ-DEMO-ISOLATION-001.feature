@SEC-REQ-DEMO-ISOLATION-001 @security @mitigation @automated @regression
Feature: Etiquetado Aislado y Purga Verificada de Datos de Modo Demo
  Como motor de persistencia de Escandallo Estética
  Quiero aislar los registros ficticios de demostración mediante flags booleanos explícitos
  Para impedir la contaminación de centros reales y evitar el borrado accidental de datos contables de producción

  @security @mitigation
  Scenario: Purga quirúrgica de datos demo sin afectación a registros reales del centro
    Given un centro de estética que contiene 3 cosméticos reales con "is_demo = false"
    And contiene 5 cosméticos de ejemplo cargados con "is_demo = true"
    When el usuario gestor ejecuta la acción "Limpiar datos de prueba"
    Then el sistema ejecuta una transacción filtrada por "is_demo = true" y "center_id = activo"
    And elimina exactamente los 5 cosméticos de ejemplo
    And los 3 cosméticos reales permanecen intactos en la base de datos

  @security @mitigation
  Scenario: Bloqueo de precarga de datos de demo por defecto al crear un nuevo centro
    Given un usuario gestor autenticado en la plataforma
    When crea un nuevo centro de estética denominado "Salón Innovación"
    Then el sistema inicializa el centro estrictamente en blanco
    And el conteo de servicios registrados es exactamente 0
    And ningún registro contiene nombres o costes de la hoja de cálculo de ejemplo ("Cleasing Milk", "Mesodermolifting")
