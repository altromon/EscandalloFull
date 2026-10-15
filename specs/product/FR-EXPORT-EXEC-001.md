---
id: "FR-EXPORT-EXEC-001"
type: "requirement"
title: "Exportación Ejecutiva de Escandallos en PDF y XLSX"
status: "draft"
version: "1.0.0"
category: "functional"
derives-from:
  - "UC-EXPORT-EXEC-001"
verifiable-by: "cucumber-bdd"
acceptance-format: "gherkin"
cucumber-tags:
  - "@FR-EXPORT-EXEC-001"
  - "@automated"
  - "@regression"
---

# Requirement: Exportación Ejecutiva de Escandallos en PDF y XLSX

El sistema debe generar exportaciones en formato PDF y Microsoft Excel (XLSX) con un diseño corporativo estilizado y adecuado para directivos y ejecutivos.

```gherkin
@FR-EXPORT-EXEC-001 @automated @regression
Feature: Exportación Ejecutiva de Escandallos en PDF y XLSX
  Como usuario gestor de un centro de estética
  Quiero descargar los escandallos en PDF o XLSX con formato ejecutivo
  Para presentar informes claros y profesionales a socios, bancos o dirección

  Scenario: Descarga de informe ejecutivo en formato PDF
    Given el centro "Clínica Dermocosmética" tiene 5 escandallos configurados
    When el gestor solicita la exportación en formato "PDF"
    Then el sistema genera un documento binario con tipo MIME "application/pdf"
    And incluye la cabecera ejecutiva, el desglose de costes y los badges de rentabilidad en color

  Scenario Outline: Generación y validación de formato según tipo de exportación solicitado
    Given un centro con al menos un escandallo activo
    When el gestor solicita exportar en <formato>
    Then el tipo MIME generado es <mime_type>
    And el archivo resultante tiene la extensión <extension>

    Examples:
      | formato | mime_type                                                           | extension |
      | "PDF"   | "application/pdf"                                                   | ".pdf"    |
      | "XLSX"  | "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" | ".xlsx"   |
```
