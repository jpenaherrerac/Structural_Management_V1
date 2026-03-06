using System;
using System.Windows.Forms;

namespace App.WinForms.UserControls.E030
{
    public partial class E030UserControl : UserControl
    {
        public event EventHandler<SeismicParametersEventArgs> ParametersChanged;

        private ComboBox _cmbZone;
        private ComboBox _cmbSoilType;
        private ComboBox _cmbUsage;
        private NumericUpDown _nudR;
        private NumericUpDown _nudCt;
        private NumericUpDown _nudAlpha;
        private NumericUpDown _nudHeight;
        private NumericUpDown _nudTp;
        private NumericUpDown _nudTl;
        private NumericUpDown _nudFa;
        private NumericUpDown _nudFd;
        private NumericUpDown _nudFs;
        private Label _lblComputedPeriod;
        private Label _lblSa;
        private Button _btnCalculate;
        private RichTextBox _rtbResults;

        public E030UserControl()
        {
            InitializeComponent();
        }

        public SeismicParameters GetCurrentParameters()
        {
            var (zone, z) = GetZoneValues();
            var (soilType, s) = GetSoilValues();
            var (usage, i) = GetUsageValues();

            return new SeismicParameters
            {
                Zone = zone, Z = z,
                SoilType = soilType, S = s,
                UsageCategory = usage, I = i,
                R = (double)_nudR.Value,
                Ct = (double)_nudCt.Value,
                Alpha = (double)_nudAlpha.Value,
                Tp = (double)_nudTp.Value,
                Tl = (double)_nudTl.Value,
                Fa = (double)_nudFa.Value,
                Fd = (double)_nudFd.Value,
                Fs = (double)_nudFs.Value,
                BuildingHeight = (double)_nudHeight.Value
            };
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            var p = GetCurrentParameters();
            double H = p.BuildingHeight;
            double T = p.Ct * Math.Pow(H, p.Alpha);
            p.ComputedPeriod = T;

            double sa = ComputeSa(p, T);
            double V_over_W = sa / p.R;

            _lblComputedPeriod.Text = $"T = {T:F3} s";
            _lblSa.Text = $"Sa(T) = {sa:F4} g";

            _rtbResults.Clear();
            _rtbResults.AppendText("═══════════════════════════════════════\n");
            _rtbResults.AppendText("     PARÁMETROS SÍSMICOS (NEC / E030)\n");
            _rtbResults.AppendText("═══════════════════════════════════════\n");
            _rtbResults.AppendText($"  Zona:            {p.Zone}  (Z = {p.Z})\n");
            _rtbResults.AppendText($"  Tipo de suelo:   {p.SoilType}  (S = {p.S})\n");
            _rtbResults.AppendText($"  Categoría de uso:{p.UsageCategory}  (I = {p.I})\n");
            _rtbResults.AppendText($"  Factor R:         {p.R}\n");
            _rtbResults.AppendText($"  Ct:               {p.Ct}\n");
            _rtbResults.AppendText($"  α:                {p.Alpha}\n");
            _rtbResults.AppendText($"  Altura (H):       {H:F1} m\n");
            _rtbResults.AppendText($"  Tp:               {p.Tp:F3} s\n");
            _rtbResults.AppendText($"  Tl:               {p.Tl:F3} s\n");
            _rtbResults.AppendText("───────────────────────────────────────\n");
            _rtbResults.AppendText($"  Período T:        {T:F3} s\n");
            _rtbResults.AppendText($"  Sa(T):            {sa:F4} g\n");
            _rtbResults.AppendText($"  V/W = Sa/R:       {V_over_W:F4}\n");
            _rtbResults.AppendText("═══════════════════════════════════════\n");

            ParametersChanged?.Invoke(this, new SeismicParametersEventArgs(p));
        }

        private static double ComputeSa(SeismicParameters p, double T)
        {
            double spectralPlateau = p.Z * p.S * p.I;
            double eta = 1.0;
            if (T <= 0) return spectralPlateau * eta;
            if (T < p.Tp)
                return spectralPlateau * eta * (1.0 + (eta - 1.0) * T / p.Tp);
            else if (T < p.Tl)
                return spectralPlateau * eta * p.Tp / T;
            else
                return spectralPlateau * eta * p.Tp * p.Tl / (T * T);
        }

        private (string zone, double z) GetZoneValues()
        {
            return _cmbZone.SelectedIndex switch
            {
                0 => ("I", 0.15),
                1 => ("II", 0.25),
                2 => ("III", 0.30),
                3 => ("IV", 0.35),
                4 => ("V", 0.40),
                5 => ("VI", 0.50),
                _ => ("IV", 0.35)
            };
        }

        private (string soilType, double s) GetSoilValues()
        {
            return _cmbSoilType.SelectedIndex switch
            {
                0 => ("A", 1.00),
                1 => ("B", 1.20),
                2 => ("C", 1.11),
                3 => ("D", 1.11),
                4 => ("E", 1.30),
                _ => ("D", 1.11)
            };
        }

        private (string usage, double i) GetUsageValues()
        {
            return _cmbUsage.SelectedIndex switch
            {
                0 => ("I – Residencial / Comercial menor", 1.00),
                1 => ("II – Comercial / Industrial", 1.25),
                2 => ("III – Esencial / Hospitales", 1.50),
                _ => ("I – Residencial / Comercial menor", 1.00)
            };
        }

        private void CmbZone_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update Tp/Tl defaults when zone changes (NEC table values)
            var (_, z) = GetZoneValues();
            var (soil, _) = GetSoilValues();
            UpdateTpTl(z, soil);
        }

        private void CmbSoilType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var (_, z) = GetZoneValues();
            var (soil, _) = GetSoilValues();
            UpdateTpTl(z, soil);
        }

        private void UpdateTpTl(double z, string soil)
        {
            // NEC SE-DS Table 3 - approximate Tp and Tl values
            var tp = soil switch
            {
                "A" => 0.10,
                "B" => 0.15,
                "C" => 0.20,
                "D" => 0.30,
                "E" => 0.40,
                _ => 0.30
            };
            var tl = soil switch
            {
                "A" => 2.0,
                "B" => 2.5,
                "C" => 3.0,
                "D" => 4.0,
                "E" => 5.0,
                _ => 4.0
            };
            _nudTp.Value = (decimal)tp;
            _nudTl.Value = (decimal)tl;
        }
    }
}
