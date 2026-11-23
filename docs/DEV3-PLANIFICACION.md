# Dev3 — Cuatro tablas de planificación financiera (Punto 1)

## Implementado

- Modelos `PresupuestoMensual`, `MetaAhorro`, `MovimientoRecurrente` y `RecordatorioPago`.
- Cuatro configuraciones independientes `IEntityTypeConfiguration<T>` en `Data/Configurations/`.
- Contexto mínimo `Data/FinanzasDbContext.cs`, nombre indicado por `Contributing.md`, con los cuatro `DbSet` y registro explícito de configuraciones.
- Tablas y columnas en singular y `snake_case`, siguiendo los nombres del README; identificadores `uuid`, importes `numeric(14,2)`, cadenas `varchar`, días/año `smallint`, fechas `date` y notas `text`.
- Índice único de presupuesto por usuario, categoría, año y mes; índices en las demás claves foráneas (el índice compuesto ya cubre el acceso por usuario de los presupuestos).
- Por petición expresa del autor, el año se denomina `Año` en C# y `año` en PostgreSQL, en lugar de `Anio`/`anio` del README original; conserva el tipo `short`/`smallint`.
- CHECK de mes, importes, estado de meta, tipo/frecuencia de recurrencia y día de ejecución.
- Valores iniciales CLR y PostgreSQL: meta `activa`, ahorro `0`, recurrencia activa y recordatorio no pagado.
- EF Core Design 10.0.15 y proveedor Npgsql EF Core 10.0.4. Se conserva OpenAPI existente.

## Relaciones y eliminación

| Entidad dependiente | Principal | FK | Opcional |
| --- | --- | --- | --- |
| PresupuestoMensual | Usuario | UsuarioId | No |
| PresupuestoMensual | Categoria | CategoriaId | No |
| MetaAhorro | Usuario | UsuarioId | No |
| MetaAhorro | Cuenta | CuentaVinculadaId | Sí |
| MovimientoRecurrente | Usuario | UsuarioId | No |
| MovimientoRecurrente | Cuenta | CuentaId | No |
| MovimientoRecurrente | Categoria | CategoriaId | No |
| RecordatorioPago | Usuario | UsuarioId | No |
| RecordatorioPago | MovimientoRecurrente | MovimientoRecurrenteId | Sí |

Todas usan `DeleteBehavior.Restrict`: no se elimina historial en cascada. Una relación opcional puede desvincularse explícitamente antes de borrar su principal.

`RelacionesDev3` configura relaciones sin navegación hacia entidades base **ya registradas** en el modelo. Si falta una, lanza un error explícito antes de crear una entidad sombra. No implementa ni modifica las entidades del líder. Solo se incluye navegación CLR hacia `MovimientoRecurrente`, que pertenece a Dev3.

## Convención de DiaEjecucion

- `semanal`: lunes=1 hasta domingo=7.
- `quincenal`: día ancla del mes entre 1 y 28; las siguientes ocurrencias se interpretarían cada 15 días desde la fecha ancla, no como dos días fijos del mes.
- `mensual`: día del mes entre 1 y 28.
- `anual`: día del mes entre 1 y 28; el mes de referencia está en `ProximaFecha`.

Estas son convenciones de almacenamiento, no un motor de ejecución. No se implementa cálculo de próximas fechas ni generación de movimientos.

## Migración: pendiente por dependencias externas

Al inspeccionar `upstream/develop` (`bdad26c`) no existen `Usuario`, `Cuenta`, `Categoria`, contexto base ni migraciones previas. Tampoco hay cadena PostgreSQL configurada en los archivos del proyecto.

**No se generó ni aplicó una migración. No se crearon tablas físicas ni se ejecutó SQL.**

El proyecto compila, pero el modelo EF de planificación está bloqueado intencionalmente hasta registrar las entidades reales del líder. `Program.cs` no se modifica ni registra un contexto incompleto. No se usa `EnsureCreated()`.

### Continuación tras integrar al líder

