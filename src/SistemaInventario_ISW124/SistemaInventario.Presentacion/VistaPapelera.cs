using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.Entidades;
using SistemaInventario.Entidades.Excepciones;
using SistemaInventario.LogicaNegocio.Comunes;
using SistemaInventario.LogicaNegocio.Interfaces;

namespace SistemaInventario.Presentacion
{
    /// <summary>
    /// Papelera: muestra los registros eliminados de cada módulo y permite restaurarlos.
    /// </summary>
    public class VistaPapelera : UserControl, IVista
    {
        private static readonly string[] NombresTipos = { "Categorías", "Suplidores", "Productos" };

        private readonly ICategoriaServicio _categorias;
        private readonly ISuplidorServicio _suplidores;
        private readonly IProductoServicio _productos;

        private List<Categoria> _categoriasEliminadas = new List<Categoria>();
        private List<Suplidor> _suplidoresEliminados = new List<Suplidor>();
        private List<Producto> _productosEliminados = new List<Producto>();

        private int _tipo;
        private readonly List<Button> _botonesTipo = new List<Button>();

        private readonly TextBox _txtBuscar = new TextBox();
        private readonly DataGridView _grid = new DataGridView();
        private readonly Label _lblResumen = new Label();
        private readonly Label _lblMensaje = new Label();

        private sealed record FilaPapelera(int Id, string Nombre, string Detalle, DateTime? Eliminado);

        /// <summary>Crea la vista con los servicios inyectados.</summary>
        public VistaPapelera(ICategoriaServicio categorias,
                             ISuplidorServicio suplidores,
                             IProductoServicio productos)
        {
            _categorias = categorias;
            _suplidores = suplidores;
            _productos = productos;
            ConstruirInterfaz();
        }

        /// <inheritdoc />
        public void Actualizar()
        {
            _lblMensaje.Text = string.Empty;
            CargarDatos();
        }

        // ---------- Construcción de la interfaz ----------

        private void ConstruirInterfaz()
        {
            Dock = DockStyle.Fill;
            BackColor = Tema.Fondo;
            Padding = new Padding(24);
            Font = Tema.FuenteBase;

            var tarjeta = new Tarjeta { Dock = DockStyle.Fill };

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 8; i++)
            {
                tabla.RowStyles.Add(i == 4
                    ? new RowStyle(SizeType.Percent, 100)
                    : new RowStyle(SizeType.AutoSize));
            }

            tabla.Controls.Add(Tema.Titulo("Papelera"), 0, 0);

            var subtitulo = new Label
            {
                Text = "Registros eliminados. Selecciona uno y presiona Restaurar.",
                AutoSize = true,
                ForeColor = Tema.Muted,
                Margin = new Padding(0, 0, 0, 12)
            };
            tabla.Controls.Add(subtitulo, 0, 1);

