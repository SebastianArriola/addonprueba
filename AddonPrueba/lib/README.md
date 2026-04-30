# SAP Business One SDK – DLLs requeridas

Coloca aqui el archivo **SAPbouiCOM.dll** (UI API Interop) antes de compilar.

## De donde obtenerlo

El archivo se encuentra en la instalacion de SAP Business One 10.0 o en el SDK:

| Ubicacion tipica | Archivo |
|---|---|
| `C:\Program Files\SAP\SAP Business One\` | `SAPbouiCOM.dll` |
| `C:\Program Files\SAP\SAP Business One SDK\UI API\` | `SAPbouiCOM.dll` |

> Asegurate de copiar la version **x64** (64-bit).

## Notas

- **SAPbouiCOM.dll** es la UI API de SAP B1 y ya esta registrada como COM en cualquier
  maquina con SAP Business One instalado. Por eso NO se redistribuye con el addon en el ZIP.
- El proyecto la referencia desde esta carpeta `lib\` solo para la compilacion.
- Si la DLL no aparece directamente, genera el interop desde la type library con:
  ```
  tlbimp "C:\Program Files\SAP\SAP Business One\SAPbouiCOM.dll" /out:lib\SAPbouiCOM.dll
  ```
