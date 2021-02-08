﻿using ScottPlot.Drawing;
using System.Drawing;

namespace ScottPlot.Renderable
{
    public class FigureBackground : IRenderable
    {
        public Color Color { get; set; } = Color.White;
        public bool IsVisible { get; set; } = true;

        public void Render(PlotDimensions dims, Bitmap bmp, bool lowQuality = false)
        {
            if (IsVisible)
            {
                using (var gfx = GDI.Graphics(bmp, lowQuality: true))
                {
                    gfx.Clear(Color);
                }
            }
        }
    }
}
