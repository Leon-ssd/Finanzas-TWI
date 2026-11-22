# Guía de Contribución (`CONTRIBUTING.md`)

Esta guía define el flujo de trabajo obligatorio para aportar código al repositorio del **Sistema de Finanzas Personales** (.NET 10 + Entity Framework Core + PostgreSQL)[cite: 2, 3].

## 1. Regla fundamental: Sin colaboradores directos en el repositorio base

* **Ningún integrante** debe ser agregado como colaborador en el repositorio público del líder.
* Todo aporte se desarrolla en un **fork** personal y se integra únicamente mediante **Pull Requests (PR)** hacia la rama **`develop`** del repositorio base.

---

## 2. Configuración inicial del Fork (una sola vez)

1. Entra al repositorio público del líder en GitHub y haz clic en **Fork** para crear tu copia personal (`https://github.com/<TU_USUARIO_GITHUB>/finanzas-personales-api`).
2. Clona **tu fork** (`origin`) en tu equipo local:
   ```bash
   git clone [https://github.com/](https://github.com/)<TU_USUARIO_GITHUB>/finanzas-personales-api.git
   cd finanzas-personales-api
   ```
3. Vincula el repositorio base del líder como remoto `upstream`:
   ```bash
   git remote add upstream <URL_DEL_REPO_BASE>.git
   git remote -v
   ```
4. Cambia a la rama `develop` y sincronízala con `upstream` usando `git pull`:
   ```bash
   git checkout develop
   git pull upstream develop
   git push -u origin develop
   ```
   *(Si la rama `develop` aún no existe en tu copia local tras clonar, créala con `git checkout -b develop` y luego haz `git pull upstream develop` y `git push -u origin develop`).*

---

## 3. Actualizar `develop` y crear una rama de trabajo

Antes de empezar cualquier tarea, descarga los últimos cambios de `upstream/develop` con `git pull` y crea una rama nueva a partir de ella:

```bash
git checkout develop
git pull upstream develop
git push origin develop
git checkout -b <tipo>/<alcance-corto>
```

**Convención para nombrar ramas:**
* `feat/<modulo-o-tablas>` — Nuevos modelos, migraciones, servicios o endpoints (ej. `feat/tablas-planificacion`, `feat/login-discord-google`).
* `fix/<problema>` — Corrección de errores (ej. `fix/validacion-saldo-cuenta`).
* `docs/<tema>` — Cambios en documentación (ej. `docs/actualizar-readme`).
* `chore/<tarea>` — Ajustes técnicos o de configuración (ej. `chore/config-npgsql`).

---

## 4. Implementar tablas, migraciones de EF Core y probar cambios

Cada integrante debe implementar sus **4 tablas asignadas** siguiendo el enfoque *Code First*[cite: 3, 9]:

1. **Crear modelos y configuraciones:**
   * Agrega tus clases en `src/FinanzasPersonales.Api/Models/`.
   * Agrega sus configuraciones Fluent API en `src/FinanzasPersonales.Api/Data/Configurations/` y regístralas en `FinanzasDbContext.cs`[cite: 3, 8, 9].
2. **Compilar para verificar que no existan errores de sintaxis o tipos:**
   ```bash
   dotnet build src/FinanzasPersonales.Api/FinanzasPersonales.Api.csproj
   ```
3. **Generar la migración de EF Core y aplicarla a PostgreSQL:**
   ```bash
   dotnet ef migrations add <NombreDescriptivoMigracion> --project src/FinanzasPersonales.Api
   dotnet ef database update --project src/FinanzasPersonales.Api
   ```
   *Ejemplo de nombre de migración:* `AgregarTablasPlanificacion` o `AgregarTransferenciasYEtiquetas`.
   *(Si necesitas deshacer la última migración antes de subirla, usa `dotnet ef migrations remove --project src/FinanzasPersonales.Api`).*
4. **Ejecutar la API y probar los cambios localmente:**
   ```bash
   dotnet run --project src/FinanzasPersonales.Api
   ```
   Verifica que las tablas, índices, claves foráneas y restricciones `CHECK` se hayan creado correctamente en PostgreSQL[cite: 3, 6, 9] y que no queden credenciales ni archivos temporales sin ignorar[cite: 10].

---

## 5. Guardar commits y subir cambios con `git push`

1. Revisa los archivos modificados y agrégalos al área de preparación:
   ```bash
   git status
   git add .
   ```
2. Crea commits claros y descriptivos siguiendo el formato `<tipo>(<alcance>): <descripción en imperativo>`:
   ```bash
   git commit -m "feat(models): agregar modelos PresupuestoMensual, MetaAhorro, MovimientoRecurrente y RecordatorioPago"
   git commit -m "feat(migrations): generar migracion AgregarTablasPlanificacion"
   ```
   **Otros ejemplos válidos de commits:**
   * `feat(models): implementar Transferencia, PreferenciaUsuario, Etiqueta y MovimientoEtiqueta`
   * `feat(migrations): generar migracion inicial para tablas base`[cite: 9, 10]
   * `fix(config): corregir restriccion CHECK en monto de Movimiento`[cite: 5, 6]
   * `docs(readme): registrar reparto de proveedores de inicio de sesion`
