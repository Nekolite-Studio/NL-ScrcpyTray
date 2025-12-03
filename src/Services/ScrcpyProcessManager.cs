using NL_ScrcpyTray.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NL_ScrcpyTray.Services
{
    /// <summary>
    /// scrcpy.exe プロセスの起動と管理を専門に担当します。
    /// </summary>
    public class ScrcpyProcessManager
    {
        private readonly Dictionary<string, Process> _runningProcesses = [];
        private readonly string _scrcpyPath;

        public ScrcpyProcessManager(string scrcpyPath)
        {
            _scrcpyPath = scrcpyPath;
        }

        /// <summary>
        /// 指定されたデバイスのミラーリングを開始します。
        /// </summary>
        public void Start(DeviceViewModel deviceVM)
        {
            if (_runningProcesses.ContainsKey(deviceVM.Id))
            {
                // 既に実行中
                return;
            }

            var profile = deviceVM.Settings.SeparateSettings && deviceVM.Status == ConnectionStatus.Wifi
                ? deviceVM.Settings.WifiProfile
                : deviceVM.Settings.UsbProfile;

            var args = BuildArguments(deviceVM, profile);

            var psi = new ProcessStartInfo
            {
                FileName = _scrcpyPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            try
            {
                var process = Process.Start(psi);
                if (process != null)
                {
                    process.EnableRaisingEvents = true;
                    process.Exited += (sender, e) =>
                    {
                        _runningProcesses.Remove(deviceVM.Id);
                        deviceVM.IsMirroring = false;
                        // TODO: Notify DeviceManager about the process exit to update UI
                    };
                    _runningProcesses[deviceVM.Id] = process;
                    deviceVM.IsMirroring = true;
                }
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                Console.WriteLine($"Failed to start scrcpy for {deviceVM.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// 指定されたデバイスのミラーリングを停止します。
        /// </summary>
        public void Stop(string deviceId)
        {
            if (_runningProcesses.TryGetValue(deviceId, out var process))
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    // TODO: Log exception
                    Console.WriteLine($"Failed to kill scrcpy for {deviceId}: {ex.Message}");
                }
                finally
                {
                    _runningProcesses.Remove(deviceId);
                }
            }
        }

        /// <summary>
        /// すべてのミラーリングを停止します。
        /// </summary>
        public void StopAll()
        {
            // ToList() を使ってキーのコレクションのコピーを作成し、反復処理中のコレクション変更エラーを回避
            foreach (var deviceId in _runningProcesses.Keys.ToList())
            {
                Stop(deviceId);
            }
        }

        /// <summary>
        /// 指定されたデバイスIDのプロセスが現在実行中かどうかを確認します。
        /// </summary>
        public bool IsProcessRunning(string deviceId)
        {
            return _runningProcesses.TryGetValue(deviceId, out var process) && !process.HasExited;
        }

        /// <summary>
        /// いずれかのscrcpyプロセスが実行中かどうかを確認します。
        /// </summary>
        public bool IsAnyProcessRunning()
        {
            // プロセスリストをクリーンアップしてから確認
            _runningProcesses.Where(kvp => kvp.Value.HasExited)
                             .Select(kvp => kvp.Key)
                             .ToList()
                             .ForEach(key => _runningProcesses.Remove(key));
            return _runningProcesses.Count > 0;
        }

        private string BuildArguments(DeviceViewModel vm, ConnectionProfile profile)
        {
            // 接続状態に応じて使用するシリアルを決定する
            // USBが利用可能なら常にUSBを優先する
            var serialToUse = vm.Status switch
            {
                ConnectionStatus.Usb => vm.UsbConnectionId,
                ConnectionStatus.UsbAndWifi => vm.UsbConnectionId,
                ConnectionStatus.Wifi => vm.WifiConnectionId,
                _ => vm.Serial // フォールバック
            };

            var args = new List<string>
            {
                $"-s {serialToUse}",
                //"--no-window" // デバッグ用に一時的にウィンドウを表示
            };

            // Video settings
            if (!profile.VideoEnabled)
            {
                args.Add("--no-video");
            }
            else
            {
                if (profile.MaxSize > 0) args.Add($"--max-size={profile.MaxSize}");
                if (profile.VideoBitrate > 0) args.Add($"--video-bit-rate={profile.VideoBitrate}M");
                if (profile.MaxFps > 0) args.Add($"--max-fps={profile.MaxFps}");
                if (profile.VideoBuffer > 0) args.Add($"--video-buffer={profile.VideoBuffer}");
                if (!string.IsNullOrEmpty(profile.VideoCodec)) args.Add($"--video-codec={profile.VideoCodec}");
            }

            if (!profile.DisplayEnabled)
            {
                args.Add("--no-window");
            }

            // Audio settings
            if (profile.AudioEnabled)
            {
                if (profile.AudioBitrate > 0) args.Add($"--audio-bit-rate={profile.AudioBitrate}K");
                if (!string.IsNullOrEmpty(profile.AudioCodec)) args.Add($"--audio-codec={profile.AudioCodec}");
                if (profile.AudioBuffer > 0) args.Add($"--audio-buffer={profile.AudioBuffer}");
            }
            else
            {
                args.Add("--no-audio");
            }
            
            return string.Join(" ", args);
        }
    }
}