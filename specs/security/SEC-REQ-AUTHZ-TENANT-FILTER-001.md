---
id: "SEC-REQ-AUTHZ-TENANT-FILTER-001"
type: "security-requirement"
title: "Aislamiento Estricto Multi-inquilino y Validación de Autorización a Nivel de Objeto"
status: "draft"
version: "1.0.0"
security-domain: "authorization"
mitigates-abuse-case:
  - "ABUSE-COST-TAMPER-001"
enforced-in-enclave: "SEC-ENC-DATA-ACCESS-001"
compliance-references:
  - "OWASP-ASVS-4.0.3-V4.1"
  - "OWASP-ASVS-4.0.3-V4.2"
acceptance-format: "gherkin"
cucumber-tags:
  - "@SEC-REQ-AUTHZ-TENANT-FILTER-001"
  - "@automated"
  - "@regression"
---

# Security Requirement: Aislamiento Multi-inquilino a Nivel de Objeto (`SEC-REQ-AUTHZ-TENANT-FILTER-001`)

El sistema debe aplicar filtros globales de consulta en el ORM (Entity Framework Core Global Query Filters) y validación de autorización a nivel de objeto (BOLA/IDOR) en todas las operaciones de lectura, mutación o eliminación de factores de coste (trabajadores, gastos generales, cosméticos), garantizando que ninguna consulta o modificación pueda afectar a registros pertenecientes a un `TenantId` o `CenterId` distinto al de la sesión autenticada.

```gherkin
@SEC-REQ-AUTHZ-TENANT-FILTER-001 @security @mitigation
Feature: Aislamiento Estricto Multi-inquilino y Validación a Nivel de Objeto (Anti-IDOR)
  Como subsistema de acceso a datos de Escandallo Estética
  Quiero garantizar que las operaciones CRUD de factores de coste se restrinjan al centro del inquilino autenticado
  Para mitigar la manipulación maliciosa de costes entre centros ajenos (BOLA/IDOR)

  @security @mitigation
  Scenario: Intento de modificación de coste de empleado de otro centro mediante manipulación de ID
    Given un usuario gestor autenticado en el centro "ctr-madrid-1"
    And existe un registro de salario de empleado con id "wrk-555" perteneciente al centro ajeno "ctr-valencia-2"
    When el usuario gestor envía una petición "PUT /api/v1/costs/workers/wrk-555" con nuevo salario de 900.00 EUR
    Then el sistema de persistencia intercepta la consulta con el filtro global de inquilino
    And responde con código HTTP 404 Not Found para no revelar la existencia del registro ajeno
    And el salario del empleado "wrk-555" en el centro "ctr-valencia-2" permanece inalterado

  @security @mitigation
  Scenario Outline: Bloqueo de peticiones CRUD directas con parámetros cruzados de centro
    Given una petición HTTP <metodo> enviada por un usuario del centro "ctr-madrid-1"
    When la petición referencia el recurso <recurso_objetivo> del centro "ctr-valencia-2"
    Then el sistema bloquea la mutación devolviendo <codigo_respuesta>
    And no se altera ningún dato en la base de datos

    Examples:
      | metodo   | recurso_objetivo              | codigo_respuesta |
      | "DELETE" | "/api/v1/costs/products/cos-9"| 404              |
      | "PUT"    | "/api/v1/costs/overhead/ovh-1"| 404              |
      | "GET"    | "/api/v1/costs/workers/wrk-55"| 404              |
```
