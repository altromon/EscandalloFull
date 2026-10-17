---
id: MAN-USER-001
type: user-manual
title: "Manual de Usuario: Escandallo Estética - Rentabilidad y Fijación de Precios"
status: draft
version: "1.0.0"
schema-version: "1.0"
applies-to-version: "v1.0.0"
target-audience:
  - ACT-USER-001
  - ACT-GUEST-001
allowed-roles:
  - ACT-USER-001
  - ACT-GUEST-001
journeys-covered: []
use-cases-covered:
  - UC-AUTH-MULTI-001
  - UC-COST-CONFIG-001
  - UC-SERVICE-MARGIN-001
  - UC-HISTORY-RECALC-001
  - UC-SUBSCRIPTION-GATE-001
  - UC-EXPORT-EXEC-001
  - UC-DEMO-MODE-001
supersedes: null
superseded-by: null
---

# MAN-USER-001: Manual de Usuario - Escandallo Estética

## 1. Finalidad del Sistema y Audiencia

**Escandallo Estética** es una plataforma web multi-inquilino de gestión financiera operativa, diseñada específicamente para propietarios, directores técnicos y colaboradores de centros y clínicas de estética. 

El sistema resuelve la falta de precisión en los márgenes de beneficio y la opacidad en los costes de cabina mediante el cálculo automatizado del **escandallo unitario por tratamiento**, desagregando con rigor matemático:
1. **Coste de Mano de Obra**: Imputación del coste por minuto del personal estético en base a salarios y seguros sociales.
2. **Coste Estructural del Local**: Imputación del coste por minuto de instalaciones, suministros, alquiler y amortización según horas de apertura.
3. **Coste Neto de Cosméticos y Desechables**: Cálculo exacto de viales, sérums, cremas y consumibles considerando mermas técnicas de aplicación.

- **Aplicación / Sistema**: `Escandallo Estética` (Monolito Modular .NET 10 / Vanilla Web UI)
- **Versión de Software**: `v1.0.0`
- **Audiencia Objetivo**: Propietarios y gestores de salones (`ACT-USER-001`), recepcionistas y colaboradores de cabina (`ACT-GUEST-001`).

---

## 2. Catálogo de Roles de Usuario y Matriz de Permisos (RBAC)

El sistema implementa control de acceso basado en roles con separación estricta de visibilidad para salvaguardar la confidencialidad de los salarios del personal y costes financieros de la empresa.

### 2.1 Catálogo de Roles Autorizados

| Identificador de Rol | Nombre del Rol | Actor Asociado | Descripción y Responsabilidad Operativa | Nivel de Acceso / Enclave |
| :--- | :--- | :--- | :--- | :--- |
| **`ROLE_MANAGER`** | Gestor / Propietario | `ACT-USER-001` | Administración total del centro: configuración de costes fijos, salarios, catálogo cosmético, creación/edición de escandallos, suscripción Pro y exportaciones. | `SEC-ENC-AUTH-GATEWAY-001` / Acceso Total |
| **`ROLE_COLLABORATOR`** | Colaborador / Invitado | `ACT-GUEST-001` | Consulta en cabina del catálogo de tratamientos, tiempos y productos requeridos. Acceso con censura estricta de salarios y alquileres. | `SEC-ENC-SERVICE-CORE-001` / Modo Lectura Restringido |

### 2.2 Matriz de Capacidades y Acciones por Rol

