# Guía de Inicio Rápido

## Introducción

Esta guía te llevará paso a paso desde la instalación hasta la ejecución del primer flujo completo de análisis y exportación con Structural Management V1.

---

## Paso 1: Requisitos del Sistema

### Software Requerido

| Software | Versión Mínima | Versión Recomendada | Notas |
|---|---|---|---|
| Windows | 10 (64-bit) | 11 (64-bit) | Solo Windows |
| .NET Framework | 4.8 | 4.8 | Incluido en Windows 10/11 |
| SAP2000 | v25 | v25 | Requiere licencia activa |
| Visual Studio | 2019 | 2022 | Para desarrollo y compilación |
| Git | 2.x | 2.x | Para clonar el repositorio |

### Componentes Opcionales (para exportación Excel)

| Paquete NuGet | Versión | Propósito |
|---|---|---|
| ClosedXML | ≥ 0.102.x | Generación de archivos Excel .xlsx |
| DocumentFormat.OpenXml | ≥ 2.19.x | Soporte de formato OpenXml |

> **Nota**: Si SAP2000 no está instalado, la aplicación compila con advertencias sobre `SAP2000v1.dll` pero no puede conectarse al software de análisis. Las demás funcionalidades (gestión de proyectos, calculadoras, exportación) funcionan sin SAP2000.

---

## Paso 2: Instalación y Compilación

### 2.1 Clonar el repositorio

```bash
git clone https://github.com/jpenaherrerac/Structural_Management_V1.git
cd Structural_Management_V1
```

### 2.2 Abrir en Visual Studio

1. Abrir Visual Studio 2022
2. Seleccionar **"Abrir una solución o proyecto"**
3. Navegar a la carpeta clonada y seleccionar `Structural_Management_V1.slnx`
4. Visual Studio abrirá los 5 proyectos automáticamente

### 2.3 Restaurar dependencias NuGet

En Visual Studio:
- **Menú > Herramientas > Administrador de paquetes NuGet > Restaurar paquetes NuGet**

O desde la terminal:
```bash
dotnet restore App.WinForms/App.WinForms.csproj
```

### 2.4 Instalar ClosedXML (si no está restaurado)

```bash
# Desde la carpeta raíz del repositorio
cd App.Infrastructure
dotnet add package ClosedXML
dotnet add package DocumentFormat.OpenXml
```

O desde la consola NuGet en Visual Studio:
```powershell
Install-Package ClosedXML -ProjectName App.Infrastructure
```

### 2.5 Compilar el proyecto

```bash
dotnet build App.WinForms/App.WinForms.csproj
```

**Advertencias esperadas durante la compilación**:
```
Advertencia CS1574: Referencia a SAP2000v1.dll no encontrada
```
Esta advertencia es normal si SAP2000 no está instalado. La compilación debe completarse exitosamente.

### 2.6 Ejecutar la aplicación

```bash
cd App.WinForms/bin/Debug
App.WinForms.exe
```

O desde Visual Studio: presionar **F5** (con depuración) o **Ctrl+F5** (sin depuración).

---

## Paso 3: Primera Ejecución — Crear un Proyecto

### 3.1 Pantalla inicial

Al iniciar la aplicación, verás la ventana principal con:
- Barra de menú: **Archivo | Conectar**
- Barra de estado inferior: muestra estado del proyecto y conexión SAP2000

### 3.2 Crear un nuevo proyecto

1. Hacer clic en **Archivo > Nuevo Proyecto**
2. Completar el formulario:

```
Nombre:              Edificio Residencial Las Palmas
Código:              ERP-2024-001
Descripción:         Edificio de vivienda multifamiliar de 10 pisos
Ubicación:           Lima, Perú
Cliente:             Inmobiliaria Las Palmas S.A.C.
Código de Diseño:    ACI 318-19 + NTE E030-2018
Sistema Estructural: Dual (Pórticos + Muros)
N° de Pisos:         10
Altura Total (m):    30.0
Uso del Edificio:    Vivienda (Categoría C)
```

3. Hacer clic en **Aceptar**
4. La barra de estado mostrará: `Proyecto: ERP-2024-001`

---

## Paso 4: Conectar a SAP2000

> **Requisito**: SAP2000 v25 debe estar instalado y con licencia activa.

1. Hacer clic en **Conectar > Conectar SAP2000**
2. La aplicación intentará conectarse a la instancia de SAP2000 en ejecución
3. Si SAP2000 no está abierto, la aplicación lo intentará abrir automáticamente
4. La barra de estado mostrará: `SAP2000 v25.0.0 — Conectado`

### Solución de problemas de conexión

| Problema | Causa probable | Solución |
|---|---|---|
| Error: "No se puede conectar" | SAP2000 no está instalado | Instalar SAP2000 v25 |
| Error: "COM no registrado" | DLL SAP2000 no registrada | Reinstalar SAP2000 |
| Error: "Licencia no válida" | Licencia expirada o inválida | Renovar licencia SAP2000 |
| Conexión lenta | SAP2000 iniciando | Esperar ~30 segundos e intentar de nuevo |

---

## Paso 5: Abrir un Modelo SAP2000

1. Hacer clic en **Archivo > Abrir**
2. Navegar hasta el archivo `.sdb` o `.sap` del modelo
3. SAP2000 abrirá el modelo
4. La barra de estado confirmará el modelo abierto

---

## Paso 6: Ejecutar Análisis y Diseño en SAP2000

> **Nota**: Este paso se realiza en SAP2000, no en esta aplicación.

En SAP2000:
1. Verificar que el modelo esté correctamente definido (geometría, materiales, secciones, cargas)
2. Ejecutar el análisis: **Analyze > Run Analysis**
3. Ejecutar el diseño: **Design > Concrete Frame Design > Start Design/Check**

