using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.Entidades.Excepciones
{
    /// <summary>
    /// Excepción lanzada cuando ocurre un error al acceder a la base de datos.
    /// </summary>
    public class AccesoDatosException : Exception
    {
        /// <summary>Crea la excepción con un mensaje y la excepción original.</summary>
        public AccesoDatosException(string mensaje, Exception? interna = null)
            : base(mensaje, interna)
        {
        }
    }
}