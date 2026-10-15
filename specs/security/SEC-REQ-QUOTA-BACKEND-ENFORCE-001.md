---
id: "SEC-REQ-QUOTA-BACKEND-ENFORCE-001"
type: "security-requirement"
title: "Validación Atómica Transaccional de Cuota Freemium en Capa de Persistencia"
status: "draft"
version: "1.0.0"
security-domain: "authorization"
mitigates-abuse-case:
  - "ABUSE-FREEMIUM-BYPASS-001"
enforced-in-enclave: "SEC-ENC-SERVICE-CORE-001"
compliance-references:
  - "OWASP-ASVS-4.0.3-V4.1"
  - "OWASP-ASVS-4.0.3-V13.3"
acceptance-format: "gherkin"
cucumber-tags:
  - "@SEC-REQ-QUOTA-BACKEND-ENFORCE-001"
  - "@security"
  - "@mitigation"
---

# Security Requirement: Control Atómico de Cuota Freemium (`SEC-REQ-QUOTA-BACKEND-ENFORCE-001`)

La creación y activación de servicios y escandallos en centros bajo el plan `Free` (`BR-FREEMIUM-LIMIT-001`, `UC-SUBSCRIPTION-GATE-001`) debe validarse a nivel de capa de datos mediante una transacción con bloqueo pesimista o nivel de aislamiento `Serializable` / `Repeatable Read` en PostgreSQL, impidiendo que peticiones concurrentes de carrera (Race Condition / TOCTOU) puedan sobrepasar el umbral de 2 escandallos activos por centro sin contar con una suscripción Premium activa en Stripe.

```gherkin
@SEC-REQ-QUOTA-BACKEND-ENFORCE-001 @security @mitigation
Feature: Validación Atómica Transaccional de Cuota Freemium
  Como subsistema de monetización y autorización de Escandallo Estética
  Quiero hacer cumplir de forma atómica el límite de 2 escandallos por centro gratuito
  Para neutralizar la evasión de cuota por condiciones de carrera concurrentes (TOCTOU)

  @security @mitigation
  Scenario: Rechazo de creación del tercer escandallo en centro con plan gratuito
    Given un centro "Estética Radiante" con suscripción "Free" y 2 escandallos activos existentes
    When el usuario gestor solicita crear un nuevo escandallo para el servicio "Lifting Facial"
    Then el backend intercepta la operación en la transacción de base de datos
    And rechaza la creación devolviendo código de error HTTP 403 Forbidden
    And la respuesta incluye el código de negocio "QUOTA_EXCEEDED"
    And el conteo total de escandallos activos en el centro se mantiene estrictamente en 2

  @security @mitigation
  Scenario: Neutralización de ataques de carrera concurrente (Race Condition TOCTOU)
    Given un centro en plan gratuito que tiene actualmente 1 escandallo activo
    When un atacante envía simultáneamente 10 peticiones concurrentes de creación de escandallos
    Then el aislamiento transaccional serializa las verificaciones de cuota en base de datos
    And exactamente 1 petición es autorizada y persistida con éxito
    And las 9 peticiones restantes son rechazadas con error "QUOTA_EXCEEDED"
    And el centro queda con exactamente 2 escandallos activos sin superar la cuota
```
