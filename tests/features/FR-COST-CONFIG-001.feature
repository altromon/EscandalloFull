@FR-COST-CONFIG-001 @automated @regression
Feature: Gestión CRUD de Factores de Coste del Escandallo
  Como usuario gestor de un centro de estética
  Quiero definir los costes de personal, local y productos
  Para que la aplicación disponga de las tarifas base necesarias para el cálculo de los servicios

  # 1. NOMINAL CASE (Happy Path)
  Scenario: Registro exitoso de un producto cosmético con cálculo de coste unitario y tasa de merma
    Given el centro "Estética Bella" está seleccionado
    When el gestor registra un cosmético con nombre "Cleasing Milk", precio 34.51 EUR, capacidad 500 "ml" y merma 5%
    Then el sistema almacena el producto correctamente
    And calcula un coste unitario bruto de 0.06902 EUR por "ml"
    And calcula un coste unitario efectivo de 0.07247 EUR por "ml"

  Scenario: Configuración estándar de horas de apertura y cálculo automático de minutos
    Given el centro "Estética Bella" está seleccionado
    When el gestor establece las horas mensuales de apertura en 176 "h/mes"
    And registra gastos generales de local 335.00 EUR, consumos 100.00 EUR y consumibles 440.00 EUR
    Then el total mensual de apertura es 10560 "min/mes"
    And el total de gastos generales mensuales es 875.00 EUR
    And el coste general imputable por minuto es 0.08286 EUR

  # 2. BOUNDARY CASES (Scenario Outline + Examples)
  Scenario Outline: Valores límite en horas de apertura y merma cosmética
    Given un factor de coste con tipo <tipo_parametro> y valor <valor_limite>
    When se valida el parámetro de configuración
    Then el resultado de la validación es <resultado>

    Examples:
      | tipo_parametro        | valor_limite | resultado    |
      | "horas_apertura_mes"  | 1.0          | "aceptado"   |
      | "horas_apertura_mes"  | 176.0        | "aceptado"   |
      | "horas_apertura_mes"  | 744.0        | "aceptado"   |
      | "tasa_merma_pct"      | 0.0          | "aceptado"   |
      | "tasa_merma_pct"      | 100.0        | "aceptado"   |

  # 3. OUT-OF-RANGE / INVALID CASES (Scenario Outline + Examples)
  Scenario Outline: Validación de datos de entrada inválidos o fuera de rango
    Given el usuario gestor introduce un factor de coste con <tipo>, <valor_numerico> y <unidades>
    When solicita guardar el factor de coste
    Then el sistema responde con <estado_validacion>

    Examples:
      | tipo               | valor_numerico | unidades       | estado_validacion     |
      | "trabajador_mes"   | 1980.00        | "euros/mes"    | "valido"              |
      | "gastos_local_mes" | 335.00         | "euros/mes"    | "valido"              |
      | "apertura_horas"   | 0              | "h/mes"        | "error_cero"          |
      | "apertura_horas"   | -10            | "h/mes"        | "error_negativo"      |
      | "apertura_horas"   | 800            | "h/mes"        | "error_excede_mes"    |
      | "producto_coste"   | -10.00         | "euros/envase" | "error_negativo"      |
      | "producto_capacidad"| 0             | "ml"           | "error_capacidad_cero"|
      | "tasa_merma_pct"   | -1.0           | "%"            | "error_merma_negativa"|
      | "tasa_merma_pct"   | 105.0          | "%"            | "error_merma_excesiva"|
