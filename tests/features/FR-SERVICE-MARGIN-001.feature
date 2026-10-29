# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/FR-SERVICE-MARGIN-001.md
# ID Requerimiento: FR-SERVICE-MARGIN-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-SERVICE-MARGIN-001 @automated @regression
Feature: Matriz de Rentabilidad y Semáforo Visual de Servicios
  Como usuario gestor o invitado
  Quiero consultar el listado de servicios con su rentabilidad y código de colores
  Para comprender de un vistazo la salud económica de cada tratamiento y pack de sesiones

  Scenario: Cálculo exacto de escandallo y sugerencia de precios con IVA general
    Given un servicio "Mesodermolifting" con duración 60 minutos e IVA al 21%
    And gastos generales por minuto de 0.08286 EUR
    And profesional asignado "Encargado" con coste por minuto de 0.27273 EUR
    And un coste total de productos cosméticos utilizados de 13.37723 EUR
    When el sistema calcula el escandallo total del servicio
    Then el escandallo total resultante es 34.71 EUR
    And el precio base sugerido con 50% de beneficio es 52.07 EUR
    And el PVP sugerido con 21% de IVA es 63.00 EUR

  Scenario: Evaluación de rentabilidad de servicio médico-estético exento de IVA
    Given un servicio "Consulta Médica Dermatológica" con escandallo total 30.00 EUR e IVA al 0% ("exento")
    When el usuario establece un PVR de 80.00 EUR
    Then el margen neto absoluto es 50.00 EUR
    And el margen real sobre venta es 62.5%
    And el semáforo visual asignado es "verde"

  Scenario Outline: Rentabilidad de Bonos y Packs de sesiones según PVR de pack
    Given un servicio con escandallo unitario de 34.71 EUR e IVA al 21%
    When se define un bono de <num_sesiones> sesiones con precio total de <precio_bono_con_iva> EUR
    Then el ingreso efectivo por sesión sin IVA es <ingreso_sesion_sin_iva> EUR
    And el margen porcentual resultante del bono es <margen_bono_pct>%
    And el color de semáforo para el bono es <color_semaforo>

    Examples:
      | num_sesiones | precio_bono_con_iva | ingreso_sesion_sin_iva | margen_bono_pct | color_semaforo |
      | 5            | 400.00              | 66.12                  | 47.5            | "verde"        |
      | 5            | 300.00              | 49.59                  | 30.0            | "amarillo"     |
      | 5            | 200.00              | 33.06                  | -5.0            | "rojo"         |

  Scenario: Detección y alerta explícita de venta a pérdidas
    Given un servicio con escandallo total de 34.71 EUR e IVA al 21%
    When el usuario establece un PVR con IVA de 30.00 EUR
    Then el ingreso neto sin IVA es 24.79 EUR
    And el beneficio neto absoluto es -9.92 EUR
    And el margen real sobre venta es -40.0%
    And el semáforo visual asignado es "rojo"
    And el sistema emite el mensaje de advertencia "Atención: Estás perdiendo 9.92 € por cada sesión"

  Scenario: Duplicación exitosa de servicio y clonación de parámetros
    Given un servicio existente "Mesodermolifting" con 3 productos y 1 bono de 5 sesiones
    And el centro cuenta con cuota disponible de escandallos
    When el usuario solicita duplicar el servicio
    Then se crea un nuevo servicio con nombre "Copia de Mesodermolifting"
    And replica exactamente la duración, categoría profesional, productos y bonos del original

  Scenario: Desactivación de escandallo para liberar cuota freemium
    Given un centro en plan "Free" con 2 escandallos activos
    When el usuario desactiva uno de los escandallos activos ("is_active = false")
    Then el conteo de escandallos activos se reduce a 1
    And se permite la creación o duplicación de un nuevo escandallo activo
