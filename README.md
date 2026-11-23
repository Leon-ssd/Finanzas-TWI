# Sistema de Finanzas Personales (API REST — .NET 10 & PostgreSQL)

API REST multiusuario para la gestión de finanzas personales, desarrollada con **ASP.NET Core (.NET 10)**, **Entity Framework Core (Npgsql)** y **PostgreSQL** mediante enfoque *Code First* y migraciones[cite: 1, 2, 3].

## Flujo de colaboración y reglas del repositorio

* **Repositorio público base:** Administrado por el líder del proyecto. **Ningún integrante debe ser agregado como colaborador directo** en el repositorio base.
* **Rama de integración:** Los cambios y aportes se integran en la rama **`develop`** del repositorio base.
* **Trabajo por Fork y Pull Request:** Cada integrante realiza un *fork* del repositorio base, desarrolla en una rama de trabajo en su *fork* y envía sus cambios mediante **Pull Request (PR)** hacia `develop` en el repositorio base.
* **Revisión e integración:**
  * En las PR de los integrantes, el líder revisa, aprueba e integra en `develop`.
  * En las PR creadas por el propio líder, otro integrante registra su revisión y el líder realiza la integración en `develop`.

## Reparto de tablas (12 entidades — 4 por integrante)

Cada entidad tiene un único responsable para evitar colisiones y duplicidad de trabajo.

| Integrante (Rol / Usuario GitHub) | Tablas asignadas (4 por persona) | Alcance funcional |
| :--- | :--- | :--- |
| **Integrante 1** (`@usuario-dev1`) | `Usuario`, `Cuenta`, `Categoria`, `Movimiento`[cite: 1] | Autenticación/usuarios, cuentas (efectivo, banco, billetera digital), categorías y registro de ingresos/gastos[cite: 2, 4, 5]. |
| **Integrante 2** (`@usuario-dev2`) | `Transferencia`, `PreferenciaUsuario`, `Etiqueta`, `MovimientoEtiqueta` | Transferencias entre cuentas propias, preferencias de usuario y etiquetado transversal de movimientos. |
| **Integrante 3** (`@usuario-dev3`) | `PresupuestoMensual`, `MetaAhorro`, `MovimientoRecurrente`, `RecordatorioPago` | Límites mensuales por período, metas de ahorro, plantillas recurrentes y recordatorios de vencimiento. |

> **Nota:** Cada aporte de tablas debe incluir sus clases de modelo (`Models/`), configuración en `DbContext` / Fluent API (`Data/Configurations/`) y su migración de EF Core (`Migrations/`) o script SQL correspondiente[cite: 3, 9, 10].

## Modelo de datos — 12 tablas y sus campos

El sistema se organiza en 12 tablas con aislamiento por usuario (`usuario_id`), identificadores `uuid` (`Guid`), montos decimales (`numeric(14,2)` / `decimal`) y fechas en `date` (`DateOnly`) y `timestamptz` (`DateTimeOffset`)[cite: 3, 8, 9].

### Bloque 1: Integrante 1 (Líder) — Núcleo financiero y catálogos

* **`Usuario`** — Representa a cada persona registrada y aísla sus datos en el sistema[cite: 2, 4].
  * **Campos:** `id` (`uuid`, PK), `nombre` (`varchar(100)`), `email` (`varchar(150)`, único sin distinguir mayúsculas), `password_hash` (`varchar(255)`), `fecha_creacion` (`timestamptz`), `activo` (`boolean`, por defecto `true`)[cite: 3, 4].
  * **Impacto:** Filtra toda consulta y escritura por usuario autenticado; un usuario inactivo no puede iniciar sesión ni operar[cite: 2, 4].
* **`Cuenta`** — Almacena los depósitos de dinero de cada persona (efectivo, banco, billetera digital)[cite: 2, 4].
  * **Campos:** `id` (`uuid`, PK), `usuario_id` (`uuid`, FK → `usuario.id`), `nombre` (`varchar(100)`), `tipo` (`varchar(30)`: `efectivo`, `banco`, `billetera_digital`), `moneda` (`varchar(3)`, por defecto `'BOB'`), `saldo_inicial` (`numeric(14,2)`, $\ge 0$), `activa` (`boolean`, por defecto `true`)[cite: 3, 4].
  * **Impacto:** Su saldo se calcula a partir de `saldo_inicial` más ingresos, menos gastos y transferencias; si se archiva (`activa = false`), conserva su historial y no recibe nuevos movimientos[cite: 4, 6].
