using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using SistemaInventario.AccesoDatos.Interfaces;
using SistemaInventario.Entidades;

namespace SistemaInventario.AccesoDatos.Repositorios
{
    /// <summary>
    /// Repositorio de categorías (incluye sus fotos).
    /// </summary>
    public class CategoriaRepositorio : RepositorioBase<Categoria>, ICategoriaRepositorio
    {
        /// <summary>Crea el repositorio con la conexión inyectada.</summary>
        public CategoriaRepositorio(ConexionBD conexion) : base(conexion)
        {
        }

        /// <inheritdoc />
        protected override string NombreTabla => "Categorias";

        /// <inheritdoc />
        protected override Categoria Mapear(SqlDataReader lector)
        {
            var categoria = new Categoria
            {
                Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                Descripcion = lector.GetString(lector.GetOrdinal("Descripcion"))
            };
            MapearCamposBase(lector, categoria);
            return categoria;
        }

        /// <inheritdoc />
        public override IEnumerable<Categoria> ObtenerActivos()
        {
            List<Categoria> categorias = base.ObtenerActivos().ToList();
            CargarFotos(categorias);
            return categorias;
        }

        /// <inheritdoc />
        public override IEnumerable<Categoria> ObtenerEliminados()
        {
            List<Categoria> categorias = base.ObtenerEliminados().ToList();
            CargarFotos(categorias);
            return categorias;
        }

        /// <inheritdoc />
        public override Categoria? ObtenerPorId(int id)
        {
            Categoria? categoria = base.ObtenerPorId(id);
            if (categoria != null)
            {
                CargarFotos(new List<Categoria> { categoria });
            }
            return categoria;
        }

        /// <inheritdoc />
        public override int Insertar(Categoria categoria)
        {
            try
            {
                using SqlConnection conexion = Conexion.ObtenerConexion();
                conexion.Open();
                using SqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    const string sql =
                        "INSERT INTO Categorias (Nombre, Descripcion) " +
                        "OUTPUT INSERTED.Id VALUES (@Nombre, @Descripcion)";

                    using SqlCommand comando = new SqlCommand(sql, conexion, transaccion);
                    comando.Parameters.AddWithValue("@Nombre", categoria.Nombre);
                    comando.Parameters.AddWithValue("@Descripcion", categoria.Descripcion);
                    int id = (int)comando.ExecuteScalar();

                    InsertarFotos(conexion, transaccion, id, categoria.Fotos);
                    transaccion.Commit();
                    return id;
                }
                catch (SqlException)
                {
                    transaccion.Rollback();
                    throw;
                }
            }
            catch (SqlException ex)
            {
                throw ConvertirError(ex, "insertar");
            }
        }

        /// <inheritdoc />
        public override bool Actualizar(Categoria categoria)
        {
            try
            {
                using SqlConnection conexion = Conexion.ObtenerConexion();
                conexion.Open();
                using SqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    const string sql =
                        "UPDATE Categorias SET Nombre = @Nombre, Descripcion = @Descripcion, " +
                        "FechaModificacion = SYSDATETIME() WHERE Id = @Id";

                    using SqlCommand comando = new SqlCommand(sql, conexion, transaccion);
                    comando.Parameters.AddWithValue("@Nombre", categoria.Nombre);
                    comando.Parameters.AddWithValue("@Descripcion", categoria.Descripcion);
                    comando.Parameters.AddWithValue("@Id", categoria.Id);

                    if (comando.ExecuteNonQuery() == 0)
                    {
                        transaccion.Rollback();
                        return false;
                    }

                    // Se reemplazan las fotos por la lista actual de la categoría.
                    using SqlCommand borrar = new SqlCommand(
                        "DELETE FROM CategoriaFotos WHERE CategoriaId = @Id", conexion, transaccion);
                    borrar.Parameters.AddWithValue("@Id", categoria.Id);
                    borrar.ExecuteNonQuery();

                    InsertarFotos(conexion, transaccion, categoria.Id, categoria.Fotos);
                    transaccion.Commit();
                    return true;
                }
                catch (SqlException)
                {
                    transaccion.Rollback();
                    throw;
                }
            }
            catch (SqlException ex)
            {
                throw ConvertirError(ex, "actualizar");
            }
        }

        /// <inheritdoc />
        public bool ExisteNombre(string nombre, int? excluirId = null)
        {
            const string sql =
                "SELECT COUNT(*) FROM Categorias " +
                "WHERE Nombre = @Nombre AND (@Excluir IS NULL OR Id <> @Excluir)";

            object? resultado = EjecutarEscalar(sql,
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@Excluir", (object?)excluirId ?? DBNull.Value));

            return Convert.ToInt32(resultado) > 0;
        }

        /// <inheritdoc />
        public bool TieneProductosActivos(int categoriaId)
        {
            const string sql =
                "SELECT COUNT(*) FROM Productos WHERE CategoriaId = @Id AND Activo = 1";

            object? resultado = EjecutarEscalar(sql, new SqlParameter("@Id", categoriaId));
            return Convert.ToInt32(resultado) > 0;
        }

        private static void InsertarFotos(SqlConnection conexion, SqlTransaction transaccion,
                                          int categoriaId, IEnumerable<CategoriaFoto> fotos)
        {
            foreach (CategoriaFoto foto in fotos)
            {
                using SqlCommand comando = new SqlCommand(
                    "INSERT INTO CategoriaFotos (CategoriaId, RutaArchivo) VALUES (@CategoriaId, @Ruta)",
                    conexion, transaccion);
                comando.Parameters.AddWithValue("@CategoriaId", categoriaId);
                comando.Parameters.AddWithValue("@Ruta", foto.RutaArchivo);
                comando.ExecuteNonQuery();
            }
        }

        private void CargarFotos(List<Categoria> categorias)
        {
            if (categorias.Count == 0)
            {
                return;
            }

            ILookup<int, CategoriaFoto> fotosPorCategoria =
                ConsultarFotos().ToLookup(f => f.CategoriaId);

            foreach (Categoria categoria in categorias)
            {
                categoria.Fotos = fotosPorCategoria[categoria.Id].ToList();
            }
        }

        private List<CategoriaFoto> ConsultarFotos()
        {
            var fotos = new List<CategoriaFoto>();
            try
            {
                using SqlConnection conexion = Conexion.ObtenerConexion();
                using SqlCommand comando = new SqlCommand(
                    "SELECT * FROM CategoriaFotos ORDER BY Id", conexion);
                conexion.Open();

                using SqlDataReader lector = comando.ExecuteReader();
                while (lector.Read())
                {
                    var foto = new CategoriaFoto
                    {
                        CategoriaId = lector.GetInt32(lector.GetOrdinal("CategoriaId")),
                        RutaArchivo = lector.GetString(lector.GetOrdinal("RutaArchivo"))
                    };
                    MapearCamposBase(lector, foto);
                    fotos.Add(foto);
                }
            }
            catch (SqlException ex)
            {
                throw ConvertirError(ex, "consultar fotos");
            }
            return fotos;
        }
    }
}