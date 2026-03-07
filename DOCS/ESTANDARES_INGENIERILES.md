# Estándares de Ingeniería

## Introducción

Structural Management V1 implementa dos normativas de ingeniería civil ampliamente utilizadas en Latinoamérica para el diseño de estructuras de concreto en zonas sísmicas:

1. **NTE E030** — Diseño Sismorresistente (Reglamento Nacional de Edificaciones, Perú)
2. **ACI 318** — Requisitos del Código para Concreto Estructural (American Concrete Institute)

Esta sección describe cómo cada normativa está implementada en el sistema y qué parámetros y fórmulas se usan.

---

## Parte I: NTE E030 — Diseño Sismorresistente

### 1.1 Descripción

La **Norma Técnica E030** del Reglamento Nacional de Edificaciones del Perú define los requisitos mínimos para el diseño sismorresistente de edificaciones. El sistema implementa los parámetros del espectro de diseño mediante la clase `E030Tables` y el control `E030UserControl`.

### 1.2 Parámetros Sísmicos Implementados

#### Factor de Zona (Z)

| Zona | Factor Z | Descripción |
|---|---|---|
| Z1 | 0.10 | Zona de baja sismicidad |
| Z2 | 0.25 | Zona de moderada sismicidad |
| Z3 | 0.35 | Zona de alta sismicidad |
| Z4 | 0.45 | Zona de muy alta sismicidad |

#### Parámetros de Suelo (S, Tp, TL)

| Zona \ Suelo | S0 | S1 | S2 | S3 |
|---|---|---|---|---|
| **S (factor)** | | | | |
| Z1 | 0.80 | 1.00 | 1.05 | 1.10 |
| Z2 | 0.80 | 1.00 | 1.15 | 1.20 |
| Z3 | 0.80 | 1.00 | 1.20 | 1.30 |
| Z4 | 0.80 | 1.00 | 1.60 | 2.00 |

| Zona \ Suelo | S0 | S1 | S2 | S3 |
|---|---|---|---|---|
| **Tp (s)** | | | | |
| Todas | 0.30 | 0.40 | 0.60 | 1.00 |

| Zona \ Suelo | S0 | S1 | S2 | S3 |
|---|---|---|---|---|
| **TL (s)** | | | | |
| Todas | 3.00 | 2.50 | 2.00 | 1.60 |

#### Factor de Uso (U)

| Categoría | Descripción | U |
|---|---|---|
| A1 | Establecimientos de salud del Sector Salud (Nivel IV) | 1.50 |
| A2 | Establecimientos de salud no comprendidos en A1 | 1.50 |
| B | Instituciones educativas, comunicaciones, emergencia | 1.30 |
| C | Edificaciones comunes (vivienda, comercio, oficinas) | 1.00 |
| D | Almacenes (no esenciales) | 0.80 |

#### Factor de Reducción Básico (R0) por Sistema Estructural

| Sistema Estructural | R0 |
|---|---|
| Acero: Pórticos especiales a momento (SMF) | 8 |
| Acero: Pórticos intermedios a momento (IMF) | 7 |
| Acero: Pórticos ordinarios a momento (OMF) | 6 |
| Acero: Pórticos con arriostres excéntricos | 8 |
| Acero: Pórticos con arriostres especiales | 6 |
| Concreto Armado: Pórticos | 8 |
| Concreto Armado: Dual | 7 |
| Concreto Armado: Muros estructurales | 6 |
| Concreto Armado: Muros de ductilidad limitada | 4 |
| Albañilería armada o confinada | 3 |

#### Irregularidades (Ia, Ip)

**En altura** (factor Ia):

| Irregularidad | Factor |
|---|---|
| Irregularidad de rigidez — Piso blando | 0.75 |
| Irregularidades de resistencia — Piso débil | 0.75 |
| Irregularidad de masa o peso | 0.90 |
| Irregularidad geométrica vertical | 0.90 |
| Discontinuidad en los sistemas resistentes | 0.80 |
| Sin irregularidad | 1.00 |

**En planta** (factor Ip):

| Irregularidad | Factor |
|---|---|
| Irregularidad torsional | 0.75 |
| Irregularidad torsional extrema | 0.60 |
| Esquinas entrantes | 0.90 |
| Discontinuidad del diafragma | 0.85 |
| Sistemas no paralelos | 0.90 |
| Sin irregularidad | 1.00 |

### 1.3 Espectro de Diseño E030

El espectro de pseudo-aceleración Sa(T) se calcula como:

