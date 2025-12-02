using NL_ScrcpyTray.Services;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace NL_ScrcpyTray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private DeviceManager? _deviceManager;
        private ScrcpyProcessManager? _processManager;
        private MainWindow? _mainWindow;
        private NotifyIcon? _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // --- 依存関係の解決 ---
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var scrcpyDir = Path.Combine(baseDir, "scrcpy");
            var scrcpyPath = Path.Combine(scrcpyDir, "scrcpy.exe");
            var adbPath = Path.Combine(scrcpyDir, "adb.exe");
            var configPath = Path.Combine(baseDir, "settings.json");

            var settingsManager = new SettingsManager(configPath);
            _processManager = new ScrcpyProcessManager(scrcpyPath);
            var adbService = new AdbService(adbPath);
            _deviceManager = new DeviceManager(settingsManager, _processManager, adbService);

            // --- メインウィンドウの作成 (まだ表示しない) ---
            _mainWindow = new MainWindow(_deviceManager);

            // --- タスクトレイアイコンの初期化 ---
            System.Drawing.Icon? appIcon = null;
            try
            {
                var iconStream = GetResourceStream(new Uri("pack://application:,,,/icon.ico")).Stream;
                appIcon = new System.Drawing.Icon(iconStream);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to load icon resource: {ex.Message}");
                // フォールバックとしてシステムのアプリケーションアイコンを使用
                appIcon = System.Drawing.SystemIcons.Application;
            }

            _notifyIcon = new NotifyIcon
            {
                Icon = appIcon,
                Text = "NL-ScrcpyTray",
                Visible = true
            };
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("設定...", null, (s, args) => ShowMainWindow());
            _notifyIcon.ContextMenuStrip.Items.Add("終了", null, (s, args) => ExitApplication());
            
            _deviceManager.StartMonitoring();
        }
        
        private void ShowMainWindow()
        {
            if (_mainWindow == null) return;

            if (_mainWindow.IsVisible)
            {
                _mainWindow.Activate();
            }
            else
            {
                _mainWindow.Show();
            }
        }

        private void ExitApplication()
        {
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _deviceManager?.StopMonitoring();
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}