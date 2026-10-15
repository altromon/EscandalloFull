---
id: "QR-DR-BACKUP-001"
type: "quality-requirement"
title: "Estrategia de Persistencia y Recuperación ante Desastres (Disaster Recovery)"
status: "draft"
version: "1.0.0"
category: "resilience-disaster-recovery"
---

# Quality Requirement: Persistencia y Recuperación ante Desastres (`QR-DR-BACKUP-001`)

## 1. Quality Attribute and Target
- **Recovery Point Objective (RPO)**: $\le 1$ hora (máxima pérdida admisible de datos ante caída catastrófica).
- **Recovery Time Objective (RTO)**: $\le 2$ horas (tiempo máximo para restaurar el servicio completo en una nueva infraestructura).

## 2. Architectural Strategy
- Motor de base de datos relacional robusto (PostgreSQL en contenedor o gestionado).
- Copias de seguridad automáticas diarias en caliente con volcado lógico encriptado (`pg_dump` con AES-256).
- Replicación continua de Write-Ahead Logging (WAL archiving via Litestream o pgBackRest / S3 sync) hacia almacenamiento de objetos S3 compatible de ultrabajo coste (ej. Cloudflare R2 sin coste de salida o Backblaze B2).
- Procedimiento de restauración reproducible como código (*Infrastructure as Code* / script de restauración automatizado verificado periódicamente).
