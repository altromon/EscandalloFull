@QR-AVAILABILITY-001 @automated @regression
Feature: Alta Disponibilidad 24/7, Resiliencia y Health Checks
  Como operador de infraestructura y usuario gestor de Escandallo Estética
  Quiero que la aplicación proporcione sondas de vitalidad y preparación y tolere fallos transitorios
  Para garantizar una disponibilidad del 99.9% anual (24/7) y recuperación autónoma sin intervención manual

  # 1. NOMINAL CASE (Happy Path - Liveness Probe)
  Scenario: Verificación de vitalidad básica del servicio (Liveness Probe)
    Given la aplicación backend ASP.NET Core está en ejecución
    When el orquestador o monitor de disponibilidad consulta el endpoint "/healthz"
    Then el sistema responde con código HTTP 200 OK
    And el estado devuelto en el payload es "Healthy"
    And el tiempo de respuesta es inferior a 200 ms

  # 2. READINESS PROBE & SUBSYSTEM CHECKS
  Scenario Outline: Sonda de preparación integral según estado de subsistemas críticos (Readiness Probe)
    Given el estado de la conexión a base de datos PostgreSQL es <estado_bd>
    And el uso de memoria RAM del proceso es <uso_memoria>
    When se consulta el endpoint de preparación "/readyz"
    Then la respuesta del sistema tiene código HTTP <codigo_http>
    And el estado general reportado es <estado_global>

    Examples:
      | estado_bd    | uso_memoria | codigo_http | estado_global |
      | "conectado"  | "85 MB"     | 200         | "Healthy"     |
      | "conectado"  | "130 MB"    | 200         | "Healthy"     |
      | "desconectado"| "90 MB"    | 503         | "Unhealthy"   |

  # 3. RESILIENCE: ISOLATION FROM EXTERNAL FAILURES (24/7 Continuity)
  Scenario: Disponibilidad continua 24/7 del núcleo de escandallos ante caída de pasarela externa Stripe
    Given la pasarela externa de pagos Stripe experimenta una caída o timeout
    When un usuario gestor accede al centro para calcular o editar escandallos de servicio
    Then el motor de escandallos y la base de datos responden normalmente
    And todas las operaciones de cálculo de costes y matriz de rentabilidad permanecen disponibles
    And el sistema aísla el fallo de la pasarela sin provocar la caída del servidor
