using System;
using System.Windows.Forms;

namespace AddonPrueba
{
    static class Program
    {
        /// <summary>
        /// SAP Business One lanza el addon pasando la cadena de conexion como primer argumento.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string connectionString = args.Length > 0 ? args[0] : string.Empty;

            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show(
                    "AddonPrueba debe ser iniciado desde SAP Business One.\n" +
                    "No se encontro cadena de conexion en los argumentos.",
                    "AddonPrueba",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Application.Run(new MainForm(connectionString));
        }
    }
}
