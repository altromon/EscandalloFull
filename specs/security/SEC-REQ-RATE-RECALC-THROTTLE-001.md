---
id: "SEC-REQ-RATE-RECALC-THROTTLE-001"
type: "security-requirement"
title: "Limitación de Tasa (Rate Limiting) y Encolamiento Asíncrono de Recálculo en Cascada"
status: "draft"
version: "1.0.0"
security-domain: "input-validation"
mitigates-abuse-case:
  - "ABUSE-RECALC-DOS-001"
enforced-in-enclave: "SEC-ENC-SERVICE-CORE-001"
compliance-references:
  - "OWASP-ASVS-4.0.3-V11.1"
  - "OWASP-ASVS-4.0.3-V13.2"
acceptance-format: "gherkin"
cucumber-tags:
  - "@SEC-REQ-RATE-RECALC-THROTTLE-001"
  - "@automated"
  - "@regression"
---

# Security Requirement: Rate Limiting en Recálculo en Cascada (`SEC-REQ-RATE-RECALC-THROTTLE-001`)

El motor de recálculo automático (`UC-HISTORY-RECALC-001`) debe aplicar una política de limitación de tasa (*rate limiting*) a nivel de centro (máximo 10 mutaciones de coste por minuto por usuario) y desacoplar los recálculos masivos de más de 10 servicios mediante un canal de ejecución en segundo plano (*background worker / channel*) con *debouncing* de 500 ms, evitando el bloqueo síncrono del hilo HTTP y preservando el límite de consumo de memoria RAM (<150 MB) establecido en `QR-DEPLOY-COST-001`.

```gherkin
@SEC-REQ-RATE-RECALC-THROTTLE-001 @security @mitigation
Feature: Limitación de Tasa y Control de Concurrencia en Recálculos en Cascada
  Como núcleo de servicios de Escandallo Estética
  Quiero estrangular ráfagas de recálculos y procesar en segundo plano las actualizaciones en bloque
  Para neutralizar ataques de denegación de servicio (DoS) y proteger la memoria del contenedor en VPS económico

  @security @mitigation
  Scenario: Estrangulamiento de ráfaga de mutaciones de coste que excede el umbral permitido
    Given un usuario gestor en el centro "Estética Madrid"
    When el usuario emite 15 peticiones consecutivas de cambio de precio en un intervalo de 3 segundos
    Then las primeras 10 peticiones son procesadas y encoladas correctamente
    And las 5 peticiones siguientes son rechazadas con código HTTP 429 Too Many Requests
    And la respuesta incluye la cabecera "Retry-After" con el tiempo de espera recomendado
    And el uso de memoria RAM del servidor permanece por debajo de 150 MB

  @security @mitigation
  Scenario: Desacoplamiento asíncrono con debouncing para productos con alta dependencia
    Given un producto cosmético "Serum Ácido Hialurónico" utilizado en 40 servicios diferentes
    When se modifica el precio de compra del producto
    Then el sistema responde inmediatamente al cliente HTTP con 202 Accepted
    And encola el recálculo en segundo plano aplicando un debouncing de 500 ms
    And los 40 servicios se recalculan secuencialmente sin saturar el pool de conexiones de base de datos
```
