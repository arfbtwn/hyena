//
// TreeView.cs
//
// Author:
//   nicholas <>
//
// Copyright 2016 nicholas
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
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Hyena.Collections;

using EventArgs = System.EventArgs;
using Menu = Gtk.Menu;

namespace Hyena.Data.Gui
{
    public class TreeView<T> : TreeView, IListView<T>
    {
        readonly Dictionary<TreeViewColumn, Column> _map = new Dictionary<TreeViewColumn, Column>();

        ColumnController _columns;

        IListModel<T> _model;

        protected TreeView () { _ (); }

        protected TreeView (IntPtr raw) : base (raw) { _ (); }

        void _ ()
        {
            base.Selection.Mode = SelectionMode.Multiple;

            base.Selection.Changed += Selection_Changed;
            SelectionProxy.Changed += SelectionProxy_Changed;
            SelectionProxy.SelectionChanged += SelectionProxy_Changed;
        }

        public ColumnController ColumnController {
            get { return _columns; }
            set {
                if (_columns == value) return;

                if (_columns != null) {
                    _columns.Updated -= ColumnsUpdated;
                }

                _columns = value;

                if (_columns != null) {
                    _columns.Updated += ColumnsUpdated;

                    GenerateColumns ();
                    UpdateColumns ();
                }
            }
        }

        public new Collections.Selection Selection => Model?.Selection;

        public SelectionProxy SelectionProxy { get; } = new SelectionProxy ();

        public new IListModel<T> Model
        {
            get { return _model; }
            set
            {
                if (_model != value)
                {
                    Log.DebugFormat ("set_Model ({0})", value);
                    _Disconnect ();

                    _model = value;

                    _Connect ();
                    return;
                }

                if (Model != null)
                {
                    Log.DebugFormat ("set_Model ({0}): refresh", value);

                    TreePath start, end;
                    GetVisibleRange (out start, out end);

                    base.Selection.Changed -= Selection_Changed;

                    base.Model = BuildModel ();

                    base.Selection.Changed += Selection_Changed;

                    SelectionProxy_Changed (null, null);

                    if (null != start)
                    {
                        ScrollToCell (start, null, false, 0.0f, 0.0f);
                    }
                }
            }
        }

        ISortable Sortable => Model as ISortable;

        void Selection_Changed (object o, EventArgs x)
        {
            Log.Debug ("base.Selection.Changed (o, x)");
            var selection = Selection;

            if (null == selection)
            {
                Log.Debug ("base.Selection.Changed (o, x): return");
                return;
            }

            selection.Clear (false);

            var rows = base.Selection.GetSelectedRows ();

            Log.DebugFormat ("base.Selection Rows Selected: {0}", rows.Length);

            foreach (var row in rows) {
                Log.DebugFormat ("base.Selection Row Selected: {0}", row);
                selection.Select (row.Index() ?? 0, false);
            }

            TreePath path;
            TreeViewColumn column;
            GetCursor (out path, out column);

            if (null != path)
            {
                Log.DebugFormat ("base.Selection Row Focus: {0}", path.Index ());
                selection.FocusedIndex = path.Index () ?? 0;
            }

            selection.Notify ();
        }

        void SelectionProxy_Changed (object o, EventArgs x)
        {
            Log.Debug ("SelectionProxy.Changed (o, x)");
            var selection = SelectionProxy.Selection;

            if (null == selection)
            {
                Log.Debug ("SelectionProxy.Changed (o, x): return");
                return;
            }

            base.Selection.Changed -= Selection_Changed;

            if (selection.AllSelected) {
                base.Selection.SelectAll ();
            } else {
                base.Selection.UnselectAll ();

                var rows = selection.RangeCollection;

                Log.DebugFormat ("SelectionProxy Rows Selected: {0}", rows.Count);

                foreach (var index in rows) {
                    Log.DebugFormat ("SelectionProxy Row Selected: {0}", index);
                    base.Selection.SelectPath (index.Path ());
                }
            }

            base.Selection.Changed += Selection_Changed;
        }

