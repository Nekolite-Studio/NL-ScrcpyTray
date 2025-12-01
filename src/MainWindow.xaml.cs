using Microsoft.Web.WebView2.Core;
using NL_ScrcpyTray.Models;
using NL_ScrcpyTray.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace NL_ScrcpyTray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DeviceManager _deviceManager;

        public MainWindow(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
            _deviceManager.DeviceListChanged += OnDeviceListChanged;

            InitializeComponent();
            InitializeWebViewAsync();
        }

        private void OnDeviceListChanged(List<DeviceViewModel> obj)
        {
            // UIスレッドで実行
            Dispatcher.Invoke(async () => await PostDeviceListUpdate());
        }

        private async void InitializeWebViewAsync()
        {
            // CoreWebView2 の初期化が完了したときのイベントハンドラを登録
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;

            // WebView2のユーザーデータフォルダをアプリケーションのディレクトリ内に設定
            var userDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebView2_UserData");
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);

            // 初期化を開始
            await webView.EnsureCoreWebView2Async(environment);
        }

        private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show($"WebView2 creation failed: {e.InitializationException.Message}");
                return;
            }

            // --- 仮想ホストの設定でCORS問題を解決 ---
            var frontendFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "frontend");

            // 仮想ホスト名 "app.nl-scrcpy.local" を "frontend" フォルダにマッピング
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.nl-scrcpy.local",
                frontendFolder,
                CoreWebView2HostResourceAccessKind.DenyCors
            );

            // ブリッジを設定
            SetupBridge();

            // 開発者ツールを有効にする (デバッグ用)
#if DEBUG
            webView.CoreWebView2.OpenDevToolsWindow();
#endif

            // 開発時はローカルサーバーを参照することも可能
            // #if DEBUG
            // webView.CoreWebView2.Navigate("http://localhost:3000");
            // #else
            if (Directory.Exists(frontendFolder))
            {
                // 仮想ホスト経由でアクセス
                webView.CoreWebView2.Navigate("https://app.nl-scrcpy.local/index.html");
            }
            // #endif
            else
            {
                webView.CoreWebView2.NavigateToString("<h1>Error: Frontend directory not found.</h1><p>Please build the frontend project.</p>");
            }
        }

        private void SetupBridge()
        {
            webView.CoreWebView2.AddHostObjectToScript("bridge", new WebViewBridge(this, _deviceManager));
        }

        public async Task PostDeviceListUpdate()
        {
            if (webView.CoreWebView2 == null) return;
            var devices = _deviceManager.GetManagedDevices();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            var json = JsonSerializer.Serialize(devices, options);
            await webView.CoreWebView2.ExecuteScriptAsync($"window.updateDeviceList({json})");
        }
    }

    public class WebViewBridge
    {
        private readonly MainWindow _mainWindow;
        private readonly DeviceManager _deviceManager;

        public WebViewBridge(MainWindow mainWindow, DeviceManager deviceManager)
        {
            _mainWindow = mainWindow;
            _deviceManager = deviceManager;
        }

        public void StartMirroring(string deviceId)
        {
            _deviceManager.StartMirroring(deviceId);
        }

        public void StopMirroring(string deviceId)
        {
            _deviceManager.StopMirroring(deviceId);
        }

        public void UpdateSettings(string deviceId, string settingsJson)
        {
            var settings = JsonSerializer.Deserialize<DeviceSettings>(settingsJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (settings != null)
            {
                _deviceManager.UpdateDeviceSettings(deviceId, settings);
            }
        }
        
        public void UpdateDeviceOrder(string[] deviceIds)
        {
            _deviceManager.UpdateDeviceOrder(deviceIds);
        }
    }
}