```
                Z · U · C · S
Sa(T) =  ─────────────────────
                    R

Donde C es el factor de amplificación sísmica:

  T < Tp:       C = 2.5
  Tp ≤ T ≤ TL:  C = 2.5 · (Tp/T)
  T > TL:       C = 2.5 · (Tp · TL / T²)

  Restricción: C/R ≥ 0.11
```

**Parámetros**:
- `Z` = Factor de zona
- `U` = Factor de uso
- `C` = Factor de amplificación sísmica
- `S` = Factor de suelo
- `R` = R0 · Ia · Ip (factor de reducción total)

### 1.4 Implementación en el Sistema

```
E030Tables (clase estática):
  GetZoneFactor(zone)          → Z
  GetSoilFactor(zone, soil)    → S
  GetTp(zone, soil)            → Tp
  GetTl(zone, soil)            → TL
  GetR0(structuralSystem)      → R0
  GetIrregularityFactor(...)   → Ia o Ip

E030UserControl:
  Combina los parámetros para calcular Sa(T)
  Genera la curva del espectro para EspectroUserControl

EspectroUserControl:
  Visualiza: Sa(T), Sd(T), pseudoaceleración
  3 gráficas en tiempo real que se actualizan al cambiar parámetros
```

---

## Parte II: ACI 318 — Concreto Estructural

### 2.1 Descripción

El **ACI 318** (Requisitos del Código para Concreto Estructural) es la norma de referencia para el diseño de elementos de concreto reforzado. El sistema implementa las verificaciones de diseño para vigas, columnas y muros de corte mediante las clases calculadoras.

### 2.2 Factores de Reducción de Resistencia (φ)

| Tipo de solicitación | Factor φ |
|---|---|
| Flexión (tensión controlada) | 0.90 |
| Cortante y torsión | 0.85 |
| Compresión con espiral | 0.75 |
| Compresión con estribos | 0.65 |
| Aplastamiento | 0.65 |

### 2.3 Diseño de Vigas (`BeamDesignCalculator`)

#### Cuantías Límite

```
Factor β1:
  f'c ≤ 280 kg/cm²: β1 = 0.85
  f'c > 280 kg/cm²: β1 = 0.85 - 0.05·(f'c - 280)/70   (mínimo 0.65)

Cuantía balanceada:
  ρb = 0.85·β1·(f'c/fy)·[6120/(6120+fy)]

Cuantía mínima:
  ρ_min = máx(0.25√f'c/fy,  1.4/fy)

Cuantía máxima:
  ρ_max = 0.75·ρb
```

#### Diseño por Flexión

```
Momento último: Mu (ton·m)  [de SAP2000]

Área de acero requerida (aproximación iterativa):
  Rn = Mu / (φ·b·d²)
  ρ = (0.85·f'c/fy)·[1 - √(1 - 2·Rn/(0.85·f'c))]
  As,req = ρ·b·d

Verificación:
  φMn = φ·As,prov·fy·(d - As,prov·fy/(1.7·f'c·b))  ≥ Mu  ✓/✗
```

#### Diseño por Cortante

```
Resistencia del concreto:
  Vc = 0.53·√f'c·b·d    (ton, f'c en kg/cm²)

Resistencia del acero (estribos):
  Vs = Av·fy·d / s       (Av: área de estribos, s: separación)

Verificación:
  φ·(Vc + Vs) ≥ Vu    ✓/✗

Límites normativos:
  Vs ≤ 2.1·√f'c·b·d    (límite máximo de estribos)
  Separación máxima: d/2 (si Vu > φ·Vc/2)
```

### 2.4 Diseño de Columnas (`ColumnDesignCalculator`)

#### Diagrama de Interacción P-M

El diseño de columnas se verifica mediante diagramas de interacción que definen la envolvente de capacidad P-Mn:

```
Punto 1 (Compresión pura):
  Pn,0 = 0.85·f'c·(Ag - Ast) + Ast·fy

Punto 2 (Balance):
  c_b = 6120·d / (6120 + fy)
  a_b = β1·c_b
  Pn,b, Mn,b calculados con distribución lineal de deformaciones

Punto 3 (Flexión pura):
  Pn = 0
  Mn calculada como viga

Para cada combinación (Pu, Mu) del análisis:
  Verificar que el punto esté dentro de la envolvente de capacidad
  φPn(φ=0.65) ≥ Pu  Y  φMn ≥ Mu   ✓/✗
```

### 2.5 Diseño de Muros de Corte (`ShearWallDesignCalculator`)

#### Refuerzo Mínimo

