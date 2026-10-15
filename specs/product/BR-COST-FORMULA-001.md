---
id: "BR-COST-FORMULA-001"
type: "business-rule"
title: "Fórmula Canónica de Escandallo de Servicio de Estética"
status: "draft"
version: "1.0.0"
category: "financial-calculation"
---

# Business Rule: Fórmula Canónica de Escandallo de Servicio de Estética (`BR-COST-FORMULA-001`)

## 1. Description and Principles
Esta regla formaliza el modelo matemático y financiero exacto del escandallo para centros de estética, derivado de la hoja de cálculo de referencia del negocio.

El coste de un tratamiento se desglosa en **tres pilares de coste** dependientes del tiempo de cabina ($T$ en minutos) y de los consumos cosméticos aplicados:
1. **Gastos Generales** (Local, Consumos de suministros y Consumibles generales de cabina).
2. **Coste de Personal** (Categoría profesional asignada al tratamiento).
3. **Coste de Productos Cosméticos Específicos** (Principio activo, viales, cremas, mascarillas).

---

## 2. Parámetros Base y Fórmulas de Cálculo

### 2.1 Base Temporal Estándar y Unidades de Medida
- **Configuración de Apertura Mensual**: La interfaz solicita directamente las **Horas mensuales de apertura ($H_{mes}$)** con su unidad explícita `h/mes` (valor estándar sugerido: $176\text{ h/mes}$, equivalente a 22 días laborables $\times$ 8 h/día).
- **Minutos mensuales calculados ($M_{mes}$)**: Se obtienen automáticamente mediante conversión:
  $$M_{mes} = H_{mes} \times 60\text{ min/h} \quad (\text{ej. } 176 \times 60 = 10.560\text{ min/mes})$$
- **Duración del servicio**: $T$ con unidad explícita `min` (minutos de cabina).
- **Norma General de Unidades de Medida**: Todo valor configurable y computado en la aplicación DEBE mostrar su unidad de medida visible (`h/mes`, `min`, `€`, `€/mes`, `€/min`, `ml`, `ud`, `g`, `%`).
- **Norma General de Porcentajes**: Todos los porcentajes de la aplicación (márgenes, IVA, mermas, beneficio objetivo) DEBEN mostrarse e introducirse en **tanto por 100** ($0\%-100\%$, con sufijo `%`) y nunca en tanto por 1 ($0.0-1.0$).

---

### 2.2 Pilar 1: Gastos Generales ($G_{generales}$)
Los gastos de estructura del centro se computan a nivel mensual y se prorratean por minuto de apertura:

| Concepto | Unidad Medida | Coste Ejemplo (€/mes) | Precio Unitario (€/min) |
| :--- | :---: | :---: | :---: |
| **Local** (Alquiler / Arrendamiento) | minutos/mes | $C_{local}$ (ej. 335,00 €) | $C_{local} / 10.560 \approx 0,031723$ €/min |
| **Consumos** (Electricidad, agua, internet) | minutos/mes | $C_{consumos}$ (ej. 100,00 €) | $C_{consumos} / 10.560 \approx 0,009470$ €/min |
| **Consumibles Generales** (Papel camilla, guantes, algodón) | minutos/mes | $C_{consumibles}$ (ej. 440,00 €) | $C_{consumibles} / 10.560 \approx 0,041667$ €/min |
| **Total Gastos Generales** | - | $C_{GG\_mensual}$ (ej. 875,00 €) | $PU_{GG} = \sum PU_i \approx 0,082860$ €/min |

Fórmula imputable por servicio de duración $T$ minutos:
$$G_{generales} = T \times PU_{GG} = T \times \left(\frac{C_{local} + C_{consumos} + C_{consumibles}}{M_{mes}}\right)$$

*(Ejemplo para 60 minutos: $60 \times 0,08285985 \approx 4,971591$ €)*

---

### 2.3 Pilar 2: Coste de Persona / Mano de Obra ($C_{persona}$)
El coste de la mano de obra se calcula a partir del coste total para la empresa del profesional asignado, prorrateado por minuto:

$$PU_{persona} = \frac{\text{Coste para Empresa}}{M_{mes}}$$

Fórmula imputable por servicio de duración $T$ minutos:
$$C_{persona} = T \times PU_{persona}$$

**Categorías Profesionales del Modelo**:
- **Técnico**: Coste Empresa = 1.980,00 €/mes $\rightarrow PU = 0,187500$ €/min. *(60 min = 11,25 €)*
- **Técnico Superior**: Coste Empresa = 2.340,00 €/mes $\rightarrow PU = 0,221591$ €/min. *(60 min = 13,30 €)*
- **Encargado**: Coste Empresa = 2.880,00 €/mes $\rightarrow PU = 0,272727$ €/min. *(60 min = 16,36 €)*
- **Enfermero**: Coste Empresa = 3.294,00 €/mes $\rightarrow PU = 0,311932$ €/min. *(60 min = 18,72 €)*
- **Doctor**: Coste Empresa = 5.625,00 €/mes $\rightarrow PU = 0,532670$ €/min. *(60 min = 31,96 €)*

---

### 2.4 Pilar 3: Coste de Productos Cosméticos Específicos ($C_{productos}$) con Tasa de Merma
Cada producto del catálogo se define por su formato de compra (cantidad $Q_i$ y unidad de medida `ml` o `ud`), su precio de compra $P_i$ y un porcentaje opcional de merma/desperdicio de envase ($\text{Merma}_i$, por defecto $0\%$ o valor estimado ej. $5\%$):

$$PU_{\text{bruto}\_i} = \frac{P_i}{Q_i}$$
$$PU_{\text{efectivo}\_i} = PU_{\text{bruto}\_i} \times (1 + \text{Merma}_i)$$

