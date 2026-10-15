using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.Entidades
{
    /// <summary>
    /// Representa una foto asociada a una categoría.
    /// </summary>
    public class CategoriaFoto : EntidadBase
    {
        /// <summary>Id de la categoría a la que pertenece la foto.</summary>
        public int CategoriaId { get; set; }

        /// <summary>Ruta del archivo de imagen (.jpg, .jpeg o .png).</summary>
        public string RutaArchivo { get; set; } = string.Empty;
    }
}