```
Refuerzo horizontal mínimo:
  ρh ≥ 0.0025   (ACI 318-19 §11.6.1)
  Separación máxima estribos: min(lw/5, 3t, 450mm)

Refuerzo vertical mínimo:
  ρv ≥ max(0.0025, 0.0025 + 0.5·(2.5 - hw/lw)·(ρh - 0.0025))
```

#### Verificación de Cortante

```
Resistencia nominal al cortante:
  Vn = Acv·(αc·√f'c + ρt·fy)

  Donde:
  Acv = lw·tw (área del alma)
  αc = 0.25 para hw/lw ≤ 1.5
  αc = 0.17 para hw/lw ≥ 2.0
  ρt = cuantía horizontal

Verificación:
  φ·Vn ≥ Vu     (φ = 0.75)
  Vn ≤ 0.83·√f'c·Acv  (límite superior)
```

#### Extremos Confinados

```
Se requieren extremos confinados cuando:
  c > lw / (600·(δu/hw))

  Donde δu/hw es la deriva de diseño
  En zonas sísmicas altas, generalmente se usan cuando:
  δu/hw > 0.007  y  c > 0.1·lw
```

---

## Parte III: Combinaciones de Carga

Las combinaciones de carga implementadas son las establecidas por ACI 318 para diseño por resistencia (LRFD):

| ID | Combinación | Descripción |
|---|---|---|
| D1 | 1.4·CM | Carga muerta amplificada |
| D2 | 1.2·CM + 1.6·CV | Cargas gravitacionales |
| D3 | 1.2·CM + 1.0·CV + 1.0·E | Con sismo X |
| D4 | 1.2·CM + 1.0·CV - 1.0·E | Con sismo X negativo |
| D5 | 1.2·CM + 1.0·CV + 1.0·Ey | Con sismo Y |
| D6 | 1.2·CM + 1.0·CV - 1.0·Ey | Con sismo Y negativo |
| D7 | 0.9·CM + 1.0·E | Verificación de levantamiento |
| D8 | 0.9·CM - 1.0·E | Verificación de levantamiento |

---

## Parte IV: Verificaciones Normativas Implementadas

### Resumen de Verificaciones

| Elemento | Tipo | Norma | Verificación |
|---|---|---|---|
| Viga | Flexión | ACI 318 | φMn ≥ Mu |
| Viga | Cortante | ACI 318 | φVn ≥ Vu |
| Viga | Cuantía mínima | ACI 318 | ρ ≥ ρ_min |
| Viga | Cuantía máxima | ACI 318 | ρ ≤ ρ_max = 0.75·ρb |
| Columna | Interacción P-M | ACI 318 | Punto dentro de envolvente |
| Muro | Cortante | ACI 318 | φVn ≥ Vu |
| Muro | Refuerzo mínimo horiz. | ACI 318 | ρh ≥ 0.0025 |
| Muro | Refuerzo mínimo vert. | ACI 318 | ρv ≥ 0.0025 |
| Estructura | Derivas | E030 | Δ/h ≤ límite por material |
| Estructura | Masa modal acumulada | E030 | ΣMx, ΣMy ≥ 90% |

### Límites de Deriva E030

| Sistema estructural | Límite de deriva inelástica |
|---|---|
| Concreto armado | 0.007 |
| Acero | 0.010 |
| Madera | 0.010 |
| Albañilería reforzada/confinada | 0.005 |
| Edificios con muros de ductilidad limitada | 0.005 |

---

## Parte V: Unidades del Sistema

El sistema trabaja consistentemente con las siguientes unidades:

| Magnitud | Unidad |
|---|---|
| Fuerza | Toneladas (ton) |
| Momento | Toneladas-metro (ton·m) |
| Longitud (estructural) | Metros (m) |
| Longitud (sección) | Centímetros (cm) |
| Resistencia del concreto f'c | Kilogramos por cm² (kg/cm²) |
| Resistencia del acero fy | Kilogramos por cm² (kg/cm²) |
| Área de acero | Centímetros cuadrados (cm²) |
| Periodo | Segundos (s) |
| Deriva | Adimensional (relación Δ/h) |

> **Nota**: SAP2000 puede trabajar en diferentes sistemas de unidades. El adaptador `SapStructureOutputReader` y `SapDesignDataReader` son responsables de la conversión de unidades al extraer datos.

---

*Anterior: [Flujos de Trabajo](./FLUJOS_DE_TRABAJO.md) | Siguiente: [Guía de Inicio Rápido](./GUIA_DE_INICIO_RAPIDO.md)*
