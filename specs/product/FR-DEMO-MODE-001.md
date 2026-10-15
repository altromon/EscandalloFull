---
id: "FR-DEMO-MODE-001"
type: "requirement"
title: "Gestión de Datos de Prueba y Aislamiento de Modo Demo"
status: "draft"
version: "1.0.0"
category: "functional"
derives-from:
  - "UC-DEMO-MODE-001"
verifiable-by: "cucumber-bdd"
acceptance-format: "gherkin"
cucumber-tags:
  - "@FR-DEMO-MODE-001"
  - "@automated"
  - "@regression"
---

# Requirement: Gestión de Datos de Prueba y Aislamiento de Modo Demo

El sistema debe inicializar todo nuevo centro de estética en blanco sin datos precargados, permitiendo únicamente bajo petición explícita del usuario gestor la carga del conjunto de datos de ejemplo (demo) y su posterior eliminación limpia sin dejar registros residuales.

```gherkin
@FR-DEMO-MODE-001 @automated @regression
Feature: Gestión de Datos de Prueba y Aislamiento de Modo Demo
  Como usuario gestor de un centro de estética
  Quiero que mi centro de producción inicie limpio y poder activar o purgar datos de demo bajo demanda
  Para no mezclar información ficticia con la operativa contable real de mi salón

  Scenario: Creación de nuevo centro en producción con estado limpio por defecto
    Given un usuario gestor autenticado en la plataforma
    When crea un nuevo centro de estética denominado "Clínica Real Estética"
    Then el centro se inicializa con 0 empleados, 0 gastos de local y 0 cosméticos
    And no contiene servicios ni escandallos precargados

  Scenario Outline: Ciclo de vida de datos de demostración en un centro
    Given un centro de estética en estado <estado_inicial>
    When el usuario gestor ejecuta la acción <accion_usuario>
    Then el estado resultante del centro es <estado_final>
    And el conteo de servicios de prueba en catálogo es <servicios_demo>

    Examples:
      | estado_inicial | accion_usuario         | estado_final     | servicios_demo |
      | "limpio"       | "cargar_datos_demo"    | "modo_demo"      | 2              |
      | "modo_demo"    | "limpiar_datos_demo"   | "limpio"         | 0              |
      | "limpio"       | "rechazar_datos_demo"  | "limpio"         | 0              |
```
