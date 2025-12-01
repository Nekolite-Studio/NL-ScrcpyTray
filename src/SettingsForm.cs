using System;
using System.Linq;
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

            // Event Handlers
            wirelessConnectButton.Click += wirelessConnectButton_Click;
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
            RefreshDeviceList();

            // Wireless Tab
            wirelessIpTextBox.Text = _config.WirelessIpAddress;
        }

        private void RefreshDeviceList()
        {
            deviceComboBox.Items.Clear();
            string scrcpyPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.ScrcpyPath);
            var devices = AdbHelper.GetConnectedDevices(scrcpyPath);

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

            // Wireless Tab
            _config.WirelessIpAddress = wirelessIpTextBox.Text;
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

        private void wirelessConnectButton_Click(object? sender, EventArgs e)
        {
            string ipAddress = wirelessIpTextBox.Text.Trim();
            if (string.IsNullOrEmpty(ipAddress))
            {
                MessageBox.Show("IPアドレスを入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string scrcpyPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.ScrcpyPath);
            bool success = AdbHelper.ConnectWirelessDevice(scrcpyPath, ipAddress, _config.AdbTcpPort);

            if (success)
            {
                MessageBox.Show($"{ipAddress} に接続しました。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _config.WirelessIpAddress = ipAddress; // 成功したら設定にも保存
                RefreshDeviceList();
                // 接続したデバイスを選択状態にする
                var connectedDevice = deviceComboBox.Items.Cast<dynamic>().FirstOrDefault(i => i.Value?.Contains(ipAddress));
                if (connectedDevice != null)
                {
                    deviceComboBox.SelectedItem = connectedDevice;
                }
            }
            else
            {
                MessageBox.Show($"{ipAddress} への接続に失敗しました。\nデバイスとPCが同じWi-Fiに接続されているか、IPアドレスが正しいか確認してください。", "接続失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}