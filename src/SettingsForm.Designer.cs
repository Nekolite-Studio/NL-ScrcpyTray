namespace ScrcpyTray
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.generalTabPage = new System.Windows.Forms.TabPage();
            this.turnScreenOffCheckBox = new System.Windows.Forms.CheckBox();
            this.enableAudioCheckBox = new System.Windows.Forms.CheckBox();
            this.enableVideoCheckBox = new System.Windows.Forms.CheckBox();
            this.autoStartCheckBox = new System.Windows.Forms.CheckBox();
            this.qualityTabPage = new System.Windows.Forms.TabPage();
            this.qualityGroupBox = new System.Windows.Forms.GroupBox();
            this.highQualityRadioButton = new System.Windows.Forms.RadioButton();
            this.lowLatencyRadioButton = new System.Windows.Forms.RadioButton();
            this.deviceTabPage = new System.Windows.Forms.TabPage();
            this.deviceComboBox = new System.Windows.Forms.ComboBox();
            this.deviceLabel = new System.Windows.Forms.Label();
            this.wirelessTabPage = new System.Windows.Forms.TabPage();
            this.wirelessConnectButton = new System.Windows.Forms.Button();
            this.wirelessIpTextBox = new System.Windows.Forms.TextBox();
            this.wirelessIpLabel = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.generalTabPage.SuspendLayout();
            this.qualityTabPage.SuspendLayout();
            this.qualityGroupBox.SuspendLayout();
            this.deviceTabPage.SuspendLayout();
            this.wirelessTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.generalTabPage);
            this.tabControl.Controls.Add(this.qualityTabPage);
            this.tabControl.Controls.Add(this.deviceTabPage);
            this.tabControl.Controls.Add(this.wirelessTabPage);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(460, 280);
            this.tabControl.TabIndex = 0;
            // 
            // generalTabPage
            // 
            this.generalTabPage.Controls.Add(this.turnScreenOffCheckBox);
            this.generalTabPage.Controls.Add(this.enableAudioCheckBox);
            this.generalTabPage.Controls.Add(this.enableVideoCheckBox);
            this.generalTabPage.Controls.Add(this.autoStartCheckBox);
            this.generalTabPage.Location = new System.Drawing.Point(4, 29);
            this.generalTabPage.Name = "generalTabPage";
            this.generalTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.generalTabPage.Size = new System.Drawing.Size(452, 247);
            this.generalTabPage.TabIndex = 0;
            this.generalTabPage.Text = "一般";
            this.generalTabPage.UseVisualStyleBackColor = true;
            // 
            // turnScreenOffCheckBox
            // 
            this.turnScreenOffCheckBox.AutoSize = true;
            this.turnScreenOffCheckBox.Location = new System.Drawing.Point(20, 125);
            this.turnScreenOffCheckBox.Name = "turnScreenOffCheckBox";
            this.turnScreenOffCheckBox.Size = new System.Drawing.Size(202, 24);
            this.turnScreenOffCheckBox.TabIndex = 3;
            this.turnScreenOffCheckBox.Text = "起動時に端末画面をOFFにする";
            this.turnScreenOffCheckBox.UseVisualStyleBackColor = true;
            // 
            // enableAudioCheckBox
            // 
            this.enableAudioCheckBox.AutoSize = true;
            this.enableAudioCheckBox.Location = new System.Drawing.Point(20, 90);
            this.enableAudioCheckBox.Name = "enableAudioCheckBox";
            this.enableAudioCheckBox.Size = new System.Drawing.Size(108, 24);
            this.enableAudioCheckBox.TabIndex = 2;
            this.enableAudioCheckBox.Text = "音声を共有する";
            this.enableAudioCheckBox.UseVisualStyleBackColor = true;
            // 
            // enableVideoCheckBox
            // 
            this.enableVideoCheckBox.AutoSize = true;
            this.enableVideoCheckBox.Location = new System.Drawing.Point(20, 55);
            this.enableVideoCheckBox.Name = "enableVideoCheckBox";
            this.enableVideoCheckBox.Size = new System.Drawing.Size(108, 24);
            this.enableVideoCheckBox.TabIndex = 1;
            this.enableVideoCheckBox.Text = "画面を共有する";
            this.enableVideoCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoStartCheckBox
            // 
            this.autoStartCheckBox.AutoSize = true;
            this.autoStartCheckBox.Location = new System.Drawing.Point(20, 20);
            this.autoStartCheckBox.Name = "autoStartCheckBox";
            this.autoStartCheckBox.Size = new System.Drawing.Size(155, 24);
            this.autoStartCheckBox.TabIndex = 0;
            this.autoStartCheckBox.Text = "USB接続時に自動開始";
            this.autoStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // qualityTabPage
            // 
            this.qualityTabPage.Controls.Add(this.qualityGroupBox);
            this.qualityTabPage.Location = new System.Drawing.Point(4, 29);
            this.qualityTabPage.Name = "qualityTabPage";
            this.qualityTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.qualityTabPage.Size = new System.Drawing.Size(452, 247);
            this.qualityTabPage.TabIndex = 1;
            this.qualityTabPage.Text = "画質";
            this.qualityTabPage.UseVisualStyleBackColor = true;
            // 
            // qualityGroupBox
            // 
            this.qualityGroupBox.Controls.Add(this.highQualityRadioButton);
            this.qualityGroupBox.Controls.Add(this.lowLatencyRadioButton);
            this.qualityGroupBox.Location = new System.Drawing.Point(20, 20);
            this.qualityGroupBox.Name = "qualityGroupBox";
            this.qualityGroupBox.Size = new System.Drawing.Size(410, 100);
            this.qualityGroupBox.TabIndex = 0;
            this.qualityGroupBox.TabStop = false;
            this.qualityGroupBox.Text = "プリセット";
            // 
            // highQualityRadioButton
            // 
            this.highQualityRadioButton.AutoSize = true;
            this.highQualityRadioButton.Location = new System.Drawing.Point(15, 60);
            this.highQualityRadioButton.Name = "highQualityRadioButton";
            this.highQualityRadioButton.Size = new System.Drawing.Size(126, 24);
            this.highQualityRadioButton.TabIndex = 1;
            this.highQualityRadioButton.TabStop = true;
            this.highQualityRadioButton.Text = "高画質 (メディア)";
            this.highQualityRadioButton.UseVisualStyleBackColor = true;
            // 
            // lowLatencyRadioButton
            // 
            this.lowLatencyRadioButton.AutoSize = true;
            this.lowLatencyRadioButton.Location = new System.Drawing.Point(15, 25);
            this.lowLatencyRadioButton.Name = "lowLatencyRadioButton";
            this.lowLatencyRadioButton.Size = new System.Drawing.Size(150, 24);
            this.lowLatencyRadioButton.TabIndex = 0;
            this.lowLatencyRadioButton.TabStop = true;
            this.lowLatencyRadioButton.Text = "低遅延 (開発/ゲーム)";
            this.lowLatencyRadioButton.UseVisualStyleBackColor = true;
            // 
            // deviceTabPage
            // 
            this.deviceTabPage.Controls.Add(this.deviceComboBox);
            this.deviceTabPage.Controls.Add(this.deviceLabel);
            this.deviceTabPage.Location = new System.Drawing.Point(4, 29);
            this.deviceTabPage.Name = "deviceTabPage";
            this.deviceTabPage.Padding = new System.Windows.Forms.Padding(10);
            this.deviceTabPage.Size = new System.Drawing.Size(452, 247);
            this.deviceTabPage.TabIndex = 2;
            this.deviceTabPage.Text = "デバイス";
            this.deviceTabPage.UseVisualStyleBackColor = true;
            // 
            // deviceComboBox
            // 
            this.deviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deviceComboBox.FormattingEnabled = true;
            this.deviceComboBox.Location = new System.Drawing.Point(20, 50);
            this.deviceComboBox.Name = "deviceComboBox";
            this.deviceComboBox.Size = new System.Drawing.Size(410, 28);
            this.deviceComboBox.TabIndex = 1;
            // 
            // deviceLabel
            // 
            this.deviceLabel.AutoSize = true;
            this.deviceLabel.Location = new System.Drawing.Point(20, 20);
            this.deviceLabel.Name = "deviceLabel";
            this.deviceLabel.Size = new System.Drawing.Size(262, 20);
            this.deviceLabel.TabIndex = 0;
            this.deviceLabel.Text = "優先デバイス (複数接続されている場合):";
            // 
            // wirelessTabPage
            // 
            this.wirelessTabPage.Controls.Add(this.wirelessConnectButton);
            this.wirelessTabPage.Controls.Add(this.wirelessIpTextBox);
            this.wirelessTabPage.Controls.Add(this.wirelessIpLabel);
            this.wirelessTabPage.Location = new System.Drawing.Point(4, 29);
            this.wirelessTabPage.Name = "wirelessTabPage";
            this.wirelessTabPage.Padding = new System.Windows.Forms.Padding(10);
            this.wirelessTabPage.Size = new System.Drawing.Size(452, 247);
            this.wirelessTabPage.TabIndex = 3;
            this.wirelessTabPage.Text = "ワイヤレス";
            this.wirelessTabPage.UseVisualStyleBackColor = true;
            // 
            // wirelessConnectButton
            // 
            this.wirelessConnectButton.Location = new System.Drawing.Point(330, 48);
            this.wirelessConnectButton.Name = "wirelessConnectButton";
            this.wirelessConnectButton.Size = new System.Drawing.Size(100, 30);
            this.wirelessConnectButton.TabIndex = 2;
            this.wirelessConnectButton.Text = "接続";
            this.wirelessConnectButton.UseVisualStyleBackColor = true;
            // 
            // wirelessIpTextBox
            // 
            this.wirelessIpTextBox.Location = new System.Drawing.Point(20, 50);
            this.wirelessIpTextBox.Name = "wirelessIpTextBox";
            this.wirelessIpTextBox.Size = new System.Drawing.Size(300, 27);
            this.wirelessIpTextBox.TabIndex = 1;
            // 
            // wirelessIpLabel
            // 
            this.wirelessIpLabel.AutoSize = true;
            this.wirelessIpLabel.Location = new System.Drawing.Point(20, 20);
            this.wirelessIpLabel.Name = "wirelessIpLabel";
            this.wirelessIpLabel.Size = new System.Drawing.Size(221, 20);
            this.wirelessIpLabel.TabIndex = 0;
            this.wirelessIpLabel.Text = "IPアドレスで直接接続 (例: 192.168.1.5):";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(266, 300);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(100, 30);
            this.saveButton.TabIndex = 1;
            this.saveButton.Text = "保存して適用";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(372, 300);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 30);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 341);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NL-ScrcpyTray 設定";
            this.tabControl.ResumeLayout(false);
            this.generalTabPage.ResumeLayout(false);
            this.generalTabPage.PerformLayout();
            this.qualityTabPage.ResumeLayout(false);
            this.qualityGroupBox.ResumeLayout(false);
            this.qualityGroupBox.PerformLayout();
            this.deviceTabPage.ResumeLayout(false);
            this.deviceTabPage.PerformLayout();
            this.wirelessTabPage.ResumeLayout(false);
            this.wirelessTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage generalTabPage;
        private System.Windows.Forms.CheckBox autoStartCheckBox;
        private System.Windows.Forms.CheckBox enableVideoCheckBox;
        private System.Windows.Forms.CheckBox enableAudioCheckBox;
        private System.Windows.Forms.CheckBox turnScreenOffCheckBox;
        private System.Windows.Forms.TabPage qualityTabPage;
        private System.Windows.Forms.GroupBox qualityGroupBox;
        private System.Windows.Forms.RadioButton highQualityRadioButton;
        private System.Windows.Forms.RadioButton lowLatencyRadioButton;
        private System.Windows.Forms.TabPage deviceTabPage;
        private System.Windows.Forms.Label deviceLabel;
        private System.Windows.Forms.ComboBox deviceComboBox;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TabPage wirelessTabPage;
        private System.Windows.Forms.Button wirelessConnectButton;
        private System.Windows.Forms.TextBox wirelessIpTextBox;
        private System.Windows.Forms.Label wirelessIpLabel;
    }
}