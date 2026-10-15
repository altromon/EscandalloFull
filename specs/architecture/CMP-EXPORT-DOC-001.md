---
id: CMP-EXPORT-DOC-001
type: component
title: "Sandbox de Exportación Ejecutiva Documental PDF y XLSX"
status: proposed
version: "1.0.0"
schema-version: "1.0"
level: 2
bounded-context: "Exportación Documental"
parent-component: null
implementation-type: service
implements-use-cases:
  - UC-EXPORT-EXEC-001
satisfies-requirements:
  - FR-EXPORT-EXEC-001
  - SEC-REQ-EXPORT-SANITIZATION-001
hosted-in-enclave: SEC-ENC-EXPORT-SANDBOX-001
interfaces:
  - name: "Document Export API"
    protocol: "REST/HTTP"
    contract-spec: "specs/architecture/contracts/export-api.json"
supersedes: null
superseded-by: null
---

# CMP-EXPORT-DOC-001: Sandbox de Exportación Ejecutiva Documental PDF y XLSX

## 1. Propósito, Responsabilidad y Contexto Delimitado
Ensambla y genera informes ejecutivos profesionales para dirección, socios e instituciones bancarias en formatos PDF y Microsoft Excel (XLSX) (`UC-EXPORT-EXEC-001`):
1. **Diseño Ejecutivo**: Cabecera corporativa, matriz de factores de coste, desglose por tratamiento y badges de color correspondientes a los semáforos de rentabilidad.
2. **Sanitización Anti-Formula Injection en XLSX (`SEC-REQ-EXPORT-SANITIZATION-001`)**: Intercepta todos los campos de texto dinámicos (nombres de servicios, descripciones de cosméticos) e inyecta preventivamente un prefijo de escape con comilla simple (`'`) en cualquier celda que comience por caracteres de fórmula (`=`, `+`, `-`, `@`, `\t`, `\r`), forzando al visor de hojas de cálculo a interpretarlo como texto plano.
3. **Sandboxing Aislado de Renderizado PDF (`SEC-REQ-EXPORT-SANITIZATION-001`)**: El motor de generación PDF (QuestPDF con licencia Community/MIT) se ejecuta con deshabilitación estricta de resolución de red externa (no emisión de peticiones HTTP/DNS), neutralizando ataques de Server-Side Request Forgery (SSRF) y fuga de metadatos de instancia.
4. **Acotamiento de Recursos**: Límite estricto de memoria RAM asignada (máximo 50 MB por compilación) y timeout de ejecución de 10 segundos para no comprometer los recursos del contenedor (.NET 10) en VPS económico (`QR-DEPLOY-COST-001`).

## 2. Diagrama Estructural y de Conectividad (arc42 Sec. 5 / NAF v4)
```mermaid
graph TD
    Client[CMP-AUTH-GATEWAY-001] -->|GET /api/v1/export/{format}| ExportEngine[CMP-EXPORT-DOC-001]
    ExportEngine -->|Recibir DTOs Sanitizados| Core[CMP-MARGIN-CALCULATOR-001]
    
    subgraph SandboxEnclave["SEC-ENC-EXPORT-SANDBOX-001 (Sin Red / Máx 50MB)"]
        FormatRouter{Formato Solicitado?}
        FormatRouter -->|XLSX| Sanitizer[FormulaInjectionSanitizer]
        Sanitizer -->|Escapar con comilla simple '| ExcelWriter[ClosedXML / OpenXML Engine]
        
        FormatRouter -->|PDF| PdfSandbox[Isolated QuestPDF Engine]
        PdfSandbox -->|Bloqueo de URLs Externas / SSRF Safe| PdfCompiler[PDF Byte Stream Compiler]
    end

    ExcelWriter -->|application/vnd.openxmlformats...| StreamResponse[Stream Binary Response]
    PdfCompiler -->|application/pdf| StreamResponse
    StreamResponse -->|Descarga Binaria| Client
```

## 3. Contratos de Interfaz y Políticas de Ejecución
- **Mecanismo de Ejecución**: Servicio in-process desacoplado en ASP.NET Core (.NET 10) que emite respuestas binarias en streaming directo (`FileStreamResult`) sin materializar archivos temporales en disco.
- **Tolerancia a Fallos y Rendimiento**: Prevención de fugas de memoria mediante streaming de chunks binarios; recolección de basura inmediata tras finalizar el flujo.

---

## 4. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición de componente sandbox de exportación ejecutiva PDF y XLSX | CHG-ARCH-001 |
