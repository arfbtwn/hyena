//
// Adapters.cs
//
// Author:
//   nicholas <>
//
// Copyright 2017 nicholas
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Cairo;
using Gtk;

using Rectangle = Gdk.Rectangle;

namespace Hyena.Data.Gui
{
    class ColumnCellBinder : CellBinder
    {
        readonly ColumnCell _cell;
        readonly ITextCell _text;

        public ColumnCellBinder (ColumnCell cell) : base (cell.Property)
        {
            _cell = cell;
            _text = cell as ITextCell;
        }

        public override void Bind (TreeViewColumn tree_column, CellRenderer cell, ITreeModel tree_model, TreeIter iter)
        {
            var o = tree_model.GetValue (iter, 0);

            _cell.Bind (o);

            if (null == _text) return;

            _text.FontWeight = Active (o) ? Pango.Weight.Bold : Pango.Weight.Normal;
        }
    }

    class ColumnCellRenderer : CellRenderer
    {
        readonly CellContext _context;
        readonly ColumnCell _cell;

        public ColumnCellRenderer (ColumnCell cell)
        {
            _cell = cell;
            _context = new CellContext ();
        }

        protected override void OnRender (Context cr, Widget widget, Rectangle background_area, Rectangle cell_area, CellRendererState flags)
        {
            base.OnRender (cr, widget, background_area, cell_area, flags);

            if (null == _context.Widget)
            {
                _context.Widget = widget;
                _context.Layout = new Pango.Layout (widget.PangoContext)
                {
                    FontDescription = widget.PangoContext.FontDescription
                };
                _context.FontDescription = _context.Layout.FontDescription;
                _context.Theme = Hyena.Gui.Theming.ThemeEngine.CreateTheme (widget);

                widget.StyleUpdated += (o, x) =>
                {
                    _context.Layout.Dispose ();
                    _context.Widget = null;
                };
            }

            _context.Context = cr;
            _context.Area = cell_area;
            _context.Selected = flags.HasFlag (CellRendererState.Selected);
            cr.Translate (cell_area.X, cell_area.Y);
            _cell.Render (_context, cell_area.Width, cell_area.Height);
        }
    }
}

