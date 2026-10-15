---
id: "ABUSE-AUTH-SPOOF-001"
type: "abuse-case"
title: "Suplantación de Identidad y Tenant Hopping en Selección de Centro"
status: "draft"
version: "1.0.0"
primary-threat-actor: "ACT-THREAT-COMPETITOR-001"
targets-use-case: "UC-AUTH-MULTI-001"
stride-category: "spoofing"
mitigated-by:
  - "SEC-REQ-AUTH-MEMBER-001"
asvs-controls:
  - "V2.1.1"
  - "V3.2.1"
  - "V4.1.1"
---

# Abuse Case: Suplantación de Identidad y Tenant Hopping (`ABUSE-AUTH-SPOOF-001`)

## 1. Description and Adversary Objective
El adversario `ACT-THREAT-COMPETITOR-001` intercepta o manipula su token de autenticación (JWT/cookie de sesión) o adultera los parámetros en el endpoint de selección de centro para vincular su sesión activa a un `CenterId` o `TenantId` ajeno.

## 2. Attack Execution Scenario
1. El atacante inicia sesión legítimamente con su cuenta en la plataforma.
2. Intercepta la petición de selección de centro `POST /api/v1/auth/select-center`.
3. Modifica el campo `centerId` en el payload JSON sustituyéndolo por el GUID de un centro competidor obtenido por enumeración o fuga previa.
4. Si el backend no valida criptográficamente la pertenencia del usuario al centro solicitado, emite un token de sesión o cookie con el `CenterId` de la víctima.
5. El atacante accede al panel de control del centro competidor y visualiza o modifica todos sus escandallos confidenciales.

## 3. Impact Assessment
- **Confidencialidad**: Crítica. Fuga total de márgenes, costes y fórmulas del competidor.
- **Integridad**: Alta. Riesgo de sabotaje o manipulación de precios.
- **Disponibilidad**: Media.