            var tipos = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 12)
            };
            for (int i = 0; i < NombresTipos.Length; i++)
            {
                Button pildora = CrearPildora(i);
                _botonesTipo.Add(pildora);
                tipos.Controls.Add(pildora);
            }
            tabla.Controls.Add(tipos, 0, 2);

            Tema.EstilizarTextBox(_txtBuscar);
            _txtBuscar.Dock = DockStyle.Fill;
            _txtBuscar.Margin = new Padding(0, 0, 0, 12);
            _txtBuscar.PlaceholderText = "Buscar en la papelera...";
            _txtBuscar.TextChanged += (s, e) => Filtrar();
            tabla.Controls.Add(_txtBuscar, 0, 3);

            Tema.EstilizarGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            tabla.Controls.Add(_grid, 0, 4);

            _lblResumen.AutoSize = true;
            _lblResumen.ForeColor = Tema.Muted;
            _lblResumen.Margin = new Padding(0, 8, 0, 0);
            tabla.Controls.Add(_lblResumen, 0, 5);

            _lblMensaje.AutoSize = true;
            _lblMensaje.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblMensaje.Margin = new Padding(0, 6, 0, 8);
            tabla.SizeChanged += (s, e) =>
                _lblMensaje.MaximumSize = new Size(Math.Max(tabla.Width - 10, 100), 0);
            tabla.Controls.Add(_lblMensaje, 0, 6);

            var acciones = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false
            };
            acciones.Controls.Add(Tema.Boton("Restaurar", EstiloBoton.Primario, 140, Restaurar));
            tabla.Controls.Add(acciones, 0, 7);

            tarjeta.Controls.Add(tabla);
            Controls.Add(tarjeta);

            ActualizarPildoras();
        }

        private Button CrearPildora(int indice)
        {
            var boton = new Button
            {
                Size = new Size(190, 34),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0),
                TabStop = false,
                UseVisualStyleBackColor = false
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.Click += (s, e) =>
            {
                _tipo = indice;
                _lblMensaje.Text = string.Empty;
                ActualizarPildoras();
                Filtrar();
            };
            return boton;
        }

        private void ActualizarPildoras()
        {
            int[] cantidades =
            {
                _categoriasEliminadas.Count,
                _suplidoresEliminados.Count,
                _productosEliminados.Count
            };

            for (int i = 0; i < _botonesTipo.Count; i++)
            {
                bool activa = i == _tipo;
                Button boton = _botonesTipo[i];
                boton.Text = $"{NombresTipos[i]} ({cantidades[i]})";
                boton.BackColor = activa ? Tema.TealClaro : Tema.Fondo;
                boton.ForeColor = activa ? Tema.TealOscuro : Tema.Muted;
                boton.FlatAppearance.MouseOverBackColor = activa ? Tema.TealClaro : Tema.Borde;
            }
        }

        // ---------- Datos ----------

        private void CargarDatos()
        {
            try
            {
                _categoriasEliminadas = _categorias.ObtenerEliminadas().ToList();
                _suplidoresEliminados = _suplidores.ObtenerEliminados().ToList();
                _productosEliminados = _productos.ObtenerEliminados().ToList();

                ActualizarPildoras();
                Filtrar();
            }
            catch (AccesoDatosException ex)
            {
                MostrarMensaje(ex.Message, false);
            }
        }

        private IEnumerable<FilaPapelera> ObtenerFilas()
        {
            return _tipo switch
            {
                0 => _categoriasEliminadas.Select(c =>
                    new FilaPapelera(c.Id, c.Nombre, c.Descripcion, c.FechaModificacion)),
                1 => _suplidoresEliminados.Select(s =>
                    new FilaPapelera(s.Id, s.NombreEmpresa,
                        $"{s.NombreContacto} · {s.Telefono}", s.FechaModificacion)),
                _ => _productosEliminados.Select(p =>
                    new FilaPapelera(p.Id, p.Nombre,
                        $"{p.Categoria?.Nombre} · {p.Suplidor?.NombreEmpresa} · {p.PrecioUnitario:N2}",
                        p.FechaModificacion))
            };
        }

        /// <summary>Filtra los registros eliminados por texto (LINQ).</summary>
        private void Filtrar()
        {
            string texto = _txtBuscar.Text.Trim();

            List<FilaPapelera> filas = ObtenerFilas()
                .Where(f => f.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase)
                         || f.Detalle.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.Eliminado)
                .ToList();

            _grid.DataSource = filas;
            _grid.Columns["Id"]!.Visible = false;
            _grid.Columns["Nombre"]!.FillWeight = 90;
            _grid.Columns["Detalle"]!.FillWeight = 180;
            _grid.Columns["Eliminado"]!.HeaderText = "Eliminado el";
            _grid.Columns["Eliminado"]!.FillWeight = 70;
            _grid.Columns["Eliminado"]!.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            _grid.ClearSelection();
            _grid.CurrentCell = null;

            _lblResumen.Text = $"{filas.Count} registros en la papelera de {NombresTipos[_tipo].ToLower()}";
        }

        // ---------- Restaurar ----------

        private void Restaurar()
        {
            if (_grid.SelectedRows.Count == 0
                || _grid.SelectedRows[0].Cells["Id"].Value is not int id)
            {
                MostrarMensaje("Selecciona un registro para restaurar.", false);
                return;
            }

            ResultadoOperacion<bool> resultado = _tipo switch
            {
                0 => _categorias.Restaurar(id),
                1 => _suplidores.Restaurar(id),
                _ => _productos.Restaurar(id)
            };

            if (!resultado.Exito)
            {
                MostrarMensaje(resultado.Mensaje, false);
                return;
            }

            CargarDatos();
            MostrarMensaje(resultado.Mensaje, true);
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            _lblMensaje.ForeColor = exito ? Tema.Verde : Tema.Rojo;
            _lblMensaje.Text = texto;
        }
    }
}