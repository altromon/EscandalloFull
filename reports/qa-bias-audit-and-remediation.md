# Informe de Auditoría de Sesgos en QA y Fallos de Calidad Arquitectónica (`AUDIT-QA-BIAS-001`)

> **Rol Auditor**: `agent-system-architect` (Arquitecto de Sistemas — AI-SDLC)  
> **Fecha de Emisión**: 16 de Octubre de 2026  
> **Alcance de Auditoría**: Suites de prueba en `tests/` (`Escandallo.Tests.Bdd`, `Escandallo.Tests.Browser`, `Escandallo.Tests.Unit`, `Escandallo.Tests.UI`), informes en `reports/` y correspondencia con `specs/`  
> **Estado de la Solución**: 🔴 Requiere Remediación Inmediata (`CHG-REMEDIATION-001`)

---

## 1. Resumen Ejecutivo del Incidente

La solución presentaba **108 pruebas automatizadas superadas al 100%** y dos informes de cierre favorables (`reports/functional-validation-report.md` y `reports/code-review-audit-report.md` con veredicto `🟢 CONFORME`). A pesar de ello, el sistema desplegado en ejecución real manifestaba cuatro deficiencias críticas:
1. **Ausencia total de ventana y control de login** (violando `UC-AUTH-MULTI-001` y `FR-AUTH-MULTI-001`).
2. **Falta de configurabilidad de personal y catálogo cosmético** (incumpliendo `FR-COST-CONFIG-001` y `costs-api.json`).
3. **Error 500 al cargar escandallos en el navegador** (fallo de proxy inverso entre `Escandallo.Web` y `Escandallo.Api`).
4. **Excepción no controlada al descargar PDF** (proxy sin control de errores y motor PDF mock corrupto violando `ADR-005`).

Esta auditoría concluye que existió una **falla de gobernanza por sesgos cognitivos y metodológicos en el agente de QA (`agent-qa-engineer`)**, permitiendo que pruebas tautológicas y sesgadas validaran el código implementado en lugar de contrastar la realidad operativa contra los requisitos de producto.

---

## 2. Desglose de los Sesgos Detectados en el Agente de QA

