@SEC-REQ-EXPORT-SANITIZATION-001 @security @mitigation @automated @regression
Feature: Sanitización Anti-Formula Injection en XLSX y Sandboxing PDF
  Como subsistema de exportación documental de Escandallo Estética
  Quiero sanitizar campos dinámicos y aislar el entorno de renderizado
  Para neutralizar ataques de Formula Injection en hojas de cálculo y SSRF en generación de PDFs

  @security @mitigation
  Scenario: Neutralización de Formula Injection en celdas de archivo XLSX exportado
    Given un servicio de estética registrado con el nombre "=cmd|'/C calc'!A0"
    And un producto cosmético con descripción "@SUM(1+1)*cmd"
    When el usuario gestor solicita la exportación ejecutiva en formato "XLSX"
    Then el motor de generación documental sanitiza las celdas de texto
    And antepone una comilla simple "'" antes del primer carácter ejecutable
    And la celda se emite con tipo de dato literal de texto ("string") y no como fórmula activa

  @security @mitigation
  Scenario Outline: Bloqueo de peticiones remotas y SSRF durante la renderización de informes PDF
    Given un centro de estética con contenido embebido conteniendo la URL <url_remota>
    When se ejecuta la compilación del informe en formato PDF
    Then el motor de renderizado dentro del sandbox bloquea la resolución de red
    And no emite ninguna petición HTTP hacia <url_remota>
    And el documento generado sustituye el elemento por un marcador de posición seguro

    Examples:
      | url_remota                                |
      | "http://169.254.169.254/latest/meta-data" |
      | "http://localhost:5000/internal-metrics"  |
      | "file:///etc/passwd"                      |
