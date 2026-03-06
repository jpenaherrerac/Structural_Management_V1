using System;
using System.Windows.Forms;
using App.WinForms.UserControls.E030;
using App.WinForms.UserControls.Espectro;

namespace App.WinForms.Forms.Seismicity
{
    public partial class SeismicityForm : Form
    {
        private TabControl _tabControl;
        private E030UserControl _e030Control;
        private EspectroUserControl _espectroControl;

        public SeismicityForm()
        {
            InitializeComponent();
        }

        private void E030Control_ParametersChanged(object sender, SeismicParametersEventArgs e)
        {
            // Propagate updated seismic parameters to the spectrum tab
            _espectroControl.UpdateFromSeismicParameters(e.Parameters);
        }
    }
}