        void ColumnsUpdated (object o, EventArgs x)
        {
            Log.DebugFormat ("ColumnsUpdated ({0}, x)", o);
            UpdateColumns ();
        }

        void UpdateColumns ()
        {
            Log.Debug ("UpdateColumns ()");
            foreach (var kv in _map) {
                var t = kv.Key;
                var c = kv.Value;

                t.Visible = c.Visible;

                var s = c as ISortableColumn;

                if (s != ColumnController.SortColumn)
                {
                    t.SortOrder = Gtk.SortType.Ascending;
                    t.SortIndicator = false;
                    continue;
                }

                var order = s?.SortType.ToGtk ();

                t.SortOrder = order ?? Gtk.SortType.Ascending;
                t.SortIndicator = order.HasValue;
            }
        }

        void GenerateColumns ()
        {
            Log.DebugFormat ("GenerateColumns (): {0}", Allocation);

            foreach (var tc in Columns) {
                tc.Clicked -= HandleHeaderClick;
                tc.Button.ButtonPressEvent -= HandleHeaderClick;
                RemoveColumn (tc);

                tc.Dispose ();
            }

            _map.Clear ();

            if (_columns == null) return;

            var a = AllocatedWidth - 2 * BorderWidth;

            foreach (var c in _columns) {
                var col = c;

                var w = a * col.Width;
                var i = !string.IsNullOrWhiteSpace(col.Id);

                var tc = new TreeViewColumn
                {
                    Title = col.Title,
                    MinWidth = col.MinWidth,
                    MaxWidth = col.MaxWidth,
                    FixedWidth = (int) w - 1,
                    Visible = col.Visible,
                    SortColumnId = -1,
                    Expand = i,
                    Resizable = i,
                    Reorderable = i,
                    Clickable = i,
                };

                tc.Clicked += HandleHeaderClick;
                tc.Button.ButtonPressEvent += HandleHeaderClick;

                try {
                    Log.Debug ($"Configuring column: {col.Id} => {col.Property}");
                    Configure (col, tc);
                } catch (Exception e) {
                    Log.Warning ($"Error processing column: {col.Id} => {col.Property}", e);
                } finally {
                    AppendColumn (tc);
                    _map [tc] = col;
                }
            }
        }

        protected virtual void Configure (Column column, TreeViewColumn tc)
        {
            var cell = column.Last ();

            var renderer = Renderer (cell);
            var binder = Binder (cell);

            binder.Active (RowBoldPropertyName);

            var size = column.FirstOrDefault()?.FixedSize;

            var w = (int) (size?.Width  ?? 1);
            var h = (int) (size?.Height ?? ColumnCellText.ComputeRowHeight(this));

            renderer.SetFixedSize (w, h);

            tc.PackStart (renderer, true);
            tc.SetCellDataFunc (renderer, new TreeCellDataFunc (binder.Bind));
        }

        protected virtual CellRenderer Renderer (ColumnCell cell)
        {
            return new ColumnCellRenderer (cell);
        }

        protected virtual CellBinder Binder (ColumnCell cell)
        {
            return new ColumnCellBinder (cell);
        }

        void HandleHeaderClick (object o, EventArgs args)
        {
            var tc = (TreeViewColumn) o;
            var real = _map [tc];

            var sort = (ISortableColumn) real;

            switch (sort.SortType)
            {
                case SortType.None:       sort.SortType = SortType.Ascending;  break;
                case SortType.Ascending:  sort.SortType = SortType.Descending; break;
                case SortType.Descending: sort.SortType = SortType.None;       break;
            }

            ColumnController.SortColumn = sort;

            Sort ();

            tc.SortIndicator = SortType.None != sort.SortType;
            tc.SortOrder = sort.SortType.ToGtk () ?? Gtk.SortType.Ascending;
        }

