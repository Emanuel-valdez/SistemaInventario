using System;
using System.Collections.Generic;
using System.Text;

using SistemaInventario.Entidades;
using SistemaInventario.Entidades.Excepciones;
using SistemaInventario.LogicaNegocio.Comunes;
using SistemaInventario.LogicaNegocio.Interfaces;
using SistemaInventario.LogicaNegocio.Validaciones;

namespace SistemaInventario.Presentacion
{
    /// <summary>
    /// Pantalla de categorías: listado a la izquierda y editor a la derecha.
    /// </summary>
    public class VistaCategorias : UserControl, IVista
    {
        private const int MaximoFotos = 5;

        private readonly ICategoriaServicio _servicio;
        private readonly List<CategoriaFoto> _fotos = new List<CategoriaFoto>();
        private List<Categoria> _todas = new List<Categoria>();
        private int _categoriaId;
        private bool _cargando;

        private readonly ErrorProvider _error = new ErrorProvider();

        private readonly TextBox _txtBuscar = new TextBox();
        private readonly DataGridView _grid = new DataGridView();
        private readonly Label _lblResumen = new Label();

        private readonly Label _lblTituloEditor = Tema.Titulo("Nueva categoría");
        private readonly TextBox _txtNombre = new TextBox();
        private readonly TextBox _txtDescripcion = new TextBox();
        private readonly ListBox _lstFotos = new ListBox();
        private readonly PictureBox _picVista = new PictureBox();
        private readonly Label _lblFechas = new Label();
        private readonly Label _lblMensaje = new Label();
        private Button _btnEliminar = null!;

        /// <summary>Crea la vista con el servicio inyectado.</summary>
        public VistaCategorias(ICategoriaServicio servicio)
        {
            _servicio = servicio;
            ConstruirInterfaz();
            LimpiarEditor();
        }

        /// <inheritdoc />
        public void Actualizar() => Cargar();

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
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            raiz.Controls.Add(ConstruirLista(), 0, 0);
            raiz.Controls.Add(ConstruirEditor(), 1, 0);

            Controls.Add(raiz);
        }

