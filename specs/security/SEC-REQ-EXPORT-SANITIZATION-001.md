---
id: "SEC-REQ-EXPORT-SANITIZATION-001"
type: "security-requirement"
title: "Sanitización Anti-Formula Injection en XLSX y Sandboxing Aislado de Renderizado PDF"
status: "draft"
version: "1.0.0"
security-domain: "input-validation"
mitigates-abuse-case:
  - "ABUSE-EXPORT-INJECT-001"
enforced-in-enclave: "SEC-ENC-EXPORT-SANDBOX-001"
compliance-references:
  - "OWASP-ASVS-4.0.3-V5.1"
  - "OWASP-ASVS-4.0.3-V12.5"
acceptance-format: "gherkin"
cucumber-tags:
  - "@SEC-REQ-EXPORT-SANITIZATION-001"
  - "@automated"
  - "@regression"
---

# Security Requirement: Sanitización en Exportaciones (`SEC-REQ-EXPORT-SANITIZATION-001`)

El subsistema de exportación ejecutiva (`UC-EXPORT-EXEC-001`) debe sanitizar todos los valores alfanuméricos inyectando un prefijo de escape (comilla simple `'`) en cualquier celda de hoja de cálculo XLSX que comience por caracteres de ejecución de fórmula (`=`, `+`, `-`, `@`, `\t`, `\r`), e inhabilitar de forma estricta el acceso a redes externas, protocolos remotos y resolución de entidades externas (XXE / SSRF) durante la compilación de documentos PDF en el backend.

```gherkin
@SEC-REQ-EXPORT-SANITIZATION-001 @security @mitigation
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
```
