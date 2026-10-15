---
id: "SEC-REQ-DATA-CENSOR-GUEST-001"
type: "security-requirement"
title: "Filtrado en Origen (Backend) y Censura de Datos Salariales y Estructurales para Invitados"
status: "draft"
version: "1.0.0"
security-domain: "data-protection"
mitigates-abuse-case:
  - "ABUSE-GUEST-DATA-LEAK-001"
enforced-in-enclave: "SEC-ENC-SERVICE-CORE-001"
compliance-references:
  - "OWASP-ASVS-4.0.3-V4.1"
  - "OWASP-ASVS-4.0.3-V8.3"
  - "GDPR-Art32"
acceptance-format: "gherkin"
cucumber-tags:
  - "@SEC-REQ-DATA-CENSOR-GUEST-001"
  - "@security"
  - "@mitigation"
---

# Security Requirement: Censura de Datos para Invitados (`SEC-REQ-DATA-CENSOR-GUEST-001`)

La capa de servicios y controladores de la API debe implementar proyección y censura estricta en origen (backend) mediante Data Transfer Objects (DTOs) específicos para el rol `ACT-GUEST-001`, garantizando que bajo ninguna circunstancia se incluyan campos salariales de trabajadores ($C_{personal}$), costes mensuales de local ($C_{local}$) o gastos de suministros en los payloads JSON enviados hacia un cliente autenticado como invitado, prohibiendo expresamente la delegación de este filtrado a la interfaz de usuario.

```gherkin
@SEC-REQ-DATA-CENSOR-GUEST-001 @security @mitigation
Feature: Filtrado en Origen y Censura de Datos Confidenciales para Invitados
  Como núcleo de servicios de Escandallo Estética
  Quiero sanitizar los payloads de respuesta en backend para usuarios con rol invitado
  Para cumplir la regla BR-GUEST-CENSOR-001 y mitigar la divulgación de salarios y alquileres

  @security @mitigation
  Scenario: Solicitud de desglose de servicio por un colaborador invitado
    Given un usuario autenticado con rol "ACT-GUEST-001" en el centro "Estética Bellavida"
    When solicita el desglose de costes del servicio "Mesodermolifting" vía API REST
    Then el backend genera un DTO sanitizado para invitado
    And el payload JSON resultante contiene las cantidades y nombres de los productos cosméticos
    And el payload JSON no contiene los campos "worker_hourly_wage", "worker_monthly_cost" ni "facility_rent"
    And los valores numéricos de salarios y alquiler no viajan en el cuerpo de la respuesta HTTP

  @security @mitigation
  Scenario Outline: Inspección de campos sensibles según el rol del usuario autenticado
    Given un usuario autenticado con rol <rol_usuario>
    When consulta la lista de escandallos del centro
    Then la visibilidad del desglose de mano de obra y alquiler en el JSON es <visibilidad_json>

    Examples:
      | rol_usuario     | visibilidad_json       |
      | "ACT-USER-001"  | "completa_sin_censura" |
      | "ACT-GUEST-001" | "estrictamente_nula"   |
```