        private Control ConstruirLista()
        {
            var tarjeta = new Tarjeta { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            tabla.Controls.Add(Tema.Titulo("Categorías"), 0, 0);

            Tema.EstilizarTextBox(_txtBuscar);
            _txtBuscar.Dock = DockStyle.Fill;
            _txtBuscar.Margin = new Padding(0, 4, 0, 12);
            _txtBuscar.PlaceholderText = "Buscar por nombre o descripción...";
            _txtBuscar.TextChanged += (s, e) => Filtrar();
            tabla.Controls.Add(_txtBuscar, 0, 1);

            Tema.EstilizarGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            _grid.SelectionChanged += (s, e) =>
            {
                if (_cargando)
                {
                    return;
                }
                Categoria? categoria = ObtenerSeleccionada();
                if (categoria != null)
                {
                    CargarEnEditor(categoria);
                }
            };
            tabla.Controls.Add(_grid, 0, 2);

            _lblResumen.AutoSize = true;
            _lblResumen.ForeColor = Tema.Muted;
            _lblResumen.Margin = new Padding(0, 8, 0, 0);
            tabla.Controls.Add(_lblResumen, 0, 3);

            tarjeta.Controls.Add(tabla);
            return tarjeta;
        }

        private Control ConstruirEditor()
        {
            var tarjeta = new Tarjeta { Dock = DockStyle.Fill, Margin = new Padding(12, 0, 0, 0) };

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 11 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 11; i++)
            {
                tabla.RowStyles.Add(i switch
                {
                    4 => new RowStyle(SizeType.Absolute, 92),
                    6 => new RowStyle(SizeType.Percent, 100),
                    _ => new RowStyle(SizeType.AutoSize)
                });
            }

            // Fila 0: título
            tabla.Controls.Add(_lblTituloEditor, 0, 0);

            // Filas 1 y 2: nombre
            tabla.Controls.Add(Tema.EtiquetaCampo("Nombre (3 a 100 caracteres)"), 0, 1);
            Tema.EstilizarTextBox(_txtNombre);
            _txtNombre.Dock = DockStyle.Fill;
            _txtNombre.MaxLength = 100;
            _txtNombre.TextChanged += (s, e) => _error.SetError(_txtNombre, string.Empty);
            _error.SetIconPadding(_txtNombre, -22);
            tabla.Controls.Add(_txtNombre, 0, 2);

            // Filas 3 y 4: descripción
            tabla.Controls.Add(Tema.EtiquetaCampo("Descripción (máximo 500 caracteres)"), 0, 3);
            Tema.EstilizarTextBox(_txtDescripcion);
            _txtDescripcion.Multiline = true;
            _txtDescripcion.ScrollBars = ScrollBars.Vertical;
            _txtDescripcion.MaxLength = 500;
            _txtDescripcion.Dock = DockStyle.Fill;
            _txtDescripcion.TextChanged += (s, e) => _error.SetError(_txtDescripcion, string.Empty);
            _error.SetIconPadding(_txtDescripcion, -22);
            tabla.Controls.Add(_txtDescripcion, 0, 4);

            // Filas 5 y 6: fotos
            tabla.Controls.Add(Tema.EtiquetaCampo(
                "Fotos (máximo 5 · .jpg/.jpeg/.png · hasta 2 MB cada una)"), 0, 5);

            var zonaFotos = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0)
            };
            zonaFotos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            zonaFotos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            zonaFotos.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _lstFotos.Dock = DockStyle.Fill;
            _lstFotos.Margin = new Padding(0, 0, 8, 0);
            _lstFotos.BorderStyle = BorderStyle.FixedSingle;
            _lstFotos.IntegralHeight = false;
            _lstFotos.Font = new Font("Segoe UI", 10F);
            _lstFotos.SelectedIndexChanged += (s, e) => MostrarVista();
            _error.SetIconPadding(_lstFotos, -22);

            _picVista.Dock = DockStyle.Fill;
            _picVista.Margin = new Padding(8, 0, 0, 0);
            _picVista.SizeMode = PictureBoxSizeMode.Zoom;
            _picVista.BorderStyle = BorderStyle.FixedSingle;
            _picVista.BackColor = Tema.Fondo;

            zonaFotos.Controls.Add(_lstFotos, 0, 0);
            zonaFotos.Controls.Add(_picVista, 1, 0);
            tabla.Controls.Add(zonaFotos, 0, 6);

            // Fila 7: botones de fotos
            var botonesFotos = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Margin = new Padding(0, 8, 0, 0)
            };
            botonesFotos.Controls.Add(Tema.Boton("Agregar foto...", EstiloBoton.Oscuro, 140, AgregarFoto, 34));
            botonesFotos.Controls.Add(Tema.Boton("Quitar foto", EstiloBoton.Neutro, 120, QuitarFoto, 34));
            tabla.Controls.Add(botonesFotos, 0, 7);

            // Fila 8: fechas
            _lblFechas.AutoSize = true;
            _lblFechas.ForeColor = Tema.Muted;
            _lblFechas.Font = new Font("Segoe UI", 9F);
            _lblFechas.Margin = new Padding(0, 10, 0, 4);
            tabla.Controls.Add(_lblFechas, 0, 8);

            // Fila 9: mensaje de resultado
            _lblMensaje.AutoSize = true;
            _lblMensaje.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblMensaje.Margin = new Padding(0, 0, 0, 8);
            tabla.SizeChanged += (s, e) =>
                _lblMensaje.MaximumSize = new Size(Math.Max(tabla.Width - 10, 100), 0);
            tabla.Controls.Add(_lblMensaje, 0, 9);

            // Fila 10: acciones
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
            tabla.Controls.Add(acciones, 0, 10);

            tarjeta.Controls.Add(tabla);
            return tarjeta;
        }

        // ---------- Listado ----------

        private void Cargar()
        {
            try
            {
                _todas = _servicio.ObtenerActivas().ToList();
                Filtrar();
            }
            catch (AccesoDatosException ex)
            {
                MostrarMensaje(ex.Message, false);
            }
        }

        /// <summary>Filtra la lista en memoria mientras el usuario escribe (LINQ).</summary>
        private void Filtrar()
        {
            string texto = _txtBuscar.Text.Trim();

            var filas = _todas
                .Where(c => c.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase)
                         || c.Descripcion.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Descripcion,
                    Fotos = c.Fotos.Count
                })
                .ToList();

            _cargando = true;
            _grid.DataSource = filas;
            _grid.Columns["Id"]!.Visible = false;
            _grid.Columns["Nombre"]!.FillWeight = 90;
            _grid.Columns["Descripcion"]!.HeaderText = "Descripción";
            _grid.Columns["Descripcion"]!.FillWeight = 160;
            _grid.Columns["Fotos"]!.FillWeight = 35;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            _cargando = false;

            _lblResumen.Text = $"Mostrando {filas.Count} de {_todas.Count} categorías";
        }

        private Categoria? ObtenerSeleccionada()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                return null;
            }
            if (_grid.SelectedRows[0].Cells["Id"].Value is not int id)
            {
                return null;
            }
            return _todas.FirstOrDefault(c => c.Id == id);
        }

        // ---------- Editor ----------

        private void CargarEnEditor(Categoria categoria)
        {
            _error.Clear();
            _lblMensaje.Text = string.Empty;

            _categoriaId = categoria.Id;
            _lblTituloEditor.Text = "Editar categoría";
            _txtNombre.Text = categoria.Nombre;
            _txtDescripcion.Text = categoria.Descripcion;

            _fotos.Clear();
            foreach (CategoriaFoto foto in categoria.Fotos)
            {
                _fotos.Add(new CategoriaFoto
                {
                    Id = foto.Id,
                    CategoriaId = foto.CategoriaId,
                    RutaArchivo = foto.RutaArchivo
                });
            }
            RefrescarFotos();

            string modificada = categoria.FechaModificacion?.ToString("dd/MM/yyyy HH:mm") ?? "—";
            _lblFechas.Text = $"Creada: {categoria.FechaCreacion:dd/MM/yyyy HH:mm}   ·   Modificada: {modificada}";
            _btnEliminar.Enabled = true;
        }

        private void LimpiarEditor()
        {
            _error.Clear();
            _lblMensaje.Text = string.Empty;

            _categoriaId = 0;
            _lblTituloEditor.Text = "Nueva categoría";
            _txtNombre.Clear();
            _txtDescripcion.Clear();
            _fotos.Clear();
            RefrescarFotos();
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

            var categoria = new Categoria
            {
                Id = _categoriaId,
                Nombre = _txtNombre.Text,
                Descripcion = _txtDescripcion.Text,
                Fotos = _fotos.ToList()
            };

            ResultadoOperacion<int> resultado = _servicio.Guardar(categoria);

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
            if (_categoriaId == 0)
            {
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                $"¿Enviar la categoría \"{_txtNombre.Text}\" a la papelera?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            ResultadoOperacion<bool> resultado = _servicio.Eliminar(_categoriaId);
            if (!resultado.Exito)
            {
                MostrarMensaje(resultado.Mensaje, false);
                return;
            }

            Cargar();
            LimpiarEditor();
            MostrarMensaje(resultado.Mensaje, true);
        }

        // ---------- Fotos ----------

        private void RefrescarFotos()
        {
            _lstFotos.Items.Clear();
            foreach (CategoriaFoto foto in _fotos)
            {
                _lstFotos.Items.Add(Path.GetFileName(foto.RutaArchivo));
            }
            MostrarVista();
        }

        private void MostrarVista()
        {
            _picVista.Image?.Dispose();
            _picVista.Image = null;

            int indice = _lstFotos.SelectedIndex;
            if (indice < 0 || indice >= _fotos.Count)
            {
                return;
            }

            string ruta = RutaFotos.Resolver(_fotos[indice].RutaArchivo);
            try
            {
                // Se carga una copia para no dejar el archivo bloqueado.
                using var flujo = new FileStream(ruta, FileMode.Open, FileAccess.Read);
                using Image original = Image.FromStream(flujo);
                _picVista.Image = new Bitmap(original);
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException
                                       || ex is UnauthorizedAccessException || ex is OutOfMemoryException)
            {
                // Si no se puede mostrar la vista previa, simplemente queda vacía.
            }
        }

        private void AgregarFoto()
        {
            _error.SetError(_lstFotos, string.Empty);

            if (_fotos.Count >= MaximoFotos)
            {
                _error.SetError(_lstFotos, "Una categoría puede tener como máximo 5 fotos.");
                return;
            }

            using var dialogo = new OpenFileDialog
            {
                Title = "Seleccionar foto",
                Filter = "Imágenes (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (dialogo.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }

            string? mensaje = ValidadorCampos.ValidarFoto(dialogo.FileName);
            if (mensaje != null)
            {
                _error.SetError(_lstFotos, mensaje);
                return;
            }

            _fotos.Add(new CategoriaFoto { RutaArchivo = dialogo.FileName });
            RefrescarFotos();
            _lstFotos.SelectedIndex = _fotos.Count - 1;
        }

        private void QuitarFoto()
        {
            int indice = _lstFotos.SelectedIndex;
            if (indice < 0)
            {
                return;
            }

            _fotos.RemoveAt(indice);
            RefrescarFotos();
            _error.SetError(_lstFotos, string.Empty);
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
                    "Descripcion" => _txtDescripcion,
                    "Fotos" => _lstFotos,
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
                _picVista.Image?.Dispose();
                _error.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}