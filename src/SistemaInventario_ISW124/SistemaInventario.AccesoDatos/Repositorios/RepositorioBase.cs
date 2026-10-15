using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using SistemaInventario.AccesoDatos.Interfaces;
using SistemaInventario.AccesoDatos.Utilidades;
using SistemaInventario.Entidades;
using SistemaInventario.Entidades.Excepciones;

namespace SistemaInventario.AccesoDatos.Repositorios
{
    /// <summary>
    /// Implementación base genérica de IRepositorio con las operaciones comunes.
    /// </summary>
    /// <typeparam name="T">Entidad del dominio.</typeparam>
    public abstract class RepositorioBase<T> : IRepositorio<T> where T : EntidadBase
    {
        /// <summary>Conexión a la base de datos (inyectada por constructor).</summary>
        protected readonly ConexionBD Conexion;

        /// <summary>Nombre de la tabla que maneja el repositorio.</summary>
        protected abstract string NombreTabla { get; }

        /// <summary>Crea el repositorio con la conexión indicada.</summary>
        protected RepositorioBase(ConexionBD conexion)
        {
            Conexion = conexion;
        }

        /// <summary>Convierte la fila actual del lector en una entidad.</summary>
        protected abstract T Mapear(SqlDataReader lector);

        /// <inheritdoc />
        public abstract int Insertar(T entidad);

        /// <inheritdoc />
        public abstract bool Actualizar(T entidad);

        /// <inheritdoc />
        public virtual IEnumerable<T> ObtenerActivos() =>
            ConsultarLista($"SELECT * FROM {NombreTabla} WHERE Activo = 1 ORDER BY Id");

        /// <inheritdoc />
        public virtual IEnumerable<T> ObtenerEliminados() =>
            ConsultarLista($"SELECT * FROM {NombreTabla} WHERE Activo = 0 ORDER BY Id");

        /// <inheritdoc />
        public virtual T? ObtenerPorId(int id) =>
            ConsultarLista($"SELECT * FROM {NombreTabla} WHERE Id = @Id",
                           new SqlParameter("@Id", id)).FirstOrDefault();

        /// <inheritdoc />
        public virtual bool Eliminar(int id) => CambiarEstado(id, false);

        /// <inheritdoc />
        public virtual bool Restaurar(int id) => CambiarEstado(id, true);

        /// <summary>Llena los campos comunes (Id, Activo, fechas) de una entidad.</summary>
        protected static void MapearCamposBase(SqlDataReader lector, EntidadBase entidad)
        {
            entidad.Id = lector.GetInt32(lector.GetOrdinal("Id"));
            entidad.Activo = lector.GetBoolean(lector.GetOrdinal("Activo"));
            entidad.FechaCreacion = lector.GetDateTime(lector.GetOrdinal("FechaCreacion"));

            int posModificacion = lector.GetOrdinal("FechaModificacion");
            entidad.FechaModificacion = lector.IsDBNull(posModificacion)
                ? null
                : lector.GetDateTime(posModificacion);
        }

        /// <summary>Ejecuta un SELECT y devuelve las filas convertidas en entidades.</summary>
        protected List<T> ConsultarLista(string sql, params SqlParameter[] parametros)
        {
            var resultado = new List<T>();
            try
            {
                using SqlConnection conexion = Conexion.ObtenerConexion();
                using SqlCommand comando = new SqlCommand(sql, conexion);
                comando.Parameters.AddRange(parametros);
                conexion.Open();

                using SqlDataReader lector = comando.ExecuteReader();
                while (lector.Read())
                {
                    resultado.Add(Mapear(lector));
                }
            }
            catch (SqlException ex)
            {
                throw ConvertirError(ex, "consultar");
            }
            return resultado;
        }

        /// <summary>Ejecuta un INSERT/UPDATE/DELETE y devuelve las filas afectadas.</summary>
        protected int EjecutarComando(string sql, params SqlParameter[] parametros)
        {
            try
            {
                using SqlConnection conexion = Conexion.ObtenerConexion();
                using SqlCommand comando = new SqlCommand(sql, conexion);
                comando.Parameters.AddRange(parametros);
                conexion.Open();
                return comando.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw ConvertirError(ex, "modificar");
            }
        }

        /// <summary>Ejecuta una consulta y devuelve el primer valor (por ejemplo un Id nuevo).</summary>
        protected object? EjecutarEscalar(string sql, params SqlParameter[] parametros)
        {
            try
            {
                using SqlConnection conexion = Conexion.ObtenerConexion();
                using SqlCommand comando = new SqlCommand(sql, conexion);
                comando.Parameters.AddRange(parametros);
                conexion.Open();
                return comando.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                throw ConvertirError(ex, "insertar");
            }
        }

        private bool CambiarEstado(int id, bool activo)
        {
            string sql = $"UPDATE {NombreTabla} " +
                         "SET Activo = @Activo, FechaModificacion = SYSDATETIME() " +
                         "WHERE Id = @Id";

            return EjecutarComando(sql,
                new SqlParameter("@Activo", activo),
                new SqlParameter("@Id", id)) > 0;
        }

        /// <summary>Registra el error en el log y lo convierte en AccesoDatosException.</summary>
        protected AccesoDatosException ConvertirError(SqlException ex, string operacion)
        {
            Logger.RegistrarError($"{GetType().Name}.{operacion}", ex);

            // 2627 y 2601 = violación de índice o restricción UNIQUE (nombre duplicado)
            if (ex.Number == 2627 || ex.Number == 2601)
            {
                return new AccesoDatosException("Ya existe un registro con ese nombre.", ex);
            }

            return new AccesoDatosException(
                $"Error de base de datos al {operacion} en {NombreTabla}.", ex);
        }
    }
}