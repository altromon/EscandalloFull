---
id: "BR-GUEST-CENSOR-001"
type: "business-rule"
title: "Censura Estricta de Costes Sensibles para Invitados"
status: "draft"
version: "1.0.0"
enforcement-level: "strict-invariant"
---

# Business Rule: Censura Estricta de Costes Sensibles para Invitados (`BR-GUEST-CENSOR-001`)

## 1. Description and Constraints
Para preservar la confidencialidad salarial y los acuerdos de arrendamiento del negocio:
1. Cuando una solicitud es originada por el actor `ACT-GUEST-001`, la capa de servicio y presentación **debe censurar y filtrar en origen (backend)** los campos:
   - Salarios, tarifas horarias de personal y coste unitario de mano de obra ($C_{personal}$).
   - Importes de alquiler, suministros y coste unitario imputable de local ($C_{local}$).
2. El invitado solo podrá visualizar:
   - Nombre y descripción del servicio.
   - Cantidades y nombres de los productos cosméticos y consumibles utilizados.
   - Tiempo de ejecución de cabina.
   - Indicador de margen y PVP sugerido (si el gestor lo habilita a nivel de centro).
3. Queda prohibida la mera ocultación en frontend por CSS; los datos sensibles jamás deben transmitirse en el payload JSON hacia un cliente autenticado como invitado.
