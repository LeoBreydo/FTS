using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TimeZoneConverter;

namespace Configurator
{
    public partial class ScheduleRecordEditor : Form
    {
        public static void AddRecords(Form parentForm,Func<ScheduledIntervalDescription, string> AddRecord,
            List<Tuple<int,string>> cfgItems, ScheduledIntervalDescription sample=null)
        {
            using (var dlg = new ScheduleRecordEditor(AddRecord, cfgItems, sample))
            {
                dlg.Text = "Add Schedule Record";
                dlg.btnOK.Text = "Add";
                dlg.btnCancel.Text = "Close";
                dlg.cbKeepWindowOpened.Visible = true;
                // todo dlg.cbKeepWindowOpened.Checked = _lastCheckedState;

                dlg.ShowDialog(parentForm);
            }
        }
        public static void EditRecord(Form parentForm, Func<ScheduledIntervalDescription, string> EditRecord,
            List<Tuple<int, string>> cfgItems, ScheduledIntervalDescription sample )
        {
            using (var dlg = new ScheduleRecordEditor(EditRecord, cfgItems, sample))
            {
                dlg.Text = "Edit Schedule Record";
                dlg.btnOK.Text = "OK";
                dlg.btnCancel.Text = "Cancel";
                dlg.cbKeepWindowOpened.Visible = false;

                dlg.ShowDialog(parentForm);
            }
        }

        private static readonly List<string> _allTimeZones =
            TZConvert.KnownIanaTimeZoneNames
                .Concat(TZConvert.KnownRailsTimeZoneNames)
                .Concat(TZConvert.KnownWindowsTimeZoneIds)
                .OrderBy(x=>x.ToLower())
                .Distinct()
                .ToList();

        private static List<string> _railsTimeZones;

        private static bool IsRailsTZ(string tz)
        {
            if (string.IsNullOrEmpty(tz)) return false;
            if (_railsTimeZones == null)
                _railsTimeZones = TZConvert.KnownRailsTimeZoneNames.ToList();

            return _railsTimeZones.Contains(tz);
        }

        private static string RailsToIana(string txt)
        {
            return IsRailsTZ(txt) ? TZConvert.RailsToIana(txt) : txt;
        }

        private TimeZoneInfo tziStop, tziDone;
        private bool UseSingleTimeZone;

