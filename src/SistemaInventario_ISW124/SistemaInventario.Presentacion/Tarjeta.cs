using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing.Drawing2D;

namespace SistemaInventario.Presentacion
{
    /// <summary>
    /// Panel blanco con esquinas redondeadas y borde suave.
    /// </summary>
    public class Tarjeta : Panel
    {
        private const int Radio = 14;

        /// <summary>Crea la tarjeta con el estilo de la aplicación.</summary>
        public Tarjeta()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
            Padding = new Padding(22);
        }

        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor ?? SystemColors.Control);
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using GraphicsPath ruta = CrearRuta(new Rectangle(0, 0, Width - 1, Height - 1), Radio);
            using var pincel = new SolidBrush(BackColor);
            using var lapiz = new Pen(Tema.Borde);

            e.Graphics.FillPath(pincel, ruta);
            e.Graphics.DrawPath(lapiz, ruta);
        }

        private static GraphicsPath CrearRuta(Rectangle r, int radio)
        {
            int d = radio * 2;
            var ruta = new GraphicsPath();
            ruta.AddArc(r.X, r.Y, d, d, 180, 90);
            ruta.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            ruta.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            ruta.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }
    }
}