using NL_ScrcpyTray.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Management;
using System.Threading;

namespace NL_ScrcpyTray.Services
{
    /// <summary>
    /// アプリケーションの頭脳。接続されている全デバイスの状態を一元管理し、
    /// ユーザー操作やイベントに応じて適切な処理をディスパッチします。
    /// </summary>
    public class DeviceManager
    {
        private readonly SettingsManager _settingsManager;
        private readonly ScrcpyProcessManager _processManager;
        private readonly AdbService _adbService;
        private AppSettings _appSettings;
        private List<DeviceViewModel> _managedDeviceVMs;

        private ManagementEventWatcher? _usbWatcher;
        private Timer? _pollingTimer;

        public event Action<List<DeviceViewModel>>? DeviceListChanged;

        public DeviceManager(SettingsManager settingsManager, ScrcpyProcessManager processManager, AdbService adbService)
        {
            _settingsManager = settingsManager;
            _processManager = processManager;
            _adbService = adbService;
            _appSettings = _settingsManager.Load();
            // 永続化データからViewModelのリストを作成
            _managedDeviceVMs = [.. _appSettings.Devices.Select(d => new DeviceViewModel(d))];
        }

        public void StartMonitoring()
        {
            StartUsbWatcher();
            _pollingTimer = new Timer(async _ => await PollDevices(), null, 0, 5000); // 5秒ごとに実行
            Task.Run(PollDevices); // 初回即時実行
        }

        public void StopMonitoring()
        {
            _usbWatcher?.Stop();
            _pollingTimer?.Dispose();
            _processManager.StopAll();
        }

        /// <summary>
        /// 現在管理しているデバイスのViewModelリストを取得します。
        /// </summary>
        public List<DeviceViewModel> GetManagedDevices()
        {
            return _managedDeviceVMs;
        }

        private void StartUsbWatcher()
        {
            try
            {
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3");
                _usbWatcher = new ManagementEventWatcher(query);
                _usbWatcher.EventArrived += (sender, e) =>
                {
                    // USBイベントは不安定な場合があるため、少し待ってからポーリングを実行
                    Task.Delay(1000).ContinueWith(async t => await PollDevices());
                };
                _usbWatcher.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start USB watcher: {ex.Message}");
                // WMIが利用できない環境でもADBポーリングで動作するようにフォールバック
            }
        }

        private async Task PollDevices()
        {
            var connectedAdbDevices = _adbService.GetConnectedDevices();
            bool listChanged = false;

            // ADBからの情報を物理シリアルでグループ化
            var adbDeviceGroups = connectedAdbDevices.GroupBy(d => d.HardwareSerial).ToList();

            // 永続化されている設定を物理シリアルで辞書化
            var settingsDict = _appSettings.Devices.ToDictionary(d => d.Serial, d => d);

            var newDeviceVMs = new List<DeviceViewModel>();
            var allKnownSerials = settingsDict.Keys.Union(adbDeviceGroups.Select(g => g.Key)).ToHashSet();

            foreach (var serial in allKnownSerials)
            {
                var adbConnections = adbDeviceGroups.FirstOrDefault(g => g.Key == serial)?.ToList();

                // ViewModelを生成または取得
                DeviceViewModel vm;
                if (settingsDict.TryGetValue(serial, out var deviceSettings))
                {
                    // 既存の設定からViewModelを作成
                    vm = new DeviceViewModel(deviceSettings);
                }
                else
                {
                    // 新規デバイス
                    var representativeDevice = adbConnections!.First();
                    var newDevice = new Device
                    {
                        Serial = serial,
                        Model = representativeDevice.Model,
                        Name = representativeDevice.Model,
                    };
                    settingsDict[serial] = newDevice; // 新しい設定を辞書に追加
                    _appSettings.Devices.Add(newDevice); // 永続化リストにも追加
                    vm = new DeviceViewModel(newDevice);
                    listChanged = true;
                }

                var oldStatus = _managedDeviceVMs.FirstOrDefault(oldVm => oldVm.Serial == serial)?.Status ?? ConnectionStatus.Offline;

                // 接続状態を更新
                if (adbConnections != null && adbConnections.Any())
                {
                    bool hasUsb = false;
                    bool hasWifi = false;
                    foreach (var conn in adbConnections)
                    {
                        if (conn.ConnectionType == ConnectionStatus.Usb)
                        {
                            hasUsb = true;
                            vm.UsbConnectionId = conn.ConnectionId;
                        }
                        else if (conn.ConnectionType == ConnectionStatus.Wifi)
                        {
                            hasWifi = true;
                            vm.WifiConnectionId = conn.ConnectionId;
                            var ipMatch = System.Text.RegularExpressions.Regex.Match(conn.ConnectionId, @"(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})");
                            if (ipMatch.Success) vm.IpAddress = ipMatch.Groups[1].Value;
                        }
                    }
                    if (hasUsb && hasWifi) vm.Status = ConnectionStatus.UsbAndWifi;
                    else if (hasUsb) vm.Status = ConnectionStatus.Usb;
                    else vm.Status = ConnectionStatus.Wifi;
                }
                else
                {
                    vm.Status = ConnectionStatus.Offline;
                }
                
                // ミラーリング状態を引き継ぐ
                var oldVm = _managedDeviceVMs.FirstOrDefault(oldVm => oldVm.Serial == serial);
                if (oldVm != null)
                {
                    vm.IsMirroring = _processManager.IsProcessRunning(oldVm.Id);
                }

                if (oldStatus != vm.Status)
                {
                    listChanged = true;
                    // --- スマートハンドオーバー (Wi-Fi -> USB) ---
                    if (vm.Settings.AutoSwitchToUsb && vm.IsMirroring && oldStatus == ConnectionStatus.Wifi && (vm.Status == ConnectionStatus.Usb || vm.Status == ConnectionStatus.UsbAndWifi))
                    {
                         Console.WriteLine($"Handover to USB for {vm.Name}");
                        _processManager.Stop(vm.Id);
                        _processManager.Start(vm);
                    }
                }
                newDeviceVMs.Add(vm);
            }

            // 順序を維持しつつリストを更新
            var orderedVMs = newDeviceVMs.OrderBy(vm => _appSettings.Devices.FindIndex(d => d.Serial == vm.Serial)).ToList();
            
            // 変更があったか最終チェック
            if (listChanged || !_managedDeviceVMs.SequenceEqual(orderedVMs))
            {
                _managedDeviceVMs = orderedVMs;
                _settingsManager.Save(_appSettings);
                DeviceListChanged?.Invoke(_managedDeviceVMs);
            }

            await HandleAutoConnection();
        }

