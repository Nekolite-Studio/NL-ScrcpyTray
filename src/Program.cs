using System;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace ScrcpyTray
{
    // feature-spec.md に基づく設定クラス
    public class AppConfig
    {
        public string ScrcpyPath { get; set; } = "scrcpy/scrcpy.exe";
        public bool AutoStartOnConnect { get; set; } = true;
        public bool EnableVideo { get; set; } = true;
        public bool EnableAudio { get; set; } = true;
        public bool TurnScreenOffOnStart { get; set; } = false;
        public string BufferMode { get; set; } = "Low Latency"; // "Low Latency" or "High Quality"
        public string? AdbDeviceSerial { get; set; } = null;
    }

    static class Program
    {
        private const string ConfigFileName = "settings.json";

        // 設定は AppConfig クラスで一元管理
        static AppConfig config = new();

        // Null許容型 (?) にして警告を回避
        static Process? currentProcess = null;
        static NotifyIcon? trayIcon;

[STAThread]
static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    // 設定ファイルの読み込み
    LoadConfig();

    // トレイアイコンの作成
            trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Application, // アプリのデフォルトアイコンを使用
                Visible = true,
                Text = "NL-ScrcpyTray (待機中)"
            };

            // コンテキストメニューの構築
            UpdateContextMenu();
            // USB監視の開始
            StartUsbWatcher();

            // アプリケーション実行（メッセージループ）
            Application.Run();
            
            // 終了処理
            StopScrcpy();
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
        }
        static void UpdateContextMenu()
        {
            if (trayIcon == null) return;

            ContextMenuStrip menu = new ContextMenuStrip();
            // 1. 状態表示 & 手動操作
            var statusItem = new ToolStripMenuItem(currentProcess == null ? "開始" : "停止");
            statusItem.Font = new Font(statusItem.Font, FontStyle.Bold);
            statusItem.Click += (s, e) => {
                if (currentProcess == null) StartScrcpy();
                else StopScrcpy();
            };
            menu.Items.Add(statusItem);

            menu.Items.Add(new ToolStripSeparator());
// 2. 設定：自動開始
var autoItem = new ToolStripMenuItem("USB接続で自動開始");
autoItem.Checked = config.AutoStartOnConnect;
autoItem.Click += (s, e) => { config.AutoStartOnConnect = !config.AutoStartOnConnect; SaveConfig(); UpdateContextMenu(); };
menu.Items.Add(autoItem);

// 3. 設定：ビデオ/オーディオ
var videoItem = new ToolStripMenuItem("画面を共有");
videoItem.Checked = config.EnableVideo;
videoItem.Click += (s, e) => { config.EnableVideo = !config.EnableVideo; SaveConfig(); UpdateContextMenu(); };
menu.Items.Add(videoItem);
var audioItem = new ToolStripMenuItem("音声を共有");
audioItem.Checked = config.EnableAudio;
audioItem.Click += (s, e) => { config.EnableAudio = !config.EnableAudio; SaveConfig(); UpdateContextMenu(); };
menu.Items.Add(audioItem);

// 4. 設定：画面オフ
var screenOffItem = new ToolStripMenuItem("端末画面をOFF (-S)");
screenOffItem.Checked = config.TurnScreenOffOnStart;
screenOffItem.Click += (s, e) => { config.TurnScreenOffOnStart = !config.TurnScreenOffOnStart; SaveConfig(); UpdateContextMenu(); };
            menu.Items.Add(screenOffItem);

            menu.Items.Add(new ToolStripSeparator());
            // 5. バッファ設定テンプレート
            var bufferMenu = new ToolStripMenuItem("モード設定");
            
var lowLatItem = new ToolStripMenuItem("低遅延 (Dev/Game)");
lowLatItem.Checked = (config.BufferMode == "Low Latency");
lowLatItem.Click += (s, e) => { config.BufferMode = "Low Latency"; SaveConfig(); UpdateContextMenu(); };
bufferMenu.DropDownItems.Add(lowLatItem);

var hqItem = new ToolStripMenuItem("高画質 (Media)");
hqItem.Checked = (config.BufferMode == "High Quality");
hqItem.Click += (s, e) => { config.BufferMode = "High Quality"; SaveConfig(); UpdateContextMenu(); };
            bufferMenu.DropDownItems.Add(hqItem);
            menu.Items.Add(bufferMenu);

            menu.Items.Add(new ToolStripSeparator());
            
            // 6. 終了
            var exitItem = new ToolStripMenuItem("終了");
            exitItem.Click += (s, e) => Application.Exit();
            menu.Items.Add(exitItem);
            trayIcon.ContextMenuStrip = menu;
        }

        static void StartScrcpy()
        {
            if (currentProcess != null) return;
            if (trayIcon == null) return;
            string args = "";

// 基本設定
if (!config.EnableVideo) args += " --no-video";
if (!config.EnableAudio) args += " --no-audio";
if (config.TurnScreenOffOnStart) args += " -S";

// テンプレート適用
if (config.BufferMode == "Low Latency")
            {
                args += " --audio-buffer=50 --video-buffer=0 --max-size=1024";
            }
            else
            {
                args += " --audio-buffer=200 --video-buffer=200 --video-bit-rate=16M";
            }
// コンソールなしで起動
args += " --no-window";

            // 実行ファイルの場所からの相対パスを解決
            string fullScrcpyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.ScrcpyPath);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fullScrcpyPath,
Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            try
            {
                currentProcess = Process.Start(psi);
                if (currentProcess != null)
                {
                    currentProcess.EnableRaisingEvents = true;
currentProcess.Exited += (s, e) => {
    currentProcess = null;
    if (trayIcon != null)
    {
        trayIcon.Text = "NL-ScrcpyTray (待機中)";
        trayIcon.Icon = SystemIcons.Application;
        trayIcon.ShowBalloonTip(1000, "デバイス切断", "画面転送が終了しました。", ToolTipIcon.Info);
        
        // メニュー再構築（非UIスレッドから呼ばれる可能性への配慮）
        UpdateContextMenu();
    }
};

trayIcon.Text = "NL-ScrcpyTray (実行中)";
trayIcon.ShowBalloonTip(1000, "実行中", "画面転送を開始しました。", ToolTipIcon.Info);
UpdateContextMenu();
}
}
catch (Exception ex)
{
trayIcon?.ShowBalloonTip(1000, "起動エラー", $"scrcpyの起動に失敗しました。\n{ex.Message}", ToolTipIcon.Error);
}
}
        static void StopScrcpy()
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                try { currentProcess.Kill(); } catch { /* 無視 */ }
                currentProcess = null;
            }
            UpdateContextMenu();
}

