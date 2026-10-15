using System;
using System.Collections.Generic;
using System.Text;

using SistemaInventario.Entidades;

namespace SistemaInventario.AccesoDatos.Interfaces
{
    /// <summary>
    /// Operaciones de acceso a datos de los productos.
    /// Por ahora usa solo las operaciones genéricas de IRepositorio.
    /// </summary>
    public interface IProductoRepositorio : IRepositorio<Producto>
    {
    }
}