        private Task HandleAutoConnection()
        {
            if (!_appSettings.GlobalAutoConnect)
            {
                return Task.CompletedTask;
            }

            // 優先順位（リストの順序）に従ってチェック
            foreach (var vm in _managedDeviceVMs)
            {
                if (vm.Settings.AutoConnect && vm.Status != ConnectionStatus.Offline && !vm.IsMirroring)
                {
                    Console.WriteLine($"Auto-connecting to {vm.Name}...");
                    _processManager.Start(vm);
                    // UIに変更を通知するためにイベントを発火
                    DeviceListChanged?.Invoke(_managedDeviceVMs);
                }
            }
            return Task.CompletedTask;
        }

        // Other public methods to be called from WebView2 bridge will be added here
        // e.g., StartMirroring, StopMirroring, UpdateDeviceSettings, etc.
        public void StartMirroring(string deviceId)
        {
            var vm = _managedDeviceVMs.FirstOrDefault(d => d.Id == deviceId);
            if (vm != null && vm.Status != ConnectionStatus.Offline)
            {
                _processManager.Start(vm);
                // IsMirroring状態を即時反映させる
                vm.IsMirroring = true;
                DeviceListChanged?.Invoke(_managedDeviceVMs);
            }
        }

        public void StopMirroring(string deviceId)
        {
            _processManager.Stop(deviceId);
            var vm = _managedDeviceVMs.FirstOrDefault(d => d.Id == deviceId);
            if (vm != null)
            {
                vm.IsMirroring = false;
            }
            DeviceListChanged?.Invoke(_managedDeviceVMs);
        }

        public void UpdateDeviceSettings(string deviceId, DeviceSettings newSettings)
        {
            var vm = _managedDeviceVMs.FirstOrDefault(d => d.Id == deviceId);
            if (vm != null)
            {
                vm.Settings = newSettings;
                // 永続化リストも更新
                var deviceToUpdate = _appSettings.Devices.FirstOrDefault(d => d.Id == deviceId);
                if(deviceToUpdate != null)
                {
                    deviceToUpdate.Settings = newSettings;
                    _settingsManager.Save(_appSettings);
                }
                DeviceListChanged?.Invoke(_managedDeviceVMs);
            }
        }

        public void UpdateDeviceOrder(IEnumerable<string> deviceIds)
        {
            var orderedVMs = deviceIds
                .Select(id => _managedDeviceVMs.FirstOrDefault(vm => vm.Id == id))
                .Where(vm => vm != null)
                .ToList();

            _managedDeviceVMs = orderedVMs!;
            
            // 永続化リストも更新
            var orderedDevices = deviceIds
                .Select(id => _appSettings.Devices.FirstOrDefault(d => d.Id == id))
                .Where(d => d != null)
                .ToList();

            _appSettings.Devices = orderedDevices!;
            _settingsManager.Save(_appSettings);
            
            DeviceListChanged?.Invoke(_managedDeviceVMs);
        }
    }
}