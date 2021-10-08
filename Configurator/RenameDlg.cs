using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Configurator
{
    public partial class RenameDlg : Form
    {
        public static string RenameStrategy(Form parentForm, string initialName, List<string> usedNames)
        {
            using (var dlg = new RenameDlg(initialName, usedNames))
            {
                dlg.Text = "Rename strategy";
                dlg.label1.Text = "Rename strategy as";
                dlg.ShowDialog(parentForm);
                return dlg._result;
            }
        }

        private string _result;
        private readonly List<string> _usedNames;
        private readonly string _initialName;
        public RenameDlg(string initialName, List<string> usedNames)
        {
            InitializeComponent();
            _usedNames = new List<string>(usedNames);
            _initialName = initialName;
            var ix=_usedNames.FindIndex(item => string.Equals(item, initialName, StringComparison.OrdinalIgnoreCase));
            if (ix >= 0)
                _usedNames.RemoveAt(ix);

            textBox1.Text = initialName;
            //textBox1.BackColor = SetTxtBackColor();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.BackColor = SetTxtBackColor();
        }


        private static readonly Color InvalidValueBackColor = Color.FromArgb(255, 230, 235);

        private Color SetTxtBackColor()
        {
            string name = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(name)) return Color.White;
            if (!name.IsIdentifier()) return InvalidValueBackColor;
            if (_usedNames.Any(item => string.Equals(item, name, StringComparison.OrdinalIgnoreCase)))
                return InvalidValueBackColor;

            return Color.White;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string err = SetResult();
            if (err == null)
                Close();
            else
                MessageBox.Show(this, err, "Rejection");
        }

        private string SetResult()
        {
            string name = textBox1.Text.Trim();
            if (name == _initialName) return null;

            if (string.IsNullOrEmpty(name)) return "Name is not set";
            if (!name.IsIdentifier())
                return
                    "Invalid Name, name must consists from alpha-numeric characters only and starts with a letter character";

            if (_usedNames.Any(item => string.Equals(item, name, StringComparison.OrdinalIgnoreCase)))
                return "Specified name is already in use";

            _result = name;
            return null;
        }
    }
}
