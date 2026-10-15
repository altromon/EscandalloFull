@FR-HISTORY-RECALC-001 @automated @regression
Feature: Histórico de Modificaciones y Recálculo Automático
  Como usuario gestor de un centro de estética
  Quiero que la variación de un precio se guarde en el historial y recalcule los servicios afectados
  Para mantener los escandallos al día sin tener que recalcular manualmente cada tratamiento

  # 1. NOMINAL CASE (Happy Path)
  Scenario: Registro histórico y actualización en cascada de servicios vinculados
    Given el producto "Crema Masaje Facial 1L" se utiliza en 3 servicios activos
    When el gestor modifica el precio de compra del producto de 30.00 EUR a 45.00 EUR
    Then el sistema registra una entrada histórica con fecha, usuario, valor anterior y nuevo
    And recalcula automáticamente los 3 servicios dependientes
    And actualiza los costes y márgenes en pantalla

  # 2. BOUNDARY CASES (Scenario Outline + Examples)
  Scenario Outline: Impacto del recálculo automático en el margen y semáforo del servicio
    Given un servicio con coste inicial <coste_previo> EUR y PVP <pvp> EUR clasificado como <color_previo>
    When un ingrediente aumenta su coste en <incremento_coste> EUR
    Then el nuevo margen es <nuevo_margen>%
    And el nuevo color asignado es <nuevo_color>

    Examples:
      | coste_previo | pvp    | color_previo | incremento_coste | nuevo_margen | nuevo_color |
      | 55.00        | 100.00 | "verde"      | 10.00            | 35.0         | "amarillo"  |
      | 75.00        | 100.00 | "amarillo"   | 10.00            | 15.0         | "rojo"      |
      | 20.00        | 100.00 | "verde"      | 5.00             | 75.0         | "verde"     |
      | 60.00        | 100.00 | "verde"      | 0.01             | 39.99        | "amarillo"  |
      | 80.00        | 100.00 | "amarillo"   | 0.01             | 19.99        | "rojo"      |

  # 3. OUT-OF-RANGE / INVALID CASES (Scenario Outline + Examples)
  Scenario Outline: Validación ante variaciones no operativas o erróneas
    Given un producto con precio actual de 30.00 EUR
    When se intenta aplicar una modificación con valor <nuevo_precio>
    Then el sistema procesa la modificación como <resultado_operacion>

    Examples:
      | nuevo_precio | resultado_operacion             |
      | 30.00        | "ignorado_sin_cambios"          |
      | -15.00       | "rechazado_error_negativo"      |
      | 0.00         | "requiere_confirmacion_gratuito"|
