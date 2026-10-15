---
id: "FR-AUTH-MULTI-001"
type: "requirement"
title: "Autenticación Multi-inquilino y Selección de Centro"
status: "draft"
version: "1.0.0"
category: "functional"
derives-from:
  - "UC-AUTH-MULTI-001"
verifiable-by: "cucumber-bdd"
acceptance-format: "gherkin"
cucumber-tags:
  - "@FR-AUTH-MULTI-001"
  - "@automated"
  - "@regression"
---

# Requirement: Autenticación Multi-inquilino y Selección de Centro

El sistema debe permitir a los usuarios autenticarse de forma segura y acceder únicamente a los centros de estética que tengan asignados bajo su cuenta u organización (inquilino).

```gherkin
@FR-AUTH-MULTI-001 @automated @regression
Feature: Autenticación Multi-inquilino y Selección de Centro
  Como usuario gestor de uno o varios centros de estética
  Quiero autenticarme y seleccionar el centro de trabajo activo
  Para gestionar exclusivamente los escandallos y costes de dicho centro con aislamiento estricto

  Scenario: Inicio de sesión exitoso y selección de centro único
    Given existe un usuario "gestor@estetica.com" con un centro asignado "Centro Belleza Madrid"
    When el usuario inicia sesión con credenciales válidas
    Then el sistema autentica al usuario exitosamente
    And establece "Centro Belleza Madrid" como el centro activo en la sesión

  Scenario Outline: Acceso a centros según permisos del usuario
    Given el usuario autenticado tiene acceso a <centros_asignados>
    When intenta seleccionar el centro <centro_solicitado>
    Then el resultado de la selección es <resultado>

    Examples:
      | centros_asignados                     | centro_solicitado      | resultado   |
      | "Centro Centro, Centro Norte"        | "Centro Centro"        | "permitido" |
      | "Centro Centro, Centro Norte"        | "Centro Norte"         | "permitido" |
      | "Centro Centro"                      | "Centro Sur Ajeno"     | "denegado"  |
```
