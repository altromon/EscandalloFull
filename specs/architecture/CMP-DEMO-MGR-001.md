---
id: CMP-DEMO-MGR-001
type: component
title: "Gestor de Aislamiento y Ciclo de Vida de Modo Demo"
status: proposed
version: "1.0.0"
schema-version: "1.0"
level: 2
bounded-context: "Gestión de Entorno y Datos de Prueba"
parent-component: null
implementation-type: service
implements-use-cases:
  - UC-DEMO-MODE-001
satisfies-requirements:
  - FR-DEMO-MODE-001
  - SEC-REQ-DEMO-ISOLATION-001
hosted-in-enclave: SEC-ENC-SERVICE-CORE-001
interfaces:
  - name: "Demo Mode Life-Cycle API"
    protocol: "REST/HTTP"
    contract-spec: "specs/architecture/contracts/demo-api.json"
supersedes: null
superseded-by: null
---

# CMP-DEMO-MGR-001: Gestor de Aislamiento y Ciclo de Vida de Modo Demo

## 1. Propósito, Responsabilidad y Contexto Delimitado
Gestiona la separación estricta entre la contabilidad real de los salones de belleza y los conjuntos de datos demostrativos (`UC-DEMO-MODE-001`, `BR-DEMO-DATA-001`):
1. **Inicialización Limpia por Defecto (`FR-DEMO-MODE-001`)**: Todo centro recién creado se inicializa en blanco (0 trabajadores, 0 gastos generales, 0 cosméticos y 0 escandallos).
2. **Carga Opt-In con Etiquetado Inmutable (`SEC-REQ-DEMO-ISOLATION-001`)**: Permite únicamente bajo petición voluntaria del gestor la carga del catálogo representativo de ejemplo (cosméticos como "Cleasing Milk", gastos de apertura de 10.560 min/mes y servicios como "Mesodermolifting"). Todos los registros se etiquetan forzosamente con `is_demo = true`.
3. **Bloqueo de Contaminación Cruzada**: Bloquea la carga de datos demo si el centro ya contiene registros reales (`is_demo = false`) a menos que medie confirmación explícita.
4. **Purga Quirúrgica Transaccional (`SEC-REQ-DEMO-ISOLATION-001`)**: Ejecuta el borrado atómico en base de datos filtrando estrictamente por `is_demo = true` y `center_id = @ActiveCenterId`, garantizando que la totalidad de los datos ficticios se eliminen sin alterar un solo registro contable real del centro.

## 2. Diagrama Estructural y de Conectividad (arc42 Sec. 5 / NAF v4)
```mermaid
graph TD
    User[ACT-USER-001] -->|POST /api/v1/demo/load o purge| DemoMgr[CMP-DEMO-MGR-001]
    DemoMgr -->|Validación Estado del Centro| CenterStateValidator
    
    subgraph ExecutionLogic["Transacciones de Ciclo de Vida Demo"]
        CenterStateValidator -->|Acción: Cargar Demo| DemoSeeder[DemoDataSeeder: Marcado is_demo = true]
        CenterStateValidator -->|Acción: Purgar Demo| DemoPurger[DemoDataPurger: DELETE WHERE is_demo=true]
    end

    DemoSeeder -->|Inserción en Lote| DataAccess[CMP-DATA-ACCESS-001]
    DemoPurger -->|Purga Quirúrgica por CenterId| DataAccess
```

## 3. Contratos de Interfaz y Políticas de Ejecución
- **Mecanismo de Ejecución**: Servicio de aplicación en ASP.NET Core (.NET 10) que interactúa con la persistencia mediante transacciones atómicas de Entity Framework Core.
- **Tolerancia a Fallos y Rendimiento**: Tiempo de purga o carga inferior a 200 ms; operaciones totalmente idempotentes.

---

## 4. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición de componente de gestión y aislamiento de modo demo | CHG-ARCH-001 |
