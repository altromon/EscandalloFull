---
id: "CHG-REMEDIATION-002"
title: "Remediación de Capacidades Funcionales: CRUD Completo de Costes y Servicios, Ciclo Freemium/Pro, Multi-sede Dinámico, Histórico y Observabilidad"
type: "proposal"
status: "proposed"
version: "1.0.0"
author: "agent-system-architect"
date: "2026-10-16"
---

# Propuesta de Cambio: Remediación de Capacidades Funcionales (`CHG-REMEDIATION-002`)

## 1. Justificación y Objetivos de Negocio
A partir de la auditoría arquitectural del sistema sobre los 8 puntos reportados por el usuario, se identifican las brechas funcionales que impiden la operación autónoma y completa de la plataforma:
1. **CRUD Incompleto de Factores de Coste**: Falta `GET parameters`, edición y eliminación de trabajadores y cosméticos.
2. **Ciclo de Vida de Escandallos Incompleto**: Falta borrado (`DELETE`) de servicios con restitución de cupo freemium.
3. **Limpieza de Demo Inoperativa**: El botón de limpiar no purga los servicios del centro en el repositorio.
4. **Ciclo de Monetización Incompleto**: Faltan endpoints y UI para contratar y cancelar el Plan Pro.
5. **Gobernanza de Sedes y Usuarios**: No se pueden crear nuevos centros de estética ni registrar nuevos usuarios.
6. **Falta de Trazabilidad y Observabilidad**: Ausencia de histórico de cambios con SHA-256 en la UI y visor de logs/trazas del sistema.

## 2. Alcance por Fases
- **Fase 1**: CRUD Completo de Costes y Eliminación de Escandallos (Puntos 1 y 2).
- **Fase 2**: Purgado Real de Demo y Ciclo de Suscripción Freemium/Pro (Puntos 5 y 6).
- **Fase 3**: Multi-sede Dinámico y Registro de Usuarios (Puntos 4 y 7).
- **Fase 4**: Histórico Criptográfico SHA-256 y Observabilidad de Logs/Trazas (Puntos 3 y 8).

## 3. Criterios de Éxito
- Todos los endpoints REST implementados con contratos limpios.
- La interfaz de usuario refleja de forma inmediata y reactiva cada acción.
- La suite de pruebas automatizadas se amplía y pasa al 100% sin regresiones.
