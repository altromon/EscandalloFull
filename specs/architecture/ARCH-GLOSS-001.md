---
id: ARCH-GLOSS-001
type: architecture-glossary
title: "12. Architecture Glossary and Terms Taxonomy - Escandallo Estética"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 12
naf-perspective: "Taxonomy & Terms"
cites-domain-terms:
  - TERM-ESCANDALLO-001
  - TERM-MERMA-002
  - TERM-SEMAFORO-003
  - TERM-ENCLAVE-004
  - TERM-FREEMIUM-005
cites-bounded-contexts:
  - "Cálculo de Costes y Escandallo"
  - "Identidad y Acceso Multicentro"
  - "Facturación y Suscripciones"
supersedes: null
superseded-by: null
---

# 12. Architecture Glossary (arc42 Sec. 12 / NAF Taxonomy)

## 1. Términos Canónicos del Dominio (`TERM-*`)
Definiciones oficiales y unificadas de conceptos clave de negocio y producto:

| Identificador | Término | Definición Canónica | Contexto Delimitado |
| :--- | :--- | :--- | :--- |
| `TERM-ESCANDALLO-001` | **Escandallo** | Determinación analítica y formulación exacta del coste de producción unitario de un tratamiento de belleza, sumando mano de obra profesional ($C_{personal}$), costes generales horarios del salón ($C_{local}$) y consumibles cosméticos ($C_{consumibles}$). | Cálculo de Costes |
| `TERM-MERMA-002` | **Tasa de Merma** | Porcentaje adicional de coste imputable al residuo no aprovechable o pérdida de cosmético en el envase durante la aplicación en cabina (fórmula: $Coste_{efectivo} = Coste_{bruto} \times (1 + Merma\%)$). | Cálculo de Costes |
| `TERM-SEMAFORO-003` | **Semáforo de Rentabilidad** | Indicador visual de rentabilidad neta por servicio: Rojo ($< 35\%$), Amarillo ($35\% - 50\%$) y Verde ($\ge 50\%$). | Cálculo de Costes |
| `TERM-ENCLAVE-004` | **Enclave de Seguridad** | Zona lógica o física delimitada en la arquitectura (`SEC-ENC-*`) con reglas perimetrales estrictas de tráfico entrante, saliente y control de acceso. | Ciberseguridad |
| `TERM-FREEMIUM-005` | **Cuota Freemium** | Restricción de negocio que permite el alta de hasta 2 escandallos activos por centro en plan gratuito, requiriendo suscripción de pago para ilimitados. | Facturación |

---

## 2. Abreviaturas y Acrónimos Técnicos
Glosario de conceptos y estándares técnicos empleados en la arquitectura del sistema:

| Acrónimo | Significado Completo | Definición en el Contexto del Sistema |
| :--- | :--- | :--- |
| **arc42** | Architecture Communication Template | Marco estándar modular para documentar y comunicar arquitecturas de software. |
| **NAF v4** | NATO Architecture Framework v4 | Metodología de modelado arquitectural empresarial y orientada a servicios. |
| **ADR** | Architecture Decision Record | Registro inmutable de una decisión técnica arquitectural significativa. |
| **BOLA / IDOR** | Broken Object Level Authorization | Vulnerabilidad de autorización por manipulación de identificadores de objetos. |
| **TOCTOU** | Time-of-Check to Time-of-Use | Condición de carrera concurrente entre la comprobación de un recurso y su uso. |
| **SSRF** | Server-Side Request Forgery | Ataque donde el servidor es inducido a emitir peticiones a recursos internos no autorizados. |
| **WAL** | Write-Ahead Logging | Mecanismo de persistencia transaccional de PostgreSQL para replicación y backup continuo. |
| **RPO / RTO** | Recovery Point / Time Objective | Métricas de objetivo de recuperación ante desastres (RPO $\le$ 1h, RTO $\le$ 2h). |
| **DTO** | Data Transfer Object | Objeto tipado utilizado para transferir datos y sanitizar proyecciones en backend. |

---

## 3. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-15 | `agent-system-architect` | Definición de glosario y taxonomía de términos arc42 / NAF | CHG-ARCH-001 |
