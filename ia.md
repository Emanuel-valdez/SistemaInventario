# Declaración de uso de IA

**Estudiante:** Emanuel Valdez · Matrícula 2024-0820
**Asignatura:** ISW-124 Programación Avanzada · Proyecto 1 (individual)
**Proyecto:** Sistema de Inventario (C#, WinForms, SQL Server)


## Herramienta utilizada

Claude (Anthropic), a través de la interfaz de chat. Una sola herramienta, en una conversación durante el desarrollo.

## 1. Consulta de conceptos

Usé la IA para entender conceptos antes de aplicarlos:

- Patrón Repository genérico (`IRepositorio<T>` y `RepositorioBase<T>`).
- Patrón Singleton para la conexión a la base de datos.
- Inyección de dependencias por constructor.
- Genéricos y métodos de extensión en C#.
- Consultas LINQ (`GroupBy`, `Average`, `MaxBy`, `GroupJoin`).
- Borrado lógico (soft-delete) y diseño de la Papelera.
- Validaciones con `ErrorProvider`.

## 2. Depuración de errores

Usé la IA para diagnosticar y corregir estos errores:

| Error | Solución aplicada |
|---|---|
| El proyecto Presentacion se creó como biblioteca de clases y no como aplicación WinForms | Edición del `.csproj` (`OutputType`, `TargetFramework`, `UseWindowsForms`) |
| Conflicto de versiones .NET 8 y .NET 10 entre proyectos | Unificación de todos los proyectos a `net10.0` |
| Ambigüedad del nombre `Application` | Uso del nombre completo `System.Windows.Forms.Application` |
| "Could not load Microsoft.Data.SqlClient" en ejecución | Instalación del paquete NuGet también en Presentacion |
| Carpeta de solución duplicada | Reorganización de las carpetas |
| Texto cortado en las gráficas de Reportes con la escala de Windows al 125 % / 150 % | Ajuste del diseño para que no dependa de anchos fijos |

 Emanuel Valdez 
