---
id: "BR-COLOR-THRESHOLDS-001"
type: "business-rule"
title: "Umbrales y Código de Colores de Rentabilidad"
status: "draft"
version: "1.0.0"
enforcement-level: "strict-invariant"
---

# Business Rule: Umbrales y Código de Colores de Rentabilidad (`BR-COLOR-THRESHOLDS-001`)

## 1. Description and Thresholds
Cada servicio o tratamiento se etiqueta visualmente según su margen de beneficio porcentual neto ($M\%$):

| Código Visual | Etiqueta | Rango de Margen | Semántica de Negocio |
| :---: | :---: | :---: | :--- |
| 🟢 **Verde** | Rentabilidad Alta | $M\% \ge 40.0\%$ | Servicio altamente rentable. Fomentar en campañas de venta. |
| 🟡 **Amarillo** | Rentabilidad Aceptable | $20.0\% \le M\% < 40.0\%$ | Servicio en rango medio. Vigilar costes de consumibles y tiempo de cabina. |
| 🔴 **Rojo** | Rentabilidad Crítica / Pérdidas | $M\% < 20.0\%$ | Servicio en riesgo o deficitario. Requiere revisión urgente de PVP o reducción de costes. |

## 2. Dynamic Threshold Configuration
Los umbrales por defecto ($40\%$ y $20\%$) pueden ser personalizados por el `ACT-USER-001` en las opciones avanzadas del centro de estética para adaptarse al modelo de negocio de su salón (ej. estética de bajo coste vs. estética de lujo).
