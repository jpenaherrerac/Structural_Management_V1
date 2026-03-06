using System;
using System.Windows.Forms;
using App.WinForms.UserControls.E030;
using App.WinForms.UserControls.Espectro;

namespace App.WinForms.Forms.Seismicity
{
    public partial class SeismicityForm : Form
    {
        private TabControl _tabControl = null!;
        private E030UserControl _e030Control = null!;
        private EspectroUserControl _espectroControl = null!;

        public SeismicityForm()
        {
            InitializeComponent();
        }

        private void E030Control_ValoresActualesChanged(object sender, EventArgs e)
        {
            _espectroControl.Refresh();
        }
    }
}
