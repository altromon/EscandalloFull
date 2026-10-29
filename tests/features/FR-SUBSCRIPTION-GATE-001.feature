# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/FR-SUBSCRIPTION-GATE-001.md
# ID Requerimiento: FR-SUBSCRIPTION-GATE-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-SUBSCRIPTION-GATE-001 @automated @regression
Feature: Control de Cuota Freemium y Pasarela de Suscripción Stripe
  Como gestor de un centro de estética o plataforma
  Quiero aplicar el límite freemium e integrar suscripciones Stripe
  Para monetizar la aplicación y desbloquear capacidades ilimitadas a centros suscritos

  Scenario: Bloqueo de creación del tercer escandallo en plan gratuito
    Given el centro "Luz y Belleza" tiene suscripción "Free" y cuenta con 2 escandallos activos
    When el gestor intenta dar de alta un tercer escandallo
    Then el sistema bloquea la creación con el error de cuota excedida
    And presenta la oferta de suscripción Premium mensual y anual

  Scenario Outline: Autorización de creación de escandallos según plan y número actual
    Given un centro con plan <plan_actual> y <escandallos_actuales> escandallos registrados
    When el usuario solicita crear un nuevo escandallo
    Then el sistema responde con <accion_sistema>

    Examples:
      | plan_actual | escandallos_actuales | accion_sistema             |
      | "Free"      | 0                    | "permitir_creacion"        |
      | "Free"      | 1                    | "permitir_creacion"        |
      | "Free"      | 2                    | "mostrar_modal_suscripcion"|
      | "Premium"   | 2                    | "permitir_creacion"        |
      | "Premium"   | 50                   | "permitir_creacion"        |

  Scenario: Bloqueo de duplicación directa en plan Free cuando ya existen 2 escandallos activos
    Given un centro con plan "Free" y 2 escandallos activos
    When el usuario solicita duplicar un servicio existente como activo
    Then el sistema bloquea la duplicación con código "QUOTA_EXCEEDED"
    And muestra el modal de suscripción Premium

  Scenario: Bloqueo de activación de escandallo archivado al alcanzar el límite en plan Free
    Given un centro con plan "Free" que ya tiene 2 escandallos activos y 1 inactivo
    When el usuario intenta activar el escandallo inactivo ("is_active = true")
    Then el sistema bloquea la activación con código "QUOTA_EXCEEDED"
    And exige pasar a Premium o desactivar previamente otro escandallo activo
