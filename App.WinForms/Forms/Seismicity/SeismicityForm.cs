using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Returns the current seismic parameter values from the E030 control.
        /// Used by MainForm to feed into the load-configuration builder.
        /// </summary>
        public IReadOnlyDictionary<string, double> GetCurrentValues() =>
            _e030Control.GetValoresActuales();
    }
}
