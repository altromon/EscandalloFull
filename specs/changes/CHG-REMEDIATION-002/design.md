---
id: "CHG-REMEDIATION-002"
title: "Diseño Técnico: CRUD de Costes y Escandallos, Ciclo Freemium/Pro, Multi-Sede, Registro, Histórico y Observabilidad"
status: "proposed"
version: "1.0.0"
author: "agent-system-architect"
date: "2026-10-16"
---

# Diseño Técnico (`CHG-REMEDIATION-002`)

## 1. Fase 1: CRUD de Costes y Ciclo de Vida de Escandallos (Puntos 1 y 2)

### Endpoints REST
- `GET /api/v1/costs/parameters`: Retorna `CenterCostParameters` del centro activo.
- `PUT /api/v1/costs/workers/{id}`: Actualiza categoría y coste mensual de la categoría laboral.
- `DELETE /api/v1/costs/workers/{id}`: Elimina la categoría laboral del centro.
- `PUT /api/v1/costs/products/{id}`: Actualiza nombre, precio, capacidad, unidad de medida y porcentaje de merma.
- `DELETE /api/v1/costs/products/{id}`: Elimina el cosmético del centro.
- `DELETE /api/v1/escandallos/{id}`: Elimina el escandallo del centro y, si estaba activo, invoca `DecrementActiveEscandallos` en `SubscriptionManager`.

---

## 2. Fase 2: Purga de Demo y Ciclo Freemium/Pro (Puntos 5 y 6)

### Lógica de Dominio
- `IDemoManager.PurgeDemoData(centerId)`: Ahora recibe o invoca `IEscandalloRepository` para eliminar todos los escandallos asociados al centro o marcados como demo, y actualiza el contador en `ISubscriptionManager`.
- `ISubscriptionManager`:
  - `UpgradeToPremium(centerId)`: Actualiza el estado a `PlanType.Premium` y `MaxAllowedEscandallos = int.MaxValue`.
  - `CancelSubscription(centerId)`: Vuelve a `PlanType.Free` con límite de 2 escandallos activos.
- `BillingController`:
  - `POST /api/v1/billing/mock-complete-checkout`: Llama a `UpgradeToPremium(centerId)`.
  - `POST /api/v1/billing/mock-cancel-subscription`: Llama a `CancelSubscription(centerId)`.

---

## 3. Fase 3: Multi-sede Dinámico y Registro de Usuarios (Puntos 4 y 7)

### Lógica de Dominio
- `IAuthGateway`:
  - `CenterMembership CreateCenter(string userId, string centerName)`: Registra un nuevo centro en el usuario, inicializa sus costes fijos por defecto y trabajadores estándar en `ICostConfigurationRepository`.
  - `AuthSession Register(string email, string password, string centerName)`: Registra un nuevo usuario con rol `User`, crea su primer centro y emite token válido.
- `AuthController`:
  - `POST /api/v1/auth/centers`: Recibe `{ centerName }` y el usuario autenticado para asociarle la nueva sede.
  - `POST /api/v1/auth/register`: Recibe `{ email, password, centerName }`.

---

## 4. Fase 4: Histórico Criptográfico SHA-256 y Observabilidad (Puntos 3 y 8)

### Auditoría Criptográfica
- `IAuditTrailRepository`: Ya contiene el hashing SHA-256.
- Inyección de `IAuditTrailRepository` en `CostsController`, `EscandallosController`, etc., para registrar eventos cuando se modifica un precio, coste o servicio.
- `AuditController`:
  - `GET /api/v1/audit`: Retorna el historial de cambios del centro activo.

### Observabilidad y Logs
- Implementación de `MemoryLogBufferProvider` como `ILoggerProvider` en memoria (buffer circular de las últimas 100 trazas).
- `SystemController`:
  - `GET /api/v1/system/logs`: Devuelve las trazas de log formateadas.
