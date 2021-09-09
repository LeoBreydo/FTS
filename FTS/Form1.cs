using System;
using System.IO;
using System.Windows.Forms;
using BrokerFacadeIB;
using Driver;

namespace FTS
{
    public partial class Form1 : Form
    {
        private MainObject _mainObject;
        private readonly string fname_IbCredentials;
        private IBCredentials credentials;
        public Form1()
        {
            InitializeComponent();
            fname_IbCredentials = Path.GetFullPath("IbCredentials.xml");
            credentials = IBCredentials.Restore(fname_IbCredentials);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (credentials == null || string.IsNullOrEmpty(credentials.Password) ||
                string.IsNullOrEmpty(credentials.Location) ||
                !File.Exists(credentials.Location)) 
            {
                var newCred=CredentialsForm.Call(credentials, fname_IbCredentials, this);
                if (newCred == null)
                    return;
                credentials = newCred;
            }

            if (_mainObject == null && !MainObject.Create(credentials, out _mainObject, out string error))
            {
                // invalid trading config or whatever
                MessageBox.Show(this, "Failed to initialize trading service: " + error, "Rejection");
                return;
            }
            _mainObject.StartWork();
            EnableButtons();
        }
        private void btnEditTwsCredentials_Click(object sender, EventArgs e)
        {
            var newCred = CredentialsForm.Call(credentials, fname_IbCredentials, this);
            if (newCred != null)
                credentials = newCred;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            InitiateToStopTradingService(false);
        }

        private void InitiateToStopTradingService(bool closeWhenIsDone)
        {
            if (closeWhenIsDone)
                _closeWhenIsDone = true;

            Cursor = Cursors.WaitCursor;
            btnStart.Enabled = btnStop.Enabled = false;

            BeginInvoke((Action)(() =>
            {
                _mainObject.StopWork();
            }));

            _secondsLeft = 0;
            timer1.Start();

        }

        private int _secondsLeft;
        private bool _closeWhenIsDone;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_mainObject.IsStopping)
            {
                btnStop.Text=string.Format("Stop ({0})", ++_secondsLeft);
                return;
            }

            timer1.Stop();

            if (_closeWhenIsDone)
                Close();
            else
            {
                btnStop.Text = "Stop";
                EnableButtons();
                Cursor = Cursors.Default;
            }
        }

        private void EnableButtons()
        {
            if (_mainObject == null || !_mainObject.IsStarted)
            {
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
            else if (_mainObject.IsStopping)
            {
                btnStart.Enabled = btnStop.Enabled = false;
            }
            else
            {
                btnStart.Enabled = false;
                btnStop.Enabled = true;
            }

            btnEditTwsCredentials.Enabled = btnStart.Enabled;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_mainObject == null || !_mainObject.IsStarted)  // not started 
                return; // ok, close is allowed

            if (_mainObject.IsStopping) // stopping
            {
                _closeWhenIsDone = true;
                e.Cancel = true;
                Cursor = Cursors.WaitCursor;
                return;
            }

            // starting or working
            if (DialogResult.Yes == MessageBox.Show(this, "Will you stop TradingService and exit?", "Confirmation", MessageBoxButtons.YesNo))
                InitiateToStopTradingService(true);

            e.Cancel = true;
        }

    }
}
