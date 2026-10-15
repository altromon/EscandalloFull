---
id: "FR-HISTORY-RECALC-001"
type: "requirement"
title: "Histórico de Modificaciones y Recálculo Automático"
status: "draft"
version: "1.0.0"
category: "functional"
derives-from:
  - "UC-HISTORY-RECALC-001"
verifiable-by: "cucumber-bdd"
acceptance-format: "gherkin"
cucumber-tags:
  - "@FR-HISTORY-RECALC-001"
  - "@automated"
  - "@regression"
---

# Requirement: Histórico de Modificaciones y Recálculo Automático

El sistema debe auditar todo cambio en factores de coste registrando su histórico y desencadenar el recálculo automático e inmediato de todos los escandallos y márgenes asociados.

```gherkin
@FR-HISTORY-RECALC-001 @automated @regression
Feature: Histórico de Modificaciones y Recálculo Automático
  Como usuario gestor de un centro de estética
  Quiero que la variación de un precio se guarde en el historial y recalcule los servicios afectados
  Para mantener los escandallos al día sin tener que recalcular manualmente cada tratamiento

  Scenario: Registro histórico y actualización en cascada de servicios vinculados
    Given el producto "Crema Masaje Facial 1L" se utiliza en 3 servicios activos
    When el gestor modifica el precio de compra del producto de 30.00 EUR a 45.00 EUR
    Then el sistema registra una entrada histórica con fecha, usuario, valor anterior y nuevo
    And recalcula automáticamente los 3 servicios dependientes
    And actualiza los costes y márgenes en pantalla

  Scenario Outline: Impacto del recálculo automático en el margen del servicio
    Given un servicio con coste inicial <coste_previo> EUR y PVP <pvp> EUR clasificado como <color_previo>
    When un ingrediente aumenta su coste en <incremento_coste> EUR
    Then el nuevo margen es <nuevo_margen>%
    And el nuevo color asignado es <nuevo_color>

    Examples:
      | coste_previo | pvp    | color_previo | incremento_coste | nuevo_margen | nuevo_color |
      | 55.00        | 100.00 | "verde"      | 10.00            | 35.0         | "amarillo"  |
      | 75.00        | 100.00 | "amarillo"   | 10.00            | 15.0         | "rojo"      |
      | 20.00        | 100.00 | "verde"      | 5.00             | 75.0         | "verde"     |
```
