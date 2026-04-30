using System;
using System.Windows.Forms;
using SAPbouiCOM;

namespace AddonPrueba
{
    /// <summary>
    /// Formulario principal completamente oculto.
    /// Actua como pump de mensajes de Windows Forms mientras el addon corre.
    /// </summary>
    public class MainForm : Form
    {
        // Tipo de formulario para Pedido de Venta (Sales Order) en SAP B1
        private const string SALES_ORDER_FORM_TYPE = "139";

        // UID del matriz de lineas de documento en el Pedido de Venta
        private const string LINES_MATRIX_UID = "38";

        // UIDs de columnas en la matriz de lineas
        private const string COL_ITEM_CODE  = "ItemCode";
        private const string COL_DISC_PRCNT = "DiscPrcnt";

        // Prefijo que activa el descuento
        private const string ITEM_PREFIX = "TEST";

        // Descuento a aplicar (%)
        private const string DISCOUNT_VALUE = "15";

        private SAPbouiCOM.Application _sapApp;

        public MainForm(string connectionString)
        {
            // Ocultar completamente la ventana
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar   = false;
            this.WindowState     = FormWindowState.Minimized;
            this.Size            = new System.Drawing.Size(1, 1);
            this.Opacity         = 0d;

            ConnectToSAP(connectionString);
        }

        // Impide que la ventana sea visible en cualquier circunstancia
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }

        private void ConnectToSAP(string connectionString)
        {
            try
            {
                SboGuiApi guiApi = new SboGuiApi();
                guiApi.Connect(connectionString);
                _sapApp = guiApi.GetApplication();

                _sapApp.AppEvent  += new _IApplicationEvents_AppEventEventHandler(OnAppEvent);
                _sapApp.ItemEvent += new _IApplicationEvents_ItemEventEventHandler(OnItemEvent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al conectar con SAP Business One:\n" + ex.Message,
                    "AddonPrueba",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        // Cierre automatico cuando SAP B1 se apaga
        private void OnAppEvent(BoAppEventTypes eventType)
        {
            if (eventType == BoAppEventTypes.aet_ShutDown)
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Escucha todos los ItemEvents de SAP B1.
        /// Detecta cuando el usuario termina de ingresar un articulo en la
        /// linea del Pedido de Venta y aplica el descuento si corresponde.
        /// </summary>
        private void OnItemEvent(
            string FormUID,
            ref SAPbouiCOM.ItemEvent pVal,
            out bool BubbleEvent)
        {
            BubbleEvent = true;

            try
            {
                // Solo Pedidos de Venta
                if (pVal.FormTypeEx != SALES_ORDER_FORM_TYPE)
                    return;

                // Evento: el usuario salio del campo ItemCode (perdida de foco, despues de la accion)
                bool isLostFocusOnItemCode =
                    pVal.EventType == BoEventTypes.et_LOST_FOCUS &&
                    !pVal.BeforeAction &&
                    pVal.ItemUID == LINES_MATRIX_UID &&
                    pVal.ColUID  == COL_ITEM_CODE &&
                    pVal.Row > 0;

                // Evento: el usuario selecciono un articulo desde el dialogo de busqueda
                bool isChooseFromListOnItemCode =
                    pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST &&
                    !pVal.BeforeAction &&
                    pVal.ItemUID == LINES_MATRIX_UID &&
                    pVal.ColUID  == COL_ITEM_CODE &&
                    pVal.Row > 0;

                if (isLostFocusOnItemCode || isChooseFromListOnItemCode)
                {
                    ApplyDiscountIfTestItem(FormUID, pVal.Row);
                }
            }
            catch
            {
                // No interrumpir el flujo de SAP B1 ante cualquier error inesperado
            }
        }

        /// <summary>
        /// Lee el ItemCode de la fila indicada.
        /// Si comienza con "TEST" (sin distincion de mayusculas) aplica 15% de descuento.
        /// </summary>
        private void ApplyDiscountIfTestItem(string formUID, int row)
        {
            try
            {
                SAPbouiCOM.Form form   = _sapApp.Forms.Item(formUID);
                Matrix          matrix = (Matrix)form.Items.Item(LINES_MATRIX_UID).Specific;

                // Leer el ItemCode de la fila actual
                string itemCode = ((EditText)matrix.Columns.Item(COL_ITEM_CODE)
                                                    .Cells.Item(row).Specific).Value ?? string.Empty;

                itemCode = itemCode.Trim();

                if (itemCode.Length == 0)
                    return;

                if (itemCode.StartsWith(ITEM_PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    // Establecer 15% de descuento en la misma fila
                    ((EditText)matrix.Columns.Item(COL_DISC_PRCNT)
                                     .Cells.Item(row).Specific).Value = DISCOUNT_VALUE;
                }
            }
            catch
            {
                // Silencioso: no interferir con SAP B1
            }
        }
    }
}