        class TargetItem
        {
            public readonly int Id;
            public readonly string Name;
            public TargetItem(Tuple<int, string> id_name)
            {
                Id = id_name.Item1;
                Name = id_name.Item2;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private readonly Func<ScheduledIntervalDescription, string> _funAddRecord;

        public ScheduleRecordEditor(Func<ScheduledIntervalDescription, string> funAddRecord,
            List<Tuple<int, string>> cfgItems, ScheduledIntervalDescription sample)
        {
            InitializeComponent();

            dtpSoftStop.CustomFormat= UserSettings.DateTimeFormat;
            dtpHardStop.CustomFormat = UserSettings.DateTimeFormat;
            dtpNoRestr.CustomFormat = UserSettings.DateTimeFormat;

            _funAddRecord = funAddRecord;

            foreach (var item in _allTimeZones)
            {
                cbtzStop.Items.Add(item);
                cbtzDone.Items.Add(item);
            }

            foreach (var id_name in cfgItems)
            {
                cbTarget.Items.Add(new TargetItem(id_name));
                if (sample!=null && id_name.Item1 == sample.Id)
                    cbTarget.SelectedIndex = cbTarget.Items.Count - 1;
            }

            if (cbTarget.SelectedIndex < 0)
                cbTarget.SelectedIndex = 0;

            if (sample != null)
            {
                textInfo.Text = sample.Info;

                if (sample.EnterTimeZoneName != null)
                    cbtzStop.SelectedItem = sample.EnterTimeZoneName;
                if (sample.ExitTimeZoneName!=null)
                    cbtzDone.SelectedItem = sample.ExitTimeZoneName;

                if (sample.HardStopTime != DateTime.MinValue)
                    dtpHardStop.Value = sample.HardStopTime;
                if (sample.NoRestrictionTime != DateTime.MinValue)
                    dtpNoRestr.Value = sample.NoRestrictionTime;

                if (sample.SoftStopTime != null && sample.SoftStopTime.Value != DateTime.MinValue)
                {
                    checkBoxSoftStop.Checked = true;
                    dtpSoftStop.Value = sample.SoftStopTime.Value;
                }
                else
                {
                    checkBoxSoftStop.Checked = false;
                    dtpSoftStop.Value = dtpHardStop.Value;
                }
            }

            tziStop = UpdateTzLabel(cbtzStop, label4);
            tziDone = UpdateTzLabel(cbtzDone, label5);
            UseSingleTimeZone = tziStop?.Id == tziDone?.Id;

            btnCancel.Text = cbKeepWindowOpened.Checked ? "Close" : "Cancel";
        }
        private void ScheduleRecordEditor_Load(object sender, EventArgs e)
        {
            dtpSoftStop.Visible = checkBoxSoftStop.Checked;
        }

        private void checkBoxSoftStop_CheckedChanged(object sender, EventArgs e)
        {
            dtpSoftStop.Visible = checkBoxSoftStop.Checked;
        }

        private void cbtzStop_SelectedIndexChanged(object sender, EventArgs e)
        {
            tziStop=UpdateTzLabel(cbtzStop, label4);

            if (UseSingleTimeZone || tziDone == null)
                cbtzDone.SelectedItem = cbtzStop.SelectedItem;
        }
        private void cbtzDone_SelectedIndexChanged(object sender, EventArgs e)
        {
            tziDone = UpdateTzLabel(cbtzDone, label5);
        }

        private void cbtzStop_Leave(object sender, EventArgs e)
        {
            var txt = cbtzStop.Text;
            if (string.IsNullOrWhiteSpace(txt))
                cbtzStop.SelectedItem = null;
            else
            {

                var bestCand =
                    _allTimeZones.FirstOrDefault(item => string.Equals(txt, item, StringComparison.OrdinalIgnoreCase))
                    ?? _allTimeZones.FirstOrDefault(item =>
                        item.StartsWith(txt, StringComparison.OrdinalIgnoreCase))
                    ?? _allTimeZones.FirstOrDefault(item => item.IndexOf(txt, StringComparison.OrdinalIgnoreCase) >= 0);
                cbtzStop.SelectedItem = bestCand;

                if (bestCand == null)
                    cbtzStop.SelectedItem = null;
            }
            cbtzStop_SelectedIndexChanged(null, null);
        }

        private void cbtzDone_Leave(object sender, EventArgs e)
        {
            var txt = cbtzDone.Text;
            if (string.IsNullOrWhiteSpace(txt))
                cbtzDone.SelectedItem = null;
            else
            {

                var bestCand =
                    _allTimeZones.FirstOrDefault(item => string.Equals(txt, item, StringComparison.OrdinalIgnoreCase))
                    ?? _allTimeZones.FirstOrDefault(item =>
                        item.StartsWith(txt, StringComparison.OrdinalIgnoreCase))
                    ?? _allTimeZones.FirstOrDefault(item => item.IndexOf(txt, StringComparison.OrdinalIgnoreCase) >= 0);
                cbtzDone.SelectedItem = bestCand;

                if (bestCand == null)
                    cbtzDone.SelectedItem = null;

            }
            cbtzDone_SelectedIndexChanged(null, null);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string err = AddRecord();
            if (err == null)
            {
                if (!cbKeepWindowOpened.Checked)
                    Close();
                return;
            }

            if (!string.IsNullOrEmpty(err))
                MessageBox.Show(this, err, "Rejection");
        }

        private string AddRecord()
        {
            if (tziStop == null)
            {
                cbtzStop.Focus();
                return "StopTime timezone is not set";
            }
            if (tziDone == null)
            {
                cbtzDone.Focus();
                return "NoRestrictionTime timezone is not set";
            }

            var targetItem = cbTarget.SelectedItem as TargetItem;
            if (targetItem == null)
            {
                cbTarget.Focus();
                return "Target item is not set";
            }
               
            var descr = new ScheduledIntervalDescription
            {
                Id = targetItem.Id,

                Info = textInfo.Text.Trim(),
                EnterTimeZoneName = tziStop.Id,
                ExitTimeZoneName = tziDone.Id,
                SoftStopTime = checkBoxSoftStop.Checked ? (DateTime?)GetBeginOfMinute(dtpSoftStop.Value) : null,
                HardStopTime = GetBeginOfMinute(dtpHardStop.Value),
                NoRestrictionTime = GetBeginOfMinute(dtpNoRestr.Value)
            };

            return _funAddRecord(descr);
        }

        private static DateTime GetBeginOfMinute(DateTime dt)
        {
            return dt.Date.AddMinutes(dt.Hour * 60 + dt.Minute);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //Close();
        }

        private void ScheduleRecordEditor_Shown(object sender, EventArgs e)
        {
            cbtzStop.SelectionLength = 0;
            cbtzDone.SelectionLength = 0;

            if (dtpSoftStop.Visible)
                dtpSoftStop.Focus();
            else
                dtpHardStop.Focus();
        }

        private void cbtzStop_TextUpdate(object sender, EventArgs e)
        {

        }

        private TimeZoneInfo UpdateTzLabel(ComboBox cb, Label label)
        {
            if (cb.SelectedItem == null)
            {
                toolTip1.SetToolTip(label, null);
                label.Text = string.IsNullOrEmpty(cb.Text) ? "Not set" : "Invalid TimeZone";
                return null;
            }

            string txt = RailsToIana(cb.SelectedItem.ToString());
            TimeZoneInfo tzi;
            try
            {
                tzi = TZConvert.GetTimeZoneInfo(txt);
            }
            catch //(Exception e)
            {
                toolTip1.SetToolTip(label, null);
                label.Text = "Invalid TimeZone!";
                return null;
            }

            label.Text = tzi.Id;//StandardName;
            toolTip1.SetToolTip(label, tzi.DisplayName);
            return tzi;
        }

    }
}
