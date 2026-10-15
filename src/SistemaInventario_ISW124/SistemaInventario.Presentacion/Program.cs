using SistemaInventario.LogicaNegocio;
using SistemaInventario.LogicaNegocio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SistemaInventario.Presentacion
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada de la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            ICategoriaServicio categorias;
            ISuplidorServicio suplidores;
            IProductoServicio productos;
            IReporteServicio reportes;

            try
            {
                categorias = FabricaServicios.CrearCategoriaServicio();
                suplidores = FabricaServicios.CrearSuplidorServicio();
                productos = FabricaServicios.CrearProductoServicio();
                reportes = FabricaServicios.CrearReporteServicio();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo iniciar la aplicación. Revisa el archivo appsettings.json.\n\n" + ex.Message,
                    "Error de configuración", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            System.Windows.Forms.Application.Run(
                new FormPrincipal(categorias, suplidores, productos, reportes));
        }
    }
}