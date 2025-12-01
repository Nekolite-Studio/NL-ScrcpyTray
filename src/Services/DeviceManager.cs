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
            _managedDeviceVMs = _appSettings.Devices.Select(d => new DeviceViewModel(d)).ToList();
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

            // --- 新規デバイスの追加 ---
            foreach (var adbDevice in connectedAdbDevices)
            {
                if (!_managedDeviceVMs.Any(vm => vm.Serial == adbDevice.Serial))
                {
                    var newDevice = new Device
                    {
                        Serial = adbDevice.Serial,
                        Model = adbDevice.Model,
                        Name = adbDevice.Model, // 初期名はモデル名
                    };
                    _appSettings.Devices.Add(newDevice); // 永続化リストに追加
                    _managedDeviceVMs.Add(new DeviceViewModel(newDevice)); // ViewModelリストに追加
                    listChanged = true;
                }
            }
            
            // --- 既存デバイスの状態更新 ---
            var connectedSerials = connectedAdbDevices.Select(d => d.Serial).ToHashSet();
            foreach (var vm in _managedDeviceVMs)
            {
                var oldStatus = vm.Status;
                
                var adbDevice = connectedAdbDevices.FirstOrDefault(d => d.Serial == vm.Serial);
                if (adbDevice != null) // adbで認識されている
                {
                    vm.Status = _adbService.IsWifiDevice(adbDevice.Serial) ? ConnectionStatus.Wifi : ConnectionStatus.Usb;
                    
                    // USB接続時にIPアドレスをキャッシュする
                    if (vm.Status == ConnectionStatus.Usb && vm.IpAddress == null)
                    {
                        Task.Run(async () => {
                            vm.IpAddress = await Task.Run(() => _adbService.GetDeviceIpAddress(vm.Serial));
                            // Note: この変更をUIに即時反映させるには、さらにイベントが必要になるが、
                            //       次のポーリングサイクルでいずれ反映されるため、一旦このまま実装する。
                        });
                    }
                }
                else // adbで認識されていない
                {
                    vm.Status = ConnectionStatus.Offline;
                }

                if (oldStatus != vm.Status)
                {
                    listChanged = true;
                }
            }
            
            if (listChanged)
            {
                // 永続化するのはデバイスの順序と追加されたデバイス情報のみ
                _appSettings.Devices = _managedDeviceVMs.Select(vm => new Device {
                    Id = vm.Id,
                    Name = vm.Name,
                    Serial = vm.Serial,
                    Model = vm.Model,
                    IpAddress = vm.IpAddress, // キャッシュしたIPを保存
                    Settings = vm.Settings
                }).ToList();

                _settingsManager.Save(_appSettings);
                DeviceListChanged?.Invoke(_managedDeviceVMs);
            }

            // --- 自動化ロジックの実行 ---
            await HandleSmartHandover(connectedAdbDevices);
            await HandleAutoConnection();
        }

        private Task HandleSmartHandover(List<AdbService.AdbDevice> connectedAdbDevices)
        {
            foreach (var vm in _managedDeviceVMs)
            {
                // --- Wi-Fi -> USB ハンドオーバー ---
                if (vm.Settings.AutoSwitchToUsb && vm.Status == ConnectionStatus.Wifi && vm.IsMirroring)
                {
                    // 同一シリアルのUSB接続があるかチェック
                    if (connectedAdbDevices.Any(d => d.Serial == vm.Serial && !_adbService.IsWifiDevice(d.Serial)))
                    {
                        Console.WriteLine($"Handover to USB for {vm.Name}");
                        _processManager.Stop(vm.Id); // Wi-Fiプロセスを停止
                        vm.Status = ConnectionStatus.Usb; // 状態をUSBに更新
                        _processManager.Start(vm); // USBプロファイルで再起動
                        DeviceListChanged?.Invoke(_managedDeviceVMs);
                    }
                }

                // --- USB -> Wi-Fi ハンドオーバー ---
                // このロジックはPollDevices内の状態変化検知と組み合わせる必要があるため、
                // PollDevicesメソッド内に直接記述する方が状態管理がシンプルになる。
                // ここでは一旦省略し、リファクタリングの余地ありとする。
            }
            return Task.CompletedTask;
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