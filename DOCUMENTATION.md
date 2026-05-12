# OpenSteam Manager - Documentación de la Aplicación


## 2. Componentes Principales de la Interfaz de Usuario (UI)

### 2.1. `App.xaml.cs`
*   **Propósito:** Es el punto de entrada principal de la aplicación WPF.
*   **Funcionalidad:** Inicializa la aplicación y carga la ventana principal (`MainWindow.xaml`) al inicio. Contiene la lógica básica de la aplicación.

### 2.2. `MainWindow.xaml.cs`
*   **Propósito:** Controla la ventana principal de la aplicación, sirviendo como el centro de navegación y control.
*   **Funcionalidad:**
    *   Muestra el estado actual del sistema (parcheado/no parcheado) y la versión de la aplicación.
    *   Inicia la comprobación de actualizaciones y la obtención de noticias al cargar.
    *   Maneja la lógica de auto-parcheo al inicio si está configurado.
    *   Proporciona botones para acceder a diversas funcionalidades:
        *   **Patch/Delete Patch:** Aplica o elimina parches de Steam.
        *   **Restart Steam:** Reinicia el cliente de Steam.
        *   **Millennium + Plugin:** Instala el plugin Millennium.
        *   **LuaLoader:** Permite la carga manual de archivos Lua.
        *   **Extra:** Abre la ventana de herramientas adicionales.
        *   **OnlineLua Store:** Abre la tienda de scripts Lua online.
        *   **Manager:** Abre la ventana de gestión de scripts Lua instalados.
        *   **Information:** Abre la ventana de información de la aplicación.
    *   Gestiona la visualización del panel de configuración, permitiendo al usuario modificar ajustes como el auto-parcheo, el cierre de Steam antes del parcheo, la desactivación del Web Helper y las alertas NSFW.
    *   Implementa animaciones de desvanecimiento para la transición entre el menú principal y el panel de configuración.
    *   Maneja el arrastre de la ventana y el cierre de la aplicación.

### 2.3. `Extra.xaml.cs`
*   **Propósito:** Proporciona acceso a herramientas externas relacionadas con Steam.
*   **Funcionalidad:**
    *   Muestra una lista de herramientas como SteamCMD, Nightlight Game Launcher, CreamInstaller, Online-fix.me y Steam Achievement Manager.
    *   Al hacer clic, abre el navegador web a la URL correspondiente para descargar o acceder a la información de la herramienta.
    *   Maneja el arrastre y el cierre de la ventana.

### 2.4. `Information.xaml.cs`
*   **Propósito:** Muestra información sobre la versión de la aplicación y los créditos.
*   **Funcionalidad:**
    *   Al cargar, obtiene y muestra la versión actual de la aplicación.
    *   Maneja el arrastre y el cierre de la ventana.

### 2.5. `LibrarySteam.xaml.cs`
*   **Propósito:** Permite la gestión de scripts Lua instalados en la biblioteca de Steam.
*   **Funcionalidad:**
    *   Detecta la ruta de instalación de Steam y crea la carpeta `stplug-in` si no existe.
    *   Lista los archivos `.lua` encontrados en la carpeta `stplug-in`.
    *   Para cada archivo Lua, intenta obtener el nombre del juego asociado desde los archivos `appmanifest_*.acf` de Steam o mediante web scraping de la tienda de Steam.
    *   Permite eliminar scripts Lua seleccionados.
    *   Permite abrir la página de la tienda de Steam para los juegos seleccionados.
    *   Maneja el arrastre y el cierre de la ventana.

### 2.6. `OnlineLua.xaml.cs`
*   **Propósito:** Proporciona una interfaz para buscar e instalar scripts Lua para juegos desde una fuente online.
*   **Funcionalidad:**
    *   Descarga una lista de juegos desde una API externa al iniciar.
    *   Permite al usuario buscar juegos por AppID o nombre.
    *   Muestra advertencias si un juego está marcado como NSFW o tiene DRM (si las alertas no están deshabilitadas en la configuración).
    *   Genera e instala el script Lua para el juego seleccionado.
    *   Proporciona un botón para acceder a información sobre cómo solucionar problemas específicos (ej. "Fix65432").
    *   Maneja el arrastre y el cierre de la ventana.