---

## Paso 7: Hidratar Resultados Sísmicos

Una vez que el análisis esté completo en SAP2000:

1. En Structural Management V1, hacer clic en **[Botón/Menú Hidratar Sísmica]**
2. La aplicación extraerá automáticamente:
   - Periodos y masas modales
   - Cortantes de historia por piso y dirección
   - Derivas de entrepiso
   - Cortante basal
3. Los datos se almacenan como `SeismicSource` asociada al proyecto activo
4. Un mensaje confirmará: `"Hidratación sísmica completada: N modos, M pisos"`

---

## Paso 8: Hidratar Resultados de Diseño

Una vez que el diseño esté completo en SAP2000:

1. Hacer clic en **[Botón/Menú Hidratar Diseño]**
2. La aplicación extraerá:
   - Datos de diseño de vigas (As,req, As,prov, φMn)
   - Datos de diseño de columnas (diagrama P-M, As total)
   - Datos de diseño de muros (ρh, ρv, φVn)
   - Fuerzas internas de todos los elementos
3. Los datos se almacenan como `DesignSource` asociada al proyecto activo

---

## Paso 9: Generar Anexos de Diseño

### 9.1 Configurar parámetros de materiales

Antes de calcular los anexos, configurar:
- **f'c** (resistencia del concreto): Ej. 280 kg/cm²
- **fy** (resistencia del acero): Ej. 4200 kg/cm²

### 9.2 Calcular verificaciones

La aplicación usa `BeamDesignCalculator`, `ColumnDesignCalculator` y `ShearWallDesignCalculator` para verificar el cumplimiento normativo de cada elemento.

### 9.3 Exportar resultados

**Exportar a Excel**:
1. Hacer clic en **[Exportar > Excel]**
2. Seleccionar la ruta de destino
3. Se generará un archivo `.xlsx` con hojas para Vigas, Columnas y Muros

**Exportar a CSV**:
1. Hacer clic en **[Exportar > CSV]**
2. Seleccionar la carpeta de destino
3. Se generarán archivos separados por tipo de elemento

**Exportar a XML**:
1. Hacer clic en **[Exportar > XML]**
2. Seleccionar la ruta de destino
3. Se generará un archivo XML estructurado

---

## Paso 10: Configurar Parámetros Sísmicos E030

1. Abrir el formulario de sismicidad: **[Menú > Sismicidad]**
2. En el control E030, seleccionar:
   - **Zona sísmica**: Z1, Z2, Z3 o Z4
   - **Tipo de suelo**: S0, S1, S2 o S3
   - **Factor de uso U**: según categoría del edificio
   - **Sistema estructural**: para obtener R0
   - **Irregularidades**: en planta y en altura
3. El espectro de diseño se actualizará automáticamente en las gráficas
4. El factor R = R0 × Ia × Ip se calculará y mostrará

---

## Estructura de Archivos de Salida

```
Exportaciones/
├── ERP-2024-001/
│   ├── Seismic/
│   │   ├── modal_results.csv        # Periodos y masas modales
│   │   ├── story_shears.csv         # Cortantes de historia
│   │   └── story_drifts.csv         # Derivas de entrepiso
│   └── Design/
│       ├── beam_design_annex.xlsx   # Anexo de vigas
│       ├── column_design_annex.xlsx # Anexo de columnas
│       ├── wall_design_annex.xlsx   # Anexo de muros
│       └── design_summary.xml       # Resumen XML
```

---

## Atajos de Teclado

| Atajo | Acción |
|---|---|
| `Ctrl+N` | Nuevo proyecto |
| `Ctrl+O` | Abrir modelo SAP2000 |
| `Ctrl+S` | Guardar modelo SAP2000 |
| `Alt+F4` | Salir |

---

## Preguntas Frecuentes

**¿Puedo usar la aplicación sin SAP2000?**
Sí. Las funcionalidades de gestión de proyectos, calculadoras de diseño y exportación no requieren SAP2000. Solo las funciones de hidratación de datos requieren la conexión activa.

**¿Se guardan los datos entre sesiones?**
En la versión actual (V1), los datos se almacenan en memoria y se pierden al cerrar la aplicación. Una versión futura implementará persistencia en base de datos.

**¿Qué versiones de SAP2000 son compatibles?**
La versión actual está optimizada para SAP2000 v25. Otras versiones pueden funcionar si la API COM es compatible, pero no están formalmente probadas.

**¿Puedo importar datos existentes?**
En la versión actual, los datos solo se obtienen directamente de SAP2000 mediante hidratación. La importación desde archivos externos está planificada para versiones futuras.

**¿El sistema soporta estructuras de acero?**
El sistema extrae datos de estructuras de acero desde SAP2000 (cortantes, derivas, periodos). Las calculadoras de diseño actuales (ACI 318) son para concreto. El soporte para diseño de acero está planificado.

---

## Soporte y Recursos

| Recurso | Descripción |
|---|---|
| [README.md](../README.md) | Resumen del proyecto |
| [ARQUITECTURA.md](./ARQUITECTURA.md) | Arquitectura del sistema |
| [CLASES_Y_ENTIDADES.md](./CLASES_Y_ENTIDADES.md) | Referencia de clases |
| [ESTANDARES_INGENIERILES.md](./ESTANDARES_INGENIERILES.md) | Normas implementadas |
| [INTEGRACION_SAP2000.md](./INTEGRACION_SAP2000.md) | Guía SAP2000 |

---

*Anterior: [Estándares de Ingeniería](./ESTANDARES_INGENIERILES.md) | Siguiente: [Integración SAP2000](./INTEGRACION_SAP2000.md)*
