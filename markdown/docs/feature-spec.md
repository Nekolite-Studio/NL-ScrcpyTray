# 🤖 NL-ScrcpyTray 機能仕様書 (v1.x)

## 1. はじめに

本ドキュメントは、UX向上フェーズにおける **`NL-ScrcpyTray`** の機能詳細を定義する。
特に「**マルチデバイス管理**」と「**有線/無線ハンドオーバー**」に関する仕様を詳述する。

-----

## 2. 設定管理 (Data Model)

`settings.json` は、`mockups/setting-ui/1.html` で定義されたデータ構造に準拠し、アプリケーション全体の構成と全デバイスのプロファイルを管理する。

### 2.1. ルート構造

```json
{
  "GlobalAutoConnect": true,
  "Theme": "System", // "Light", "Dark", "System"
  "Devices": [
    // Deviceオブジェクトの配列 (後述)
  ]
}
```

### 2.2. Device オブジェクト構造

```json
{
  "Id": "dev-1701388800000", // 内部管理用の一意なID
  "Name": "Pixel 7",          // ユーザーが設定可能な表示名
  "Serial": "9A281FFAZ00000",  // デバイスのシリアルID (物理デバイスとの紐付けキー)
  "Model": "Pixel 7",           // adbから取得したモデル名
  "IpAddress": "192.168.1.15",  // 最後にキャッシュされたIPアドレス
  "Settings": {
    // DeviceSettingsオブジェクト (後述)
  }
}
```

### 2.3. DeviceSettings オブジェクト構造

```json
{
  // 一般設定
  "AutoConnect": true,
  "AutoSwitchToWifi": true,
  "AutoSwitchToUsb": true,
  "SeparateSettings": false,

  // プロファイル
  "UsbProfile": {
    // ConnectionProfileオブジェクト (後述)
  },
  "WifiProfile": {
    // ConnectionProfileオブジェクト (後述)
  },

  // 録画設定
  "Recording": {
    "RecordVideo": false,
    "RecordAudio": false,
    "RecordFormat": "mp4", // "mp4" or "mkv"
    "SavePath": "C:\\Users\\User\\Videos\\Scrcpy"
  }
}
```

### 2.4. ConnectionProfile オブジェクト構造

```json
{
  "videoEnabled": true,
  "displayEnabled": true,
  "videoBitrate": 8,       // Mbps
  "maxFps": 60,
  "maxSize": 0,            // 0: オリジナル
  "videoCodec": "h264",    // "h264", "h265", "av1"
  "videoBuffer": 50,       // ms

  "audioEnabled": true,
  "audioBitrate": 128,     // Kbps
  "audioCodec": "opus",    // "opus", "aac", "raw"
  "audioBuffer": 50        // ms
}
```

### 2.5. デバイスリスト仕様

  * **順序の意味:** `Devices` 配列のインデックス順を、**自動接続時の優先順位**として扱う。UI上でのドラッグ＆ドロップ操作を反映する。
  * **新規デバイス:** 未登録のシリアルIDを持つデバイスが接続された場合、配列の末尾に**自動追加**する。

-----

## 3. 接続ロジック

### 3.1. 優先順位付き自動接続

1.  **トリガー:** `DeviceManager` がデバイスの新規接続を検知。
2.  **スキャン:** `adb devices -l` で現在有効なデバイス一覧を取得。
3.  **マッチング:** `Devices` リストの**上位から順に**、スキャン結果に含まれるかを判定。
4.  **実行:**
      * 最初にマッチしたデバイスの設定 `AutoConnect` が `true` で、かつ `GlobalAutoConnect` も `true` であれば `scrcpy` を起動。
      * **複数同時ミラーリング対応:** 既に他のデバイスが実行中でも、追加で起動する。

### 3.2. スマート・ハンドオーバー (USB → Wi-Fi)

1.  **前提条件:**
      * デバイス設定の `AutoSwitchToWifi` が有効であること。
      * USB接続中に `DeviceManager` がデバイスのIPアドレスを正常に取得・キャッシュしていること。
2.  **フロー:**
      * `DeviceManager` がUSB切断を検知 → 即時に `AdbHelper.ConnectWirelessDevice` を実行。
      * 接続成功後、`WifiProfile` を適用して `scrcpy` を再起動。

### 3.3. スマート・ハンドオーバー (Wi-Fi → USB)

1.  **前提条件:**
      * デバイス設定の `AutoSwitchToUsb` が有効であること。
      * 現在Wi-Fi接続で `scrcpy` が稼働中であること。
2.  **フロー:**
      * `DeviceManager` が同一シリアルのUSB接続を検知 → Wi-Fi接続の `scrcpy` プロセスを終了。
      * `UsbProfile` を適用して `scrcpy` を再起動（低遅延・高画質化）。

-----

## 4. UI仕様 (WPF + WebView2 + React)

`mockups/setting-ui/1.html` で定義されたデザインと機能に完全に準拠する。

### 4.1. メイン画面構成 (React App)

  * **ヘッダー:**
      * アプリタイトル
      * **テーマ切り替え (Light/Dark/System)**
      * **一括自動接続トグル (`GlobalAutoConnect`)**
  * **デバイスリスト:**
      * 登録デバイスを**カード形式**でリスト表示。
      * **ドラッグ＆ドロップ**による優先順位の並べ替え機能。
      * 各カードに [ミラーリング開始/停止] ボタン、[詳細設定] ボタンを配置。
      * **接続状態:** `USB`, `Wi-Fi`, `USB+Wi-Fi`, `Offline` の4状態を視覚的に表示。
      * **デバイス情報:** デバイス名の下に、Wi-Fi接続時はIPアドレスを表示。
      * ミラーリング状態、設定概要を視覚的に表示。

### 4.2. 詳細設定モーダル (React App)

  * **タブ構成:** [一般] [映像] [音声] [録画] [情報] の5タブ構成。
  * **プロファイル切り替え:**
      * `SeparateSettings` が有効な場合、[映像] [音声] タブ内に「**有線設定 / 無線設定**」のサブタブを表示し、`UsbProfile` と `WifiProfile` を個別に編集可能にする。
  * **即時反映:**
      * 「設定を保存」ボタン押下時、`WebView2 Bridge` 経由でバックエンドの `DeviceManager` に設定更新を通知する。
      * 該当デバイスがミラーリング中であれば、`DeviceManager` は `ScrcpyProcessManager` にプロセスの再起動を指示し、新設定を反映させる。

-----

## 5. 通知仕様

| イベント | メッセージ | アクション |
| :--- | :--- | :--- |
| **ハンドオーバー成功** | `(デバイス名) を無線接続に切り替えました。` | - |
| **ハンドオーバー失敗** | `(デバイス名) の無線接続に失敗しました。` | 再試行ボタン |
| **新規デバイス登録** | `新しいデバイス (モデル名) が登録されました。` | 設定画面を開く |