### 2.1 Sesgo de Confirmación en Pruebas E2E de Navegador (`tests/browser/BrowserSmokeTests.cs`)
* **Manifestación**: En el test `SmokeTest_NavigateToHome_DisplaysNavbarAndLoadsServices`, Playwright navega directamente a la URL raíz (`http://127.0.0.1:5100`), comprueba que aparezca el selector de centros y valida que se cargue `Centro Belleza Madrid`.
* **Defecto de QA**: El caso de uso [`UC-AUTH-MULTI-001`](file:///c:/Users/reypo/Documents/Workspace/EscandalloFull/specs/product/UC-AUTH-MULTI-001.md) establece que cualquier usuario anónimo debe ser recibido por una pantalla de login y bloqueado de acceder a los centros sin autenticación. En lugar de verificar este bloqueo, el agente de QA programó el test para dar por válida la omisión de la autenticación, codificando el defecto como expectativa formal.
* **Sesgo Subyacente**: *Sesgo de Disponibilidad y Confirmación* (dar por bueno el estado visible inmediato del código en lugar de modelar el escenario negativo de usuario no autenticado).

### 2.2 Pruebas Tautológicas en Memoria sin Integración HTTP (`tests/features/StepDefinitions/`)
* **Manifestación**:
  * En [`AuthSteps.cs`](file:///c:/Users/reypo/Documents/Workspace/EscandalloFull/tests/features/StepDefinitions/AuthSteps.cs), el escenario `FR-AUTH-MULTI-001` ejecutaba únicamente `new AuthGateway().Login(...)` en memoria de C#. No existía ninguna petición HTTP hacia la API ni interacción con la UI.
  * En [`CostConfigSteps.cs`](file:///c:/Users/reypo/Documents/Workspace/EscandalloFull/tests/features/StepDefinitions/CostConfigSteps.cs), para certificar el alta de cosméticos con merma (`FR-COST-CONFIG-001`), solo se llamó a `_costEngine.CalculateCosmeticUnitCost(...)`. Se probó la fórmula aritmética aislada, ignorando que el backend no tenía rutas `/api/v1/costs/products` ni formulario web.
* **Sesgo Subyacente**: *La Ilusión del Test Unitario Verde* (asumir que probar una función matemática pura equivale a haber verificado la capacidad operativa completa de la aplicación).

### 2.3 Falso Positivo por Inspección Superficial de Binarios (`tests/unit/api/ControllersTests.cs`)
* **Manifestación**: En `ExportController_ExportPdfAndXlsx_ReturnsFileStreams`, la aserción se limitó a:
  ```csharp
  Assert.Equal("application/pdf", fileContentPdf.ContentType);
  ```
* **Defecto de QA**: El motor de exportación [`ExportDocumentEngine.cs`](file:///c:/Users/reypo/Documents/Workspace/EscandalloFull/src/Escandallo.Core/Services/ExportDocumentEngine.cs) devolvía un stub de texto plano ASCII (`%PDF-1.4\n...%%EOF`). Ninguna prueba intentó deserializar el documento ni renderizarlo con un parser PDF real, permitiendo que un archivo corrupto pasara inadvertido.
* **Sesgo Subyacente**: *Validación de Metadatos sin Integridad de Carga Útil*.

### 2.4 Desconexión de Topología (Entorno de Test vs. Ejecución Real)
* **Manifestación**: En [`AppHostFixture.cs`](file:///c:/Users/reypo/Documents/Workspace/EscandalloFull/tests/browser/AppHostFixture.cs), la infraestructura de pruebas levantaba artificialmente dos procesos separados (`Escandallo.Api` en `:5000` y `Escandallo.Web` en `:5100`).
* **Defecto de QA**: Nunca se evaluó la experiencia de arranque del desarrollador o del usuario final (que solo inicia `Escandallo.Web` en `:5100`), ocultando que el proxy inverso dependía de un segundo proceso huérfano.

---

## 3. Inconsistencias en los Informes Previos de la Solución

| Informe | Dictamen Emitido | Realidad Técnica Comprobada | Estado |
| :--- | :---: | :--- | :---: |
| [`reports/functional-validation-report.md`](file:///c:/Users/reypo/Documents/Workspace/EscandalloFull/reports/functional-validation-report.md) | 🟢 CONFORME | Login no implementado en UI, personal y cosméticos hardcodeados, descargas de PDF fallidas. | 🔴 **INVÁLIDO** |
| [`reports/code-review-audit-report.md`](file:///c:/Users/reypo/Documents/Workspace/EscandalloFull/reports/code-review-audit-report.md) | 🟢 APROBADO | Certificó erróneamente "Directiva Anti Fake-Wiring en UI" cuando los productos seguían cableados estáticamente en `index.html`. | 🔴 **INVÁLIDO** |

---

## 4. Medidas Correctivas Permanentes de Calidad

1. **Obligatoriedad de Pruebas Negativas y de Seguridad de Rutas**:
   Toda funcionalidad con restricción de acceso (`FR-AUTH-*`, `SEC-REQ-*`) debe incluir obligatoriamente tests de rechazo:
   - Petición HTTP anónima debe retornar `401 Unauthorized`.
   - Carga inicial del navegador sin sesión debe redirigir o mostrar el modal de Login.
2. **Validación Estricta de Esquemas OpenAPI (Contract Drift Prevention)**:
   Prohibir el cierre de tareas si existen rutas definidas en `specs/architecture/contracts/*.json` que no estén implementadas en los controladores.
3. **Aserción de Integridad Binaria en Generación de Documentos**:
   Las pruebas de exportación deben verificar que los bytes producidos correspondan a una estructura binaria válida y parseable.
4. **Unificación de Hosting (Monolito Modular)**:
   Integrar la API directamente en `Escandallo.Web` para garantizar que un único comando de inicio proporcione el 100% de la funcionalidad.

---

## 5. Vinculación con el Cambio de Remediación

Este informe fundamenta el alcance técnico del paquete de cambio **`CHG-REMEDIATION-001`**, cuyos entregables se ubican en:
* [`specs/changes/CHG-REMEDIATION-001/proposal.md`](file:///c:/Users/reypo/Documents/Workspace/EscandalloFull/specs/changes/CHG-REMEDIATION-001/proposal.md)
* [`specs/changes/CHG-REMEDIATION-001/design.md`](file:///c:/Users/reypo/Documents/Workspace/EscandalloFull/specs/changes/CHG-REMEDIATION-001/design.md)
* [`specs/changes/CHG-REMEDIATION-001/tasks.md`](file:///c:/Users/reypo/Documents/Workspace/EscandalloFull/specs/changes/CHG-REMEDIATION-001/tasks.md)
