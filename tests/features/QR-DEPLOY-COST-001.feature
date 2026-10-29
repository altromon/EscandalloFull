@QR-DEPLOY-COST-001 @automated @regression
Feature: Mínimo Coste de Despliegue e Infraestructura y Eficiencia de Recursos
  Como operador de infraestructura y arquitecto de Escandallo Estética
  Quiero que la aplicación completa opere con una huella mínima de memoria y recursos
  Para permitir un coste de despliegue en producción inferior a 15 €/mes en VPS o contenedores

  Scenario: Verificación del consumo de memoria RAM en reposo del host unificado
    Given el proceso unificado Escandallo.Web está iniciado en el contenedor de ejecución
    When se monitoriza el consumo de memoria de trabajo (Working Set) del backend
    Then la memoria RAM en reposo permanece estrictamente por debajo de 150 MB
    And el servicio atiende las peticiones de salud "/healthz" en menos de 50 ms

  Scenario Outline: Eficiencia de cómputo bajo carga concurrente de peticiones de escandallo
    Given el sistema recibe un lote de <concurrencia> cálculos simultáneos de escandallo
    When el motor de costes procesa todos los tratamientos del centro
    Then el tiempo medio de respuesta por cálculo es inferior a <latencia_maxima_ms> ms
    And el incremento de memoria RAM temporal no excede los <umbral_memoria_mb> MB

    Examples:
      | concurrencia | latencia_maxima_ms | umbral_memoria_mb |
      | 10           | 100                | 20                |
      | 25           | 250                | 40                |