        void HandleHeaderClick (object o, ButtonPressEventArgs args)
        {
            if (3 != args.Event.Button)
            {
                return;
            }

            args.RetVal = true;

            var tc = _map.Keys.First (x => x.Button == o);
            var real = _map [tc];

            var columns = ColumnController.Where (x => x.Id != null)
                                          .Except (new [] { real })
                                          .OrderBy (x => x.Title)
                                          .ToArray ();

            var menu = new Menu
            {
                new ListView<T>.ColumnToggleMenuItem (real),
                new SeparatorMenuItem ()
            };

            foreach (var item in columns)
            {
                menu.Append (new ListView<T>.ColumnToggleMenuItem (item));
            }

            menu.ShowAll ();
            menu.Popup ();
        }

        ITreeModel BuildModel ()
        {
            return new TreeModelSort (new TreeModelAdapter (new ListModel<T> (Model)));
        }

        void _Connect ()
        {
            Log.Debug ("Connect ()");

            if (null == Model)
            {
                Log.Debug ("Connect (): return");
                return;
            }

            Model.Cleared += OnModelCleared;
            Model.Reloaded += OnModelReloaded;

            IsEverReorderable = Model.CanReorder;

            //Sort ();

            //if (ViewLayout != null)
            //{
            //    ViewLayout.Model = Model;
            //}

            base.Model = BuildModel ();

            SelectionProxy.Selection = Selection;
        }

        void _Disconnect ()
        {
            Log.Debug ("Disconnect ()");
            SelectionProxy.Selection = null;
            base.Model = null;

            if (null == Model) return;

            Model.Cleared -= OnModelCleared;
            Model.Reloaded -= OnModelReloaded;
        }

        public void CenterOn (int index)
        {
            Log.DebugFormat ("CenterOn ({0})", index);
            ScrollToCell (index.Path (), null, true, 0.5f, 0.0f);
        }

        protected virtual void CenterOnSelection ()
        {
            Log.Debug ("CenterOnSelection ()");
            if (0 < Selection.Count)
            {
                CenterOn (Selection.FirstIndex);
            }
        }

        public void ScrollTo (int index) => CenterOn (index);

        public void SetModel (IListModel<T> model)
        {
            Log.DebugFormat ("SetModel ({0})", model);
            Model = model;
        }

        public virtual void SetModel (IListModel<T> value, double vpos)
        {
            Log.DebugFormat ("SetModel ({0}, {1})", value, vpos);
            SetModel (value);
            Vadjustment.Value = vpos;
        }

        void Sort ()
        {
            var sortable = Sortable;

            if (null == sortable) return;

            var sort_column = ColumnController.SortColumn ?? ColumnController.DefaultSortColumn;
            if (sort_column != null)
            {
                if (sortable.Sort (sort_column))
                {
                    Model.Reload ();
                }
            }
        }

        void OnModelCleared (object o, EventArgs x)
        {
            Log.DebugFormat ("OnModelCleared ({0}, x)", o);
            Model = null;
        }

        void OnModelReloaded (object o, EventArgs x)
        {
            Log.DebugFormat ("OnModelReloaded ({0}, x)", o);
            OnModelReloaded ();
        }

        protected virtual TargetEntry [] DragDropSourceEntries { get; } = new [] { ListViewDragDropTarget.ModelSelection };

        protected virtual DataViewLayout ViewLayout { get; set; }

        [Obsolete]
        protected virtual Gdk.Size ChildSize { get; set; }

        bool _force_drag;

        public bool ForceDragSourceSet
        {
            get { return _force_drag; }
            set {
                _force_drag = value;
                OnDragSet ();
            }
        }

        protected virtual void OnDragSet ()
        {
            if (ForceDragSourceSet || IsReorderable)
            {
                var buttons = Gdk.ModifierType.Button1Mask | Gdk.ModifierType.Button3Mask;
                var actions = Gdk.DragAction.Copy | Gdk.DragAction.Move;

                EnableModelDragDest (DragDropSourceEntries, actions);
                EnableModelDragSource (buttons, DragDropSourceEntries, actions);
            }
            else
            {
                UnsetRowsDragSource ();
                UnsetRowsDragDest ();
            }
        }

        public bool HeaderVisible {
            get { return HeadersVisible; }
            set { HeadersVisible = value; }
        }

