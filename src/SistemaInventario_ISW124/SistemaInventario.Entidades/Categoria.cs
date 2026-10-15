using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.Entidades
{
    /// <summary>
    /// Representa una categoría de productos.
    /// </summary>
    public class Categoria : EntidadBase
    {
        /// <summary>Nombre único de la categoría (3 a 100 caracteres).</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Descripción de la categoría (máximo 500 caracteres).</summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Fotos asociadas a la categoría (máximo 5).</summary>
        public List<CategoriaFoto> Fotos { get; set; } = new List<CategoriaFoto>();

        /// <summary>Devuelve el nombre de la categoría.</summary>
        public override string ToString() => Nombre;
    }
}