---
id: "ACT-THREAT-EXTERNAL-001"
type: "threat-actor"
title: "Ciberatacante Externo Oportunista"
status: "draft"
version: "1.0.0"
threat-capability: "script-kiddie"
motivation: "financial-gain"
attack-surfaces:
  - "auth-endpoints"
  - "stripe-webhook-listener"
  - "public-api-surface"
---

# Threat Actor: Ciberatacante Externo Oportunista (`ACT-THREAT-EXTERNAL-001`)

## 1. Profile and Characteristics
- **Actor Class**: Adversario externo no autenticado en la red pública de Internet.
- **Capability Level**: `script-kiddie` / automatizaciones ofensivas con herramientas comunes (Burp Suite, OWASP ZAP, escáneres masivos de vulnerabilidades, scripts curl).
- **Primary Motivation**: `financial-gain` (acceso gratuito a planes Premium sin suscripción, explotación de recursos ajenos, falsificación de pasarelas de pago) y `reputational-damage`.

## 2. Attack Vectors and Capabilities
- Ataques de fuerza bruta, credential stuffing y pulverización de contraseñas contra `/api/v1/auth/login`.
- Inyección de cargas maliciosas simulando webhooks de Stripe (`checkout.session.completed`) sin firma o con firmas truncadas para eludir la cuota freemium.
- Escaneo de puertos y sondeo de endpoints no protegidos de exportación o recálculo para provocar indisponibilidad.