* **`Categoria`** — Clasifica los ingresos y gastos de cada usuario[cite: 4, 5].
  * **Campos:** `id` (`uuid`, PK), `usuario_id` (`uuid`, FK → `usuario.id`), `nombre` (`varchar(100)`), `tipo` (`varchar(10)`: `ingreso` o `gasto`), `limite_mensual` (`numeric(14,2)`, opcional, $\ge 0$ solo en gasto), `activa` (`boolean`, por defecto `true`)[cite: 3, 4].
  * **Impacto:** Evita nombres duplicados entre categorías activas del mismo usuario y tipo, y exige que el movimiento coincida en propietario y tipo (`ingreso` o `gasto`)[cite: 5].
* **`Movimiento`** — Registra cada ingreso o gasto real (incluyendo sueldos ingresados manualmente)[cite: 2, 5, 7].
  * **Campos:** `id` (`uuid`, PK), `usuario_id` (`uuid`, FK), `cuenta_id` (`uuid`, FK), `categoria_id` (`uuid`, FK), `tipo` (`varchar(10)`: `ingreso` o `gasto`), `monto` (`numeric(14,2)`, $> 0$), `concepto` (`varchar(200)`), `fecha_movimiento` (`date`), `notas` (`text`, opcional), `fecha_registro` (`timestamptz`)[cite: 3, 5].
  * **Impacto:** Alimenta el historial, los totales mensuales, los gráficos por categoría y el cálculo del saldo de cada cuenta[cite: 6, 7, 8].

### Bloque 2: Integrante 2 — Transferencias, preferencias y etiquetas

* **`Transferencia`** — Registra el envío de dinero entre dos cuentas propias del mismo usuario.
  * **Campos:** `id` (`uuid`, PK), `usuario_id` (`uuid`, FK), `cuenta_origen_id` (`uuid`, FK → `cuenta.id`), `cuenta_destino_id` (`uuid`, FK → `cuenta.id`), `monto` (`numeric(14,2)`, $> 0$), `comision` (`numeric(14,2)`, $\ge 0$, por defecto `0`), `fecha_transferencia` (`date`), `concepto` (`varchar(200)`), `fecha_registro` (`timestamptz`).
  * **Impacto:** Ajusta los saldos de la cuenta origen (`-monto - comision`) y destino (`+monto`) sin crear filas en `Movimiento` para no inflar ingresos ni gastos.
* **`PreferenciaUsuario`** — Guarda configuraciones personales y de interfaz separadas de `Usuario`.
  * **Campos:** `id` (`uuid`, PK), `usuario_id` (`uuid`, FK única → `usuario.id`), `moneda_principal` (`varchar(3)`, por defecto `'BOB'`), `dia_inicio_mes` (`smallint`, `1` a `28`, por defecto `1`), `alerta_limite_porcentaje` (`smallint`, `1` a `100`, por defecto `80`), `tema_ui` (`varchar(20)`, por defecto `'claro'`).
  * **Impacto:** Define el ciclo mensual de consulta y el umbral de alerta de límites sin modificar el saldo de las cuentas.
* **`Etiqueta`** — Define etiquetas transversales por usuario para agrupar gastos o ingresos (por ejemplo, *Viaje* o *Reembolsable*).
  * **Campos:** `id` (`uuid`, PK), `usuario_id` (`uuid`, FK → `usuario.id`), `nombre` (`varchar(50)`), `color_hex` (`varchar(7)`), `activa` (`boolean`, por defecto `true`).
  * **Impacto:** Permite filtrar y agrupar movimientos de distintas categorías bajo un mismo tema sin alterar los saldos.
* **`MovimientoEtiqueta`** — Tabla intermedia (`N:M`) entre `Movimiento` y `Etiqueta`.
  * **Campos:** `movimiento_id` (`uuid`, PK, FK → `movimiento.id`), `etiqueta_id` (`uuid`, PK, FK → `etiqueta.id`).
  * **Impacto:** Vincula uno o varios tags a un movimiento; al eliminar un movimiento, se limpia su relación sin afectar el saldo de la cuenta.

### Bloque 3: Integrante 3 — Planificación, metas y recurrencia

