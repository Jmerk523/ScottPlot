using ScottPlot.Drawing;
using System.Drawing;

namespace ScottPlot.Renderable
{
    public class FigureBackground : IRenderable
    {
        public Color Color { get; set; } = Color.White;
        public bool IsVisible { get; set; } = true;
        public bool Blend { get; set; } = false;

        public void Render(PlotDimensions dims, Bitmap bmp, bool lowQuality = false)
        {
            if (IsVisible)
            {
                using (var gfx = GDI.Graphics(bmp, lowQuality: true))
                {
                    if (Blend)
                    {
                        using (var fill = new SolidBrush(Color))
                        {
                            gfx.FillRegion(fill, gfx.Clip);
                        }
                    }
                    else
                    {
                        gfx.Clear(Color);
                    }
                }
            }
        }
    }
}
