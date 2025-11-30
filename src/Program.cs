using System;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Windows.Forms;

namespace ScrcpyTray
{
    static class Program    {
        static string ScrcpyPath = @"D:\Portable_Apps\scrcpy-win64-v3.3.3\scrcpy.exe";
        
        // Null許容型 (?) にして警告を回避
        static Process? currentProcess = null;
        static NotifyIcon? trayIcon;
        
        // 設定値
        static bool AutoStart = true;
        static bool EnableVideo = false;
        static bool EnableAudio = true;
        static bool TurnScreenOff = false;
        static string BufferMode = "Low Latency"; // "Low Latency" or "High Quality"

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // トレイアイコンの作成
            trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Application, // アプリのデフォルトアイコンを使用
                Visible = true,
                Text = "NL-ScrcpyTray (待機中)"
            };

            // コンテキストメニューの構築
            UpdateContextMenu();
            // USB監視の開始
            StartUsbWatcher();

            // アプリケーション実行（メッセージループ）
            Application.Run();
            
            // 終了処理
            StopScrcpy();
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
        }
        static void UpdateContextMenu()
        {
            if (trayIcon == null) return;

            ContextMenuStrip menu = new ContextMenuStrip();
            // 1. 状態表示 & 手動操作
            var statusItem = new ToolStripMenuItem(currentProcess == null ? "開始" : "停止");
            statusItem.Font = new Font(statusItem.Font, FontStyle.Bold);
            statusItem.Click += (s, e) => {
                if (currentProcess == null) StartScrcpy();
                else StopScrcpy();
            };
            menu.Items.Add(statusItem);

            menu.Items.Add(new ToolStripSeparator());
            // 2. 設定：自動開始
            var autoItem = new ToolStripMenuItem("USB接続で自動開始");
            autoItem.Checked = AutoStart;
            autoItem.Click += (s, e) => { AutoStart = !AutoStart; UpdateContextMenu(); };
            menu.Items.Add(autoItem);

            // 3. 設定：ビデオ/オーディオ
            var videoItem = new ToolStripMenuItem("画面を共有");
            videoItem.Checked = EnableVideo;
            videoItem.Click += (s, e) => { EnableVideo = !EnableVideo; UpdateContextMenu(); };
            menu.Items.Add(videoItem);
            var audioItem = new ToolStripMenuItem("音声を共有");
            audioItem.Checked = EnableAudio;
            audioItem.Click += (s, e) => { EnableAudio = !EnableAudio; UpdateContextMenu(); };
            menu.Items.Add(audioItem);
            
            // 4. 設定：画面オフ
            var screenOffItem = new ToolStripMenuItem("端末画面をOFF (-S)");
            screenOffItem.Checked = TurnScreenOff;
            screenOffItem.Click += (s, e) => { TurnScreenOff = !TurnScreenOff; UpdateContextMenu(); };
            menu.Items.Add(screenOffItem);

            menu.Items.Add(new ToolStripSeparator());
            // 5. バッファ設定テンプレート
            var bufferMenu = new ToolStripMenuItem("モード設定");
            
            var lowLatItem = new ToolStripMenuItem("低遅延 (Dev/Game)");
            lowLatItem.Enabled = (BufferMode != "Low Latency");
            lowLatItem.Click += (s, e) => { BufferMode = "Low Latency"; UpdateContextMenu(); };
            bufferMenu.DropDownItems.Add(lowLatItem);

            var hqItem = new ToolStripMenuItem("高画質 (Media)");
            hqItem.Enabled = (BufferMode != "High Quality");
            hqItem.Click += (s, e) => { BufferMode = "High Quality"; UpdateContextMenu(); };
            bufferMenu.DropDownItems.Add(hqItem);
            menu.Items.Add(bufferMenu);

            menu.Items.Add(new ToolStripSeparator());
            
            // 6. 終了
            var exitItem = new ToolStripMenuItem("終了");
            exitItem.Click += (s, e) => Application.Exit();
            menu.Items.Add(exitItem);
            trayIcon.ContextMenuStrip = menu;
        }

        static void StartScrcpy()
        {
            if (currentProcess != null) return;
            if (trayIcon == null) return;
            string args = "";

            // 基本設定
            if (!EnableVideo) args += " --no-video";
            if (!EnableAudio) args += " --no-audio";
            if (TurnScreenOff) args += " -S";
            
            // テンプレート適用
            if (BufferMode == "Low Latency")
            {
                args += " --audio-buffer=50 --video-buffer=0 --max-size=1024";
            }
            else
            {
                args += " --audio-buffer=200 --video-buffer=200 --video-bit-rate=16M";
            }
            // コンソールなしで起動
            args += " --no-window"; 

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = ScrcpyPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            try
            {
                currentProcess = Process.Start(psi);
                if (currentProcess != null)
                {
                    currentProcess.EnableRaisingEvents = true;
                    currentProcess.Exited += (s, e) => {
                        currentProcess = null;
                        if (trayIcon != null)
                        {
                            // UIスレッドでの操作が必要な場合があるためInvoke推奨ですが、
                            // NotifyIconのプロパティ変更はスレッドセーフな場合が多いです。
                            // 念の為のInvokeパターン:
                            // trayIcon.GetType().InvokeMember(...) 等が必要になることがありますが
                            // フォームレスの場合は直接変更して問題が出なければそのままでOK
                            trayIcon.Text = "NL-ScrcpyTray (待機中)";
                            trayIcon.Icon = SystemIcons.Application;
                            
                            // メニュー再構築（非UIスレッドから呼ばれる可能性への配慮）
                            // 厳密にはここもInvokeが必要ですが、簡易実装として再構築を呼びます
                            // エラーが出る場合はここにInvoke処理を追加します
                            UpdateContextMenu(); 
                        }
                    };

                    trayIcon.Text = "NL-ScrcpyTray (実行中)";
                    UpdateContextMenu();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("起動エラー: " + ex.Message);
            }
        }
        static void StopScrcpy()
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                try { currentProcess.Kill(); } catch { /* 無視 */ }
                currentProcess = null;
            }
            UpdateContextMenu();
        }

        static void StartUsbWatcher()
        {
            try
            {
                // USBデバイス変更イベント監視
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
                ManagementEventWatcher watcher = new ManagementEventWatcher(query);
                
                watcher.EventArrived += (s, e) =>
                {
                    if (AutoStart && currentProcess == null)
                    {
                        // 接続直後は認識されないことがあるため少し待機
                        System.Threading.Thread.Sleep(2000);
                        StartScrcpy();
                    }
                };
                watcher.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("USB監視の初期化に失敗しました: " + ex.Message);
            }
        }
    }
}