// 設定をJSONファイルに保存
static void SaveConfig()
{
    try
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(config, options);
        File.WriteAllText(ConfigFileName, jsonString);
    }
    catch (Exception ex)
    {
        MessageBox.Show("設定の保存に失敗しました: " + ex.Message);
    }
}

// JSONファイルから設定を読み込み
static void LoadConfig()
{
    try
    {
        if (File.Exists(ConfigFileName))
        {
            string jsonString = File.ReadAllText(ConfigFileName);
            var loadedConfig = JsonSerializer.Deserialize<AppConfig>(jsonString);
            if (loadedConfig != null)
            {
                config = loadedConfig;
            }
        }
        else
        {
            // 設定ファイルがなければデフォルトで作成
            SaveConfig();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("設定の読み込みに失敗しました: " + ex.Message);
        // 読み込みに失敗した場合はデフォルト設定で続行
    }
}

static void StartUsbWatcher()
{
            try
            {
                // USBデバイス変更イベント監視
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
                ManagementEventWatcher watcher = new ManagementEventWatcher(query);
                
watcher.EventArrived += (s, e) =>
{
    if (config.AutoStartOnConnect && currentProcess == null)
    {
        trayIcon?.ShowBalloonTip(1000, "デバイス接続", "デバイスが接続されました。scrcpyを開始します。", ToolTipIcon.Info);
        // 接続直後は認識されないことがあるため少し待機
        System.Threading.Thread.Sleep(2000);
        StartScrcpy();
    }
                };
                watcher.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("USB監視の初期化に失敗しました: " + ex.Message);
            }
        }
    }
}