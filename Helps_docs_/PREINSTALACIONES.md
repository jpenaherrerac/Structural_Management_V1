# PREINSTALACIONES

Procedimiento de instalación de `ClosedXML` para proyecto .NET Framework4.8 (no SDK) usando `packages.config`.

Requisitos previos:
- Visual Studio con NuGet Package Manager habilitado.
- Proyecto `Arbol_de_Cargas` abierto.

Pasos en la Consola del Administrador de Paquetes (PMC):
- Tools ? NuGet Package Manager ? Package Manager Console
- Asegura `Default project` = `Arbol_de_Cargas` (o usa `-ProjectName Arbol_de_Cargas`)
- Ejecuta:
 - Install-Package ClosedXML -ProjectName Arbol_de_Cargas

Luego instala dependencia requerida:
- Install-Package DocumentFormat.OpenXml -ProjectName Arbol_de_Cargas

Restore y compilación:
- Build ? Restore NuGet Packages ? Rebuild Solution

Verificación:
- Confirma que `packages.config` esté incluido bajo el proyecto en el Solution Explorer.
- Compila. `using ClosedXML.Excel;` debe resolverse en `Program.cs`.