### 2.7. `NotificationWindow.xaml.cs`
*   **Propósito:** Muestra mensajes de notificación temporales al usuario.
*   **Funcionalidad:**
    *   Muestra un mensaje dado por un número específico de segundos.
    *   Se cierra automáticamente después del tiempo especificado con una animación de desvanecimiento.

## 3. Capa de Servicios (Service Layer)

### 3.1. `Service/Attach.cs`
*   **Propósito:** Gestiona la aplicación y eliminación de parches en la instalación de Steam.
*   **Funcionalidad:**
    *   **Parchear:** Descarga un archivo `inject.zip` desde un repositorio de GitHub, lo extrae en la ruta de Steam y muestra una notificación de éxito.
    *   **Desparchear:** Lee un archivo `OpenSteamDel.json` para determinar qué archivos deben eliminarse. Si el JSON no existe o falla, intenta eliminar un conjunto predefinido de DLLs (`xinput1_4.dll`, `hid.dll`, `dwmapi.dll`). Muestra una notificación de éxito.
    *   Utiliza una instancia `static readonly HttpClient` para las solicitudes web.

### 3.2. `Service/Game.cs`
*   **Propósito:** Define el modelo de datos para un objeto de juego.
*   **Funcionalidad:**
    *   Contiene propiedades para `appid`, `name`, `type`, `tags`, `nsfw` (no seguro para el trabajo) y `drm`.
    *   Incluye una propiedad calculada `IsDemo` para identificar si un juego es una demo.

### 3.3. `Service/LuaLoaders.cs`
*   **Propósito:** Maneja la carga de scripts Lua, tanto localmente como desde fuentes online.
*   **Funcionalidad:**
    *   **`Load(string path)`:** Permite al usuario seleccionar un archivo `.lua` localmente y lo copia a la carpeta `stplug-in` de Steam.
    *   **`SteamLuaGenerator(int appId, string path, int cacheDays)`:** Genera un script Lua para un `appId` dado.
        *   Obtiene información de los depósitos de una API externa (`api.steamproof.net`).
        *   Obtiene claves de depósitos de un repositorio de GitLab, con un mecanismo de caché local.
        *   Construye el contenido del archivo Lua con llamadas `addappid` y `setManifestid`.
        *   Utiliza una instancia `static readonly HttpClient` para las solicitudes web.
    *   **`OnlineLoad(string ID, string path)`:** Orquesta la instalación de un script Lua online.
        *   Llama a `SteamLuaGenerator` para crear el archivo Lua.
        *   Llama a `SteamUtils.FixManifests` para asegurar que los manifiestos estén correctos.
        *   Muestra una notificación de éxito y reinicia Steam.
        *   Utiliza una instancia `readonly HttpClient` para las solicitudes web.

### 3.4. `Service/Plugins.cs`
*   **Propósito:** Gestiona la instalación de plugins específicos.
*   **Funcionalidad:**
    *   **`ManagePluginsInstall()`:** Descarga y ejecuta el instalador de Millennium desde GitHub. Espera a que el instalador finalice y muestra una notificación.
    *   **`LuaManagerInstallerAsync(string steamPath)`:** Descarga un archivo `LuaManager.zip` desde GitHub, lo extrae en la carpeta de plugins de Steam y muestra una notificación.
    *   Utiliza una instancia `static readonly HttpClient` para las solicitudes web.

