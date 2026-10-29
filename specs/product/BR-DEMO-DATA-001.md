---
id: "BR-DEMO-DATA-001"
type: "business-rule"
title: "Aislamiento de Datos de Ejemplo y Restricción a Modo Demo"
status: "draft"
version: "1.0.0"
enforcement-level: "strict-invariant"
---

# Business Rule: Aislamiento de Datos de Ejemplo y Restricción a Modo Demo (`BR-DEMO-DATA-001`)

## 1. Description and Principles
1. **Estado Inicial Limpio por Defecto (*Clean Slate*)**:
   - Todo nuevo centro de estética creado por un cliente/inquilino se inicializa estrictamente vacío:
     - 0 trabajadores o categorías salariales.
     - 0 importes de gastos generales (local, consumos, consumibles fijados a 0).
     - 0 productos cosméticos en catálogo.
     - 0 servicios o escandallos formulados.
2. **Prohibición de Precarga en Producción**:
   - Queda estrictamente prohibido transferir, exportar o sembrar por defecto en la base de datos de producción los nombres, precios, salarios o servicios de la hoja de cálculo de referencia (`Cleasing Milk`, `Mesodermolifting`, salarios de ejemplo, etc.).
3. **Activación Exclusiva bajo Demanda (Modo Demo / Sandbox)**:
   - Los datos de ejemplo de la Excel únicamente podrán cargarse si el usuario gestor (`ACT-USER-001`) pulsa conscientemente la opción *"Activar Modo Demo"* o *"Cargar datos de ejemplo"*.
4. **Reversibilidad y Limpieza**:
   - El sistema debe ofrecer la opción de *"Limpiar datos de demo"* para eliminar en bloque todos los registros de prueba y devolver el centro a su estado operativo limpio sin afectar a configuraciones de la cuenta.
