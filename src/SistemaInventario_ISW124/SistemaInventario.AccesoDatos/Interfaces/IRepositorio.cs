using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.Entidades;

namespace SistemaInventario.AccesoDatos.Interfaces
{
    /// <summary>
    /// Contrato genérico de operaciones CRUD con borrado lógico.
    /// </summary>
    /// <typeparam name="T">Entidad del dominio.</typeparam>
    public interface IRepositorio<T> where T : EntidadBase
    {
        /// <summary>Obtiene los registros activos.</summary>
        IEnumerable<T> ObtenerActivos();

        /// <summary>Obtiene los registros eliminados (papelera).</summary>
        IEnumerable<T> ObtenerEliminados();

        /// <summary>Obtiene un registro por su Id (activo o eliminado).</summary>
        T? ObtenerPorId(int id);

        /// <summary>Inserta un registro y devuelve el Id generado.</summary>
        int Insertar(T entidad);

        /// <summary>Actualiza un registro. Devuelve true si se modificó.</summary>
        bool Actualizar(T entidad);

        /// <summary>Elimina lógicamente un registro (Activo = 0).</summary>
        bool Eliminar(int id);

        /// <summary>Restaura un registro eliminado (Activo = 1).</summary>
        bool Restaurar(int id);
    }
}