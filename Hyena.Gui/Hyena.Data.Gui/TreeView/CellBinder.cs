//
// Binder.cs
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

using Gtk;

namespace Hyena.Data.Gui
{
    public abstract class CellBinder
    {
        protected Bound<object> _get;
        protected Bound<bool> _active;

        protected CellBinder (string target)
        {
            _get = new Bound<object> { Property = target };
        }

        protected bool Active (object o)
        {
            return _active?.Get (o) ?? false;
        }

        protected object Get (object o)
        {
            return _get.Get (o);
        }

        public CellBinder Active (string bold)
        {
            _active = new Bound<bool> { Property = bold };
            return this;
        }

        public abstract void Bind (TreeViewColumn tree_column, CellRenderer cell, ITreeModel tree_model, TreeIter iter);
    }
}
