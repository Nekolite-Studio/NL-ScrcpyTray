# NL-ScrcpyTray 機能仕様書

## 1. はじめに

このドキュメントは `NL-ScrcpyTray` の各機能の具体的な振る舞い（仕様）を定義します。
実装はこの仕様書に基づいて行われ、コードとドキュメントの乖離を防ぎます。

## 2. 設定管理 (`settings.json`)

アプリケーションの動作は、実行ファイルと同じディレクトリに配置される `settings.json` によって制御されます。ファイルが存在しない場合、アプリケーションは以下のデフォルト値で設定ファイルを自動生成します。

### 2.1. 設定項目

設定ファイルは、以下の構造を持つJSONオブジェクトです。

```json
{
  "ScrcpyPath": "scrcpy/scrcpy.exe",
  "AutoStartOnConnect": true,
  "EnableVideo": true,
  "EnableAudio": true,
  "TurnScreenOffOnStart": false,
  "BufferMode": "Low Latency",
  "AdbDeviceSerial": null
}
```

### 2.2. 各項目の詳細

| キー | 型 | デフォルト値 | 説明 |
| :--- | :--- | :--- | :--- |
| `ScrcpyPath` | string | `"scrcpy/scrcpy.exe"` | `scrcpy.exe` への相対パスまたは絶対パス。 |
| `AutoStartOnConnect` | boolean | `true` | USBデバイス接続時に `scrcpy` を自動で起動するかどうか。 |
| `EnableVideo` | boolean | `true` | 画面のミラーリングを有効にするか。`false` の場合、音声のみ転送される。 |
| `EnableAudio` | boolean | `true` | 音声の転送を有効にするか。 |
| `TurnScreenOffOnStart` | boolean | `false` | `scrcpy` 起動時に、接続されたデバイスの物理スクリーンをOFFにするか。 |
| `BufferMode` | string | `"Low Latency"` | 転送モードのプリセット。`"Low Latency"` または `"High Quality"` を指定可能。 |
| `AdbDeviceSerial` | string/null | `null` | 複数デバイス接続時に、対象とするデバイスのシリアル番号。`null` の場合は `adb` が最初に見つけたデバイスを対象とする。 |

## 3. scrcpy 起動ロジック

`scrcpy` プロセスは、`settings.json` の内容に基づいて構築されたコマンドライン引数で起動されます。

### 3.1. 引数マッピング

| 設定項目 | 値 | 生成される引数 |
| :--- | :--- | :--- |
| `EnableVideo` | `false` | `--no-video` |
| `EnableAudio` | `false` | `--no-audio` |
| `TurnScreenOffOnStart` | `true` | `-S` |
| `BufferMode` | `"Low Latency"` | `--audio-buffer=50 --video-buffer=0 --max-size=1024` |
| `BufferMode` | `"High Quality"` | `--audio-buffer=200 --video-buffer=200 --video-bit-rate=16M` |
| `AdbDeviceSerial` | (シリアル番号) | `-s (シリアル番号)` |

**Note:** 上記に加え、コンソールウィンドウを非表示にするための `--no-window` 引数が常に追加されます。

## 4. 通知機能

ユーザーへのフィードバックは、Windows標準の通知機能（バルーンチップまたはトースト通知）を用いて行います。

### 4.1. 通知イベント

| イベント | タイトル | メッセージ | クリック時の動作 |
| :--- | :--- | :--- | :--- |
| **USBデバイス接続** | デバイス接続 | `(デバイス名) が接続されました。` | (未定) scrcpyウィンドウをアクティブ化 |
| **USBデバイス切断** | デバイス切断 | `(デバイス名) が切断されました。` | 何もしない |
| **scrcpy起動成功** | 実行中 | `(デバイス名) への画面転送を開始しました。\nモード: (モード名)` | scrcpyウィンドウをアクティブ化 |
| **scrcpy起動失敗** | 起動エラー | `scrcpyの起動に失敗しました。設定を確認してください。` | 設定画面を開く |
| **scrcpy異常終了** | デバイス切断 | `音声共有はAndroid 11以降の機能です。`など、`scrcpy`のエラー出力を基にしたメッセージ。 | 設定画面を開く |
| **複数デバイス検出** | デバイス選択が必要 | `複数のデバイスが検出されました。対象を1つ選択してください。` | デバイス選択UIを開く |

**Note:** `(デバイス名)` の取得は `adb devices -l` コマンドの結果を利用することを想定しています。

## 5. UI (コンテキストメニュー)

タスクトレイアイコンを右クリックすると、以下の項目を持つコンテキストメニューが表示されます。メニューの状態は `settings.json` の内容と `scrcpy` の実行状態に応じて動的に更新されます。

| メニュー項目 | 種別 | 状態ロジック | アクション |
| :--- | :--- | :--- | :--- |
| **開始 / 停止** | ボタン | scrcpy実行中なら「停止」、停止中なら「開始」と表示。 | `scrcpy` プロセスの起動/停止をトグルする。 |
| (セパレーター) | - | - | - |
| **USB接続で自動開始** | チェックボックス | `AutoStartOnConnect` の値と連動。 | `AutoStartOnConnect` の値をトグルし、`settings.json` に保存。 |
| **画面を共有** | チェックボックス | `EnableVideo` の値と連動。 | `EnableVideo` の値をトグルし、`settings.json` に保存。 |
| **音声を共有** | チェックボックス | `EnableAudio` の値と連動。 | `EnableAudio` の値をトグルし、`settings.json` に保存。 |
| **端末画面をOFF** | チェックボックス | `TurnScreenOffOnStart` の値と連動。 | `TurnScreenOffOnStart` の値をトグルし、`settings.json` に保存。 |
| (セパレーター) | - | - | - |
| **モード設定** | サブメニュー | - | - |
|  L **低遅延 (Dev/Game)** | ラジオボタン | `BufferMode` が `"Low Latency"` なら選択状態。 | `BufferMode` を `"Low Latency"` に設定し、`settings.json` に保存。 |
| L **高画質 (Media)** | ラジオボタン | `BufferMode` が `"High Quality"` なら選択状態。 | `BufferMode` を `"High Quality"` に設定し、`settings.json` に保存。 |
| (セパレーター) | - | - | - |
| **対象デバイス** | サブメニュー | 接続デバイスが複数ある場合のみ表示。 | - |
|  L **(デバイス名)** | ラジオボタン | `AdbDeviceSerial` の値と連動。 | `AdbDeviceSerial` を選択されたデバイスのシリアルに設定し、`settings.json` に保存。`scrcpy` 実行中なら再起動。 |
| **設定...** | ボタン | (常に有効) | 設定ウィンドウ (`SettingsForm`) を開く。 |
| **終了** | ボタン | (常に有効) | アプリケーションを終了する。 |

## 6. UI (設定画面 `SettingsForm`)

設定画面はタブで構成され、各機能の設定をGUIで変更できます。

| タブ | UIコンポーネント | 状態ロジック | アクション |
| :--- | :--- | :--- | :--- |
| **一般** | (各種チェックボックス) | `AppConfig` の値と連動。 | 対応する `AppConfig` のプロパティをトグルする。 |
| **画質** | (各種ラジオボタン) | `BufferMode` の値と連動。 | `BufferMode` の値を変更する。 |
| **デバイス** | **優先デバイス** ドロップダウンリスト | `AdbDeviceSerial` の値と連動。 | `AdbDeviceSerial` を選択されたデバイスのシリアルに設定する。 |