Para una sesión que consume una cantidad $c_i$ del producto $i$:
$$\text{Coste Producto}_i = c_i \times PU_{\text{efectivo}\_i}$$

Coste total de cosméticos aplicados en el servicio (con soporte de 1 a $N$ productos):
$$C_{productos} = \sum_{i=1}^n \left[ c_i \times \left(\frac{P_i}{Q_i}\right) \times (1 + \text{Merma}_i) \right]$$

---

### 2.5 Escandallo Total del Servicio
$$\text{Escandallo Total} = G_{generales} + C_{persona} + C_{productos}$$

*(Nota de simplificación MVP: La amortización de aparatología y maquinaria estética permanece consolidada dentro de los Gastos Generales mensuales $G_{generales}$ para preservar la simplicidad operativa).*

---

## 3. Formación de Precios, Tipos de IVA y Rentabilidad

### 3.1 Parámetros de Margen e Impuestos por Servicio
- **Beneficio Objetivo ($B_{\%}$)**: Margen de beneficio objetivo sobre coste (por defecto $50\%$ o $0,5$, configurable por centro o servicio).
- **Tipo de IVA por Servicio ($\text{IVA}_{\%}$)**: Seleccionable a nivel de cada servicio para contemplar la realidad del centro:
  - **General (21%)**: Tratamientos estéticos convencionales.
  - **Reducido (10%)**: Terapias de balneario o aguas termales cuando aplique.
  - **Exento (0%)**: Tratamientos médico-estéticos realizados por facultativos con finalidad curativa/médica (Art. 20 Ley IVA).

### 3.2 Precios Sugeridos por el Sistema
1. **Precio Base Sugerido (sin IVA)**:
   $$\text{Precio Base} = \text{Escandallo Total} \times (1 + B_{\%}) = \text{Escandallo Total} \times 1,50$$
2. **PVP Recomendado Sugerido (con IVA del servicio)**:
   $$\text{PVP Recomendado} = \text{Precio Base} \times (1 + \text{IVA}_{\%})$$

### 3.3 Precio de Venta Real (PVR) y Margen Real por Sesión Suelta
El centro define el **PVR** final (con IVA incluido) cobrado al cliente:
1. **PVR sin IVA**:
   $$\text{PVR}_{\text{sin IVA}} = \frac{\text{PVR}}{1 + \text{IVA}_{\%}}$$
2. **Beneficio Real Absoluto por Sesión**:
   $$\text{Beneficio Real} = \text{PVR}_{\text{sin IVA}} - \text{Escandallo Total}$$
3. **Margen Real Porcentual sobre Venta**:
   $$\text{Margen Real \%} = \left(\frac{\text{Beneficio Real}}{\text{PVR}_{\text{sin IVA}}}\right) \times 100$$

### 3.4 Bonos y Packs de Sesiones (Rentabilidad por Volumen)
Para tratamientos vendidos en paquetes (ej. Bono 5 o Bono 10 sesiones) con precio especial de pack ($PVR_{\text{bono}}$ con IVA):
1. **PVR del Bono sin IVA**:
   $$PVR_{\text{bono\_sin\_IVA}} = \frac{PVR_{\text{bono}}}{1 + \text{IVA}_{\%}}$$
2. **PVR Efectivo por Sesión del Bono**:
   $$PVR_{\text{sesión\_bono}} = \frac{PVR_{\text{bono\_sin\_IVA}}}{N_{\text{sesiones}}}$$
3. **Beneficio Real por Sesión en Bono**:
   $$\text{Beneficio Real}_{\text{bono}} = PVR_{\text{sesión\_bono}} - \text{Escandallo Total}$$
4. **Margen Real Porcentual del Bono**:
   $$\text{Margen Bono \%} = \left(\frac{\text{Beneficio Real}_{\text{bono}}}{PVR_{\text{sesión\_bono}}}\right) \times 100$$
5. El sistema evalúa el semáforo visual (Verde / Amarillo / Rojo) tanto para la sesión individual como para cada Bono registrado en el servicio.

### 3.5 Alerta Explícita de Pérdida Absoluta (Venta a Pérdidas)
Si el PVR introducido por el usuario es inferior al escandallo total del servicio o del bono ($\text{Beneficio Real} < 0$ €):
1. El semáforo visual se fija inmediatamente en 🔴 **Rojo**.
2. La interfaz de usuario muestra de forma obligatoria un mensaje de advertencia destacado en rojo bajo el semáforo:
   - Para la sesión suelta: `Atención: Estás perdiendo X,XX € por cada sesión` (donde $X,XX = |\text{Beneficio Real}|$).
   - Para bonos deficitarios: `Atención: En este bono pierdes X,XX € por sesión` (donde $X,XX = |\text{Beneficio Real}_{bono}|$).

---

## 4. Política de Datos: Ejemplos de Referencia vs. Entorno de Producción
1. **Naturaleza Ilustrativa**: Todos los importes numéricos (salarios, alquileres, suministros, precios de cosméticos y el servicio `Mesodermolifting`) contenidos en esta regla y en la hoja de cálculo de origen son **estrictamente datos de ejemplo y referencia**.
2. **Prohibición de Carga Automática en Producción**: Ningún valor, producto, servicio o tarifa horaria de la Excel debe ser exportado o precargado por defecto en centros de producción reales. Todo centro nuevo se inicializa en estado vacío (*clean slate*).
3. **Exclusividad del Modo Demo**: Estos datos de ejemplo únicamente podrán poblarse en el sistema cuando el usuario active explícitamente el **Modo Demo / Cargar datos de prueba** (`BR-DEMO-DATA-001`, `UC-DEMO-MODE-001`).

