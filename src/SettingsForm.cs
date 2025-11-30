using System;
using System.Windows.Forms;

namespace ScrcpyTray
{
    public partial class SettingsForm : Form
    {
        private AppConfig _config;

        public SettingsForm(AppConfig config)
        {
            InitializeComponent();
            _config = config;

            // Load existing settings into the form
            LoadConfigIntoForm();
        }

        private void LoadConfigIntoForm()
        {
            // General Tab
            autoStartCheckBox.Checked = _config.AutoStartOnConnect;
            enableVideoCheckBox.Checked = _config.EnableVideo;
            enableAudioCheckBox.Checked = _config.EnableAudio;
            turnScreenOffCheckBox.Checked = _config.TurnScreenOffOnStart;

            // Quality Tab
            switch (_config.BufferMode)
            {
                case "Low Latency":
                    lowLatencyRadioButton.Checked = true;
                    break;
                case "High Quality":
                    highQualityRadioButton.Checked = true;
                    break;
                // TODO: Add "Custom" mode handling
            }

            // Device Tab
            deviceComboBox.Items.Clear();
            var devices = AdbHelper.GetConnectedDevices(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.ScrcpyPath));

            // "自動選択" を追加
            var autoSelectItem = new { Display = "自動選択", Value = (string?)null };
            deviceComboBox.Items.Add(autoSelectItem);
            deviceComboBox.DisplayMember = "Display";
            deviceComboBox.ValueMember = "Value";

            if (devices.Count == 0)
            {
                deviceComboBox.SelectedIndex = 0;
                deviceComboBox.Enabled = false;
            }
            else
            {
                deviceComboBox.Enabled = true;
                foreach (var device in devices)
                {
                    string displayName = string.IsNullOrEmpty(device.Model) || device.Model == "Unknown"
                                       ? device.Serial
                                       : $"{device.Model} ({device.Serial})";
                    deviceComboBox.Items.Add(new { Display = displayName, Value = device.Serial });
                }

                // 現在の設定を選択
                var currentSelection = deviceComboBox.Items.Cast<dynamic>().FirstOrDefault(i => i.Value == _config.AdbDeviceSerial);
                if (currentSelection != null)
                {
                    deviceComboBox.SelectedItem = currentSelection;
                }
                else
                {
                    deviceComboBox.SelectedIndex = 0;
                }
            }
        }

        private void ApplyConfigFromForm()
        {
            // General Tab
            _config.AutoStartOnConnect = autoStartCheckBox.Checked;
            _config.EnableVideo = enableVideoCheckBox.Checked;
            _config.EnableAudio = enableAudioCheckBox.Checked;
            _config.TurnScreenOffOnStart = turnScreenOffCheckBox.Checked;

            // Quality Tab
            if (lowLatencyRadioButton.Checked)
            {
                _config.BufferMode = "Low Latency";
            }
            else if (highQualityRadioButton.Checked)
            {
                _config.BufferMode = "High Quality";
            }
            // TODO: Add "Custom" mode handling

            // Device Tab
            if (deviceComboBox.SelectedItem != null)
            {
                _config.AdbDeviceSerial = ((dynamic)deviceComboBox.SelectedItem).Value;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            ApplyConfigFromForm();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}