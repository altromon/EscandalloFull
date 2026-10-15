---
id: "UC-EXPORT-EXEC-001"
type: "use-case"
title: "Exportación Ejecutiva de Escandallos en PDF y XLSX"
status: "draft"
version: "1.0.0"
primary-actor: "ACT-USER-001"
secondary-actors: []
---

# Use Case: Exportación Ejecutiva de Escandallos en PDF y XLSX (`UC-EXPORT-EXEC-001`)

## 1. Intent and Outcome
As a `ACT-USER-001`, I want to export the complete cost and profit breakdown of my aesthetic center's services into professionally formatted PDF and XLSX documents, to present polished executive summaries to partners, investors, or clinic directors.

## 2. Preconditions
- The user is authenticated as `ACT-USER-001` and the selected center has at least one configured escandallo.

## 3. Main Success Scenario
1. The user selects "Export Center Escandallos" and chooses the desired format: PDF or XLSX.
2. The user optionally filters by treatment category or selects specific services.
3. The system compiles the dataset and generates the document adhering to executive design standards:
   - Header with center branding, timestamp, and active currency.
   - Professional corporate color palette (slate/navy/emerald accents) avoiding garish contrasts.
   - Clean tables with aligned monetary figures, percentages, and visual status badges.
   - For PDF: Vector pagination, page numbering, and executive summary KPI cards.
   - For XLSX: Native numeric cell types, dynamic Excel formulas for sums and margins, and print-ready worksheet formatting.
4. The system delivers the file as an immediate browser download.

## 4. Alternative and Error Flows
- **Exporting Empty Center**: If no services are defined, the system disables the export button and displays a guidance tooltip.
