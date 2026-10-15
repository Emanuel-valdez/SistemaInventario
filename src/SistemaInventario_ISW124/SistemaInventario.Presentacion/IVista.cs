using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.Presentacion
{
    /// <summary>
    /// Contrato de las pantallas que se muestran dentro de la ventana principal.
    /// </summary>
    public interface IVista
    {
        /// <summary>Recarga los datos de la pantalla cada vez que se muestra.</summary>
        void Actualizar();
    }
}