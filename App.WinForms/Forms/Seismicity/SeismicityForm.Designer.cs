using System.Windows.Forms;
using App.WinForms.UserControls.E030;
using App.WinForms.UserControls.Espectro;

namespace App.WinForms.Forms.Seismicity
{
    partial class SeismicityForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Text = "Parámetros Sísmicos";
            Size = new System.Drawing.Size(1280, 850);
            MinimumSize = new System.Drawing.Size(1024, 700);
            StartPosition = FormStartPosition.CenterParent;

            _tabControl = new TabControl { Dock = DockStyle.Fill };

            // ── Tab E030 ─────────────────────────────────────────────────────
            var tabE030 = new TabPage("E030");
            _e030Control = new E030UserControl { Dock = DockStyle.Fill };
            _e030Control.ValoresActualesChanged += E030Control_ValoresActualesChanged;
            tabE030.Controls.Add(_e030Control);

            // ── Tab Espectro ─────────────────────────────────────────────────
            var tabEspectro = new TabPage("Espectro");
            _espectroControl = new EspectroUserControl { Dock = DockStyle.Fill };
            _espectroControl.SetParameterProvider(() => _e030Control.GetValoresActuales());
            tabEspectro.Controls.Add(_espectroControl);

            _tabControl.TabPages.Add(tabE030);
            _tabControl.TabPages.Add(tabEspectro);

            // Build Espectro UI when user switches to that tab
            _tabControl.SelectedIndexChanged += (s, e) =>
            {
                if (_tabControl.SelectedTab == tabEspectro)
                    _espectroControl.Build();
            };

            // ── Bottom panel with Close button ───────────────────────────────
            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 44 };
            var btnCerrar = new Button
            {
                Text = "Cerrar", Width = 100,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Left = this.ClientSize.Width - 120, Top = 8
            };
            btnCerrar.Click += (s, e) => Close();
            btnPanel.Controls.Add(btnCerrar);

            Controls.Add(_tabControl);
            Controls.Add(btnPanel);
        }
    }
}
