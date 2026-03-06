using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using App.WinForms.UserControls.E030;

namespace App.WinForms.UserControls.Espectro
{
    public partial class EspectroUserControl : UserControl
    {
        private List<(double T, double Sa)> _spectrumPoints = new List<(double, double)>();

        public EspectroUserControl()
        {
            InitializeComponent();
        }

        public void UpdateFromSeismicParameters(SeismicParameters p)
        {
            if (p == null) return;

            _nudZ.Value = (decimal)p.Z;
            _nudS.Value = (decimal)p.S;
            _nudI.Value = (decimal)p.I;
            _nudR.Value = (decimal)p.R;
            _nudTp.Value = (decimal)p.Tp;
            _nudTl.Value = (decimal)p.Tl;

            GenerateSpectrum();
        }

        private void BtnGenerate_Click(object sender, EventArgs e) => GenerateSpectrum();

        private void GenerateSpectrum()
        {
            double z = (double)_nudZ.Value;
            double s = (double)_nudS.Value;
            double i = (double)_nudI.Value;
            double r = (double)_nudR.Value;
            double tp = (double)_nudTp.Value;
            double tl = (double)_nudTl.Value;
            double eta = (double)_nudEta.Value;
            double tMax = (double)_nudTMax.Value;
            int nPoints = (int)_nudNPoints.Value;

            _spectrumPoints = new List<(double, double)>();
            _dgvSpectrum.Rows.Clear();

            double spectralPlateau = z * s * i;
            double dt = tMax / nPoints;

            for (int idx = 0; idx <= nPoints; idx++)
            {
                double T = idx * dt;
                double sa = ComputeSa(spectralPlateau, eta, tp, tl, T);
                double saOverR = sa / r;
                _spectrumPoints.Add((T, sa));
                _dgvSpectrum.Rows.Add(T.ToString("F3"), sa.ToString("F4"), saOverR.ToString("F4"));
            }

            _panelChart.Invalidate();

            _lblInfo.Text = $"Z·S·I = {spectralPlateau:F4}   |   Sa_plateau = {spectralPlateau * eta:F4} g   |   Tp = {tp:F3}s   |   Tl = {tl:F2}s";
        }

        private static double ComputeSa(double spectralPlateau, double eta, double tp, double tl, double T)
        {
            if (T <= 0) return spectralPlateau * eta;
            if (T < tp) return spectralPlateau * eta * (1.0 + (eta - 1.0) * T / tp);
            if (T < tl) return spectralPlateau * eta * tp / T;
            return spectralPlateau * eta * tp * tl / (T * T);
        }

        private void PanelChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var panel = _panelChart;
            int pw = panel.Width, ph = panel.Height;
            int mx = 50, my = 20, mr = 20, mb = 40;
            int chartW = pw - mx - mr, chartH = ph - my - mb;
            if (chartW <= 0 || chartH <= 0 || _spectrumPoints.Count == 0) return;

            g.Clear(Color.FromArgb(240, 244, 248));

            // Grid
            using var gridPen = new Pen(Color.FromArgb(200, 200, 210));
            for (int gx = 0; gx <= 5; gx++)
            {
                int x = mx + gx * chartW / 5;
                g.DrawLine(gridPen, x, my, x, my + chartH);
            }
            for (int gy = 0; gy <= 4; gy++)
            {
                int y = my + gy * chartH / 4;
                g.DrawLine(gridPen, mx, y, mx + chartW, y);
            }

            // Axes
            using var axisPen = new Pen(Color.FromArgb(80, 90, 110), 2);
            g.DrawRectangle(axisPen, mx, my, chartW, chartH);

            double tMax = (double)_nudTMax.Value;
            double saMax = 0;
            foreach (var (_, sa) in _spectrumPoints) if (sa > saMax) saMax = sa;
            saMax = Math.Max(saMax * 1.1, 0.1);

            // Labels
            using var labelFont = new Font("Segoe UI", 7.5F);
            using var labelBrush = new SolidBrush(Color.FromArgb(60, 70, 90));
            for (int gx = 0; gx <= 5; gx++)
            {
                double t = gx * tMax / 5.0;
                int x = mx + gx * chartW / 5;
                g.DrawString(t.ToString("F1"), labelFont, labelBrush, x - 8, my + chartH + 4);
            }
            for (int gy = 0; gy <= 4; gy++)
            {
                double sa = saMax * (1.0 - gy / 4.0);
                int y = my + gy * chartH / 4;
                g.DrawString(sa.ToString("F3"), labelFont, labelBrush, 0, y - 6);
            }

            // Axis titles
            using var titleFont = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            g.DrawString("T (s)", titleFont, labelBrush, mx + chartW / 2 - 15, my + chartH + 22);

            // Spectrum curve
            using var specPen = new Pen(Color.FromArgb(0, 114, 189), 2.5F);
            var pts = new List<PointF>();
            foreach (var (T, Sa) in _spectrumPoints)
            {
                float x = mx + (float)(T / tMax * chartW);
                float y = my + chartH - (float)(Sa / saMax * chartH);
                pts.Add(new PointF(x, y));
            }
            if (pts.Count >= 2) g.DrawLines(specPen, pts.ToArray());

            // Sa/R curve
            double r = (double)_nudR.Value;
            using var reducedPen = new Pen(Color.FromArgb(217, 83, 25), 2F) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            var rPts = new List<PointF>();
            foreach (var (T, Sa) in _spectrumPoints)
            {
                float x = mx + (float)(T / tMax * chartW);
                float y = my + chartH - (float)(Sa / r / saMax * chartH);
                rPts.Add(new PointF(x, y));
            }
            if (rPts.Count >= 2) g.DrawLines(reducedPen, rPts.ToArray());

            // Legend
            g.FillRectangle(new SolidBrush(Color.FromArgb(0, 114, 189)), mx + 8, my + 8, 20, 3);
            g.DrawString("Sa(T)", labelFont, labelBrush, mx + 32, my + 3);
            g.FillRectangle(new SolidBrush(Color.FromArgb(217, 83, 25)), mx + 8, my + 22, 20, 3);
            g.DrawString("Sa(T)/R", labelFont, labelBrush, mx + 32, my + 17);
        }
    }
}
