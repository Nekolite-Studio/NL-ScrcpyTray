using NL_ScrcpyTray.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NL_ScrcpyTray.Services
{
    /// <summary>
    /// adb.exe との対話を担当するサービス。
    /// </summary>
    public class AdbService
    {
        private readonly string _adbPath;

        public AdbService(string scrcpyPath)
        {
            var scrcpyDir = Path.GetDirectoryName(scrcpyPath);
            if (string.IsNullOrEmpty(scrcpyDir) || !Directory.Exists(scrcpyDir))
            {
                throw new FileNotFoundException("scrcpy directory not found.", scrcpyDir);
            }
            _adbPath = Path.Combine(scrcpyDir, "adb.exe");
            if (!File.Exists(_adbPath))
            {
                throw new FileNotFoundException("adb.exe not found in scrcpy directory.", _adbPath);
            }
        }

        /// <summary>
        /// 現在PCに接続されているAndroidデバイスのリストを取得します。
        /// </summary>
        public List<AdbDevice> GetConnectedDevices()
        {
            var devices = new List<AdbDevice>();
            string output = ExecuteAdbCommand("devices -l");

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                              .Where(line => !line.StartsWith("List of devices"));

            foreach (var line in lines)
            {
                if (line.Contains("device") && !line.Contains("unauthorized"))
                {
                    var serialMatch = Regex.Match(line, @"^([a-zA-Z0-9\._:\-]+)\s+");
                    var modelMatch = Regex.Match(line, @"model:(\S+)");

                    if (serialMatch.Success)
                    {
                        // AdbDeviceはModels名前空間のものを直接利用する想定
                        // ただし、DataModels.csで定義したDeviceクラスとは異なるので注意
                        // ここでは一時的な情報運搬用としてAdbDeviceクラスを内部定義する
                        devices.Add(new AdbDevice
                        {
                            Serial = serialMatch.Groups[1].Value,
                            Model = modelMatch.Success ? modelMatch.Groups[1].Value : "Unknown",
                        });
                    }
                }
            }
            return devices;
        }

        /// <summary>
        /// 指定されたデバイスのIPアドレスを取得します。
        /// </summary>
        public string? GetDeviceIpAddress(string deviceSerial)
        {
            string output = ExecuteAdbCommand($"-s {deviceSerial} shell ip addr show wlan0");
            var match = Regex.Match(output, @"inet (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})");
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        /// 指定されたデバイスでTCP/IPモードを有効にします。
        /// </summary>
        public bool EnableTcpipMode(string deviceSerial, int port)
        {
            string output = ExecuteAdbCommand($"-s {deviceSerial} tcpip {port}");
            return output.Contains($"restarting in TCP mode port: {port}");
        }

        /// <summary>
        /// ワイヤレスでデバイスに接続します。
        /// </summary>
        public bool ConnectWirelessDevice(string ipAddress, int port)
        {
            string address = $"{ipAddress}:{port}";
            string output = ExecuteAdbCommand($"connect {address}");
            return output.Contains($"connected to {address}") || output.Contains($"already connected to {address}");
        }

        /// <summary>
        /// ワイヤレス接続を切断します。
        /// </summary>
        public bool DisconnectWirelessDevice(string ipAddress, int port)
        {
            string address = $"{ipAddress}:{port}";
            string output = ExecuteAdbCommand($"disconnect {address}");
            return output.Contains($"disconnected {address}");
        }

        private string ExecuteAdbCommand(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = _adbPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            try
            {
                using (var process = Process.Start(psi))
                {
                    if (process == null) return "";

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error) && !error.Contains("adb server is out of date"))
                    {
                        Console.WriteLine($"ADB Error: {error}");
                    }
                    return output + error;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to execute adb command: {ex.Message}");
                return "";
            }
        }
        
        /// <summary>
        /// 指定されたシリアルがIPアドレス形式（Wi-Fi接続）かどうかを判定します。
        /// </summary>
        public bool IsWifiDevice(string serial)
        {
            return Regex.IsMatch(serial, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d+");
        }

        /// <summary>
        /// ADBからのデバイス情報を保持する内部クラス。
        /// </summary>
        public class AdbDevice
        {
            public string Serial { get; set; } = "";
            public string Model { get; set; } = "";
        }
    }
}