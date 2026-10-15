@FR-EXPORT-EXEC-001 @automated @regression
Feature: Exportación Ejecutiva de Escandallos en PDF y XLSX
  Como usuario gestor de un centro de estética
  Quiero descargar los escandallos en PDF o XLSX con formato ejecutivo
  Para presentar informes claros y profesionales a socios, bancos o dirección

  # 1. NOMINAL CASE (Happy Path)
  Scenario: Descarga de informe ejecutivo en formato PDF
    Given el centro "Clínica Dermocosmética" tiene 5 escandallos configurados
    When el gestor solicita la exportación en formato "PDF"
    Then el sistema genera un documento binario con tipo MIME "application/pdf"
    And incluye la cabecera ejecutiva, el desglose de costes y los badges de rentabilidad en color

  # 2. BOUNDARY CASES (Scenario Outline + Examples)
  Scenario Outline: Generación y validación de formato según tipo de exportación solicitado
    Given un centro con al menos un escandallo activo
    When el gestor solicita exportar en <formato>
    Then el tipo MIME generado es <mime_type>
    And el archivo resultante tiene la extensión <extension>

    Examples:
      | formato | mime_type                                                           | extension |
      | "PDF"   | "application/pdf"                                                   | ".pdf"    |
      | "XLSX"  | "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" | ".xlsx"   |

  Scenario Outline: Exportación según volumen de escandallos configurados
    Given un centro con <numero_escandallos> escandallos registrados
    When se genera el informe en formato "PDF"
    Then el documento contiene exactamente <numero_escandallos> bloques de escandallo

    Examples:
      | numero_escandallos |
      | 1                  |
      | 2                  |
      | 50                 |

  # 3. OUT-OF-RANGE / INVALID CASES (Scenario Outline + Examples)
  Scenario Outline: Manejo de solicitudes inválidas o formatos no soportados
    Given un centro con escandallos activos
    When se solicita exportar con formato no válido <formato_invalido>
    Then el sistema responde con error de solicitud <codigo_error>

    Examples:
      | formato_invalido | codigo_error             |
      | "DOCX"           | "UNSUPPORTED_FORMAT"     |
      | "CSV"            | "UNSUPPORTED_FORMAT"     |
      | ""               | "MISSING_FORMAT"         |
