---
id: "ABUSE-RECALC-DOS-001"
type: "abuse-case"
title: "Denegación de Servicio por Inundación de Recálculos en Cascada"
status: "draft"
version: "1.0.0"
primary-threat-actor: "ACT-THREAT-RESOURCE-ABUSER-001"
targets-use-case: "UC-HISTORY-RECALC-001"
stride-category: "denial-of-service"
mitigated-by:
  - "SEC-REQ-RATE-RECALC-THROTTLE-001"
asvs-controls:
  - "V11.1.4"
  - "V13.2.3"
---

# Abuse Case: Denegación de Servicio por Recálculos en Cascada (`ABUSE-RECALC-DOS-001`)

## 1. Description and Adversary Objective
El adversario `ACT-THREAT-RESOURCE-ABUSER-001` aprovecha la funcionalidad de recálculo automático en cascada (`UC-HISTORY-RECALC-001`) para enviar ráfagas continuas de modificaciones mínimas de precios sobre cosméticos base vinculados a cientos de tratamientos, saturando los hilos de CPU y el consumo de RAM en el contenedor ligero (.NET 10 en VPS compartido de bajo coste, `QR-DEPLOY-COST-001`).

## 2. Attack Execution Scenario
1. El atacante define un producto cosmético (ej. "Agua Micelar Base") que vincula como ingrediente a 50 tratamientos activos.
2. Mediante un script automatizado, emite peticiones consecutivas `PATCH /api/v1/costs/products/{id}` modificando el precio en 0.01 € cada 5 milisegundos.
3. Si el sistema ejecuta el recálculo en el hilo de la petición HTTP (síncrono y bloqueante) sin rate-limiting por usuario/centro, el pool de conexiones de base de datos se agota y la CPU se eleva al 100%.
4. La aplicación web entera deja de responder a usuarios legítimos de todos los centros del inquilino, provocando un fallo generalizado del contenedor (`QR-AVAILABILITY-001`).

## 3. Impact Assessment
- **Confidencialidad**: Nula.
- **Integridad**: Baja.
- **Disponibilidad**: Crítica. Caída del servicio y degradación severa de rendimiento en infraestructura de coste mínimo.
