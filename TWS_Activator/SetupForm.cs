using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TWS_Activator
{
    public partial class SetupForm : Form
    {
        public static IBClientInfo Call(IBClientInfo info)
        {
            using var dlg = new SetupForm(info);
            dlg.ShowDialog();
            return dlg._result;
        }

        private IBClientInfo _result;
        private SetupForm(IBClientInfo info)
        {
            InitializeComponent();
            if (info != null)
            {
                textLocation.Text = info.ClientLocation;
                textLogin.Text = info.Login;
                textPassword.Text = info.Password;
            }
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {
            Font = new Font(this.Font.FontFamily, 10);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = @"TWS application (tws.exe)|tws.exe|All files (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = true,
                RestoreDirectory = true,
                Multiselect = false,
                Title = @"Select location of the TraderWorkstation"
            };
            if (File.Exists(textLocation.Text))
                dlg.FileName = textLocation.Text;

            if (dlg.ShowDialog() == DialogResult.OK)
                textLocation.Text = dlg.FileName;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string err = SetResult();
            if (err == null)
                Close();
            else
                MessageBox.Show(this, err, @"Rejection");
        }

        private string SetResult()
        {
            if (string.IsNullOrWhiteSpace(textLocation.Text))
            {
                btnBrowse.Focus();
                return "Path to IB TraderWorkstation is not set";
            }
            if (!File.Exists(textLocation.Text))
            {
                btnBrowse.Focus();
                return "Path to IB TraderWorkstation is invalid (file not found)";
            }

            textLogin.Text = textLogin.Text.Trim();
            if (string.IsNullOrWhiteSpace(textLogin.Text))
            {
                textLogin.Focus();
                return "Login is not set";
            }

            textPassword.Text = textPassword.Text.Trim();
            if (string.IsNullOrWhiteSpace(textPassword.Text))
            {
                textPassword.Focus();
                return "Password is not set";
            }

            _result = new IBClientInfo
            {
                ClientLocation = textLocation.Text,
                Login = textLogin.Text,
                Password = textPassword.Text
            };
            // login and password are not verified at this point, so we save the only specified path to application
            new IBClientInfo { ClientLocation = textLocation.Text }.Save();

            return null;
        }
    }
}
