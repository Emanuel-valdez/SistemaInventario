using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.Entidades
{
    /// <summary>
    /// Representa un producto asociado a una categoría y a un suplidor.
    /// </summary>
    public class Producto : EntidadBase
    {
        /// <summary>Nombre del producto.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Precio unitario (mayor a 0, máximo 2 decimales).</summary>
        public decimal PrecioUnitario { get; set; }

        /// <summary>Id de la categoría asociada.</summary>
        public int CategoriaId { get; set; }

        /// <summary>Id del suplidor asociado.</summary>
        public int SuplidorId { get; set; }

        /// <summary>Categoría asociada (se carga al consultar).</summary>
        public Categoria? Categoria { get; set; }

        /// <summary>Suplidor asociado (se carga al consultar).</summary>
        public Suplidor? Suplidor { get; set; }
    }
}