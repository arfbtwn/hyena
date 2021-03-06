//
// GtkTheme.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007-2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Cairo;
using Gtk;

namespace Hyena.Gui.Theming
{
    public class GtkTheme : Theme
    {
        private Cairo.Color border_color;

        public GtkTheme (Widget widget) : base (widget)
        {
        }

        public static Cairo.Color GetCairoTextMidColor (Widget widget)
        {
            Color text_color = CairoExtensions.GdkRGBAToCairoColor (widget.StyleContext.GetColor (StateFlags.Normal));
            Color background_color = CairoExtensions.GdkRGBAToCairoColor (widget.StyleContext.GetBackgroundColor (StateFlags.Normal));
            return CairoExtensions.AlphaBlend (text_color, background_color, 0.5);
        }

        protected override void OnColorsRefreshed ()
        {
            base.OnColorsRefreshed ();

            // On Windows we use Normal b/c Active incorrectly returns black (at least on XP)
            // TODO: Check if this is still needed with GTK 3
            border_color = CairoExtensions.GdkRGBAToCairoColor (Widget.StyleContext.GetBorderColor (
                    Hyena.PlatformDetection.IsWindows ? StateFlags.Normal : StateFlags.Active));
        }

        public override void DrawPie (double fraction)
        {
            // Calculate the pie path
            fraction = Theme.Clamp (0.0, 1.0, fraction);
            double a1 = 3.0 * Math.PI / 2.0;
            double a2 = a1 + 2.0 * Math.PI * fraction;

            if (fraction == 0.0) {
                return;
            }

            Context.Cairo.MoveTo (Context.X, Context.Y);
            Context.Cairo.Arc (Context.X, Context.Y, Context.Radius, a1, a2);
            Context.Cairo.LineTo (Context.X, Context.Y);

            // Fill the pie
            Color color_a = CairoExtensions.GdkRGBAToCairoColor (
                Widget.StyleContext.GetBackgroundColor (StateFlags.Selected));
            Color color_b = CairoExtensions.ColorShade (color_a, 1.4);

            using (var fill = new RadialGradient (Context.X, Context.Y, 0, Context.X, Context.Y,
                                                  2.0 * Context.Radius)) {
                fill.AddColorStop (0, color_a);
                fill.AddColorStop (1, color_b);
                Context.Cairo.SetSource (fill);

                Context.Cairo.FillPreserve ();
            }

            // Stroke the pie
            Context.Cairo.SetSourceColor (CairoExtensions.ColorShade (color_a, 0.8));
            Context.Cairo.LineWidth = Context.LineWidth;
            Context.Cairo.Stroke ();
        }

        public override void DrawArrow (Context cr, Gdk.Rectangle alloc, double rotation)
        {
            // 0 means pointing to the right for us, but it means pointing to the top for RenderArrow
            rotation += Math.PI / 2.0;

            int size = Math.Min (alloc.Height, alloc.Width);

            Widget.StyleContext.RenderArrow (cr, rotation, alloc.X, alloc.Y, size);
        }

        public override void DrawFrameBackground (Cairo.Context cr, Gdk.Rectangle alloc, Cairo.Color color, Cairo.Pattern pattern)
        {
            color.A = Context.FillAlpha;
            if (pattern != null) {
                cr.SetSource (pattern);
            } else {
                cr.SetSourceColor (color);
            }
            CairoExtensions.RoundedRectangle (cr, alloc.X, alloc.Y, alloc.Width, alloc.Height, Context.Radius, CairoCorners.All);
            cr.Fill ();
        }

        public override void DrawFrameBorder (Cairo.Context cr, Gdk.Rectangle alloc)
        {
            var corners = CairoCorners.All;
            double top_extend = 0;
            double bottom_extend = 0;
            double left_extend = 0;
            double right_extend = 0;

            if (Context.ToplevelBorderCollapse) {
                if (Widget.Allocation.Top <= Widget.Toplevel.Allocation.Top) {
                    corners &= ~(CairoCorners.TopLeft | CairoCorners.TopRight);
                    top_extend = cr.LineWidth;
                }

                if (Widget.Allocation.Bottom >= Widget.Toplevel.Allocation.Bottom) {
                    corners &= ~(CairoCorners.BottomLeft | CairoCorners.BottomRight);
                    bottom_extend = cr.LineWidth;
                }

                if (Widget.Allocation.Left <= Widget.Toplevel.Allocation.Left) {
                    corners &= ~(CairoCorners.BottomLeft | CairoCorners.TopLeft);
                    left_extend = cr.LineWidth;
                }

                if (Widget.Allocation.Right >= Widget.Toplevel.Allocation.Right) {
                    corners &= ~(CairoCorners.BottomRight | CairoCorners.TopRight);
                    right_extend = cr.LineWidth;
                }
            }

            // FIXME Windows; shading the color by .8 makes it blend into the bg
            if (Widget.HasFocus && !Hyena.PlatformDetection.IsWindows) {
                cr.LineWidth = BorderWidth * 1.5;
                cr.SetSourceColor (CairoExtensions.ColorShade (border_color, 0.8));
            } else {
                cr.LineWidth = BorderWidth;
                cr.SetSourceColor (border_color);
            }

            double offset = (double)cr.LineWidth / 2.0;

            CairoExtensions.RoundedRectangle (cr,
                alloc.X + offset - left_extend,
                alloc.Y + offset - top_extend,
                alloc.Width - cr.LineWidth + left_extend + right_extend,
                alloc.Height - cr.LineWidth - top_extend + bottom_extend,
                Context.Radius,
                corners);

            cr.Stroke ();
        }

        public override void DrawHighlightFrame (Cairo.Context cr, int x, int y, int width, int height)
        {
            cr.LineWidth = 1.0;
            cr.SetSourceColor (SelectionStroke);
            CairoExtensions.RoundedRectangle (cr, x + 0.5, y + 0.5, width - 1, height - 1, Context.Radius);
            cr.Stroke ();
        }

        public override void DrawRowCursor (Cairo.Context cr, int x, int y, int width, int height,
                                            Cairo.Color color, CairoCorners corners)
        {
            cr.LineWidth = 1.25;
            cr.SetSourceColor (color);
            CairoExtensions.RoundedRectangle (cr, x + cr.LineWidth/2.0, y + cr.LineWidth/2.0,
                width - cr.LineWidth, height - cr.LineWidth, Context.Radius, corners, true);
            cr.Stroke ();
        }
    }
}
