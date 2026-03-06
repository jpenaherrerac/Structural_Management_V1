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
            Size = new System.Drawing.Size(900, 700);
            MinimumSize = new System.Drawing.Size(800, 600);
            StartPosition = FormStartPosition.CenterParent;

            _tabControl = new TabControl { Dock = DockStyle.Fill };

            var tabE030 = new TabPage("E030 / NEC");
            _e030Control = new E030UserControl { Dock = DockStyle.Fill };
            _e030Control.ParametersChanged += E030Control_ParametersChanged;
            tabE030.Controls.Add(_e030Control);

            var tabEspectro = new TabPage("Espectro");
            _espectroControl = new EspectroUserControl { Dock = DockStyle.Fill };
            tabEspectro.Controls.Add(_espectroControl);

            _tabControl.TabPages.Add(tabE030);
            _tabControl.TabPages.Add(tabEspectro);

            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 44 };
            var btnCerrar = new Button
            {
                Text = "Cerrar",
                Width = 100,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Left = this.ClientSize.Width - 120,
                Top = 8
            };
            btnCerrar.Click += (s, e) => Close();
            btnPanel.Controls.Add(btnCerrar);

            Controls.Add(_tabControl);
            Controls.Add(btnPanel);
        }
    }
}
