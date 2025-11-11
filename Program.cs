using System;
using System.Drawing;
using System.Windows.Forms;

namespace LabWorkGraph20
{
    public class GraphForm : Form
    {
        const double XMin = 2.3;
        const double XMax = 8.3;
        const double TickDx = 0.6;
        const int Margin = 50;

        public GraphForm()
        {
            Text = "Графік: y = (x+2)^2 / sqrt(x^2+1)";
            DoubleBuffered = true;
            ClientSize = new Size(900, 550);
            Resize += (_, __) => Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int w = ClientSize.Width, h = ClientSize.Height;
            int plotW = Math.Max(1, w - 2 * Margin);
            int plotH = Math.Max(1, h - 2 * Margin);
            var plot = new Rectangle(Margin, Margin, plotW, plotH);

            double yMin = double.PositiveInfinity, yMax = double.NegativeInfinity;
            for (double x = XMin; x <= XMax; x += 0.001)
            {
                double y = F(x);
                if (double.IsFinite(y))
                {
                    if (y < yMin) yMin = y;
                    if (y > yMax) yMax = y;
                }
            }
            if (!double.IsFinite(yMin) || !double.IsFinite(yMax) || yMin == yMax)
            { yMin = -5; yMax = 5; }

            PointF ToPix(double x, double y)
            {
                float px = (float)(plot.Left + (x - XMin) / (XMax - XMin) * plot.Width);
                float py = (float)(plot.Bottom - (y - yMin) / (yMax - yMin) * plot.Height);
                return new PointF(px, py);
            }

            g.FillRectangle(Brushes.White, plot);
            g.DrawRectangle(Pens.Gray, plot);

            using var axisPen = new Pen(Color.Black, 2);
            using var gridPen = new Pen(Color.LightGray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
            using var curvePen = new Pen(Color.Red, 2);
            using var font = new Font("Segoe UI", 9f);

            if (yMin < 0 && 0 < yMax) g.DrawLine(axisPen, ToPix(XMin, 0), ToPix(XMax, 0));
            g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);

            for (double x = XMin; x <= XMax + 1e-9; x += TickDx)
            {
                var pB = ToPix(x, yMin);
                var pT = ToPix(x, yMax);
                g.DrawLine(gridPen, pB.X, pT.Y, pB.X, pB.Y);
                g.DrawLine(Pens.Black, pB.X, plot.Bottom, pB.X, plot.Bottom + 6);
                var s = x.ToString("0.0");
                var sz = g.MeasureString(s, font);
                g.DrawString(s, font, Brushes.Black, pB.X - sz.Width / 2, plot.Bottom + 6);
            }

            for (int i = 1; i <= 5; i++)
            {
                double y = yMin + i * (yMax - yMin) / 6.0;
                g.DrawLine(gridPen, ToPix(XMin, y), ToPix(XMax, y));
            }

            PointF? prev = null;
            for (double x = XMin; x <= XMax; x += 0.001)
            {
                double y = F(x);
                if (!double.IsFinite(y)) { prev = null; continue; }
                var p = ToPix(x, y);
                if (prev.HasValue) g.DrawLine(curvePen, prev.Value, p);
                prev = p;
            }

            g.DrawString("X", font, Brushes.Black, plot.Right + 15, plot.Bottom - 15);
            g.DrawString("Y", font, Brushes.Black, plot.Left - 15, plot.Top - 20);
            g.DrawString($"y=(x+2)^2/√(x^2+1),  x∈[{XMin}; {XMax}],  Δx={TickDx}", font, Brushes.Black, Margin, Margin - 30);
        }

        static double F(double x) => ((x + 2) * (x + 2)) / Math.Sqrt(x * x + 1);
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GraphForm());
        }
    }
}
