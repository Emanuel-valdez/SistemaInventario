using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using SistemaInventario.Entidades;
using SistemaInventario.Entidades.Excepciones;
using SistemaInventario.LogicaNegocio.Comunes;
using SistemaInventario.LogicaNegocio.Interfaces;

namespace SistemaInventario.Presentacion
{
    /// <summary>
    /// Pantalla de productos: listado con filtros a la izquierda y editor a la derecha.
    /// </summary>
    public class VistaProductos : UserControl, IVista
    {
        private const decimal PrecioMaximo = 99999999.99m;

        private readonly IProductoServicio _servicio;
        private readonly ICategoriaServicio _categorias;
        private readonly ISuplidorServicio _suplidores;

        private List<Producto> _todos = new List<Producto>();
        private int _productoId;
        private bool _cargando;
        private bool _llenandoCombos;

        private readonly ErrorProvider _error = new ErrorProvider();

        // Listado y filtros
        private readonly TextBox _txtBuscar = new TextBox();
        private readonly ComboBox _cmbFiltroCategoria = new ComboBox();
        private readonly ComboBox _cmbFiltroSuplidor = new ComboBox();
        private readonly TextBox _txtPrecioMin = new TextBox();
        private readonly TextBox _txtPrecioMax = new TextBox();
        private readonly DataGridView _grid = new DataGridView();
        private readonly Label _lblResumen = new Label();

        // Editor
        private readonly Label _lblTituloEditor = Tema.Titulo("Nuevo producto");
        private readonly TextBox _txtNombre = new TextBox();
        private readonly NumericUpDown _numPrecio = new NumericUpDown();
        private readonly ComboBox _cmbCategoria = new ComboBox();
        private readonly ComboBox _cmbSuplidor = new ComboBox();
        private readonly Label _lblFechas = new Label();
        private readonly Label _lblMensaje = new Label();
        private Button _btnEliminar = null!;

        /// <summary>Crea la vista con los servicios inyectados.</summary>
        public VistaProductos(IProductoServicio servicio,
                              ICategoriaServicio categorias,
                              ISuplidorServicio suplidores)
        {
            _servicio = servicio;
            _categorias = categorias;
            _suplidores = suplidores;
            ConstruirInterfaz();
            LimpiarEditor();
        }

        /// <inheritdoc />
        public void Actualizar()
        {
            _lblMensaje.Text = string.Empty;
            Cargar();
        }

        // ---------- Construcción de la interfaz ----------

        private void ConstruirInterfaz()
        {
            Dock = DockStyle.Fill;
            BackColor = Tema.Fondo;
            Padding = new Padding(24);
            Font = Tema.FuenteBase;

            _error.ContainerControl = this;
            _error.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            var raiz = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            raiz.Controls.Add(ConstruirLista(), 0, 0);
            raiz.Controls.Add(ConstruirEditor(), 1, 0);

            Controls.Add(raiz);
        }

