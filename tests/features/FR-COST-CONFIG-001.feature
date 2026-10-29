# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/FR-COST-CONFIG-001.md
# ID Requerimiento: FR-COST-CONFIG-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-COST-CONFIG-001 @automated @regression
Feature: Gestión CRUD de Factores de Coste del Escandallo
  Como usuario gestor de un centro de estética
  Quiero definir los costes de personal, local y productos
  Para que la aplicación disponga de las tarifas base necesarias para el cálculo de los servicios

  Scenario: Registro exitoso de un producto cosmético con cálculo de coste unitario y tasa de merma
    Given el centro "Estética Bella" está seleccionado
    When el gestor registra un cosmético con nombre "Cleasing Milk", precio 34.51 EUR, capacidad 500 "ml" y merma 5%
    Then el sistema almacena el producto correctamente
    And calcula un coste unitario bruto de 0.06902 EUR por "ml"
    And calcula un coste unitario efectivo de 0.07247 EUR por "ml"

  Scenario Outline: Validación de datos de entrada en factores de coste
    Given el usuario gestor introduce un factor de coste con <tipo>, <valor_numerico> y <unidades>
    When solicita guardar el factor de coste
    Then el sistema responde con <estado_validacion>

    Examples:
      | tipo               | valor_numerico | unidades       | estado_validacion |
      | "trabajador_mes"   | 1980.00        | "euros/mes"    | "valido"          |
      | "gastos_local_mes" | 335.00         | "euros/mes"    | "valido"          |
      | "apertura_minutos" | 0              | "minutos/mes"  | "error_cero"      |
      | "producto_coste"   | -10.00         | "euros/envase" | "error_negativo"  |
