# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/security/SEC-REQ-RATE-RECALC-THROTTLE-001.md
# ID Requerimiento: SEC-REQ-RATE-RECALC-THROTTLE-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

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
