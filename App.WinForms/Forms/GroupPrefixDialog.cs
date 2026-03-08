using System;
using System.Drawing;
using System.Windows.Forms;
using App.Domain.Entities.Elements;

namespace App.WinForms.Forms
{
    /// <summary>
    /// Dialog to view and edit group-prefix names used to discover SAP2000 groups
    /// for each structural element type (Vigas, Columnas, Muros, Losas).
    /// </summary>
    public sealed class GroupPrefixDialog : Form
    {
        private TextBox _txtBeamPrefix;
        private TextBox _txtColumnPrefix;
        private TextBox _txtWallPrefix;
        private TextBox _txtSlabPrefix;
        private Button _btnOk;
        private Button _btnCancel;
        private Button _btnDefaults;

        /// <summary>The resulting configuration after the user clicks OK.</summary>
        public GroupPrefixConfiguration ResultConfiguration { get; private set; }

        public GroupPrefixDialog(GroupPrefixConfiguration current)
        {
            current = current ?? new GroupPrefixConfiguration();
            ResultConfiguration = current;
            InitializeLayout(current);
        }

        private void InitializeLayout(GroupPrefixConfiguration cfg)
        {
            Text = "Configuración de Prefijos de Grupos";
            Size = new Size(440, 320);
            MinimumSize = new Size(400, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var fLabel = new Font("Segoe UI", 9.5F);
            int y = 20;

            // Vigas
            Controls.Add(new Label { Text = "Prefijo para Vigas:", Location = new Point(20, y), AutoSize = true, Font = fLabel });
            _txtBeamPrefix = new TextBox { Text = cfg.BeamPrefix, Location = new Point(200, y - 2), Width = 200, Font = fLabel };
            Controls.Add(_txtBeamPrefix);
            y += 40;

            // Columnas
            Controls.Add(new Label { Text = "Prefijo para Columnas:", Location = new Point(20, y), AutoSize = true, Font = fLabel });
            _txtColumnPrefix = new TextBox { Text = cfg.ColumnPrefix, Location = new Point(200, y - 2), Width = 200, Font = fLabel };
            Controls.Add(_txtColumnPrefix);
            y += 40;

            // Muros de corte
            Controls.Add(new Label { Text = "Prefijo para Muros:", Location = new Point(20, y), AutoSize = true, Font = fLabel });
            _txtWallPrefix = new TextBox { Text = cfg.ShearWallPrefix, Location = new Point(200, y - 2), Width = 200, Font = fLabel };
            Controls.Add(_txtWallPrefix);
            y += 40;

            // Losas
            Controls.Add(new Label { Text = "Prefijo para Losas:", Location = new Point(20, y), AutoSize = true, Font = fLabel });
            _txtSlabPrefix = new TextBox { Text = cfg.SlabPrefix, Location = new Point(200, y - 2), Width = 200, Font = fLabel };
            Controls.Add(_txtSlabPrefix);
            y += 50;

            // Buttons
            _btnDefaults = new Button { Text = "Restaurar valores", Location = new Point(20, y), Size = new Size(130, 30) };
            _btnDefaults.Click += (s, e) =>
            {
                var def = new GroupPrefixConfiguration();
                _txtBeamPrefix.Text = def.BeamPrefix;
                _txtColumnPrefix.Text = def.ColumnPrefix;
                _txtWallPrefix.Text = def.ShearWallPrefix;
                _txtSlabPrefix.Text = def.SlabPrefix;
            };
            Controls.Add(_btnDefaults);

            _btnOk = new Button { Text = "Aceptar", Location = new Point(220, y), Size = new Size(85, 30), DialogResult = DialogResult.OK };
            _btnOk.Click += (s, e) =>
            {
                ResultConfiguration = new GroupPrefixConfiguration
                {
                    BeamPrefix = _txtBeamPrefix.Text.Trim(),
                    ColumnPrefix = _txtColumnPrefix.Text.Trim(),
                    ShearWallPrefix = _txtWallPrefix.Text.Trim(),
                    SlabPrefix = _txtSlabPrefix.Text.Trim()
                };
            };
            Controls.Add(_btnOk);

            _btnCancel = new Button { Text = "Cancelar", Location = new Point(315, y), Size = new Size(85, 30), DialogResult = DialogResult.Cancel };
            Controls.Add(_btnCancel);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
        }
    }
}
