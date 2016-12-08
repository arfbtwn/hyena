//
// _TreeViewExtensions.cs
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
using Gtk;

namespace Hyena.Data.Gui
{
    static class _TreeViewExtensions
    {
        public static Gtk.SortType? ToGtk (this SortType sort)
        {
            switch (sort)
            {
                case SortType.None:       return null;
                case SortType.Ascending:  return Gtk.SortType.Ascending;
                case SortType.Descending: return Gtk.SortType.Descending;
                default:
                    throw new ArgumentOutOfRangeException (nameof(sort));
            }
        }

        public static TreeIter Iter  (this int i) => new TreeIter { Stamp = i };

        public static TreePath Path  (this int i) => new TreePath (new [] { i });

        public static int?     Index (this TreePath path) => path?.Indices [0];

        public static TreeIter Iter  (this TreePath path) => (path.Index () ?? -1).Iter ();
    }
}
