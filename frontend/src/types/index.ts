export type ConnectionStatus = 'Offline' | 'Usb' | 'Wifi';

// 接続プロファイル
export interface ConnectionProfile {
  videoBitrate: number; // Mbps
  maxFps: number;
  maxSize: number; // 0 for original
  videoCodec: 'h264' | 'h265' | 'av1';
  videoBuffer: number; // ms
  
  audioEnabled: boolean;
  audioBitrate: number; // Kbps
  audioCodec: 'opus' | 'aac' | 'raw';
  audioBuffer: number; // ms
}

// 録画設定
export interface RecordingSettings {
    recordVideo: boolean;
    recordAudio: boolean;
    recordFormat: 'mp4' | 'mkv';
    savePath: string;
}

export interface DeviceSettings {
  // General
  autoConnect: boolean;
  autoSwitchToWifi: boolean;
  autoSwitchToUsb: boolean;
  separateSettings: boolean;

  // Profiles
  usbProfile: ConnectionProfile;
  wifiProfile: ConnectionProfile;

  // Recording
  recording: RecordingSettings;
}

export interface DeviceViewModel {
  // --- 永続化されるプロパティ ---
  id: string;
  name: string;
  serial: string;
  model: string;
  ipAddress?: string;
  settings: DeviceSettings;

  // --- 揮発性の状態 ---
  status: ConnectionStatus;
  isMirroring: boolean;
}