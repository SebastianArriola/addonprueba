# AddonPrueba – SAP Business One Addon

Addon para **SAP Business One 10.0 (SP 2408, 64-bit)** que aplica automáticamente
un **15% de descuento** en las líneas de un Pedido de Venta cuyo código de artículo
comience con **`TEST`**.

---

## Características

| Item | Detalle |
|---|---|
| SAP B1 | 10.0 (10.00.260 SP 2408) 64-bit |
| .NET | Framework 4.8 |
| Plataforma | x64 |
| Interfaz | Sin ventana visible (auto-close / hidden WinForms) |
| Trigger | Pedido de Venta → columna **ItemCode** pierde el foco |
| Lógica | Si `ItemCode` empieza con `TEST` → `DiscPrcnt = 15` |

---

## Estructura del proyecto

```
AddonPrueba/
├── AddonPrueba.sln              # Solución Visual Studio
├── Build-Package.ps1            # Script de compilación y empaquetado
└── AddonPrueba/
    ├── AddonPrueba.csproj       # Proyecto C# (.NET 4.8, WinExe, x64)
    ├── AddonPrueba.b1s          # Manifiesto del addon (Extension Manager)
    ├── Program.cs               # Punto de entrada
    ├── MainForm.cs              # Formulario oculto + lógica de eventos SAP
    ├── Properties/
    │   └── AssemblyInfo.cs
    └── lib/
        ├── README.md            # Instrucciones para obtener el SDK
        └── SAPbouiCOM.dll       # ← DEBES copiar esta DLL (no incluida en el repo)
```

---

## Requisitos previos

1. **Visual Studio 2019 / 2022** (o Build Tools for .NET Framework).
2. **SAP Business One 10.0** instalado en la misma máquina de desarrollo.
3. Copiar `SAPbouiCOM.dll` a `AddonPrueba\lib\`:
   - Ubicación típica: `C:\Program Files\SAP\SAP Business One\SAPbouiCOM.dll`
   - Ver `AddonPrueba\lib\README.md` para más detalles.

---

## Compilar y empaquetar

Abre PowerShell en la raíz del repositorio y ejecuta:

```powershell
.\Build-Package.ps1
```

Esto compilará el proyecto en **Release|x64** y generará **`AddonPrueba.zip`** listo
para importar en el Extension Manager.

---

## Instalar en SAP Business One

1. Abre SAP Business One 10.0.
2. Ve a **Administración → Extension Manager**.
3. Haz clic en **Importar** y selecciona `AddonPrueba.zip`.
4. Asigna el addon a la(s) sociedad(es) deseada(s).
5. Reinicia SAP B1; el addon se cargará automáticamente.

---

## Comportamiento en tiempo de ejecución

1. SAP B1 lanza `AddonPrueba.exe` pasándole la cadena de conexión como argumento.
2. El addon se conecta a SAP B1 mediante la **UI API** (SAPbouiCOM) sin mostrar
   ninguna ventana.
3. Cuando el usuario agrega o modifica un artículo en un **Pedido de Venta** y el
   campo `ItemCode` pierde el foco (o se selecciona mediante la búsqueda), el addon:
   - Lee el código del artículo en esa línea.
   - Si comienza con `TEST` (sin distinción de mayúsculas), escribe `15` en la
     columna **% Descuento** de esa misma línea.
4. El addon se cierra automáticamente cuando SAP B1 se apaga.
