---
tasks:
  - id: "TSK-REM-001"
    title: "Unificación de Host Monolítico Modular en Escandallo.Web"
    complexity: "MEDIUM"
    risk-level: "MEDIUM"
    autonomy-mode: "AUTONOMOUS"
    assigned-to: "agent-developer"
    status: "COMPLETED"
    verification:
      method: "COMMAND_VERIFICATION"
      command-or-criteria: "dotnet build src/Escandallo.Web/Escandallo.Web.csproj && curl http://127.0.0.1:5100/healthz"

  - id: "TSK-REM-002"
    title: "Implementación de Modal de Login, Auth Guard y Validación de Sesión"
    complexity: "MEDIUM"
    risk-level: "HIGH"
    autonomy-mode: "AUTONOMOUS"
    assigned-to: "agent-developer"
    status: "COMPLETED"
    verification:
      method: "COMMAND_VERIFICATION"
      command-or-criteria: "dotnet test tests/Escandallo.Tests.UI --filter Category=AuthSecurity"

  - id: "TSK-REM-003"
    title: "CRUD de Personal y Cosméticos en Backend y UI según Contrato costs-api.json"
    complexity: "MEDIUM"
    risk-level: "MEDIUM"
    autonomy-mode: "AUTONOMOUS"
    assigned-to: "agent-developer"
    status: "COMPLETED"
    verification:
      method: "COMMAND_VERIFICATION"
      command-or-criteria: "dotnet test tests/unit --filter Category=FR-COST-CONFIG-001"

  - id: "TSK-REM-004"
    title: "Motor de Exportación PDF Real con QuestPDF Sandbox (ADR-005)"
    complexity: "MEDIUM"
    risk-level: "MEDIUM"
    autonomy-mode: "AUTONOMOUS"
    assigned-to: "agent-developer"
    status: "COMPLETED"
    verification:
      method: "COMMAND_VERIFICATION"
      command-or-criteria: "dotnet test tests/unit --filter Category=SEC-REQ-EXPORT-SANITIZATION-001"

  - id: "TSK-REM-005"
    title: "Remediación de la Suite de Pruebas y Eliminación de Sesgos de QA"
    complexity: "MEDIUM"
    risk-level: "MEDIUM"
    autonomy-mode: "AUTONOMOUS"
    assigned-to: "agent-developer"
    status: "COMPLETED"
    verification:
      method: "COMMAND_VERIFICATION"
      command-or-criteria: "dotnet test"
---

# Plan de Tareas de Remediación: CHG-REMEDIATION-001

## Descripción General
Este plan desglosa la ejecución atómica para solventar las 4 deficiencias reportadas por el usuario y neutralizar los sesgos de confirmación detectados en la suite de pruebas del agente de QA.

---

### Tarea 1: TSK-REM-001 — Unificación de Host Monolítico Modular en `Escandallo.Web`
* **Objetivo**: Resolver de raíz el fallo 500 al cargar escandallos y el split-brain de repositorios en memoria.
* **Acciones**:
  1. Añadir referencia a los controladores de `Escandallo.Api` o registrarlos directamente en `Escandallo.Web/Program.cs` con `builder.Services.AddControllers()` y `app.MapControllers()`.
  2. Registrar todos los servicios de dominio (`EscandalloRepository`, `CostConfigurationRepository`, `AuthGateway`, `SubscriptionManager`, `ExportDocumentEngine`, `DemoManager`) como `Singleton` en el host web.
  3. Eliminar el proxy inverso frágil `ProxyApiCall` hacia el puerto `:5000`.
* **Verificación**: `dotnet build src/Escandallo.Web` y verificación de respuesta en `/api/v1/healthz`.

---

### Tarea 2: TSK-REM-002 — Modal de Login, Auth Guard y Validación de Sesión
* **Objetivo**: Garantizar que nadie pueda acceder ni alternar entre centros sin previa autenticación y autorización (`UC-AUTH-MULTI-001`, `FR-AUTH-MULTI-001`).
* **Acciones**:
  1. Diseñar en `index.html` el modal de Login no cerrable para usuarios anónimos con inputs de email y contraseña.
  2. En `app.js`, verificar la presencia de token al arrancar; si no existe, bloquear la interfaz y solicitar credenciales.
  3. Tras login exitoso contra `POST /api/v1/auth/login`, poblar el selector de centros exclusivamente con los centros autorizados del usuario.
  4. Enviar `Authorization: Bearer <token>` y `X-Center-Id` en todas las llamadas API.
  5. En los controladores de backend, verificar que la sesión corresponda a un centro asignado al usuario.
* **Verificación**: Pruebas de integración que comprueben el rechazo con 401 a peticiones anónimas y acceso denegado a centros ajenos.

---

### Tarea 3: TSK-REM-003 — CRUD de Personal y Cosméticos en Backend y UI
* **Objetivo**: Completar la parametrización de los pilares 2 y 3 de la Excel (`BR-COST-FORMULA-001`, `FR-COST-CONFIG-001`).
* **Acciones**:
  1. Implementar en `CostConfigurationRepository` la persistencia y consulta de trabajadores y productos cosméticos por centro.
  2. Implementar en `CostsController`:
     - `GET /api/v1/costs/workers` y `POST /api/v1/costs/workers`.
     - `GET /api/v1/costs/products` y `POST /api/v1/costs/products`.
     - `PUT /api/v1/costs/products/{id}`.
  3. Crear en `index.html` y `app.js` las pestañas de "Gastos de Salón", "Personal" y "Catálogo de Cosméticos".
  4. Conectar el modal de "Nuevo Tratamiento" para que los desplegables de categoría laboral y producto cosmético se lean dinámicamente de la configuración.
* **Verificación**: Tests de endpoints y cálculo dinámico de costes efectivos con merma.

---

### Tarea 4: TSK-REM-004 — Motor de Exportación PDF Real con QuestPDF
* **Objetivo**: Erradicar el archivo falso/corrupto y las excepciones al descargar el informe (`ADR-005`, `FR-EXPORT-EXEC-001`).
* **Acciones**:
  1. Añadir el paquete `QuestPDF` a `Escandallo.Core.csproj`.
  2. Implementar en `ExportDocumentEngine.ExportPdf()` la generación de un documento PDF binario real y formateado.
  3. En `app.js`, gestionar la descarga con `fetch` a blob para descargar el archivo de forma segura con nombre predeterminado y capturar posibles errores con alerta visual.
* **Verificación**: Test que lea los bytes generados y valide que inicien con `%PDF-` y contengan estructura PDF ejecutable.

---

### Tarea 5: TSK-REM-005 — Remediación de la Suite de Pruebas (Erradicación de Sesgos)
* **Objetivo**: Garantizar que los tests prueben la realidad operativa del sistema y no mocks tautológicos.
* **Acciones**:
  1. Actualizar `BrowserSmokeTests.cs` para validar que la primera pantalla sea el Login y que el acceso a los centros requiera autenticarse.
  2. Añadir tests negativos en `ControllersTests.cs` comprobando bloqueos por falta de token.
  3. Probar la integración completa con `dotnet test`.
* **Verificación**: `dotnet test` pasa al 100% con suites de navegador, BDD, unitarias y de integración.
