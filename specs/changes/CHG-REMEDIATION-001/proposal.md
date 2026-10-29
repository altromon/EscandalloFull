---
id: "CHG-REMEDIATION-001"
title: "Remediación Integral: Autenticación de Centros, Configuración de Costes, Host Unificado y Exportación PDF Real"
type: "proposal"
status: "proposed"
version: "1.0.0"
author: "agent-system-architect"
date: "2026-10-16"
---

# Propuesta de Cambio: Remediación Integral (`CHG-REMEDIATION-001`)

## 1. Justificación y Problema de Negocio
Durante la revisión operativa de la solución se detectaron 4 problemas críticos que impiden la puesta en marcha segura y correcta del producto:
1. **Acceso irrestricto sin autenticación**: No existe ventana de login en la UI ni control de acceso en la API (`UC-AUTH-MULTI-001`). Cualquier usuario puede alternar de centro en el menú desplegable sin credenciales.
2. **Factores de coste no configurables**: Los datos salariales de personal y el catálogo de productos cosméticos con merma están codificados en duro (`hardcoded`) en el código fuente, incumpliendo la regla canónica `BR-COST-FORMULA-001` y el requisito `FR-COST-CONFIG-001`.
3. **Error 500 al cargar escandallos**: La arquitectura de doble proceso (`Escandallo.Web` en `:5100` y `Escandallo.Api` en `:5000`) genera fallos de conexión por proxy inverso caído y desincronización de repositorios en memoria (*Split-Brain*).
4. **Excepción no controlada y PDF corrupto**: El botón de descarga de PDF falla con excepción de red no capturada en el proxy, y el generador documental emite un mock de texto plano ASCII que no es interpretado por ningún lector PDF (`ADR-005`).

## 2. Alcance de la Solución
1. **Pilar 1: Seguridad y Autenticación**:
   - Modal y vista de login obligatorio al abrir la aplicación web.
   - Restricción estricta del selector de centros a los autorizados en la sesión del usuario.
   - Validación de cabecera `Authorization: Bearer <token>` en las peticiones API.
2. **Pilar 2: Configuración Completa de Costes**:
   - Implementación de los endpoints definidos en `costs-api.json`:
     - `/api/v1/costs/workers` (categorías profesionales y costes de personal).
     - `/api/v1/costs/products` (catálogo de cosméticos con formatos y mermas).
   - Vistas de configuración en pestañas dentro de `Escandallo.Web`.
3. **Pilar 3: Hosting Unificado y Alta Disponibilidad**:
   - Unificación de endpoints de API directamente en el host ASP.NET Core de `Escandallo.Web`.
   - Eliminación del proxy inverso frágil y compartición consistente del estado en memoria.
4. **Pilar 4: Exportación Documental Conforme a ADR-005**:
   - Integración de `QuestPDF` (Community MIT) en `Escandallo.Core`.
   - Generación de informe PDF ejecutivo real con tabla de tratamientos, márgenes y semáforos.
   - Manejo robusto de descargas mediante `fetch` + `blob` con retroalimentación visual en caso de error.

## 3. Criterios de Aceptación Globales
- `CRIT-01`: Un usuario anónimo no puede acceder a ningún dato de centros sin autenticarse primero con credenciales válidas.
- `CRIT-02`: El gestor puede crear, editar y listar categorías de personal y productos cosméticos desde la UI, y estos alimentan dinámicamente el modal de creación de escandallos.
- `CRIT-03`: Al ejecutar únicamente `dotnet run --project src/Escandallo.Web`, todos los escandallos cargan fluidamente sin errores 500.
- `CRIT-04`: Al hacer clic en "Descargar PDF", se genera y descarga un archivo PDF binario válido que se abre correctamente en cualquier lector.
- `CRIT-05`: La suite de pruebas automatizadas incluye pruebas negativas y de integración real sin sesgos de confirmación.