### 3.5. `Service/SettingsFunction.cs`
*   **Propósito:** Proporciona funciones de utilidad para la configuración de Steam.
*   **Funcionalidad:**
    *   **`CleanSteamCache()`:** Elimina la carpeta `appcache` de Steam.
    *   **`OpenFolder()`:** Abre la carpeta de instalación de Steam en el explorador de archivos.
    *   **`BackupSteamConfig()`:** Crea una copia de seguridad de la carpeta `config` de Steam en un directorio local.
    *   Todas las funciones obtienen la ruta de Steam a través de `SteamUtils.GetSteamPath()`.

### 3.6. `Service/SteamUtils.cs`
*   **Propósito:** Contiene funciones de utilidad fundamentales relacionadas con la detección de Steam, la gestión de listas de juegos y la corrección de manifiestos.
*   **Funcionalidad:**
    *   **`Reset()`:** Cierra todos los procesos de Steam y luego lo reinicia, opcionalmente con parámetros para deshabilitar el Web Helper.
    *   **`GetSteamPath()`:** Detecta la ruta de instalación de Steam leyendo el registro de Windows, con un fallback a una ruta predeterminada. La ruta detectada se almacena en caché para evitar búsquedas repetidas.
    *   **`DownloadGameListAsync()`:** Descarga una lista de juegos en formato JSON desde una URL de GitHub. Implementa un mecanismo de caché diario para evitar descargas redundantes.
    *   **`DeserializeGames(string json)`:** Deserializa una cadena JSON en una lista de objetos `Game`.
    *   **`GetFilteredGames(string searchInput, List<Game> fullGameList)`:** Filtra la lista completa de juegos basándose en un `AppID` numérico o un nombre de juego (búsqueda insensible a mayúsculas y minúsculas).
    *   **`FixManifests(string steamPath)`:** Examina los archivos Lua instalados, identifica los manifiestos de depósitos faltantes o desactualizados, descarga los manifiestos necesarios de una API externa (`api.steamproof.net`) y actualiza los archivos Lua con la información de los manifiestos.
    *   Utiliza una instancia `static readonly HttpClient` para las solicitudes web.

### 3.7. `Service/Update.cs`
*   **Propósito:** Gestiona la comprobación de actualizaciones de la aplicación, la descarga, la instalación y la obtención de noticias.
*   **Funcionalidad:**
    *   **`GetVersion()`:** Obtiene la versión actual de la aplicación.
    *   **`CheckForUpdates()`:** Compara la versión actual con la última versión disponible en un archivo `version.txt` en GitHub. Si hay una nueva versión, pregunta al usuario si desea actualizar.
    *   **`DownloadAndInstallUpdate()`:** Descarga la nueva versión del ejecutable desde GitHub y crea un script por lotes (`.bat`) para reemplazar el ejecutable actual y reiniciar la aplicación.
    *   **`GetNews()`:** Descarga noticias desde un archivo en GitHub. Almacena la última noticia mostrada en un archivo temporal para evitar mostrar la misma noticia repetidamente.
    *   Utiliza una instancia `static readonly HttpClient` para las solicitudes web.

## 4. Configuración y Recursos

### 4.1. `Properties/Settings.settings` y `Properties/Settings.Designer.cs`
*   **Propósito:** Almacenan la configuración de usuario de la aplicación.
*   **Funcionalidad:** Define propiedades como `DeleteOnClose`, `AutoPatchLaunch`, `CloseSteamBefore`, `DisableWebHelper` y `DisableNFSWAlert`, que controlan el comportamiento de la aplicación. `Settings.Designer.cs` es un archivo generado automáticamente que proporciona acceso fuertemente tipado a estas configuraciones.

### 4.2. `Properties/Resources.resx` y `Properties/Resources.Designer.cs`
*   **Propósito:** Almacenan recursos de la aplicación como cadenas, imágenes, iconos, etc.
*   **Funcionalidad:** `Resources.resx` es el archivo XML donde se definen los recursos, y `Resources.Designer.cs` es un archivo generado automáticamente que proporciona acceso fuertemente tipado a estos recursos.
