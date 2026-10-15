---
id: "SEC-REQ-STRIPE-SIGNATURE-001"
type: "security-requirement"
title: "Verificación Criptográfica de Firma HMAC-SHA256 y Prevención de Repetición en Webhooks Stripe"
status: "draft"
version: "1.0.0"
security-domain: "cryptography"
mitigates-abuse-case:
  - "ABUSE-STRIPE-SPOOF-001"
enforced-in-enclave: "SEC-ENC-PAYMENT-INTEGRATION-001"
compliance-references:
  - "OWASP-ASVS-4.0.3-V6.2"
  - "OWASP-ASVS-4.0.3-V13.4"
acceptance-format: "gherkin"
cucumber-tags:
  - "@SEC-REQ-STRIPE-SIGNATURE-001"
  - "@security"
  - "@mitigation"
---

# Security Requirement: Verificación de Firma Stripe (`SEC-REQ-STRIPE-SIGNATURE-001`)

El sistema debe exigir y verificar de forma estricta la firma criptográfica HMAC-SHA256 contenida en la cabecera `Stripe-Signature` utilizando el secreto de webhook configurado en el servidor, validando una ventana temporal máxima de 300 segundos contra ataques de repetición (Replay Attacks) y descartando de manera idempotente eventos duplicados antes de alterar el estado de suscripción de un centro.

```gherkin
@SEC-REQ-STRIPE-SIGNATURE-001 @security @mitigation
Feature: Verificación Criptográfica de Firma HMAC-SHA256 en Webhooks Stripe
  Como enclave de integración de pagos de Escandallo Estética
  Quiero rechazar eventos de Stripe falsificados, sin firmar o repetidos
  Para evitar la activación fraudulenta de cuota Premium y eludir la pasarela de pago

  @security @mitigation
  Scenario: Rechazo de webhook de Stripe con firma criptográfica ausente o inválida
    Given el endpoint receptor de webhooks "/api/v1/billing/stripe-webhook" está activo
    When un atacante envía un evento "checkout.session.completed" con cabecera "Stripe-Signature" inválida o vacía
    Then el enclave de pagos rechaza inmediatamente la petición
    And responde con código de error HTTP 400 Bad Request
    And no ejecuta ninguna actualización en la suscripción del centro especificado
    And registra en logs de seguridad el intento de inyección de webhook espurio

  @security @mitigation
  Scenario Outline: Mitigación de ataques de repetición y eventos duplicados
    Given un evento válido de Stripe recibido con identificador <event_id> y timestamp <timestamp_diferencia>
    When el webhook es procesado por el enclave de pagos
    Then la validación criptográfica y temporal responde con <resultado_validacion>
    And el estado del centro en base de datos es <estado_resultante>

    Examples:
      | event_id       | timestamp_diferencia | resultado_validacion         | estado_resultante |
      | "evt_test_001" | "450 segundos atras" | "rechazado_por_antiguedad"   | "sin_cambios"     |
      | "evt_test_002" | "ya_procesado_antes" | "ignorado_por_idempotencia"  | "sin_cambios"     |
      | "evt_test_003" | "10 segundos atras"  | "aceptado_y_verificado"      | "actualizado"     |
```
