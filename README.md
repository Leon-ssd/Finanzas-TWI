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

## Reparto de inicio de sesión externo (2 proveedores por integrante)

Registrado **antes de programar**. Cada integrante implementa **2 botones funcionales**: **1 de lista fácil** y **1 de lista media o difícil**, sin repetir proveedores entre integrantes del equipo.

* **Lista fácil:** Discord, Twitch, Dropbox.
* **Lista media:** Google, Microsoft, Facebook, Slack, Spotify.
* **Lista difícil:** Apple, X, Meta Advanced Access.

| Integrante | Proveedor fácil | Proveedor medio o difícil |
| :--- | :--- | :--- |
| **Integrante 1** (`@usuario-dev1`) | Discord | Google |
| **Integrante 2** (`@usuario-dev2`) |||
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