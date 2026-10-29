---
id: "BR-FREEMIUM-LIMIT-001"
type: "business-rule"
title: "Límite de Cuota Freemium de Escandallos por Centro"
status: "draft"
version: "1.0.0"
enforcement-level: "strict-invariant"
---

# Business Rule: Límite de Cuota Freemium de Escandallos por Centro (`BR-FREEMIUM-LIMIT-001`)

## 1. Description and Constraints
1. Todo centro de estética registrado en la plataforma inicia en modalidad **Free / Freemium**.
2. Un centro en modalidad Free puede tener como máximo **2 escandallos de servicio activos** simultáneamente (`is_active = true`).
3. **Desactivación / Archivado de Escandallos**:
   - El usuario gestor (`ACT-USER-001`) puede desactivar un escandallo existente (`is_active = false`) en cualquier momento.
   - Los escandallos inactivos no computan para el límite de cuota, permitiendo al usuario Free formular nuevos tratamientos sin perder el histórico de datos de servicios anteriores.
4. **Duplicación de Servicios**:
   - El sistema permite duplicar un servicio existente con todos sus parámetros, tiempos y consumibles cosméticos.
   - Si el centro opera en modalidad Free y ya cuenta con 2 escandallos activos, la duplicación directa como servicio activo queda **bloqueada** con código `QUOTA_EXCEEDED`, requiriendo actualizar a Premium o desactivar previamente un escandallo activo.
5. Si el usuario intenta crear, activar o duplicar como activo un 3er escandallo en plan Free:
   - La operación de guardado se bloquea a nivel de API con código de error de negocio `QUOTA_EXCEEDED` (HTTP 402/403).
   - La interfaz muestra proactivamente la opción de actualizar a plan **Premium** (suscripción recurrente mensual o anual procesada por Stripe).
6. La suscripción Premium elimina el límite de escandallos activos para todos los centros cubiertos por el plan.
