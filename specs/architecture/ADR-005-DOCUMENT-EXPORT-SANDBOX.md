---
id: ADR-005-DOCUMENT-EXPORT-SANDBOX
type: architecture-decision-record
title: "Generación Documental Segura con Sanitización Anti-Formula Injection y Sandbox PDF sin Red"
status: accepted
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
decision-date: "2026-10-15"
affects-components:
  - CMP-EXPORT-DOC-001
supersedes: null
superseded-by: null
---

# ADR-005: Generación Documental Segura con Sanitización Anti-Formula Injection y Sandbox PDF sin Red

## 1. Context and Problem Statement
El requisito `FR-EXPORT-EXEC-001` demanda la exportación de escandallos en formatos PDF y XLSX con estética corporativa y paleta ejecutiva. Esta capacidad presenta dos vectores de ciberataque críticos documentados en `specs/security/`:
1. **Inyección de Fórmulas (CSV/Formula Injection)** en archivos Excel abiertos por directores o socios (`ABUSE-EXPORT-INJECT-001`).
2. **Server-Side Request Forgery (SSRF) y XXE** durante el renderizado de PDFs si el motor resuelve recursos remotos o entidades externas (`SEC-REQ-EXPORT-SANITIZATION-001`).
3. Además, las librerías elegidas deben ser conformes con `license-policy.yaml` (prohibido software GPL/AGPL viral o de pago comercial sin licencia corporativa).

## 2. Considered Technology Options
1. **Opción A: Motores basados en headless Chromium (Puppeteer / Playwright)**:
   - *Pros*: Renderizado CSS de alta fidelidad.
   - *Contras*: Huella de memoria masiva (> 300-500 MB de RAM por instancia), incompatible con el límite de 150 MB en reposo de `QR-DEPLOY-COST-001`.
2. **Opción B: Librería iText 7 / iText 8**:
   - *Pros*: Motor PDF clásico en la industria.
   - *Contras*: Sujeto a licencia dual AGPLv3 / Comercial; terminantemente prohibido por `license-policy.yaml` (violación de copyleft viral en SaaS).
3. **Opción C: QuestPDF (Licencia Community MIT) + ClosedXML / OpenXML (MIT) en Sandbox Aislado** (Elegida):
   - *Pros*: Licencia 100% permisiva compatible con `license-policy.yaml`; compilación programática de PDFs mediante DSL fuertemente tipado en C# con consumo de RAM mínimo (< 25-40 MB por documento); generación en streaming directo sin escribir ficheros temporales en disco; configuración nativa que inhabilita sockets de red durante la compilación.

## 3. Decision Outcome
Se adopta la **Opción C**.
Se implementa `CMP-EXPORT-DOC-001` dentro del enclave `SEC-ENC-EXPORT-SANDBOX-001`:
1. **Sanitización de Celdas XLSX**: Todo valor textual pasa por un filtro que antepone una comilla simple (`'`) si el primer carácter es `=`, `+`, `-`, `@`, `\t` o `\r`.
2. **Sandbox PDF**: Deshabilitación estricta de carga remota de imágenes y protocolos externos (`http://`, `file://`, `ftp://`).
3. **Control de Recursos**: Límite de 50 MB de memoria de trabajo y timeout duro de 10 segundos por generación.

## 4. Consequences
- **Positive**: Neutralización total de ataques de Formula Injection y SSRF; coste de memoria residual mínimo en el contenedor Linux; cumplimiento estricto de licencias OSS.
- **Negative / Trade-offs**: Los logotipos o marcas de agua de los informes deben embeberse como recursos estáticos locales compilados en el ensamblado (imágenes SVG / PNG en base64 internas).

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Decisión arquitectural aceptada | CHG-ARCH-001 |
