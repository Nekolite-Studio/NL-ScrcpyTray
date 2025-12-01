using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NL_ScrcpyTray.Models
{
    /// <summary>
    /// デバイスの接続状態を示します。
    /// </summary>
    public enum ConnectionStatus
    {
        Offline,
        Usb,
        Wifi
    }

    /// <summary>
    /// UIに表示するための、揮発的な状態を含むデバイスのViewModel。
    /// </summary>
    public class DeviceViewModel
    {
        // --- 永続化されるプロパティ (Deviceからコピー) ---
        public string Id { get; set; }
        public string Name { get; set; }
        public string Serial { get; set; }
        public string Model { get; set; }
        public string? IpAddress { get; set; }
        public DeviceSettings Settings { get; set; }

        // --- 揮発性の状態 ---
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ConnectionStatus Status { get; set; } = ConnectionStatus.Offline;
        public bool IsMirroring { get; set; } = false;

        // コンストラクタ
        public DeviceViewModel(Device device)
        {
            Id = device.Id;
            Name = device.Name;
            Serial = device.Serial;
            Model = device.Model;
            IpAddress = device.IpAddress;
            Settings = device.Settings;
        }
    }


    // --- 以下、既存のデータモデル ---

    /// <summary>
    /// settings.json のルートオブジェクトに対応します。
    /// </summary>
    public class AppSettings
    {
        public bool GlobalAutoConnect { get; set; } = true;
        public string Theme { get; set; } = "System"; // "Light", "Dark", "System"
        public List<Device> Devices { get; set; } = new();
    }

    /// <summary>
    /// 管理対象の単一デバイスを表します。(永続化用)
    /// </summary>
    public class Device
    {
        public string Id { get; set; } = $"dev-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        public string Name { get; set; } = "New Device";
        public required string Serial { get; set; }
        public string Model { get; set; } = "Unknown";
        public string? IpAddress { get; set; }
        public DeviceSettings Settings { get; set; } = new();
    }

    /// <summary>
    /// デバイスごとの詳細設定を保持します。
    /// </summary>
    public class DeviceSettings
    {
        public bool AutoConnect { get; set; } = true;
        public bool AutoSwitchToWifi { get; set; } = true;
        public bool AutoSwitchToUsb { get; set; } = true;
        public bool SeparateSettings { get; set; } = false;
        public ConnectionProfile UsbProfile { get; set; } = new();
        public ConnectionProfile WifiProfile { get; set; } = new() { VideoBitrate = 4, VideoBuffer = 200 };
        public RecordingSettings Recording { get; set; } = new();
    }

    /// <summary>
    /// scrcpyの接続プロファイル (映像・音声設定) を定義します。
    /// </summary>
    public class ConnectionProfile
    {
        public int VideoBitrate { get; set; } = 8;       // Mbps
        public int MaxFps { get; set; } = 60;
        public int MaxSize { get; set; } = 0;            // 0: オリジナル
        public string VideoCodec { get; set; } = "h264";    // "h264", "h265", "av1"
        public int VideoBuffer { get; set; } = 50;       // ms
        public bool AudioEnabled { get; set; } = true;
        public int AudioBitrate { get; set; } = 128;     // Kbps
        public string AudioCodec { get; set; } = "opus";    // "opus", "aac", "raw"
        public int AudioBuffer { get; set; } = 50;       // ms
    }

    /// <summary>
    /// 録画設定を定義します。
    /// </summary>
    public class RecordingSettings
    {
        public bool RecordVideo { get; set; } = false;
        public bool RecordAudio { get; set; } = false;
        public string RecordFormat { get; set; } = "mp4"; // "mp4" or "mkv"
        public string SavePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
    }
}