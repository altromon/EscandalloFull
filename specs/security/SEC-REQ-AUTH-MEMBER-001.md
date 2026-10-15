---
id: "SEC-REQ-AUTH-MEMBER-001"
type: "security-requirement"
title: "Validación Criptográfica de Sesión y Verificación de Membresía Multicentro"
status: "draft"
version: "1.0.0"
security-domain: "authentication"
mitigates-abuse-case:
  - "ABUSE-AUTH-SPOOF-001"
enforced-in-enclave: "SEC-ENC-AUTH-GATEWAY-001"
compliance-references:
  - "OWASP-ASVS-4.0.3-V2.1"
  - "OWASP-ASVS-4.0.3-V3.2"
acceptance-format: "gherkin"
cucumber-tags:
  - "@SEC-REQ-AUTH-MEMBER-001"
  - "@security"
  - "@mitigation"
---

# Security Requirement: Validación de Membresía Multicentro (`SEC-REQ-AUTH-MEMBER-001`)

El sistema debe verificar en cada petición autenticada y en el endpoint de selección de centro que el identificador de centro solicitado (`CenterId`) pertenece explícitamente a la lista de centros autorizados del usuario y a su `TenantId`, invalidando de forma inmediata cualquier intento de salto de inquilino o suplantación de centro ajeno.

```gherkin
@SEC-REQ-AUTH-MEMBER-001 @security @mitigation
Feature: Validación Criptográfica de Sesión y Verificación de Membresía Multicentro
  Como subsistema de seguridad perimetral de Escandallo Estética
  Quiero rechazar intentos de selección o acceso a centros ajenos al usuario autenticado
  Para mitigar la suplantación de identidad y el salto entre inquilinos (Tenant Hopping)

  @security @mitigation
  Scenario: Rechazo de selección de centro no asignado a la membresía del usuario
    Given un usuario autenticado con identificador "usr-100" perteneciente al inquilino "ten-alpha"
    And el usuario tiene membresía exclusivamente en los centros:
      | center_id      | center_name        |
      | "ctr-madrid-1" | "Estética Centro"  |
    When el usuario envía una petición para seleccionar el centro activo con id "ctr-barcelona-9"
    Then el gateway de autenticación bloquea la petición
    And emite una respuesta HTTP 403 Forbidden con código de seguridad "TENANT_MEMBERSHIP_VIOLATION"
    And registra una alerta de auditoría de seguridad con la IP, usuario y centro ajeno solicitado
    And el centro activo de la sesión del usuario no sufre modificaciones

  @security @mitigation
  Scenario Outline: Intento de inyección de identificadores adulterados en cabeceras o token
    Given una petición entrante a la API con token de sesión de "usr-100"
    When el atacante adultera <origen_adulteracion> con el valor <valor_manipulado>
    Then el middleware de seguridad rechaza la petición con código HTTP <codigo_http>
    And no se devuelven datos de escandallos ni centros

    Examples:
      | origen_adulteracion      | valor_manipulado  | codigo_http |
      | "cabecera X-Center-Id"   | "ctr-ajeno-999"   | 403         |
      | "cabecera X-Tenant-Id"   | "ten-competidor"  | 403         |
      | "claim center_id en JWT" | "firma_invalida"  | 401         |
```