        public bool IsReorderable {
            get { return Reorderable; }
            set { 
                Reorderable = value;
                OnDragSet ();
            }
        }

        public bool IsEverReorderable { get; set; }

        public string RowOpaquePropertyName { get; set; }
        public string RowBoldPropertyName { get; set; }

        protected virtual void OnModelReloaded () {
            Log.Debug ("OnModelReloaded ()");
            Model = Model;
        }

        protected virtual void OnDragSourceSet () {
            Log.Debug ("OnDragSourceSet ()");
        }

        protected virtual void InvalidateList () {
            Log.Debug ("InvalidateList ()");
        }

        protected virtual bool ActivateSelection ()
        {
            Log.DebugFormat ("Activate Selection (): {0}", Selection.FocusedIndex.Path ());
            ActivateRow (Selection.FocusedIndex.Path (), base.Columns[0]);
            return true;
        }

        [Obsolete]
        protected virtual Gdk.Size OnMeasureChild () => new Gdk.Size ();

        protected virtual int TranslateToListY (int y)
        {
            int wx, wy;
            ConvertBinWindowToWidgetCoords (0, y, out wx, out wy);

            Log.DebugFormat ("TranslateToListY ({0}) => {1}", y, wy);

            return wy;
        }

        protected virtual int GetModelRowAt (int x, int y)
        {
            TreePath path;
            TreeViewDropPosition pos;

            GetDestRowAtPos (x, y, out path, out pos);

            var row = path.Index () ?? -1;

            Log.DebugFormat ("GetModelRowAt ({0}, {1}) => {2}", x, y, row);

            return row;
        }

        protected virtual bool IsRowVisible (int x)
        {
            Log.DebugFormat ("IsRowVisible ({0})", x);

            TreePath start, end;
            if (GetVisibleRange (out start, out end))
            {
                var visible = start.Index () <= x && x <= end.Index ();

                Log.DebugFormat ("IsRowVisible ({0}): {1}", x, visible);

                return visible;
            }

            return true;
        }

        #region GTK new / overrides

        public new void ScrollToCell (TreePath path, TreeViewColumn column, bool use_align, float row_align, float col_align)
        {
            Log.DebugFormat ("ScrollToCell ({0}, {1}, {2}, {3}, {4})",
                             path.Index (), column,
                             use_align, row_align, col_align);
            base.ScrollToCell (path, column, use_align, row_align, col_align);
        }

        protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
        {
            if (evnt.Device.InputSource != Gdk.InputSource.Mouse)
            {
                return base.OnButtonPressEvent (evnt);
            }

            bool in_selection = false;
            if (GetPathAtPos ((int) evnt.X, (int) evnt.Y, out _waiting, out _column, out _cell_x, out _cell_y))
            {
                in_selection = base.Selection.PathIsSelected (_waiting);
            }

            if (in_selection)
            {
                base.Selection.SelectFunction = Suppress;
                _modifier = evnt.State;
            }
            else
            {
                base.Selection.SelectFunction = Allow;
                _waiting = null;
            }

            var handled = base.OnButtonPressEvent (evnt);

            if (3 == evnt.Button)
            {
                OnPopupMenu ();
            }

            return handled;
        }

        protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
        {
            if (evnt.Device.InputSource != Gdk.InputSource.Mouse)
            {
                return base.OnButtonPressEvent (evnt);
            }

            if (null == _waiting) return base.OnButtonReleaseEvent (evnt);

            base.Selection.SelectFunction = Allow;

            if (_modifier.HasFlag (Gdk.ModifierType.ShiftMask))
            {
                base.Selection.UnselectPath (_waiting);
            }
            else
            {
                SetCursor (_waiting, _column, false);
            }

            _waiting = null;

            return base.OnButtonPressEvent (evnt);
        }

        Gdk.ModifierType _modifier;
        TreePath _waiting;
        TreeViewColumn _column;
        int _cell_x;
        int _cell_y;

        static TreeSelectionFunc Suppress = (s, m, p, pcs) => false;
        static TreeSelectionFunc Allow    = (s, m, p, pcs) => true;

        #endregion
    }
}