1. Integrar las entidades, configuraciones y migraciones base del líder. Conciliar el único `FinanzasDbContext` conservando los `DbSet` y configuraciones de ambos aportes; no mantener contextos duplicados.
2. Registrar/configurar `Usuario`, `Cuenta` y `Categoria` antes de aplicar las cuatro configuraciones Dev3. Verificar los nombres y mapeos reales del bloque base.
3. Conservar o completar el registro `AddDbContext`/`UseNpgsql` del líder con configuración local mediante variables de entorno, sin versionar secretos. No es necesario añadir una fábrica de diseño si el host integrado permite crear el contexto.
4. Verificar que el snapshot base representa las tablas del líder. Si no existe una migración/snapshot base válido, detenerse: no generar una inicial que mezcle aportes.
5. Usar una herramienta `dotnet-ef` 10.x compatible: la herramienta global encontrada en este entorno es 11.0.0 y no se modificó.
6. Solo entonces ejecutar:

   ```bash
   dotnet restore src/FinanzasPersonales.Api
   dotnet build src/FinanzasPersonales.Api
   dotnet ef migrations list --project src/FinanzasPersonales.Api --context FinanzasDbContext
   dotnet ef migrations add AgregarTablasDev3Planificacion --project src/FinanzasPersonales.Api --context FinanzasDbContext
   git diff -- src/FinanzasPersonales.Api/Migrations
   ```

7. Revisar también los nuevos archivos no rastreados de la migración: `Up` debe crear exclusivamente las cuatro tablas Dev3, sus FK, índices y CHECK, sin alterar tablas ajenas. Revisar el SQL entre la migración base y la nueva antes de aplicarlo.
8. Únicamente con una conexión confirmada a PostgreSQL **local de desarrollo**, ejecutar `dotnet ef database update --project src/FinanzasPersonales.Api --context FinanzasDbContext`. No ejecutar este comando ahora.

## Pruebas realizadas

- `dotnet restore src/FinanzasPersonales.Api`: correcto.
- `dotnet build src/FinanzasPersonales.Api`: correcto, **0 errores y 0 advertencias** tras actualizar los paquetes nuevos a los parches indicados.
- `dotnet list src/FinanzasPersonales.Api package --vulnerable --include-transitive`: sin paquetes vulnerables reportados por los orígenes actuales.
- Verificación temporal fuera del código versionado, compilada con el SDK existente sin crear otro proyecto .NET: valores iniciales correctos, recurrencia opcional y rechazo del modelo sin la entidad real `Usuario`. No abrió una conexión de base de datos.
- No se comprobaron restricciones/FK físicamente en PostgreSQL: falta la integración base.

## Alcance

Solo Punto 1, aporte Dev3. Sin CRUD, endpoints nuevos, autenticación, servicios automáticos, ajustes de saldos, soft delete, frontend ni despliegue. No se modifica el código existente de `Program.cs` ni los modelos de otros integrantes.

## Evidencias reproducibles

Capturar la salida real de:

```bash
git branch --show-current
git remote -v
git status --short
git diff --check
dotnet restore src/FinanzasPersonales.Api
dotnet build src/FinanzasPersonales.Api
```

Después del commit: `git show --stat HEAD` permite acreditar la autoría y los archivos del aporte. Antes del commit, `git diff` no incluye el contenido de archivos nuevos sin rastrear: revisarlos también en el editor o mediante `git diff --cached` después de agregarlos.

Solo después de aplicar una migración válida en desarrollo local, obtener evidencia SQL real (por ejemplo con `psql` conectado a esa base):

```sql
SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN ('presupuesto_mensual', 'meta_ahorro',
                     'movimiento_recurrente', 'recordatorio_pago');

SELECT conrelid::regclass AS tabla, conname, pg_get_constraintdef(oid)
FROM pg_constraint
WHERE conrelid IN ('public.presupuesto_mensual'::regclass,
                   'public.meta_ahorro'::regclass,
                   'public.movimiento_recurrente'::regclass,
                   'public.recordatorio_pago'::regclass)
ORDER BY tabla, conname;

SELECT tablename, indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public'
  AND tablename IN ('presupuesto_mensual', 'meta_ahorro',
                    'movimiento_recurrente', 'recordatorio_pago');
```

No presentar capturas de tablas o migraciones como realizadas mientras continúen pendientes.
