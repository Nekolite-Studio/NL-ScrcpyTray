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
    }

    /// <summary>
    /// adb.exe との対話を担当するヘルパークラス。
    /// </summary>
    public static class AdbHelper
    {
        /// <summary>
        /// scrcpy のパスに基づいて、内蔵されている adb.exe のフルパスを取得します。
        /// </summary>
        /// <param name="scrcpyFullPath">scrcpy.exe のフルパス。</param>
        /// <returns>adb.exe のフルパス。見つからない場合は null。</returns>
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
        /// <param name="scrcpyFullPath">scrcpy.exe のフルパス。</param>
        /// <returns>接続されているデバイスのリスト。</returns>
        public static List<AdbDevice> GetConnectedDevices(string scrcpyFullPath)
        {
            var devices = new List<AdbDevice>();
            var adbPath = GetAdbPath(scrcpyFullPath);

            if (adbPath == null)
            {
                // adb.exe が見つからない場合は空のリストを返す
                return devices;
            }

            var psi = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = "devices -l",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            try
            {
                using (var process = Process.Start(psi))
                {
                    if (process == null) return devices;

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // "List of devices attached" の行はスキップ
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Where(line => !line.StartsWith("List of devices"));

                    foreach (var line in lines)
                    {
                        // line format: "serial_number    device product:PRODUCT model:MODEL device:DEVICE transport_id:ID"
                        // 例: "R5CR8232GDE      device product:star2lteks model:SM_G965N device:star2lte transport_id:2"
                        if (line.Contains("device") && !line.Contains("unauthorized"))
                        {
                            var serialMatch = Regex.Match(line, @"^([a-zA-Z0-9\._-]+)\s+");
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
                }
            }
            catch (Exception ex)
            {
                // 本来はロギングするべき
                Debug.WriteLine($"Failed to get connected devices: {ex.Message}");
            }

            return devices;
        }
    }
}