* **`PresupuestoMensual`** — Registra límites de gasto por categoría para un año y mes específicos.
  * **Campos:** `id` (`uuid`, PK), `usuario_id` (`uuid`, FK), `categoria_id` (`uuid`, FK → `categoria.id`), `anio` (`smallint`), `mes` (`smallint`, `1` a `12`), `monto_limite` (`numeric(14,2)`, $\ge 0$).
  * **Impacto:** Permite mantener límites históricos o puntuales por mes (complementando `limite_mensual` de `Categoria`[cite: 4, 7]) sin alterar saldos reales.
* **`MetaAhorro`** — Representa objetivos de ahorro personales con avance registrado.
  * **Campos:** `id` (`uuid`, PK), `usuario_id` (`uuid`, FK), `cuenta_vinculada_id` (`uuid`, opcional, FK → `cuenta.id`), `nombre` (`varchar(100)`), `monto_objetivo` (`numeric(14,2)`, $> 0$), `monto_ahorrado` (`numeric(14,2)`, $\ge 0$, por defecto `0`), `fecha_limite` (`date`, opcional), `estado` (`varchar(20)`: `activa`, `completada`, `pausada`).
  * **Impacto:** Muestra el progreso hacia metas de ahorro en el panel sin bloquear ni duplicar el saldo real de `Cuenta`.
* **`MovimientoRecurrente`** — Almacena plantillas de ingresos o gastos periódicos (sueldo, alquiler, suscripciones)[cite: 2].
  * **Campos:** `id` (`uuid`, PK), `usuario_id` (`uuid`, FK), `cuenta_id` (`uuid`, FK), `categoria_id` (`uuid`, FK), `tipo` (`varchar(10)`: `ingreso` o `gasto`), `monto_estimado` (`numeric(14,2)`, $> 0$), `concepto` (`varchar(200)`), `frecuencia` (`varchar(20)`: `semanal`, `quincenal`, `mensual`, `anual`), `dia_ejecucion` (`smallint`), `proxima_fecha` (`date`), `activo` (`boolean`, por defecto `true`).
  * **Impacto:** Sirve como plantilla para proyectar pagos/ingresos y generar un `Movimiento` confirmado cuando corresponda, avanzando `proxima_fecha`.
* **`RecordatorioPago`** — Agenda vencimientos de pagos puntuales o asociados a una recurrencia.
  * **Campos:** `id` (`uuid`, PK), `usuario_id` (`uuid`, FK), `movimiento_recurrente_id` (`uuid`, opcional, FK → `movimiento_recurrente.id`), `titulo` (`varchar(120)`), `monto_estimado` (`numeric(14,2)`, opcional, $\ge 0$), `fecha_vencimiento` (`date`), `pagado` (`boolean`, por defecto `false`), `fecha_pago` (`date`, opcional), `notas` (`text`, opcional).
  * **Impacto:** Muestra avisos de vencimientos próximos o pendientes sin modificar por sí solo el saldo de ninguna cuenta.

## Reparto de inicio de sesión externo (2 proveedores por integrante)

Registrado **antes de programar**. Cada integrante implementa **2 botones funcionales**: **1 de lista fácil** y **1 de lista media o difícil**, sin repetir proveedores entre integrantes del equipo.

* **Lista fácil:** Discord, Twitch, Dropbox.
* **Lista media:** Google, Microsoft, Facebook, Slack, Spotify.
* **Lista difícil:** Apple, X, Meta Advanced Access.

| Integrante | Proveedor fácil | Proveedor medio o difícil |
| :--- | :--- | :--- |
| **Integrante 1** (`@usuario-dev1`) | Discord | Google |
| **Integrante 2** (`@usuario-dev2`) | Twitch | Spotify |
| **Integrante 3** (`@usuario-dev3`) |||

> Las credenciales de cada proveedor y las URL de retorno se configuran mediante **variables de entorno** (no se escriben credenciales en el código ni se suben al repositorio)[cite: 10].

## Requisitos y ejecución local

1. Tener instalado **.NET 10 SDK** y **PostgreSQL**[cite: 2, 3].
2. Configurar la cadena de conexión a PostgreSQL y las variables de entorno de autenticación mediante variables de entorno o configuración local no versionada (sin subir credenciales sensibles)[cite: 9, 10].
3. Restaurar dependencias y aplicar migraciones[cite: 9, 10]:
   ```bash
   dotnet restore
   dotnet ef database update --project src/FinanzasPersonales.Api
   ```
4. Ejecutar la API:
   ```bash
   dotnet run --project src/FinanzasPersonales.Api
   ```

## Documentación de contribución

Consulta la guía completa en [CONTRIBUTING.md](CONTRIBUTING.md).