| Capacidad del Sistema / Módulo | `ROLE_MANAGER` (`ACT-USER-001`) | `ROLE_COLLABORATOR` (`ACT-GUEST-001`) | Tipo de Autenticación |
| :--- | :---: | :---: | :--- |
| **Inicio de Sesión y Selector de Centro** (`UC-AUTH-MULTI-001`) | ✅ Permitido | ✅ Permitido | Credencial / Bearer Token |
| **Alta y Gestión Multicentro** (`UC-AUTH-MULTI-001`) | ✅ Permitido | ❌ Denegado | Rol Propietario |
| **Carga y Purga de Datos Semilla Demo** (`UC-DEMO-MODE-001`) | ✅ Permitido | ❌ Denegado | Rol Propietario |
| **Configuración de Costes de Salón y Alquiler** (`UC-COST-CONFIG-001`) | ✅ Permitido | ❌ Denegado (Censurado) | Rol Propietario |
| **Gestión de Personal y Salarios (€/mes)** (`UC-COST-CONFIG-001`) | ✅ Permitido | ❌ Denegado (Censurado) | Rol Propietario |
| **Gestión de Catálogo Cosméticos y Mermas** (`UC-COST-CONFIG-001`) | ✅ Permitido | ❌ Denegado | Rol Propietario |
| **Consulta de Tratamientos y Semáforo de Margen** (`UC-SERVICE-MARGIN-001`)| ✅ Permitido | ✅ Permitido | Consulta Autorizada |
| **Visualización Desglose Confidencial (Coste Laboral/Local)** | ✅ Permitido | ❌ Censurado en Origen (Backend) | RBAC Filter |
| **Formulación y Duplicado de Escandallos** (`UC-SERVICE-MARGIN-001`) | ✅ Permitido | ❌ Denegado | Rol Propietario |
| **Histórico Inmutable Criptográfico SHA-256** (`UC-HISTORY-RECALC-001`) | ✅ Permitido | ❌ Denegado | Rol Propietario |
| **Gestión de Suscripción Stripe Plan Pro** (`UC-SUBSCRIPTION-GATE-001`) | ✅ Permitido | ❌ Denegado | Rol Propietario |
| **Exportación Ejecutiva PDF y Excel XLSX** (`UC-EXPORT-EXEC-001`) | ✅ Permitido | ❌ Denegado | Rol Propietario |
| **Visor de Trazas y Logs de Observabilidad** | ✅ Permitido | ❌ Denegado | Rol Propietario |

---

## 3. Matriz de Compatibilidad de Versiones y Plataforma

### 3.1 Compatibilidad Cliente-Servidor (APIs)

| Versión de Backend (.NET 10) | Versión Web UI | Endpoint Base API | Estado de Compatibilidad | Notas |
| :---: | :---: | :---: | :---: | :--- |
| **`v1.0.0`** (Actual) | `v1.0.0` | `/api/v1` | ✅ Oficial Soportada | Arquitectura de Monolito Modular con soporte SPA. |

### 3.2 Navegadores y Dispositivos Homologados

| Navegador / Entorno | Versión Mínima Soportada | Nivel de Soporte | Observaciones de Rendimiento |
| :--- | :--- | :---: | :--- |
| **Google Chrome / Chromium** | `>= 115` | Tier 1 (Óptimo) | Renderizado óptimo de modales y compatibilidad con Blob PDF/XLSX. |
| **Mozilla Firefox** | `>= 115 ESR` | Tier 1 (Óptimo) | Compatibilidad completa con inputs numéricos de coma flotante. |
| **Microsoft Edge** | `>= 115` | Tier 1 (Óptimo) | Rendimiento óptimo en entornos Windows de escritorio. |
| **Safari (macOS / iPadOS / iOS)** | `>= 16.4` | Tier 2 (Funcional) | Soporte de vista táctil en tablets de cabina estética. |

---

## 4. Acceso al Sistema y Configuración Inicial

### 4.1 Canales de Acceso
- **Portal Web del Salón**: Acceso mediante navegador web estándar a la URL corporativa (por defecto en local: `http://localhost:5100` o dominio asignado `https://app.escandalloestetica.es`).
- **Requisitos Previos**: Conexión a Internet / red local, navegador compatible con JavaScript activado y credenciales suministradas por el administrador.

### 4.2 Credenciales Semilla por Defecto

| Tipo de Perfil | Correo Electrónico | Contraseña por Defecto | Privilegios Iniciales |
| :--- | :--- | :--- | :--- |
| **Usuario Gestor** | `gestor@estetica.com` | `SecretPass123!` | Acceso total a `Centro Belleza Madrid` y sedes asociadas. |
| **Colaborador Invitado** | `guest@collaborator.com` | `GuestPass123!` | Modo lectura con censura económica en `Centro Belleza Madrid`. |

---

## 5. Guía Operativa Paso a Paso por Caso de Uso

### 5.1 `UC-AUTH-MULTI-001`: Autenticación Multi-inquilino y Selección de Centro

Permite el acceso seguro al sistema y el cambio inmediato entre diferentes centros o franquicias estéticas sin cerrar sesión.

```mermaid
sequenceDiagram
  autonumber
  actor U as ACT-USER-001 (Gestor)
  participant UI as Navbar / Modal Auth
  participant API as Auth Gateway (.NET)
  participant DB as Escandallo Store

  U->>UI: Introduce credenciales (Email + Password)
  UI->>API: POST /api/v1/auth/login
  API->>DB: Valida credenciales y carga centros autorizados
  API-->>UI: Devuelve token y catálogo de centros (TenantId, CenterId)
  UI->>UI: Establece centro activo y actualiza badge en Navbar
  UI->>API: GET /api/v1/centers/{centerId}/summary
  API-->>UI: Devuelve costes y tratamientos del centro