3. Trae posibles cambios recientes de `upstream/develop` antes de subir para evitar conflictos:
   ```bash
   git pull upstream develop
   ```
4. Sube tu rama de trabajo a **tu fork** (`origin`):
   ```bash
   git push -u origin <tipo>/<alcance-corto>
   ```

---

## 6. Abrir Pull Request (PR) hacia `develop` y solicitar revisión

1. En GitHub, entra a tu fork y haz clic en **Contribute** → **Open pull request**.
2. Verifica que:
   * **base repository:** `<usuario-lider>/finanzas-personales-api` y **base branch:** `develop`.
   * **head repository:** `<tu-usuario>/finanzas-personales-api` y **compare branch:** `<tipo>/<alcance-corto>`.
3. Completa la descripción de la PR detallando:
   * Las tablas o funcionalidades implementadas (modelos, contexto, migración o controladores/servicios).
   * Los comandos ejecutados para probar la compilación, la base de datos o los flujos de autenticación.
4. **Asignación de revisor (`Reviewers`):**
   * **Si el autor de la PR es un integrante:** Asigna como revisor al **líder** del proyecto.
   * **Si el autor de la PR es el líder:** Asigna como revisor a **otro integrante** del equipo (ya que GitHub impide que el autor apruebe su propia PR).
5. **Correcciones e integración:**
   * Si el revisor solicita cambios, realiza los ajustes en tu rama local, haz `git commit` y súbelos con `git push` (la PR se actualizará automáticamente).
   * Una vez aprobada, el **líder** realiza el *merge* hacia `develop` en el repositorio base.
6. **Limpieza tras el merge:**
   ```bash
   git checkout develop
   git pull upstream develop
   git push origin develop
   git branch -d <tipo>/<alcance-corto>
   ```

---

## 7. Inicio de sesión con proveedores externos

Cada integrante implementa **dos botones funcionales de inicio de sesión** para la aplicación:
* **1 botón** con un proveedor de la **lista fácil** (`Discord`, `Twitch`, `Dropbox`).
* **1 botón** con un proveedor de la **lista media** (`Google`, `Microsoft`, `Facebook`, `Slack`, `Spotify`) o **difícil** (`Apple`, `X`, `Meta Advanced Access`).

### 7.1 Reglas antes de programar
1. **Sin repetir proveedores:** Los proveedores elegidos no deben repetirse entre integrantes del mismo equipo.
2. **Registro previo:** El reparto de proveedores de cada integrante debe quedar registrado en el repositorio (en el `README.md` mediante PR hacia `develop`) **antes de programar** la autenticación externa.

### 7.2 Organización de controladores y código por proveedor
* **Evita colisiones en `develop`:** Para que los integrantes no editen las mismas líneas ni generen conflictos de *merge* al abrir Pull Requests, cada persona debe crear sus propios controladores o servicios dedicados a los proveedores asignados (por ejemplo, un controlador o servicio por proveedor, o un controlador/servicio propio por integrante) en lugar de que todos modifiquen un único archivo compartido al mismo tiempo.
* **Rutas y responsabilidades separadas:** Cada proveedor debe contar con su propio flujo de inicio de sesión y retorno, manteniendo aislada su lógica para facilitar la revisión de la PR.

### 7.3 Variables de entorno (credenciales y URL de retorno)
* Configura las credenciales de cada proveedor (identificadores/claves de cliente y secretos) y la **URL de retorno** (*redirect / callback URI*) mediante **variables de entorno**[cite: 10].
* Asegúrate de que la URL de retorno configurada en el panel del proveedor coincida exactamente con la variable de entorno y con la ruta de retorno de la aplicación.
* Nunca escribas credenciales reales en el código ni las subas al repositorio[cite: 10].

### 7.4 Comportamiento obligatorio y pruebas de sesión
Cada uno de los dos botones implementados debe cumplir y probarse en el ciclo completo:
1. **Inicio de sesión real con el proveedor:** Al pulsar el botón, redirige al proveedor externo y completa el inicio de sesión real con dicho proveedor.
2. **Regreso y sesión autenticada:** Al regresar a la aplicación, el usuario queda autenticado.
3. **Identidad de la sesión:** La aplicación muestra la identidad de la sesión activa del usuario autenticado.
4. **Cerrar sesión:** Permite cerrar sesión y comprobar que la sesión finaliza y la identidad/sesión activa ya no permanece abierta.

### 7.5 Consideraciones para Meta Advanced Access
Si un integrante elige **Meta Advanced Access** como proveedor difícil:
* **Explicación del flujo de Meta:** Documenta cómo inicia el flujo, cómo se solicitan los permisos, cómo regresa la respuesta/autorización a la aplicación y cómo se establece la sesión del usuario.
* **Permisos que requieren acceso avanzado:** Identifica y documenta los permisos solicitados que requieren **Advanced Access** en Meta y explica para qué los necesita la aplicación.
* **Implementación y pruebas:** Implementa el flujo completo con variables de entorno para credenciales y URL de retorno, verificando el regreso a la aplicación, la visualización de la identidad de la sesión y el cierre de sesión.