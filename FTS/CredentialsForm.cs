using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using BrokerFacadeIB;

namespace FTS
{
    public partial class CredentialsForm : Form
    {
        private const string defaultLocation = @"C:\Jts\tws.exe";
        public static IBCredentials Call(IBCredentials toEdit, string fileNameToSave, Form parentForm)
        {
            using (var dlg = new CredentialsForm(toEdit, fileNameToSave))
            {
                dlg.ShowDialog(parentForm);
                return dlg._result;
            }
        }

        private readonly string _fileNameToSave;
        private IBCredentials _result;

        public CredentialsForm(IBCredentials toEdit, string fileNameToSave)
        {
            _fileNameToSave = fileNameToSave;
            InitializeComponent();

            if (toEdit == null)
            {
                textPort.Text = "3497";
                cbRememberPassword.Checked = true;
                if (File.Exists(defaultLocation))
                    textLocation.Text = defaultLocation;
            }
            else
            {
                textLocation.Text = toEdit.Location ?? "";
                textLogin.Text = toEdit.Login ?? "";
                textPassword.Text = toEdit.Password ?? "";
                textPort.Text = toEdit.Port.ToString();
                cbRememberPassword.Checked = !string.IsNullOrEmpty(toEdit.Password);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog
            {
                Filter = "TWS application (tws.exe)|tws.exe|All files (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = true,
                RestoreDirectory = true,
                Multiselect = false,
                Title = "Select location of the TraderWorkstation"
            })
            {
                if (File.Exists(textLocation.Text))
                    dlg.FileName = textLocation.Text;

                if (dlg.ShowDialog() == DialogResult.OK)
                    textLocation.Text = dlg.FileName;
            }
        }

        private string SetResult(out IBCredentials result)
        {
            result = null;
            var location = textLocation.Text;
            if (string.IsNullOrWhiteSpace(location))
            {
                btnBrowse.Focus();
                return "Path to IB TraderWorkstation is not set";
            }
            if (!File.Exists(location))
            {
                btnBrowse.Focus();
                return "Path to IB TraderWorkstation is invalid (file not found)";
            }

            var login=textLogin.Text = textLogin.Text.Trim();
            if (string.IsNullOrWhiteSpace(login))
            {
                textLogin.Focus();
                return "Login is not set";
            }

            var password=textPassword.Text = textPassword.Text.Trim();
            if (string.IsNullOrWhiteSpace(password))
            {
                textPassword.Focus();
                return "Password is not set";
            }

            if (!int.TryParse(textPort.Text.Trim(), out int port) || port <= 0)
            {
                textPort.Focus();
                return "Invalid port number";
            }

            result = new IBCredentials
            {
                Location = location,
                Login = login,
                Password = cbRememberPassword.Checked ? password : null,
                Port = port
            };
            result.Save(_fileNameToSave);
            result.Password = password;
            return null;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string err = SetResult(out _result);
            if (err == null)
                Close();
            else 
                MessageBox.Show(this, err, "Rejection");
        }
        private void btnTry_Click(object sender, EventArgs e)
        {
            string err = SetResult(out var res);
            if (err != null)
            {
                MessageBox.Show(this, err, "Rejection");
                return;
            }

            var startInfo =
                new ProcessStartInfo(res.Location,
                    string.Format("username={0} password={1}", res.Login, res.Password))
                {
                    WorkingDirectory = Path.GetDirectoryName(res.Location)
                };

            new Process {StartInfo = startInfo}.Start();
        }
    }
}
