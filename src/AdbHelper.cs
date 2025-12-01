using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScrcpyTray
{
    // デバイス情報を保持するクラス
    public class AdbDevice
    {
        public string Serial { get; set; } = "";
        public string Model { get; set; } = "";
        public string Product { get; set; } = "";
        public string TransportId { get; set; } = "";
        public bool IsWireless => Serial.Contains(":");
    }

    /// <summary>
    /// adb.exe との対話を担当するヘルパークラス。
    /// </summary>
    public static class AdbHelper
    {
        /// <summary>
        /// adb コマンドを非同期で実行し、標準出力を返します。
        /// </summary>
        private static string ExecuteAdbCommand(string adbPath, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = adbPath,
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

                    // "adb server is out of date" は無視できるエラーなのでログから除外
                    if (!string.IsNullOrEmpty(error) && !error.Contains("adb server is out of date"))
                    {
                        Debug.WriteLine($"ADB Error: {error}");
                    }
                    // 正常出力とエラー出力を両方返す
                    return output + error;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to execute adb command: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// scrcpy のパスに基づいて、内蔵されている adb.exe のフルパスを取得します。
        /// </summary>
        private static string? GetAdbPath(string scrcpyFullPath)
        {
            string? scrcpyDir = Path.GetDirectoryName(scrcpyFullPath);
            if (string.IsNullOrEmpty(scrcpyDir) || !Directory.Exists(scrcpyDir))
            {
                return null;
            }
            string adbPath = Path.Combine(scrcpyDir, "adb.exe");
            return File.Exists(adbPath) ? adbPath : null;
        }

        /// <summary>
        /// 現在PCに接続されているAndroidデバイスのリストを取得します。
        /// </summary>
        public static List<AdbDevice> GetConnectedDevices(string scrcpyFullPath)
        {
            var devices = new List<AdbDevice>();
            var adbPath = GetAdbPath(scrcpyFullPath);

            if (adbPath == null)
            {
                // adb.exe が見つからない場合は空のリストを返す
                return devices;
            }

            string output = ExecuteAdbCommand(adbPath, "devices -l");

            // "List of devices attached" の行はスキップ
            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                              .Where(line => !line.StartsWith("List of devices"));

            foreach (var line in lines)
            {
                // line format: "serial_number    device product:PRODUCT model:MODEL device:DEVICE transport_id:ID"
                // 例: "192.168.1.5:5555 device product:star2lteks model:SM_G965N device:star2lte transport_id:2"
                if (line.Contains("device") && !line.Contains("unauthorized"))
                {
                    var serialMatch = Regex.Match(line, @"^([a-zA-Z0-9\._:\-]+)\s+"); // IP:Port形式を許容
                    var modelMatch = Regex.Match(line, @"model:(\S+)");
                    var productMatch = Regex.Match(line, @"product:(\S+)");
                    var transportIdMatch = Regex.Match(line, @"transport_id:(\S+)");

                    if (serialMatch.Success)
                    {
                        devices.Add(new AdbDevice
                        {
                            Serial = serialMatch.Groups[1].Value,
                            Model = modelMatch.Success ? modelMatch.Groups[1].Value : "Unknown",
                            Product = productMatch.Success ? productMatch.Groups[1].Value : "Unknown",
                            TransportId = transportIdMatch.Success ? transportIdMatch.Groups[1].Value : "Unknown",
                        });
                    }
                }
            }
            return devices;
        }

        /// <summary>
        /// 指定されたデバイスのIPアドレスを取得します。
        /// </summary>
        public static string? GetDeviceIpAddress(string scrcpyFullPath, string deviceSerial)
        {
            var adbPath = GetAdbPath(scrcpyFullPath);
            if (adbPath == null) return null;

            string output = ExecuteAdbCommand(adbPath, $"-s {deviceSerial} shell ip addr show wlan0");

            // 例: "inet 192.168.1.5/24 brd 192.168.1.255 scope global wlan0"
            var match = Regex.Match(output, @"inet (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})");
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        /// 指定されたデバイスでTCP/IPモードを有効にします。
        /// </summary>
        public static bool EnableTcpipMode(string scrcpyFullPath, string deviceSerial, int port)
        {
            var adbPath = GetAdbPath(scrcpyFullPath);
            if (adbPath == null) return false;

            string output = ExecuteAdbCommand(adbPath, $"-s {deviceSerial} tcpip {port}");
            return output.Contains($"restarting in TCP mode port: {port}");
        }

        /// <summary>
        /// ワイヤレスでデバイスに接続します。
        /// </summary>
        public static bool ConnectWirelessDevice(string scrcpyFullPath, string ipAddress, int port)
        {
            var adbPath = GetAdbPath(scrcpyFullPath);
            if (adbPath == null) return false;

            string address = $"{ipAddress}:{port}";
            string output = ExecuteAdbCommand(adbPath, $"connect {address}");
            // 成功時は "connected to 192.168.1.5:5555"
            // 失敗時は "failed to connect to 192.168.1.5:5555"
            // 既に接続済みの場合は "already connected to 192.168.1.5:5555"
            return output.Contains($"connected to {address}") || output.Contains($"already connected to {address}");
        }

        /// <summary>
        /// ワイヤレス接続を切断します。
        /// </summary>
        public static bool DisconnectWirelessDevice(string scrcpyFullPath, string ipAddress, int port)
        {
            var adbPath = GetAdbPath(scrcpyFullPath);
            if (adbPath == null) return false;

            string address = $"{ipAddress}:{port}";
            string output = ExecuteAdbCommand(adbPath, $"disconnect {address}");
            return output.Contains($"disconnected {address}");
        }
    }
}