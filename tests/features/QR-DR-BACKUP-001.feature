@QR-DR-BACKUP-001 @automated @regression
Feature: Persistencia, Copias de Seguridad y Recuperación ante Desastres (Disaster Recovery)
  Como responsable de operaciones e infraestructura de Escandallo Estética
  Quiero garantizar la replicación continua y respaldo automatizado de datos contables
  Para asegurar un RPO menor o igual a 1 hora y un RTO menor o igual a 2 horas

  Scenario: Verificación de persistencia transaccional y respaldo automatizado
    Given existen centros de estética con escandallos, factores de coste y registros de auditoría
    When se ejecuta el procedimiento programado de copia de seguridad en caliente
    Then se genera un volcado íntegro de la base de datos sellado criptográficamente
    And el volcado contiene las tablas de escandallos, costes, usuarios y logs históricos sin corrupción

  Scenario Outline: Simulación de recuperación ante desastre con cumplimiento de objetivos RPO y RTO
    Given un incidente de fallo de infraestructura con <tipo_desastre>
    When se activa el protocolo automatizado de restauración sobre una nueva instancia
    Then el tiempo total de restauración del servicio es inferior a <rto_limite_horas> horas
    And la pérdida máxima de datos transaccionales es inferior a <rpo_limite_horas> horas

    Examples:
      | tipo_desastre             | rto_limite_horas | rpo_limite_horas |
      | "caida_nodo_vps"          | 2                | 1                |
      | "corrupcion_almacenamiento"| 2               | 1                |
