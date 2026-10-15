---
id: ARCH-CROSS-001
type: cross-cutting-concepts
title: "08. Cross-Cutting Concepts - Escandallo Estética"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 8
naf-perspective: "Information & Security"
data-models:
  - DATA-MODEL-ESCANDALLO-001
  - DATA-MODEL-COSTS-002
  - DATA-MODEL-TENANT-003
security-enclaves:
  - SEC-ENC-AUTH-GATEWAY-001
  - SEC-ENC-SERVICE-CORE-001
  - SEC-ENC-DATA-ACCESS-001
  - SEC-ENC-EXPORT-SANDBOX-001
  - SEC-ENC-PAYMENT-INTEGRATION-001
security-policies:
  - SEC-POL-TLS-001
  - SEC-POL-TENANT-ISOLATION-002
  - SEC-POL-GUEST-CENSORSHIP-003
  - SEC-POL-FORMULA-SHIELD-004
  - SEC-POL-CRYPTO-AUDIT-005
supersedes: null
superseded-by: null
---

# 08. Cross-Cutting Concepts (arc42 Sec. 8 / NAF Information & Security)

## 1. Concepto Transversal de Seguridad y Modelo de Confianza Cero
La plataforma adopta el principio de *Security-by-Default* y modelo de zonas de confianza segregadas por enclaves (`SEC-ENC-*`):

### 1.1 Autenticación y Contexto de Inquilino
- Autenticación sin estado basada en JSON Web Tokens (JWT) firmados con clave simétrica HMAC-SHA256 rotada regularmente.
- El token contiene las credenciales verificadas del usuario (`sub`), identificador de organización (`tenant_id`), centros autorizados y rol activo (`role`).
- En cada solicitud HTTP, el middleware perimetral resuelve el objeto inmutable `TenantContext` y lo inyecta mediante Inyección de Dependencias (DI) con ciclo de vida *Scoped*, garantizando que ningún servicio o repositorio acceda a la persistencia sin un contexto de inquilino validado.

### 1.2 Políticas Obligatorias de Seguridad (`SEC-POL-*`)

| Identificador | Alcance | Descripción del Control | Requisito Mitigado |
| :--- | :--- | :--- | :--- |
| `SEC-POL-TLS-001` | Comunicaciones de Red | TLS 1.3 obligatorio en el punto de entrada con suites de cifrado seguras y cabeceras HSTS activadas. | `SEC-ENC-AUTH-GATEWAY-001` |
| `SEC-POL-TENANT-ISOLATION-002` | Capa de Persistencia | Aplicación obligatoria de *Global Query Filters* en EF Core por `tenant_id` y `center_id` en todas las consultas y validación anti-IDOR. | `SEC-REQ-AUTHZ-TENANT-FILTER-001` |
| `SEC-POL-GUEST-CENSORSHIP-003` | Proyección de Respuestas | Proyección estricta en backend mediante DTOs sanitizados para el rol `ACT-GUEST-001`, eliminando en origen salarios y alquileres. | `SEC-REQ-DATA-CENSOR-GUEST-001` |
| `SEC-POL-FORMULA-SHIELD-004` | Exportaciones XLSX | Inyección de escape con comilla simple (`'`) en todo texto alfanumérico que comience por `=`, `+`, `-`, `@`. | `SEC-REQ-EXPORT-SANITIZATION-001` |
| `SEC-POL-CRYPTO-AUDIT-005` | Trazabilidad Histórica | Registro inmutable de variaciones en tabla append-only con hash SHA-256 encadenado y bloqueo de operaciones de mutación. | `SEC-REQ-AUDIT-IMMUTABLE-001` |

---

## 2. Modelos de Datos Transversales y Contratos Compartidos (`DATA-*`)
En cumplimiento de la directiva **Contract-First Full-Stack Integration**, todas las estructuras de intercambio de datos se tipan formalmente:

| Identificador | Formato / Tipo | Ruta de Especificación | Propósito |
| :--- | :--- | :--- | :--- |
| `DATA-MODEL-AUTH-001` | JSON / OpenAPI | `specs/architecture/contracts/auth-api.json` | Contrato de inicio de sesión, token y selector de centro |
| `DATA-MODEL-COSTS-002`| JSON / OpenAPI | `specs/architecture/contracts/costs-api.json` | Contrato de factores de coste (personal, apertura, consumibles) |
| `DATA-MODEL-ESCANDALLO-003`| JSON / OpenAPI | `specs/architecture/contracts/escandallo-api.json` | Contrato de escandallos, precios sugeridos y semáforos |
| `DATA-MODEL-BILLING-004`| JSON / OpenAPI | `specs/architecture/contracts/billing-api.json` | Contrato de suscripciones y recepción de webhooks Stripe |
| `DATA-TYPES-UI-005`   | TypeScript Definitions | `specs/architecture/contracts/ui-contracts.d.ts` | Tipos TypeScript compartidos para consumo de API en frontend |

---

## 3. Observabilidad, Telemetría y Salud del Sistema
- **Health Checks Nativos**:
  - `/healthz`: Verificación de liveness del proceso .NET 10.
  - `/readyz`: Verificación de conectividad activa y latencia contra el pool de PostgreSQL.
- **Registro Estructurado (Structured Logging)**: Emisión determinista en formato JSON por salida estándar (`stdout`) mediante `Serilog` / `Microsoft.Extensions.Logging`, incorporando `Timestamp`, `LogLevel`, `TenantId`, `CenterId`, `TraceId` y `ComponentId`.
- **Monitoreo de Memoria**: Muestreo continuo del consumo de memoria del proceso (`Process.GetCurrentProcess().WorkingSet64`) para certificar el límite $< 150$ MB RAM establecido en `QR-DEPLOY-COST-001`.

---

## 4. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición inicial de conceptos transversales de seguridad y datos arc42 / NAF | CHG-ARCH-001 |
