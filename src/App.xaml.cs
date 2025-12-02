using NL_ScrcpyTray.Services;
using System.IO;
using System.Windows;

namespace NL_ScrcpyTray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private DeviceManager? _deviceManager;
        private ScrcpyProcessManager? _processManager;

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

            // --- メインウィンドウの作成 ---
            var mainWindow = new MainWindow(_deviceManager);
            mainWindow.Show();

            _deviceManager.StartMonitoring();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _deviceManager?.StopMonitoring();
            base.OnExit(e);
        }
    }
}