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
        private readonly Dictionary<string, Process> _runningProcesses = new();
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

            var args = BuildArguments(deviceVM.Serial, profile);

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
            foreach (var deviceId in _runningProcesses.Keys)
            {
                Stop(deviceId);
            }
        }

        private string BuildArguments(string serial, ConnectionProfile profile)
        {
            var args = new List<string>
            {
                $"-s {serial}",
                "--no-window" // コンソールウィンドウは表示しない
            };

            // Video settings
            if (profile.MaxSize > 0) args.Add($"--max-size={profile.MaxSize}");
            if (profile.VideoBitrate > 0) args.Add($"--video-bit-rate={profile.VideoBitrate}M");
            if (profile.MaxFps > 0) args.Add($"--max-fps={profile.MaxFps}");
            if (profile.VideoBuffer > 0) args.Add($"--video-buffer={profile.VideoBuffer}");
            if (!string.IsNullOrEmpty(profile.VideoCodec)) args.Add($"--video-codec={profile.VideoCodec}");

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