//
// ListModel.cs
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

using GLib;
using Gtk;

namespace Hyena.Data.Gui
{
    class ListModel<T> : Object,
                         ITreeModelImplementor,
                         ITreeDragDestImplementor,
                         ITreeDragSourceImplementor
    {
        static TreeIter _iter (int i) => i.Iter ();

        static TreePath _path (int i) => i.Path ();

        static bool _false (out TreeIter iter)
        {
            iter = TreeIter.Zero;
            return false;
        }

        readonly IListModel<T> _model;

        public ListModel (IListModel<T> model)
        {
            _model = model;
        }

        public TreeModelFlags Flags => TreeModelFlags.ItersPersist | TreeModelFlags.ListOnly;

        public int NColumns => 1;

        public TreeIterCompareFunc DefaultSortFunc { get; set; }

        public bool HasDefaultSortFunc => null != DefaultSortFunc;

        public GType GetColumnType (int index) => GType.Object;

        bool _bounded (TreeIter iter) => 0 <= iter.Stamp && iter.Stamp < _model.Count;

        public bool GetIter (out TreeIter iter, TreePath path) => _bounded (iter = path.Iter ());

        public TreePath GetPath (TreeIter iter) => iter.Stamp.Path ();

        public void GetValue (TreeIter iter, int column, ref Value value)
        {
            var val = _bounded (iter) ? _model.GetItem (iter.Stamp) : null;

            value = null == val ? Value.Empty : new Value (val);
        }

        public bool IterChildren (out TreeIter iter, TreeIter parent) => _false(out iter);

        public bool IterHasChild (TreeIter iter) => false;

        public int IterNChildren (TreeIter iter) => _model.Count;

        public bool IterNext (ref TreeIter iter) => _bounded (iter = _iter (iter.Stamp + 1));

        public bool IterNthChild (out TreeIter iter, TreeIter parent, int n) => _false (out iter);

        public bool IterParent (out TreeIter iter, TreeIter child) => _false (out iter);

        public bool IterPrevious (ref TreeIter iter) => _bounded (iter = _iter (iter.Stamp - 1));

        public void RefNode (TreeIter iter)
        {
            //Log.DebugFormat ("RefNode ({0})", iter.Stamp);
        }

        public void UnrefNode (TreeIter iter)
        {
            //Log.DebugFormat ("UnrefNode ({0})", iter.Stamp);
        }

        #region ITreeDragDestImplementor

        public bool DragDataReceived (TreePath dest, SelectionData selection_data)
        {
            Log.DebugFormat ("DragDataReceived ({0}, {1})", dest.Index (), selection_data.Text);
            return false;
        }

        public bool RowDropPossible (TreePath dest_path, SelectionData selection_data)
        {
            Log.DebugFormat ("RowDropPossible ({0}, {1})", dest_path.Index (), selection_data.Text);
            return true;
        }

        #endregion

        #region ITreeDragSourceImplementor

        public bool RowDraggable (TreePath path)
        {
            Log.DebugFormat ("RowDraggable ({0})", path.Index ());
            return true;
        }

        public bool DragDataGet (TreePath path, SelectionData selection_data)
        {
            Log.DebugFormat ("DragDataGet ({0}, {1})", path.Index (), selection_data.Text);
            return false;
        }

        public bool DragDataDelete (TreePath path)
        {
            Log.DebugFormat ("DragDataDelete ({0})", path.Index ());
            return false;
        }

        #endregion
    }
}