        private Control ConstruirLista()
        {
            var tarjeta = new Tarjeta { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // título
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // buscador
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));      // filtros (altura fija)
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));      // tabla
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // resumen

            tabla.Controls.Add(Tema.Titulo("Productos"), 0, 0);

            Tema.EstilizarTextBox(_txtBuscar);
            _txtBuscar.Dock = DockStyle.Fill;
            _txtBuscar.Margin = new Padding(0, 4, 0, 8);
            _txtBuscar.PlaceholderText = "Buscar por nombre del producto...";
            _txtBuscar.TextChanged += (s, e) => Filtrar();
            tabla.Controls.Add(_txtBuscar, 0, 1);

            // Filtros: categoría, suplidor y rango de precios
            var filtros = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Margin = new Padding(0, 0, 0, 8)
            };
            for (int i = 0; i < 4; i++)
            {
                filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }
            filtros.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            filtros.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            EstilizarCombo(_cmbFiltroCategoria);
            _cmbFiltroCategoria.Margin = new Padding(0, 0, 8, 8);
            _cmbFiltroCategoria.SelectedIndexChanged += (s, e) => { if (!_llenandoCombos) Filtrar(); };
            filtros.Controls.Add(_cmbFiltroCategoria, 0, 0);
            filtros.SetColumnSpan(_cmbFiltroCategoria, 2);

            EstilizarCombo(_cmbFiltroSuplidor);
            _cmbFiltroSuplidor.Margin = new Padding(0, 0, 0, 8);
            _cmbFiltroSuplidor.SelectedIndexChanged += (s, e) => { if (!_llenandoCombos) Filtrar(); };
            filtros.Controls.Add(_cmbFiltroSuplidor, 2, 0);
            filtros.SetColumnSpan(_cmbFiltroSuplidor, 2);

            Tema.EstilizarTextBox(_txtPrecioMin);
            _txtPrecioMin.Dock = DockStyle.Fill;
            _txtPrecioMin.Margin = new Padding(0, 0, 8, 0);
            _txtPrecioMin.PlaceholderText = "Precio mín.";
            _txtPrecioMin.TextChanged += (s, e) => Filtrar();
            filtros.Controls.Add(_txtPrecioMin, 0, 1);

            Tema.EstilizarTextBox(_txtPrecioMax);
            _txtPrecioMax.Dock = DockStyle.Fill;
            _txtPrecioMax.Margin = new Padding(0, 0, 8, 0);
            _txtPrecioMax.PlaceholderText = "Precio máx.";
            _txtPrecioMax.TextChanged += (s, e) => Filtrar();
            filtros.Controls.Add(_txtPrecioMax, 1, 1);

            Button btnLimpiar = Tema.Boton("Limpiar", EstiloBoton.Neutro, 120, LimpiarFiltros, 32);
            btnLimpiar.Margin = new Padding(0);
            filtros.Controls.Add(btnLimpiar, 2, 1);

            tabla.Controls.Add(filtros, 0, 2);

            Tema.EstilizarGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            _grid.SelectionChanged += (s, e) =>
            {
                if (_cargando)
                {
                    return;
                }
                Producto? producto = ObtenerSeleccionado();
                if (producto != null)
                {
                    CargarEnEditor(producto);
                }
            };
            tabla.Controls.Add(_grid, 0, 3);

            _lblResumen.AutoSize = true;
            _lblResumen.ForeColor = Tema.Muted;
            _lblResumen.Margin = new Padding(0, 8, 0, 0);
            tabla.Controls.Add(_lblResumen, 0, 4);

            tarjeta.Controls.Add(tabla);
            return tarjeta;
        }

        private Control ConstruirEditor()
        {
            var tarjeta = new Tarjeta { Dock = DockStyle.Fill, Margin = new Padding(12, 0, 0, 0) };

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 12 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 12; i++)
            {
                tabla.RowStyles.Add(i == 9
                    ? new RowStyle(SizeType.Percent, 100)
                    : new RowStyle(SizeType.AutoSize));
            }

            tabla.Controls.Add(_lblTituloEditor, 0, 0);

            // Nombre
            tabla.Controls.Add(Tema.EtiquetaCampo("Nombre del producto (obligatorio)"), 0, 1);
            Tema.EstilizarTextBox(_txtNombre);
            _txtNombre.Dock = DockStyle.Fill;
            _txtNombre.MaxLength = 150;
            _txtNombre.TextChanged += (s, e) => _error.SetError(_txtNombre, string.Empty);
            _error.SetIconPadding(_txtNombre, -22);
            tabla.Controls.Add(_txtNombre, 0, 2);

            // Precio
            tabla.Controls.Add(Tema.EtiquetaCampo("Precio unitario (mayor a 0, máximo 2 decimales)"), 0, 3);
            _numPrecio.Dock = DockStyle.Fill;
            _numPrecio.Font = new Font("Segoe UI", 11F);
            _numPrecio.DecimalPlaces = 2;
            _numPrecio.Minimum = 0;
            _numPrecio.Maximum = PrecioMaximo;
            _numPrecio.ThousandsSeparator = true;
            _numPrecio.ValueChanged += (s, e) => _error.SetError(_numPrecio, string.Empty);
            _error.SetIconPadding(_numPrecio, -44);
            tabla.Controls.Add(_numPrecio, 0, 4);

            // Categoría
            tabla.Controls.Add(Tema.EtiquetaCampo("Categoría (debe estar activa)"), 0, 5);
            EstilizarCombo(_cmbCategoria);
            _cmbCategoria.SelectedIndexChanged += (s, e) => _error.SetError(_cmbCategoria, string.Empty);
            _error.SetIconPadding(_cmbCategoria, -44);
            tabla.Controls.Add(_cmbCategoria, 0, 6);

            // Suplidor
            tabla.Controls.Add(Tema.EtiquetaCampo("Suplidor (debe estar activo)"), 0, 7);
            EstilizarCombo(_cmbSuplidor);
            _cmbSuplidor.SelectedIndexChanged += (s, e) => _error.SetError(_cmbSuplidor, string.Empty);
            _error.SetIconPadding(_cmbSuplidor, -44);
            tabla.Controls.Add(_cmbSuplidor, 0, 8);

            // Fila 9: fechas (pegadas abajo)
            _lblFechas.AutoSize = true;
            _lblFechas.Dock = DockStyle.Bottom;
            _lblFechas.ForeColor = Tema.Muted;
            _lblFechas.Font = new Font("Segoe UI", 9F);
            _lblFechas.Margin = new Padding(0, 10, 0, 4);
            tabla.Controls.Add(_lblFechas, 0, 9);

            // Fila 10: mensaje de resultado
            _lblMensaje.AutoSize = true;
            _lblMensaje.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblMensaje.Margin = new Padding(0, 0, 0, 8);
            tabla.SizeChanged += (s, e) =>
                _lblMensaje.MaximumSize = new Size(Math.Max(tabla.Width - 10, 100), 0);
            tabla.Controls.Add(_lblMensaje, 0, 10);

            // Fila 11: acciones
            var acciones = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false
            };
            acciones.Controls.Add(Tema.Boton("Guardar", EstiloBoton.Primario, 130, Guardar));
            acciones.Controls.Add(Tema.Boton("Nuevo", EstiloBoton.Oscuro, 110, LimpiarEditor));
            _btnEliminar = Tema.Boton("Eliminar", EstiloBoton.Peligro, 110, Eliminar);
            acciones.Controls.Add(_btnEliminar);
            tabla.Controls.Add(acciones, 0, 11);

            tarjeta.Controls.Add(tabla);
            return tarjeta;
        }

        private static void EstilizarCombo(ComboBox combo)
        {
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Font = new Font("Segoe UI", 11F);
            combo.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Método genérico propio: llena un desplegable con una opción inicial
        /// (por ejemplo "Todas") seguida de los elementos, conservando la selección actual.
        /// </summary>
        /// <typeparam name="T">Entidad del dominio (tiene Id).</typeparam>
        private static void LlenarCombo<T>(ComboBox combo, T primero,
                                           IEnumerable<T> elementos, string miembro) where T : EntidadBase
        {
            int seleccionado = combo.SelectedValue is int id ? id : 0;

            var lista = new List<T> { primero };
            lista.AddRange(elementos);

            combo.DataSource = lista;
            combo.DisplayMember = miembro;
            combo.ValueMember = nameof(EntidadBase.Id);
            combo.SelectedValue = seleccionado;

            if (combo.SelectedIndex < 0 && combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        // ---------- Datos y listado ----------

        private void Cargar()
        {
            try
            {
                CargarCombos();
                _todos = _servicio.ObtenerActivos().ToList();
                Filtrar();
            }
            catch (AccesoDatosException ex)
            {
                MostrarMensaje(ex.Message, false);
            }
        }

        private void CargarCombos()
        {
            List<Categoria> categorias = _categorias.ObtenerActivas().ToList();
            List<Suplidor> suplidores = _suplidores.ObtenerActivos().ToList();

            _llenandoCombos = true;
            LlenarCombo(_cmbFiltroCategoria, new Categoria { Id = 0, Nombre = "Todas las categorías" },
                categorias, nameof(Categoria.Nombre));
            LlenarCombo(_cmbFiltroSuplidor, new Suplidor { Id = 0, NombreEmpresa = "Todos los suplidores" },
                suplidores, nameof(Suplidor.NombreEmpresa));
            LlenarCombo(_cmbCategoria, new Categoria { Id = 0, Nombre = "Selecciona una categoría" },
                categorias, nameof(Categoria.Nombre));
            LlenarCombo(_cmbSuplidor, new Suplidor { Id = 0, NombreEmpresa = "Selecciona un suplidor" },
                suplidores, nameof(Suplidor.NombreEmpresa));
            _llenandoCombos = false;
        }

        private static decimal? LeerPrecio(TextBox cuadro)
        {
            return decimal.TryParse(cuadro.Text.Trim(), NumberStyles.Number,
                CultureInfo.CurrentCulture, out decimal valor)
                ? valor
                : null;
        }

        /// <summary>
        /// Filtra por nombre, categoría, suplidor y rango de precios (LINQ).
        /// </summary>
        private void Filtrar()
        {
            string texto = _txtBuscar.Text.Trim();
            int idCategoria = _cmbFiltroCategoria.SelectedValue is int c ? c : 0;
            int idSuplidor = _cmbFiltroSuplidor.SelectedValue is int s ? s : 0;
            decimal? minimo = LeerPrecio(_txtPrecioMin);
            decimal? maximo = LeerPrecio(_txtPrecioMax);

            var filas = _todos
                .Where(p => p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .Where(p => idCategoria == 0 || p.CategoriaId == idCategoria)
                .Where(p => idSuplidor == 0 || p.SuplidorId == idSuplidor)
                .Where(p => minimo == null || p.PrecioUnitario >= minimo)
                .Where(p => maximo == null || p.PrecioUnitario <= maximo)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    Categoria = p.Categoria?.Nombre ?? string.Empty,
                    Suplidor = p.Suplidor?.NombreEmpresa ?? string.Empty,
                    Precio = p.PrecioUnitario
                })
                .ToList();

            _cargando = true;
            _grid.DataSource = filas;
            _grid.Columns["Id"]!.Visible = false;
            _grid.Columns["Nombre"]!.FillWeight = 120;
            _grid.Columns["Categoria"]!.HeaderText = "Categoría";
            _grid.Columns["Categoria"]!.FillWeight = 80;
            _grid.Columns["Suplidor"]!.FillWeight = 90;
            _grid.Columns["Precio"]!.FillWeight = 60;
            _grid.Columns["Precio"]!.DefaultCellStyle.Format = "N2";
            _grid.Columns["Precio"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            _cargando = false;

            _lblResumen.Text = $"Mostrando {filas.Count} de {_todos.Count} productos";
        }

        private void LimpiarFiltros()
        {
            _llenandoCombos = true;
            _txtBuscar.Clear();
            _txtPrecioMin.Clear();
            _txtPrecioMax.Clear();
            if (_cmbFiltroCategoria.Items.Count > 0) _cmbFiltroCategoria.SelectedIndex = 0;
            if (_cmbFiltroSuplidor.Items.Count > 0) _cmbFiltroSuplidor.SelectedIndex = 0;
            _llenandoCombos = false;

            Filtrar();
        }

        private Producto? ObtenerSeleccionado()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                return null;
            }
            if (_grid.SelectedRows[0].Cells["Id"].Value is not int id)
            {
                return null;
            }
            return _todos.FirstOrDefault(p => p.Id == id);
        }

        // ---------- Editor ----------

        private void CargarEnEditor(Producto producto)
        {
            _error.Clear();
            _lblMensaje.Text = string.Empty;

            _productoId = producto.Id;
            _lblTituloEditor.Text = "Editar producto";
            _txtNombre.Text = producto.Nombre;
            _numPrecio.Value = Math.Min(Math.Max(producto.PrecioUnitario, 0m), PrecioMaximo);

            _cmbCategoria.SelectedValue = producto.CategoriaId;
            if (_cmbCategoria.SelectedIndex < 0 && _cmbCategoria.Items.Count > 0)
            {
                _cmbCategoria.SelectedIndex = 0;
            }

            _cmbSuplidor.SelectedValue = producto.SuplidorId;
            if (_cmbSuplidor.SelectedIndex < 0 && _cmbSuplidor.Items.Count > 0)
            {
                _cmbSuplidor.SelectedIndex = 0;
            }

            string modificada = producto.FechaModificacion?.ToString("dd/MM/yyyy HH:mm") ?? "—";
            _lblFechas.Text = $"Creado: {producto.FechaCreacion:dd/MM/yyyy HH:mm}   ·   Modificado: {modificada}";
            _btnEliminar.Enabled = true;
        }

        private void LimpiarEditor()
        {
            _error.Clear();
            _lblMensaje.Text = string.Empty;

            _productoId = 0;
            _lblTituloEditor.Text = "Nuevo producto";
            _txtNombre.Clear();
            _numPrecio.Value = 0;
            if (_cmbCategoria.Items.Count > 0) _cmbCategoria.SelectedIndex = 0;
            if (_cmbSuplidor.Items.Count > 0) _cmbSuplidor.SelectedIndex = 0;
            _lblFechas.Text = string.Empty;
            _btnEliminar.Enabled = false;

            _cargando = true;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            _cargando = false;
        }

        private void Guardar()
        {
            _error.Clear();
            _lblMensaje.Text = string.Empty;

            var producto = new Producto
            {
                Id = _productoId,
                Nombre = _txtNombre.Text,
                PrecioUnitario = _numPrecio.Value,
                CategoriaId = _cmbCategoria.SelectedValue is int cat ? cat : 0,
                SuplidorId = _cmbSuplidor.SelectedValue is int sup ? sup : 0
            };

            ResultadoOperacion<int> resultado = _servicio.Guardar(producto);

            if (resultado.Exito)
            {
                Cargar();
                LimpiarEditor();
                MostrarMensaje(resultado.Mensaje, true);
                return;
            }

            MostrarErrores(resultado.Errores);
            MostrarMensaje(resultado.Mensaje, false);
        }

        private void Eliminar()
        {
            if (_productoId == 0)
            {
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                $"¿Enviar el producto \"{_txtNombre.Text}\" a la papelera?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            ResultadoOperacion<bool> resultado = _servicio.Eliminar(_productoId);
            if (!resultado.Exito)
            {
                MostrarMensaje(resultado.Mensaje, false);
                return;
            }

            Cargar();
            LimpiarEditor();
            MostrarMensaje(resultado.Mensaje, true);
        }

        // ---------- Mensajes y errores ----------

        private void MostrarMensaje(string texto, bool exito)
        {
            _lblMensaje.ForeColor = exito ? Tema.Verde : Tema.Rojo;
            _lblMensaje.Text = texto;
        }

        private void MostrarErrores(Dictionary<string, string> errores)
        {
            foreach (KeyValuePair<string, string> error in errores)
            {
                Control? control = error.Key switch
                {
                    "Nombre" => _txtNombre,
                    "PrecioUnitario" => _numPrecio,
                    "CategoriaId" => _cmbCategoria,
                    "SuplidorId" => _cmbSuplidor,
                    _ => null
                };

                if (control != null)
                {
                    _error.SetError(control, error.Value);
                }
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _error.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}