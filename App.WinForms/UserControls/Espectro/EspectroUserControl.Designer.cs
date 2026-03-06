namespace App.WinForms.UserControls.Espectro
{
    partial class EspectroUserControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            _gridParams?.Dispose();
            _gridEspectro?.Dispose();
            _toolbar?.Dispose();
            _pbSa?.Dispose();
            _pbSv?.Dispose();
            _pbSd?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
        }
    }
}
